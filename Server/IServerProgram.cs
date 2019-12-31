using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	/// <summary>
	/// Handles running a server of a specific type.
	/// </summary>
	public interface IServerProgram
	{
		/// <summary>
		/// Handles a client's input.
		/// </summary>
		/// <returns>The server's response.</returns>
		string HandleInput(Socket connection, string data);
	}
}
