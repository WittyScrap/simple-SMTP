﻿using NetworkSecurity;
using Shell;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
	/// <summary>
	/// Represents a shell that can listen to any incoming connection and can
	/// function as a generic server for any kind of application.
	/// </summary>
	public class ServerShell : AsyncShell<ServerCommandSet>
	{
		/// <summary>
		/// The program that is currently loaded on this shell.
		/// </summary>
		public IServerProgram Program => _serverProgram;

		/// <summary>
		/// Loads a server program into this server shell.
		/// </summary>
		/// <param name="server"></param>
		public void LoadServer(IServerProgram server)
		{
			_serverProgram = server;
		}

		/// <summary>
		/// Removes a specific connection.
		/// </summary>
		/// <param name="source"></param>
		private void RemoveConnection(Socket connection)
		{
			if (_clients.ContainsKey(connection))
			{
				connection.Close();
				_clients.Remove(connection);
			}
		}

		/// <summary>
		/// Runs the server.
		/// </summary>
		public void StartServer(string host, int port)
		{
			if (_serverProgram == null)
			{
				throw new InvalidOperationException("Could not start a server because no server program was loaded, please use the server command to load a server program.");
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
			_handlers = new List<Thread>();
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

				_handlers.RemoveAll(thread => !thread.IsAlive);

				foreach (ClientState client in _clients.Values)
				{
					_read.Add(client.Connection);
				}

				Task selection = Task.Run(() => // Runs selection in parallel as to not stop the thread from closing if necessary.
				{
					try
					{
						Socket.Select(_read, null, null, Variables.Get<int>("network.select_timeout"));
					}
					catch (SocketException)
					{
						; // NOP, just end the task.
					}
				});

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
							Thread handler = new Thread(() =>
							{
								try
								{
									HandleConnection(socket);
								}
								catch (Exception readError)
								{
									Format.Error(this, entityMachine, readError.Message);
								}
							});

							_handlers.Add(handler);
							handler.Start();
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
		/// Handles the optional welcome message.
		/// </summary>
		/// <param name="connection">The connection to send the message to.</param>
		private void WelcomeMessage(Socket connection)
		{
			string hostName = GetRemote(connection);
			string init = _serverProgram.OnConnection(connection);

			if (init != null)
			{
				Send(connection, init);
				EnqueueCommand(() => Print($"[{entityMachine}->{hostName}]", init));
			}

			ClearDisconnected();
		}

		/// <summary>
		/// Handles a connection.
		/// </summary>
		/// <param name="connection">The open connection.</param>
		private void HandleConnection(Socket connection)
		{
			if (!_clients.ContainsKey(connection))
			{
				return;
			}

			ClientState client = _clients[connection];
			int bytesReceived = 0;

			try
			{
				bytesReceived = connection.Receive(client.ReadBuffer);
			}
			catch (SocketException/* e*/)
			{
				RemoveConnection(connection);
//				Format.Error(this, entityMachine, e.Message);	// Okay but we don't want this. Do we want this? No. Yes? Nah.
																//
																// The reason why is that we can sometimes get exceptions thrown
																// with regards to a socket that is being read from the listening
																// thread immediately after its connection had already been closed.
																// This yields a SocketException, and due to there not being much 
																// can be done to change it, it's best to mute it instead for cleanliness.

				return;
			}

			string hostName = GetRemote(connection);

			if (bytesReceived == 0)
			{
				RemoveConnection(connection);
				EnqueueCommand(() => Print(entityMachine, $"Connection closed with: {hostName}."));

				return;
			}

			string data = Decode(client.ReadBuffer);

			if (!UsingEncryption || _clients[connection].Key.IsReady)
			{
				EvaluateResponse(connection, data);
			}
			else
			{
				OnKeyExchangeCheck(connection, data);
			}

			ClearBuffer(client.ReadBuffer);
			ClearDisconnected();
		}

		/// <summary>
		/// Passes the received data to the server program to evaluate
		/// a response and sends it back to the client.
		/// </summary>
		private void EvaluateResponse(Socket connection, string data)
		{
			if (Variables.Get<bool>("network.encrypted"))
			{
				data = DecryptMessage(data, _clients[connection].Key.EncryptionKey);
			}

			if (data != null)
			{
				EnqueueCommand(() => Print(GetRemote(connection), data));
				string response = _serverProgram.HandleInput(connection, data);

				if (response != null)
				{
					SendMultiline(connection, response);
				}
			}
		}

		/// <summary>
		/// Updates the key exchange sequence.
		/// </summary>
		private void OnKeyExchangeCheck(Socket connection, string data)
		{
			ExchangeListener key = _clients[connection].Key;
			string response = null;

			if (!key.HasFailed && !key.IsReady)
			{
				response = key.Evaluate(data);
			}

			if (response != null)
			{
				Send(connection, response);
			}
			else
			{
				if (key.HasFailed)
				{
					EnqueueCommand(() => Print(entityMachine, "Key transaction failed, closing..."));
					RemoveConnection(connection);
				}
				else if (key.IsReady)
				{
					WelcomeMessage(connection);
				}
			}
		}

		/// <summary>
		/// Removes and closes all sockets that have requested a disconnection.
		/// </summary>
		private void ClearDisconnected()
		{
			foreach (Socket disconnected in _serverProgram.Disconnected)
			{
				RemoveConnection(disconnected);
			}
		}

		/// <summary>
		/// Sends a response to a given connection, with support for multiple lines.
		/// </summary>
		/// <param name="multilineResponse">The response to send, will be split across carriage return/linefeed sequences.</param>
		private void SendMultiline(Socket connection, string multilineResponse)
		{
			if (multilineResponse == "")
			{
				return;
			}

			string hostName = GetRemote(connection);
			LinkedList<string> sections = new LinkedList<string>(multilineResponse.Split(new string[] { "\r\n" }, StringSplitOptions.None));

			if (string.IsNullOrEmpty(sections.Last.Value))
			{
				sections.RemoveLast();
			}

			foreach (string response in sections)
			{
				Send(connection, response);
				EnqueueCommand(() => Print($"[{entityMachine}->{hostName}]", response));
			}
		}

		/// <summary>
		/// Sends a message to a remote connection.
		/// </summary>
		/// <param name="connection">The connection to send the message to.</param>
		/// <param name="message">The message to transmit</param>
		public void Send(Socket connection, string message)
		{
			if (_clients[connection].Key.IsReady && UsingEncryption)
			{
				message = EncryptMessage(message, _clients[connection].Key.EncryptionKey);
			}

			byte[] sendBytes = Encode(message);
			connection.Send(sendBytes);
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
			try
			{
				IPEndPoint endPoint = (IPEndPoint)connection.RemoteEndPoint;
				IPAddress ipAddress = endPoint.Address;

				return ipAddress.ToString() + ":" + endPoint.Port;
			}
			catch (SocketException)
			{
				return "disconnected";
			}
			catch (ObjectDisposedException)
			{
				return "disposed";
			}
		}

		/// <summary>
		/// Stops a running server.
		/// </summary>
		public void StopServer()
		{
			if (!_serverStarted)
			{
				throw new InvalidOperationException("No server is currently running, skipping...");
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
		/// Encrypts a message.
		/// </summary>
		private string EncryptMessage(string source, string key)
		{
			EncryptedMessage encryptedMessage = new EncryptedMessage(source, null, null); // Not required for now, maybe in the future... the future... future... ture... it's 2am help.
			encryptedMessage.Encrypt(_encryptor, key);
			encryptedMessage.RecalculateHash();

			return encryptedMessage.Pack();
		}

		/// <summary>
		/// Decrypts a message.
		/// </summary>
		private string DecryptMessage(string source, string key)
		{
			EncryptedMessage encryptedMessage = EncryptedMessage.Unpack(source);

			if (encryptedMessage != null && encryptedMessage.CompareHash())
			{
				encryptedMessage.Decrypt(_encryptor, key);
				return encryptedMessage.Data;
			}
			else if (encryptedMessage != null) // Uh-oh, CompareHash has failed?
			{
				EnqueueCommand(() => Print(entityMachine, "Hash comparison failed on read, discarding package."));
				return null;
			}
			else // Cannot dechipher, maybe the server did not pack the message correctly, or at all?
			{
				return source;
			}
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

		/// <summary>
		/// Whether or not this shell is using encryption to communicate with servers.
		/// </summary>
		public bool UsingEncryption => Variables.Get<bool>("network.encrypted");

		/* ------------ */
		/* --- Data --- */
		/* ------------ */

		private List<Socket> _read;
		private List<Thread> _handlers;
		private Dictionary<Socket, ClientState> _clients;
		private Socket _listener;

		private Thread _selectorThread;
		private IServerProgram _serverProgram;
		private IEncryptor _encryptor = new CaesarCypher();

		private bool _serverStarted;
		private bool _serverIdle;

		// -- Network data --

		/// <summary>
		/// The agreed size of the buffer.
		/// </summary>
		private int BufferSize => Variables.Get<int>("network.buffer_size");
	}
}
