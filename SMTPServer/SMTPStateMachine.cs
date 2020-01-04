﻿using System;
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
					// Help command.
					return command.Response;

				case "QUIT":
					if (command.IsFormatted)
					{
						Quit();
					}
					return command.Response;

				default:
					// Nothing stateless, proceed as normal.
					return null;
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
					State++;
				}

				return helo.Response;
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
					_sender = mail.Address;
					State++;

					// No need to verify mailboxes for now, send mail...
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
						_recipients.Add(rcpt.Address);

						// No need to verify mailboxes for now, send mail...
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
				Reset();
				return SMTPCodes.Compose(SMTPCodes.Status.SVOK, "Mail composition OK, sent.");
			}

			return null;
		}

		/// <summary>
		/// Resets the state of the server to <see cref="SessionState.Identified"/>.
		/// </summary>
		private void Reset()
		{

			_mailData = "";
			_sender = "";
			_recipients.Clear();

			State = SessionState.Connected;
		}

		/// <summary>
		/// Resets the state of the server to <see cref="SessionState.Connected"/>.
		/// </summary>
		private void Quit()
		{
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
		public SMTPStateMachine()
		{
			_recipients = new HashSet<string>();
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

		private string _mailData;
	}
}
