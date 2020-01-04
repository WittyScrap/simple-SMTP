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
		/// Handles when a connection should immediately
		/// terminate as a result of an error, user action, or
		/// a server's decision.
		/// </summary>
		HashSet<Socket> Disconnected { get; }

		/// <summary>
		/// Handles a client's input.
		/// </summary>
		/// <returns>The server's response.</returns>
		string HandleInput(Socket connection, string data);

		/// <summary>
		/// Handles what needs to happen once a new connection has been accepted.
		/// </summary>
		/// <param name="connection">The newly accepted connection.</param>
		/// <returns>Any data that should be sent through the connection.</returns>
		string OnConnection(Socket connection);
	}
}
