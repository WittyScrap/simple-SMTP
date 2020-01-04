using Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SMTPServer
{
	/// <summary>
	/// The core SMTP server program.
	/// </summary>
	class SMTPServer : IServerProgram
	{
		// All active sessions.
		private Dictionary<Socket, SMTPSession> _activeSessions;

		/// <summary>
		/// Creates a new SMTP server.
		/// </summary>
		public SMTPServer()
		{
			_activeSessions = new Dictionary<Socket, SMTPSession>();
		}

		/// <summary>
		/// Handles all sockets that have received a QUIT command.
		/// </summary>
		public HashSet<Socket> Disconnected { get; } = new HashSet<Socket>();

		/// <summary>
		/// Handles responding to any incoming command.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public string HandleInput(Socket connection, string data)
		{
			string message = _activeSessions[connection].OnMessage(data);

			if (_activeSessions[connection].ShouldQuit)
			{
				Disconnected.Add(connection);
				_activeSessions[connection].Dispose();
				_activeSessions.Remove(connection);
			}

			return message;
		}

		/// <summary>
		/// Newly accepted connection.
		/// </summary>
		/// <param name="connection">The connection that has just been created.</param>
		/// <returns>The SMTP welcome message.</returns>
		public string OnConnection(Socket connection)
		{
			SMTPSession acceptedSession = new SMTPSession();
			_activeSessions[connection] = acceptedSession;

			return acceptedSession.OnWelcome();
		}
	}
}
