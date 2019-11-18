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
				_connection.Connect(endPoint);

				Receive(entityMachine, "Successfully connected to " + Remote);

				// TODO: Write connection loop...
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
				Receive(entityMachine, "Disconnected from " + Remote);

				_connection.Close();
				_connection = null;
			}
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
		/// Tests if the socket has fully connected.
		/// </summary>
		public bool IsConnected => _connection != null && _connection.Connected;

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
