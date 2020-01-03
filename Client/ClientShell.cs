using Shell;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace Client
{
	/// <summary>
	/// An asynchronous shell system to better handle
	/// over-the-network connections. More specialised
	/// versions of this can be made for specific uses.
	/// </summary>
	public class ClientShell : AsyncShell<ClientCommandSet>
	{
		/// <summary>
		/// Connects this shell to the specified host and port number.
		/// </summary>
		/// <param name="host">The host to connect to.</param>
		/// <param name="port">The port number to connect to.</param>
		public void Connect(string host, int port)
		{
			if (!IPAddress.TryParse(host, out IPAddress ipAddress))
			{
				IPHostEntry ipHost = Dns.GetHostEntry(host);
				ipAddress = ipHost.AddressList[0];
			}

			IPEndPoint endPoint = new IPEndPoint(ipAddress, port);

			try
			{
				_connection = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				_connection.ReceiveTimeout = (int)Variables["network.timeout"];
				_connection.SendTimeout = (int)Variables["network.timeout"];
				_connection.Connect(endPoint);
			}
			catch (SocketException e)
			{
				Print(entityMachine, @"\b\cf1Error:\b0\i\cf2  " + e.Message + @"\i0\cf3");
				_connection = null;
				return;
			}

			Print(entityMachine, "Successfully connected to " + Remote);

			_senderThread = new Thread(() =>
			{
				while (IsConnected)
				{
					SendAllMessages();
				}
			});

			_listenThread = new Thread(() =>
			{
				while (IsConnected)
				{
					if (Listen)
					{
						DisplayResponse();
					}
				}
			});

			IsConnected = true;

			_senderThread.Start();
			_listenThread.Start();
		}

		/// <summary>
		/// Closes the connection, if it already exists.
		/// </summary>
		public void Disconnect()
		{
			if (IsConnected)
			{
				string remote = Remote;

				_connection.Shutdown(SocketShutdown.Both);
				_connection.Close();
				_connection = null;

				Print(entityMachine, "Disconnected from " + remote);

				IsConnected = false;
			}
		}

		/// <summary>
		/// Attempts a single retrieval from the connected socket,
		/// regardless of whether or not any more potential messages might be on the way.
		/// </summary>
		/// <returns>The amount of bytes received.</returns>
		public int ReceiveOnce(byte[] buffer)
		{
			try
			{
				if (!IsConnected || _connection.Available == 0)
				{
					return 0;
				}
			}
			catch (ObjectDisposedException)
			{
				Disconnect();

				return 0;
			}

			for (int i = 0; i < buffer.Length; ++i)
			{
				buffer[i] = 0;
			}

			_connection.ReceiveTimeout = (int)Variables["network.timeout"];

			try
			{
				return _connection.Receive(buffer);
			}
			catch (SocketException e)
			{
				Print(entityMachine, @"\b\cf1Error:\b0\i\cf2  " + e.Message + @", disconnecting...\i0\cf3");
				Disconnect();

				return 0;
			}
		}

		/// <summary>
		/// Attempts to retrieve data from the connection socket.
		/// </summary>
		/// <returns>True if any data was received, false otherwise.</returns>
		public bool TryReceive(out string message)
		{
			message = null;

			if (!IsConnected)
			{
				return false;
			}

			byte[] receivedBytes = new byte[BufferSize];
			bool any = false;

            while (IsConnected && ReceiveOnce(receivedBytes) > 0)
            {
                message += Decode(receivedBytes);
                any = true;
            }

			return any;
		}

		/// <summary>
		/// Receives data from the connected socket.
		/// </summary>
		/// <returns>The received data. If the socket fails to receive anything, this will be null.</returns>
		public string Receive()
		{
			if (TryReceive(out string receivedData))
			{
				return receivedData;
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Blocks the current thread until a response has been given by the remote host.
		/// </summary>
		/// <returns>The received data. If the socket disconnects before any data has been sent, this will be an empty string.</returns>
		public string WaitForResponse()
		{
			byte[] receivedBytes = new byte[BufferSize];
			string message = "";

			while (IsConnected && (ReceiveOnce(receivedBytes) > 0 || message == ""))
			{
				message += Decode(receivedBytes);
			}

			return message;
		}

		/// <summary>
		/// Sends a message to the remote host.
		/// </summary>
		/// <param name="message">The message to send to the remote host.</param>
		public void Send(string message, bool ignoreCarriageRule = false)
		{
			if (!ignoreCarriageRule && Variables.Get<bool>("network.add_carriage_return"))
			{
				message += "\r\n";
			}

			if (!IsConnected)
			{
				throw new IOException("Shell not connected, could not read from or write to remote host.");
			}

			_messageQueue.Enqueue(message);
		}

		/// <summary>
		/// Initialisation for the shell.
		/// </summary>
		protected override bool OnShellInit()
		{
			_messageQueue = new ConcurrentQueue<string>();
			return base.OnShellInit();
		}

		/// <summary>
		/// Disconnect the shell and terminate all threads immediately.
		/// </summary>
		protected override void OnShellDestruction()
		{
			Disconnect();
			base.OnShellDestruction();
		}

		/// <summary>
		/// Receives data from the connected socket and
		/// displays it to the shell.
		/// </summary>
		private void DisplayResponse()
		{
			if (TryReceive(out string receivedData))
			{
				Print(entityHost, receivedData);
			}
		}

		/// <summary>
		/// Sends all messages stored in the message queue to the remote host.
		/// </summary>
		private void SendAllMessages()
		{
			while (!_messageQueue.IsEmpty)
			{
				string nextMessage;
				while (!_messageQueue.TryDequeue(out nextMessage));

				if (IsConnected)
				{
					_connection.SendTimeout = (int)Variables["network.timeout"];

					try
					{
						_connection.Send(Encode(nextMessage));
					}
					catch (SocketException e)
					{
						Print(entityMachine, @"\b\cf1Error:\b0\i\cf2  " + e.Message + @", disconnecting...\i0\cf3");
						Disconnect();

						return;
					}
				}
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
		/// Sends a command directly to the connected host, if a connected host exists (shorthand for SEND command).
		/// </summary>
		protected override void OnUnmanagedCommand(string command)
		{
			try
			{
				Send(command);
			}
			catch (IOException)
			{
				EnqueueCommand(() => SendCommandSilent(command));
			}
		}

		/// <summary>
		/// Tests if the socket has fully connected.
		/// </summary>
		public bool IsConnected { get; private set; }

		/// <summary>
		/// Information on the remote host this shell is
		/// connected to.
		/// </summary>
		public string Remote {
			get
			{
				IPEndPoint endPoint = (IPEndPoint)_connection.RemoteEndPoint;
				IPAddress ipAddress = endPoint.Address;

				return ipAddress.ToString() + ":" + endPoint.Port;
			}
		}

		/// <summary>
		/// Whether or not the shell should be constantly listening for responses.
		/// </summary>
		public bool Listen { get; set; } = true;

		/* ------------ */
		/* --- Data --- */
		/* ------------ */

		private Socket _connection;
		private Thread _senderThread;
		private Thread _listenThread;
		private ConcurrentQueue<string> _messageQueue;

		// -- Network data --

		/// <summary>
		/// The agreed size of the buffer.
		/// </summary>
		private int BufferSize => (int)Variables["network.buffer_size"];

		// -- Entity management override --

		/// <summary>
		/// Identifies the working machine.
		/// </summary>
		protected override string entityHost {
			get
			{
				if (_connection == null || !_connection.Connected)
				{
					return base.entityHost;
				}
				else
				{
					return Remote;
				}
			}
		}
	}
}
