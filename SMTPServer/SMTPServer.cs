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
		/// Handles responding to any incoming command.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="data"></param>
		/// <returns></returns>
		public string HandleInput(Socket connection, string data)
		{
			if (!_activeSessions.ContainsKey(connection))
			{
				_activeSessions[connection] = new SMTPSession();
			}

			return _activeSessions[connection].OnMessage(data);
		}
	}
}
