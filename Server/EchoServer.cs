using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	/// <summary>
	/// Simple testing echo server.
	/// </summary>
	class EchoServer : IServerProgram
	{
		/// <summary>
		/// Send back input.
		/// </summary>
		public string HandleInput(Socket connection, string data)
		{
			return data;
		}
	}
}
