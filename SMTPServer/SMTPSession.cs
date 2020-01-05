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
	/// Represents a single SMTP session running on a single client.
	/// </summary>
	class SMTPSession : IDisposable
	{
		/// <summary>
		/// The username that this session is being identified as.
		/// </summary>
		public string Username { get; private set; } = null;

		/// <summary>
		/// Handles whether this session should be disconnected.
		/// </summary>
		public bool ShouldQuit => _stateMachine.State < 0;

		/// <summary>
		/// Attempts to perform a log in.
		/// </summary>
		/// <param name="messageData">The data sent from the user.</param>
		/// <returns>True if the login was successful, false otherwise.</returns>
		private bool TryLogin(string messageData, out string message)
		{
			message = null;

			// L O G I <SP> <USER> <SP> <PW>
			if (messageData.Length < 8 || messageData.Substring(messageData.Length - 2) != "\r\n")
			{
				message = SMTPCodes.Compose(SMTPCodes.ClientError.SNTX, "Invalid command syntax.");
				return false;
			}

			string[] components = messageData.Substring(0, messageData.Length - 2).Split(' ');

			if (components.Length != 3)
			{
				message = SMTPCodes.Compose(SMTPCodes.ClientError.SNTX, "Invalid command syntax.");
				return false;
			}

			if (components[0] != "LOGI")
			{
				message = SMTPCodes.Compose(SMTPCodes.ClientError.ORDR, "Bad sequence of commands; you must login before continuing.");
				return false;
			}

			string username = components[1];
			string password = components[2];

			// Check that username exists
			User user = _data.Verify(username);

			if (user != null)
			{
				password = SMTPData.HashPassword(password, user.Salt);

				if (password == user.Password)
				{
					Username = username;
					return true;
				}
				else
				{
					message = SMTPCodes.Compose(SMTPCodes.ClientError.NOMB, "Login failed; invalid password.");
					return false;
				}
			}
			else
			{
				message = SMTPCodes.Compose(SMTPCodes.ClientError.NOMB, "Login failed; invalid username.");
				return false;
			}
		}

		/// <summary>
		/// Handles an incoming message.
		/// </summary>
		/// <param name="messageData">The incoming message data.</param>
		public string OnMessage(string messageData)
		{
			if (_stateMachine.State < SMTPStateMachine.SessionState.Identified && Username != null)
			{
				Username = null;
			}

			if (_stateMachine.State > SMTPStateMachine.SessionState.Connected && Username == null)
			{
				if (TryLogin(messageData, out string message))
				{
					return SMTPCodes.Compose(SMTPCodes.Status.SVOK, $"Login OK; welcome, {Username}!");
				}
				else
				{
					return message;
				}
			}
			else if (messageData.Length >= 4 && messageData.Substring(0, 4) == "LOGI")
			{
				return SMTPCodes.Compose(SMTPCodes.ClientError.ORDR, $"Bad sequence of commands.");
			}

			if (_stateMachine.State != SMTPStateMachine.SessionState.ReadingData && _stateMachine.CheckStateless(messageData, out string statelessResponse))
			{
				return statelessResponse;
			}
			else
			{
				return _stateMachine.Process(messageData);
			}
		}

		/// <summary>
		/// Which message should be sent back when a session has been initialised.
		/// </summary>
		public string OnWelcome()
		{
			return SMTPCodes.Compose(SMTPCodes.Status.REDY, "Simple Mail Transfer Service Ready.");
		}

		/// <summary>
		/// Clears any memory allocated for this session.
		/// </summary>
		public void Dispose()
		{
			_stateMachine.Dispose();
		}

		/// <summary>
		/// Initialises a new SMTP session.
		/// </summary>
		public SMTPSession(SMTPData data)
		{
			_data = data;
			_stateMachine = new SMTPStateMachine(data);
		}

		// The SMTP state machine.
		private readonly SMTPStateMachine _stateMachine;
		private SMTPData _data;
	}
}
