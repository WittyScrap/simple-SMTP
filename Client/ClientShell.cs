using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Net.Sockets;
using Shell;
using System.Net;
using VariableManagement;

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
			IPHostEntry ipHost = Dns.GetHostEntry(host);
			IPAddress ipAddress = ipHost.AddressList[0];
			IPEndPoint endPoint = new IPEndPoint(ipAddress, port);

			if (!TestConnection(host, port))
			{
				throw new Exception("Could not connect to the specified host on the specified port.");
			}

			_socketThread = new Thread(() =>
			{
				_connection = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
				_connection.Blocking = (bool)Variables["environment.blocking"];
				_connection.ReceiveTimeout = (int)Variables["environment.timeout"];
				_connection.SendTimeout = (int)Variables["environment.timeout"];
				_connection.Connect(endPoint);

				Print(entityMachine, "Successfully connected to " + Remote);

				while (IsConnected)
				{
					SendAllMessages();
					DisplayResponse();
				}
			});

			_socketThread.Start();
		}

		/// <summary>
		/// Closes the connection, if it already exists.
		/// </summary>
		public void Disconnect()
		{
			if (IsConnected)
			{
				Print(entityMachine, "Disconnected from " + Remote);

				_connection.Close();
				_connection = null;
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

			while (_connection.Receive(receivedBytes) > 0)
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
		/// Sends a message to the remote host.
		/// </summary>
		/// <param name="message">The message to send to the remote host.</param>
		public void Send(string message)
		{
			if (!IsConnected)
			{
				return;
			}

			_messageQueue.Enqueue(message);
		}

		/// <summary>
		/// Initialisation for the shell.
		/// </summary>
		protected override bool OnShellInit()
		{
			_messageQueue = new ConcurrentQueue<string>();
			try
			{
				_enviromentVars = Variables.Load("../../Environment.vars");
			}
			catch (Exception e)
			{
				Print(entityMachine, e.Message);
			}
			return base.OnShellInit();
		}

		/// <summary>
		/// Tests a connection on a given host and port.
		/// </summary>
		private bool TestConnection(string host, int port)
		{
			using (TcpClient temp = new TcpClient())
			{
				try
				{
					temp.Connect(host, port);
				}
				catch (Exception)
				{
					return false;
				}
			}

			return true;
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
			while (!_messageQueue.IsEmpty && IsConnected)
			{
				string nextMessage;
				while (!_messageQueue.TryDequeue(out nextMessage));
				_connection.Send(Encode(nextMessage));
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
			return Encoding.UTF8.GetString(sourceBytes);
		}

		/// <summary>
		/// Tests if the socket has fully connected.
		/// </summary>
		public bool IsConnected => _connection != null && _connection.Connected;

		/// <summary>
		/// Accessor for the environmental variables.
		/// </summary>
		public Variables Variables => _enviromentVars;

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

		/* ------------ */
		/* --- Data --- */
		/* ------------ */

		private Socket _connection;
		private Thread _socketThread;
		private Variables _enviromentVars;
		private ConcurrentQueue<string> _messageQueue;

		// -- Network data --

		/// <summary>
		/// The agreed size of the buffer.
		/// </summary>
		private int BufferSize => (int)Variables["environment.buffer_size"];

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
