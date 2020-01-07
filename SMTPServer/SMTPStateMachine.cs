using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPServer
{
	/// <summary>
	/// Handles the state of an <see cref="SMTPSession"/>.
	/// </summary>
	class SMTPStateMachine : IDisposable
	{
		/// <summary>
		/// The server's domain.
		/// </summary>
		public const string Domain = "fakegmail.co.uk";

		/// <summary>
		/// The current session's username.
		/// </summary>
		public string Username { get; set; }

		/// <summary>
		/// The currently active user.
		/// </summary>
		public string ActiveUser => Username ?? _domain ?? "Anonymous";

		/// <summary>
		/// The current state of the SMTP session.
		/// </summary>
		public enum SessionState
		{
			Unavailable = -1,
			Connected,
			Identified,
			Recipients,
			ReadingData
		}

		/// <summary>
		/// Attempts to run a stateless command.
		/// </summary>
		/// <param name="response">The response to the command.</param>
		/// <returns>True if the command has been detected to be stateless, false otherwise.</returns>
		public bool CheckStateless(string command, out string response)
		{
			response = null;

			if (State < 0) // Invalid state.
			{
				return false;
			}

			if (SMTPParser.Parse(command, out ISMTPCommand parsedCommand))
			{
				response = OnStateAny(parsedCommand);
			}

			return response != null;
		}

		/// <summary>
		/// Handles checking whether a command is unsupported or invalid.
		/// </summary>
		/// <param name="command">The command to verify.</param>
		/// <returns>The appropriate response code and message.</returns>
		private string UnrecognisedCommand(string command)
		{
			if (command.Length < 6 || !SMTPCommandLookup.CommandExists(command.Substring(0, 4)))
			{
				return SMTPCodes.Compose(SMTPCodes.ClientError.SNTX, "Invalid or unrecognised command.");
			}
			else
			{
				return SMTPCodes.Compose(SMTPCodes.ClientError.NIMP, "Command not implemented.");
			}
		}

		/// <summary>
		/// Processes an ISMTP command.
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		public string Process(string command)
		{
			if (!SMTPParser.Parse(command, out ISMTPCommand parsedCommand) && State != SessionState.ReadingData)
			{
				return UnrecognisedCommand(command);
			}

			switch (State)
			{
				case SessionState.Connected:
					return OnStateConnected(parsedCommand);

				case SessionState.Identified:
					return OnStateIdentified(parsedCommand);

				case SessionState.Recipients:
					return OnStateRecipients(parsedCommand);

				case SessionState.ReadingData:
					return OnStateCollecting(command);

				default:
					return BadState;
			}
		}

		/// <summary>
		/// Runs through the command options that can be executed regardless of state.
		/// </summary>
		/// <param name="command"></param>
		/// <returns></returns>
		private string OnStateAny(ISMTPCommand command)
		{
			switch (command.Name)
			{
				case "RSET":
					if (command.IsFormatted)
					{
						Reset();
					}
					return command.Response;

				case "NOOP":
					// No-operation.
					return command.Response;

				case "HELP":
					_data.LogAction(ActiveUser, "Requested a HELP command.");
					return command.Response;

				case "QUIT":
					if (command.IsFormatted)
					{
						Quit();
					}
					return command.Response;

				case "VRFY":
					// Handle VRFY command appropriately.
					return ManageVerify((VRFYCommand)command);

				default:
					// Nothing stateless, proceed as normal.
					return null;
			}
		}

		/// <summary>
		/// Handles which message to return from a VRFY command.
		/// </summary>
		private string ManageVerify(VRFYCommand command)
		{
			if (!command.IsComplete || !command.IsFormatted)
			{
				return command.Response;
			}
			else
			{
				_data.LogAction(ActiveUser, $"Tried verifying account {command.Username}.");
				User checkingUser = _data.Verify(command.Username);

				if (checkingUser != null)
				{
					return SMTPCodes.Compose(SMTPCodes.Status.SVOK, $"OK, {checkingUser.Name} <{checkingUser.Email.Address}> verified.");
				}
				else
				{
					return SMTPCodes.Compose(SMTPCodes.ClientError.USNL, $"{command.Username} could not be verified, user not local.");
				}

			}
		}

		/// <summary>
		/// Runs through the connected state options.
		/// </summary>
		private string OnStateConnected(ISMTPCommand command)
		{
			if (command.Name == "HELO")
			{
				HELOCommand helo = (HELOCommand)command;

				if (helo.IsFormatted && helo.Domain != null)
				{
					_domain = helo.Domain;
					_data.LogAction(ActiveUser, "Identified itself through a HELO command and started a session.");
					State++;

					return SMTPCodes.Compose(SMTPCodes.Status.SVOK, $"Welcome to {Domain}, {_domain}.");
				}
				else
				{
					return helo.Response;
				}
			}
			else
			{
				return BadOrder;
			}
		}

		/// <summary>
		/// Runs through the identified state options.
		/// </summary>
		private string OnStateIdentified(ISMTPCommand command)
		{
			if (command.Name == "MAIL")
			{
				MAILCommand mail = (MAILCommand)command;

				if (mail.IsFormatted)
				{
					if (_data.VerifyMail(mail.Address) == null)
					{
						return SMTPCodes.Compose(SMTPCodes.ClientError.USNL, $"User not local.");
					}

					_sender = mail.Address;
					State++;

					return SMTPCodes.Compose(SMTPCodes.Status.SVOK, $"OK, sending from {_sender}.");
				}
				else
				{
					return mail.Response;
				}
			}
			else
			{
				return BadOrder;
			}
		}

		/// <summary>
		/// Runs through the recipients waiting state options.
		/// </summary>
		private string OnStateRecipients(ISMTPCommand command)
		{
			switch (command.Name)
			{
				case "RCPT":
				{
					RCPTCommand rcpt = (RCPTCommand)command;

					if (rcpt.IsFormatted)
					{
						if (_data.VerifyMail(rcpt.Address) == null)
						{
							return SMTPCodes.Compose(SMTPCodes.ClientError.USNL, $"User not local.");
						}

						_recipients.Add(rcpt.Address);

						return SMTPCodes.Compose(SMTPCodes.Status.SVOK, $"OK, sending to {rcpt.Address} ({_recipients.Count} total).");
					}
					else
					{
						return rcpt.Response;
					}
				}
				
				case "DATA":
				{
					if (_recipients.Count == 0)
					{
						return BadOrder;
					}

					DATACommand data = (DATACommand)command;

					if (data.IsFormatted)
					{
						State++;
					}

					return data.Response;
				}

				default:
					// Incorrect command scope.
					return BadOrder;
			}
		}

		/// <summary>
		/// Collects mail data and awaits mail termination by a single .
		/// </summary>
		private string OnStateCollecting(string messageData)
		{
			_mailData += messageData;

			if (_mailData.Length >= 5 && _mailData.Substring(_mailData.Length - 5) == "\r\n.\r\n")
			{
				SendMail();
				Reset();

				return SMTPCodes.Compose(SMTPCodes.Status.SVOK, "Mail composition OK, sent.");
			}

			return null;
		}

		/// <summary>
		/// Sends a pending mail.
		/// </summary>
		private void SendMail()
		{
			_mailData = _mailData.Substring(0, _mailData.Length - 5);

			foreach (string receiver in _recipients)
			{
				_data.SaveMail(new Mail(_sender, receiver, _mailData));
			}

			_data.LogAction(ActiveUser, $"Sent an email from address {_sender} to {_recipients.Count} recipient address{(_recipients.Count != 1 ? "es" : "")}.");
		}

		/// <summary>
		/// Resets the state of the server to <see cref="SessionState.Identified"/>.
		/// </summary>
		private void Reset()
		{
			_mailData = "";
			_sender = "";
			_recipients.Clear();
			_data.LogAction(ActiveUser, "Reset its session.");

			State = SessionState.Connected;
		}

		/// <summary>
		/// Resets the state of the server to <see cref="SessionState.Connected"/>.
		/// </summary>
		private void Quit()
		{
			_data.LogAction(ActiveUser, "Quit a running session.");
			State = SessionState.Unavailable;
		}

		/// <summary>
		/// Clears the current session's state machine.
		/// </summary>
		public void Dispose()
		{
			_recipients.Clear();

			_domain = null;
			_recipients = null;
			_sender = null;
			_mailData = null;
		}

		/// <summary>
		/// Creates a new SMTP state machine.
		/// </summary>
		public SMTPStateMachine(SMTPData data)
		{
			_recipients = new HashSet<string>();
			_data = data;
		}

		/// <summary>
		/// Message to be returned in the event of the server finding itself in an invalid state.
		/// </summary>
		private static string BadState => SMTPCodes.Compose(SMTPCodes.ServerError.SVER, "Invalid server state.");

		/// <summary>
		/// Message to be returned when a correct command is issued in the wrong occasion.
		/// </summary>
		private static string BadOrder => SMTPCodes.Compose(SMTPCodes.ClientError.ORDR, "Bad sequence of commands.");

		/// <summary>
		/// The current state of the SMTP session.
		/// </summary>
		public SessionState State { get; private set; }

		/* ---------- */
		/* -- Data -- */
		/* ---------- */

		private string _domain;

		// State-specific data.
		private string _sender;
		private HashSet<string> _recipients;
		private SMTPData _data;

		private string _mailData;
	}
}
