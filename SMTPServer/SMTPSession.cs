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
	class SMTPSession
	{
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
				return _stateMachine.Process(messageData);
			}
		}

		/// <summary>
		/// Initialises a new SMTP session.
		/// </summary>
		public SMTPSession()
		{
			_stateMachine = new SMTPStateMachine();
		}

		// The SMTP state machine.
		private readonly SMTPStateMachine _stateMachine;
	}
}
