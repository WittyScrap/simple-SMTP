using Shell;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
	/// <summary>
	/// Represents a shell that can listen to any incoming connection and can
	/// function as a generic server for any kind of application.
	/// </summary>
	class ServerShell : AsyncShell<ServerCommandSet>
	{
		/// <summary>
		/// Loads a server program into this server shell.
		/// </summary>
		/// <param name="server"></param>
		public void LoadServer(IServerProgram server)
		{
			_serverProgram = server;
		}

		/// <summary>
		/// Runs the server.
		/// </summary>
		public void StartServer(string host, int port)
		{
			if (_serverProgram == null)
			{
				throw new InvalidOperationException("Could not start a server because no server program was loaded, please use the program command to load a server program.");
			}

			if (_serverStarted)
			{
				throw new InvalidOperationException("Could not start a server because a server has already been created, please use the stop command before starting a new server.");
			}

			SocketPermission permission = new SocketPermission(NetworkAccess.Accept, TransportType.Tcp, host, port);

			if (!IPAddress.TryParse(host, out IPAddress ipAddress))
			{
				IPHostEntry ipHost = Dns.GetHostEntry(host);
				ipAddress = ipHost.AddressList[0];
			}

			IPEndPoint endPoint = new IPEndPoint(ipAddress, port);

			_read = new List<Socket>();
			_clients = new Dictionary<Socket, ClientState>();

			_listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
			_listener.Bind(endPoint);
			_listener.Listen(Variables.Get<int>("network.max_clients"));

			EnqueueCommand(() => Print(entityMachine, $"Server started on {host}:{port}"));

			// Main server thread.
			_selectorThread = new Thread(ServerLoop);
			_selectorThread.Start();
		}

		/// <summary>
		/// Main thread server loop.
		/// </summary>
		private void ServerLoop()
		{
			_serverStarted = true;
			_serverIdle = false;

			while (_serverStarted)
			{
				_read.Clear();
				_read.Add(_listener);

				foreach (ClientState client in _clients.Values)
				{
					_read.Add(client.Connection);
				}

				Task selection = Task.Run(() => Socket.Select(_read, null, null, Variables.Get<int>("network.select_timeout"))); // Runs selection in parallel as to not stop the thread from closing if necessary.

				while (_serverStarted && !selection.IsCompleted)
				{
					; // NOP
				}

				if (_serverStarted)
				{
					foreach (Socket socket in _read)
					{
						if (socket == _listener)
						{
							AcceptConnection(socket);
						}
						else
						{
							HandleConnection(socket);
						}
					}
				}
			}

			_serverIdle = true;
		}

		/// <summary>
		/// Accepts a new connection.
		/// </summary>
		/// <param name="listener">The listening socket.</param>
		private void AcceptConnection(Socket listener)
		{
			Socket client = listener.Accept();
			ClientState state = new ClientState(client, BufferSize);

			EnqueueCommand(() => Print(entityMachine, $"Accepting connection from: {GetRemote(client)}."));

			_clients[client] = state;
		}

		/// <summary>
		/// Handles a connection.
		/// </summary>
		/// <param name="connection">The open connection.</param>
		private void HandleConnection(Socket connection)
		{
			ClientState client = _clients[connection];
			int bytesReceived = 0;

			try
			{
				bytesReceived = connection.Receive(client.ReadBuffer);
			}
			catch (SocketException e)
			{
				connection.Close();
				_clients.Remove(connection);

				EnqueueCommand(() => Print(entityMachine, $"Connection receive exception: {e.ToString()}."));
				return;
			}

			string hostName = GetRemote(connection);

			if (bytesReceived == 0)
			{
				connection.Close();
				_clients.Remove(connection);

				EnqueueCommand(() => Print(entityMachine, $"Connection closed with: {hostName}."));
				return;
			}

			string data = Decode(client.ReadBuffer);
			string response = _serverProgram.HandleInput(connection, data);

			byte[] sendBytes = Encode(response);
			connection.Send(sendBytes);

			EnqueueCommand(() => Print(hostName, data));
			EnqueueCommand(() => Print($"[{entityMachine}->{hostName}]", response));

			ClearBuffer(client.ReadBuffer);
		}

		/// <summary>
		/// Clears the input buffer.
		/// </summary>
		/// <param name="buffer">The buffer to be cleared.</param>
		private void ClearBuffer(byte[] buffer)
		{
			for (int i = 0; i < buffer.Length; ++i)
			{
				buffer[i] = 0;
			}
		}

		/// <summary>
		/// Retrieves the remote's friendly name.
		/// </summary>
		/// <param name="connection">The connection to retrieve the name from.</param>
		/// <returns>The name of the connection's endpoint.</returns>
		private string GetRemote(Socket connection)
		{
			IPEndPoint endPoint = (IPEndPoint)connection.RemoteEndPoint;
			IPAddress ipAddress = endPoint.Address;

			return ipAddress.ToString() + ":" + endPoint.Port;
		}

		/// <summary>
		/// Stops a running server.
		/// </summary>
		public void StopServer()
		{
			if (!_serverStarted)
			{
				throw new InvalidOperationException("Cannot stop a server that is not running!");
			}

			_serverStarted = false;

			EnqueueCommand(() => Print(entityMachine, "Server loop stopped, waiting for worker thread to exit..."));

			while (!_serverIdle)
			{
				; // NOP, wait until server thread has finished while-loop body and exited safely.
			}

			EnqueueCommand(() => Print(entityMachine, $"Worker thread has quit main server loop, ending {_clients.Count} existing connection(s)..."));
			int clientId = 1;

			foreach (ClientState client in _clients.Values)
			{
				client.Connection.Dispose();

				EnqueueCommand(() => Print(entityMachine, $"Closed connection #{clientId++}..."));
			}

			EnqueueCommand(() => Print(entityMachine, "Done, refusing any uninitialised connection and closing listener..."));

			foreach (Socket readable in _read)
			{
				readable.Dispose();
			}

			try
			{
				_listener.Dispose();
			}
			catch (ObjectDisposedException)
			{
				// Oh, looks like this was completely unnecessary...
			}

			EnqueueCommand(() => Print(entityMachine, "Done, server closed!"));
		}

		/// <summary>
		/// Converts a string into an array of bytes for transferral across
		/// a network.
		/// </summary>
		/// <param name="sourceString">The string to convert to an array of bytes.</param>
		/// <returns>Array of bytes from the source string.</returns>
		private byte[] Encode(string sourceString)
		{
			return Encoding.UTF8.GetBytes(sourceString);
		}

		/// <summary>
		/// Converts a sequence of bytes into a readable string format.
		/// </summary>
		/// <param name="sourceBytes">The original byte array segment.</param>
		/// <returns>A string composed of the original array of bytes.</returns>
		private string Decode(byte[] sourceBytes)
		{
			return Encoding.UTF8.GetString(sourceBytes).Replace("\0", "");
		}

		/// <summary>
		/// Unmanaged command handler - ignore (unsupported).
		/// </summary>
		protected override void OnUnmanagedCommand(string command)
		{
			; // NOP
		}

		/* ------------ */
		/* --- Data --- */
		/* ------------ */

		private List<Socket> _read;
		private Dictionary<Socket, ClientState> _clients;
		private Socket _listener;

		private Thread _selectorThread;
		private IServerProgram _serverProgram;

		private bool _serverStarted;
		private bool _serverIdle;

		// -- Network data --

		/// <summary>
		/// The agreed size of the buffer.
		/// </summary>
		private int BufferSize => Variables.Get<int>("network.buffer_size");
	}
}
