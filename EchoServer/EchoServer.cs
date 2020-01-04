using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Server;

namespace EchoServer
{
	/// <summary>
	/// Simple testing echo server.
	/// </summary>
	class EchoServer : IServerProgram
	{
		/// <summary>
		/// This will not be used here, as there are
		/// no instances in which an Echo server will wish to
		/// disconnect abruptly.
		/// </summary>
		public HashSet<Socket> Disconnected { get; } = new HashSet<Socket>();

		/// <summary>
		/// Send back input.
		/// </summary>
		public string HandleInput(Socket connection, string data)
		{
			return data;
		}

		/// <summary>
		/// Unimplemented.
		/// </summary>
		public string OnConnection(Socket connection)
		{
			return null;
		}
	}
}
