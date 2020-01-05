using Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using VariableManagement;

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
		/// The root for all permanent SMTP data.
		/// </summary>
		public SMTPData Source { get; private set; }

		/// <summary>
		/// Creates a new SMTP server.
		/// </summary>
		public SMTPServer()
		{
			_activeSessions = new Dictionary<Socket, SMTPSession>();
			Configure("SMTP.vars");
		}

		/// <summary>
		/// Configures this server's permanent data management.
		/// </summary>
		/// <param name="configurationFile">The configuration file.</param>
		public void Configure(string configurationFile)
		{
			Variables config;

			try
			{
				config = Variables.Load(configurationFile);
			}
			catch (Exception)
			{
				return;
			}

			Source = new SMTPData
			(
				config.Get<string>("SMTP.rootFolder"),
				config.Get<string>("SMTP.databaseName")
			);
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
			if (Source == null)
			{
				Disconnected.Add(connection);
				return SMTPCodes.Compose(SMTPCodes.ServerError.SVER, "Invalid server configuration, closing connection...");
			}

			SMTPSession acceptedSession = new SMTPSession(Source);
			_activeSessions[connection] = acceptedSession;

			return acceptedSession.OnWelcome();
		}
	}
}
