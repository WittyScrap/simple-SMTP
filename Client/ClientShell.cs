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

		/* ------------ */
		/* --- Data --- */
		/* ------------ */

		private Socket _connection;

		// -- Entity management override --

		/// <summary>
		/// Identifies the working machine.
		/// </summary>
		protected override string entityMachine {
			get
			{
				if (_connection == null || !_connection.Connected)
				{
					return base.entityMachine;
				}
				else
				{
					IPEndPoint endPoint = (IPEndPoint)_connection.RemoteEndPoint;
					IPAddress ipAddress = endPoint.Address;
					int ipPort = endPoint.Port;

					IPHostEntry hostEntry = Dns.GetHostEntry(ipAddress);
					string hostName = hostEntry.HostName;

					return hostName + ":" + ipPort;
				}
			}
		}
	}
}
