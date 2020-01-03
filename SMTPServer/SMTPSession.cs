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
			return _manager.Process(messageData);
		}

		/// <summary>
		/// Initialises a new SMTP session.
		/// </summary>
		public SMTPSession()
		{
			_manager = new SMTPManager();
		}

		// The SMTP state machine.
		SMTPManager _manager;
	}
}
