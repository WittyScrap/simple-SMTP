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
		/// The current state of the SMTP session.
		/// </summary>
		enum SessionState
		{
			Connected,
			Identified,
			Recipients,
			ReadingData
		}

		/// <summary>
		/// The command to be executed next.
		/// </summary>
		enum Command
		{
			Invalid,
			Hello,
			Mail,
			Receipt,
			Data,
			Reset,
			Noop,
			Help,
			Quit
		}

		/// <summary>
		/// Parses a HELO command.
		/// </summary>
		private static bool ParseHelo(string message, out Command command, out string data)
		{
			// Default values
			data = null;
			command = Command.Invalid;


			// Check for final <CRLF>.
			if (message.Substring(message.Length - 2) != "\r\n")
			{
				return false;
			}

			message = message.Substring(0, message.Length - 2);

			// Parse out message and return.
			command = Command.Hello;
			data = message.Substring(5);

			return true;
		}

		/// <summary>
		/// Parses a MAIL FROM: <> command.
		/// </summary>
		private static bool ParseMail(string message, out Command command, out string data)
		{
			// Default values
			data = null;
			command = Command.Invalid;

			// Structure: MAIL FROM: <SP> <mail@addr.com> <CRLF>
			// Step 1: Get rid of final CRLF, if it is correctly present:
			if (message.Substring(message.Length - 2) != "\r\n")
			{
				return false;
			}

			message = message.Substring(0, message.Length - 2);

			// Step 2: split message across spaces, check that first two tokens are correctly "MAIL" and "FROM:"...
			string[] components = message.Split(' ');
			if (components.Length != 3 || components[0] != "MAIL" || components[1] != "FROM:")
			{
				return false;
			}

			// Step 3: extract email data from third token, remove <> and use as output data.
			command = Command.Mail;
			data = components[2].Substring(1).Substring(0, components[2].Length - 1);

			return true;
		}

		/// <summary>
		/// Parses a RECPT TO: <> command.
		/// </summary>
		private static bool ParseReceipt(string message, out Command command, out string data)
		{
			// Default values
			data = null;
			command = Command.Invalid;

			// Structure: RECPT TO: <SP> <mail@addr.com> <CRLF>
			// Step 1: Get rid of final CRLF, if it is correctly present:
			if (message.Substring(message.Length - 2) != "\r\n")
			{
				return false;
			}

			message = message.Substring(0, message.Length - 2);

			// Step 2: split message across spaces, check that first two tokens are correctly "RCPT" and "TO:"...
			string[] components = message.Split(' ');
			if (components.Length != 3 || components[0] != "RCPT" || components[1] != "TO:")
			{
				return false;
			}

			// Step 3: extract email data from third token, remove <> and use as output data.
			command = Command.Mail;
			data = components[2].Substring(1).Substring(0, components[2].Length - 1);

			return true;
		}

		/// <summary>
		/// Parses a HELP [COMMAND]
		/// </summary>
		private static bool ParseHelp(string message, out Command command, out string data)
		{
			// Default values
			data = null;
			command = Command.Invalid;

			// Structure: HELP [COMMAND] <CRLF>
			// Step 1: Get rid of final CRLF, if it is correctly present:
			if (message.Substring(message.Length - 2) != "\r\n")
			{
				return false;
			}

			message = message.Substring(0, message.Length - 2);
			command = Command.Help;

			// Check for optional argument.
			if (message.Length > 5 /* Includes the space -> H E L P <SP> [COMMAND]*/)
			{
				data = message.Substring(5);
			}

			return true;
		}

		/// <summary>
		/// Attempts to parse a message into a command and its data.
		/// </summary>
		private static bool TryParse(string message, out Command command, out string data)
		{
			command = Command.Invalid;
			data = null;

			string[] tokens = message.Split(' ');
			string header = tokens[0];

			bool isValid;

			switch (header)
			{
				case "HELO":
					isValid = ParseHelo(message, out command, out data);
					break;

				case "MAIL":
					isValid = ParseMail(message, out command, out data);
					break;

				case "RCPT":
					isValid = ParseReceipt(message, out command, out data);
					break;

				case "DATA":
					command = Command.Data;
					isValid = true;
					break;

				case "RSET":
					command = Command.Reset;
					isValid = true;
					break;

				case "NOOP":
					command = Command.Noop;
					isValid = true;
					break;

				case "HELP":
					isValid = ParseHelp(message, out command, out data);
					break;

				case "QUIT":
					command = Command.Quit;
					isValid = true;
					break;

				default:
					isValid = false;
					break;
			}

			return isValid;
		}

		/// <summary>
		/// Handles an incoming message.
		/// </summary>
		/// <param name="messageData">The incoming message data.</param>
		public string OnMessage(string messageData)
		{
			if (TryParse(messageData, out Command command, out string data))
			{
				return ManageCommand(command, data);
			}
			else
			{
				return $"500 Unrecognised command: {messageData}";
			}
		}

		/// <summary>
		/// Manages which action to take with this command.
		/// </summary>
		private string ManageCommand(Command command, string data)
		{
			// TODO: create state machine...
			return "$500 Service unavailable, need to work on it.";
		}

		/* ---------- */
		/* -- Data -- */
		/* ---------- */

		private SessionState _state;
	}
}
