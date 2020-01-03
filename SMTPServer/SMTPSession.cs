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
			if (components.Length != 3 || components[0] != "MAIL" || components[1] != "FROM:" || components[2].Length < 2)
			{
				return false;
			}

			// Ensure address is properly formatted with < and >
			if (components[2][0] != '<' || components[2][components[2].Length - 1] != '>')
			{
				return false;
			}

			// Step 3: extract email data from third token, remove <> and use as output data.
			command = Command.Mail;
			data = components[2].Substring(1).Substring(0, components[2].Length - 1);

			return true;
		}

		/// <summary>
		/// Parses a RCPT TO: <> command.
		/// </summary>
		private static bool ParseReceipt(string message, out Command command, out string data)
		{
			// Default values
			data = null;
			command = Command.Invalid;

			// Structure: RCPT TO: <SP> <mail@addr.com> <CRLF>
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

			// Ensure address is properly formatted with < and >
			if (components[2][0] != '<' || components[2][components[2].Length - 1] != '>')
			{
				return false;
			}

			// Step 3: extract email data from third token, remove <> and use as output data.
			command = Command.Receipt;
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
		/// Checks that a command exisss and is supported.
		/// </summary>
		private static bool CommandExists(string command)
		{
			// Using switch as it gets converted into a hash
			// set after compilation, which gives it an efficiency of O(1)
			// and prevents it from allocating unnecessary memory.
			switch (command)
			{
				case "HELO":
				case "MAIL":
				case "RCPT":
				case "DATA":
				case "RSET":
				case "NOOP":
				case "HELP":
				case "QUIT":
					return true;

				default:
					return false;
			}
		}

		/// <summary>
		/// Attempts to parse a message into a command and its data.
		/// </summary>
		private bool TryParse(string message, out Command command, out string data)
		{
			command = Command.Invalid;
			data = null;

			if (_state == SessionState.ReadingData)
			{
				data = message;
				return true;
			}

			string[] tokens = message.Split(' ');
			string header = tokens[0].Replace("\r\n", "");

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
			switch (command)
			{
				case Command.Noop:
					// No-operation (noop), this comment is useless I just wanted to leave this as two lines.
					return "250 Ok, done nothing, thanks for wasting a CPU cycle.";

				case Command.Reset:
					_state = _state > SessionState.Connected ? SessionState.Identified : _state;
					return "250 Ok!";

				case Command.Quit:
					_state = SessionState.Connected;
					return "250 Ok!";

				case Command.Help:
					// Another useless comment yay.
					return HelpRequest(data);
			}

			switch (_state)
			{
				case SessionState.Connected:
					return ConnectedState(command, data);

				case SessionState.Identified:
					return IdentifiedState(command, data);

				case SessionState.Recipients:
					return RecipientsState(command, data);

				case SessionState.ReadingData:
					return DataState(data);

				default:
					return $"471 Look this is not supposed to happen, you know how enums work? SessionState only has 4 possible state options and somehow you ended up in none of them. Yeah.";
			}
		}

		/// <summary>
		/// Help command response.
		/// </summary>
		private string HelpRequest(string data)
		{
			if (data != null)
			{
				return CommandExists(data) ? "250 Ok sure but also no." : $"550 Invalid command argument: {data} - no such command.";
			}
			else
			{
				return "250 Ok but what about no.";
			}
		}

		/// <summary>
		/// Handles the connected state only.
		/// </summary>
		/// <param name="command">The command to handle.</param>
		/// <param name="data">Any data associated with the command.</param>
		/// <returns></returns>
		private string ConnectedState(Command command, string data)
		{
			if (command == Command.Hello)
			{
				if (data == null)
				{
					return "550 Must provide domain.";
				}

				_domain = data;
				_state = SessionState.Identified;

				return $"250 Nice to meet you, {data}.";
			}
			else
			{
				return "503 Bad sequence of commands (must use HELO).";
			}
		}

		/// <summary>
		/// Manages any action that can be taken whilst in an identified state.
		/// </summary>
		private string IdentifiedState(Command command, string data)
		{
			if (command == Command.Mail)
			{
				if (_recipients == null)
				{
					_recipients = new HashSet<string>();
				}
				else
				{
					_recipients.Clear();
				}

				_sender = data;
				_state = SessionState.Recipients;

				return "250 Ok!";
			}
			else
			{
				return "503 Bad sequence of commands, use QUIT, MAIL.";
			}
		}

		/// <summary>
		/// Manages which actions can be taken in the Recipients state.
		/// </summary>
		private string RecipientsState(Command command, string data)
		{
			switch (command)
			{
				case Command.Receipt:
				{
					_recipients.Add(data);

					return "250 Ok!";
				}
				case Command.Data:
				{
					if (_recipients.Count > 0)
					{
						_mailData = "";
						_state = SessionState.ReadingData;

						return "354 Start mail input; end with <CRLF>.<CRLF>";
					}
					else
					{
						return "503 Bad sequence of commands, use RCPT, RSET, QUIT.";
					}
				}
				default:
					return "503 Bad sequence of commands, use RCPT, DATA, RSET, QUIT.";
			}
		}

		/// <summary>
		/// Manages reading in mail data and sending the email once a . has been registered.
		/// </summary>
		private string DataState(string data)
		{
			_mailData += data;

			if (_mailData.Substring(_mailData.Length - 5) == "\r\n.\r\n")
			{
				_state = SessionState.Identified;
				return "250 Ok!";
			}
			else
			{
				return null; // No response, waiting...
			}
		}

		/// <summary>
		/// Creates a new SMTP session.
		/// </summary>
		public SMTPSession()
		{
			_state = SessionState.Connected;
		}

		/* ---------- */
		/* -- Data -- */
		/* ---------- */

		private SessionState _state;
		private string _domain;

		// State-specific data.
		private string _sender;
		private HashSet<string> _recipients;

		private string _mailData;
	}
}
