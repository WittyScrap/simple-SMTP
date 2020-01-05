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
		/// Handles whether this session should be disconnected.
		/// </summary>
		public bool ShouldQuit => _stateMachine.State < 0;

		/// <summary>
		/// Handles an incoming message.
		/// </summary>
		/// <param name="messageData">The incoming message data.</param>
		public string OnMessage(string messageData)
		{
			if (_stateMachine.State != SMTPStateMachine.SessionState.ReadingData && _stateMachine.CheckStateless(messageData, out string statelessResponse))
			{
				return statelessResponse;
			}
			else
			{
				if (_stateMachine.State > SMTPStateMachine.SessionState.Connected && !_authenticator.IsAuthenticated)
				{
					return _authenticator.TryLogin(messageData) ?? SMTPCodes.Compose(SMTPCodes.Status.SVOK, $"Login OK; Welcome back, {_authenticator.Username}!");
				}
				else if (_stateMachine.State < SMTPStateMachine.SessionState.Identified && _authenticator.IsAuthenticated)
				{
					_authenticator.Logout();
				}

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
			_authenticator = new Authenticator(data);
		}

		// The SMTP state machine.
		private readonly SMTPStateMachine _stateMachine;
		private readonly Authenticator _authenticator;
		private SMTPData _data;
	}
}
