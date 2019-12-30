using System.Net.Mail;
using System.IO;
using Shell;
using Client;
using System.Collections.Generic;

namespace SMTP
{
	/// <summary>
	/// Manages mail sending and receival (SMTP equivalent: MAIL/RCPT).
	/// </summary>
	class MailCommand : ICommand
	{
		/// <summary>
		/// Displays related help message.
		/// </summary>
		public string Help => Format.Name(Name, new Arg("from", "sender_address"), new Arg("to[1, 2, ...]", "receiver_address"), new Arg("data/block", "data (or none)")) + Format.Text("Sends an email to all receiver addresses (SMTP equivalent: MAIL/RCPT/DATA).");

		/// <summary>
		/// The name of the command (mail).
		/// </summary>
		public string Name => "mail";

		/// <summary>
		/// Validates the email address.
		/// </summary>
		private bool ValidEmail(string email)
		{
			try
			{
				MailAddress mail = new MailAddress(email);
				return mail.Address == email;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Runs the MAIL command followed by the RCPT command.
		/// </summary>
		public bool Execute(IShell sourceShell, ParameterSet args = null)
		{
			if (sourceShell is ClientShell)
			{
				ClientShell clientShell = sourceShell as ClientShell;

				if (args == null)
				{
					return Format.Error(clientShell, Name, "No arguments detected, please refer to the help screen.");
				}

				bool hasSender = args.Either(out string sender, "from");
				bool hasReceiver = args.Either(out string mainReceiver, "to");

				if (!hasSender)
				{
					return Format.Error(clientShell, Name, "No sender has been found, please refer to the help screen.");
				}

				if (!hasReceiver)
				{
					return Format.Error(clientShell, Name, "No receivers have been found, please refer to the help screen.");
				}

				if (!ValidEmail(sender))
				{
					return Format.Error(clientShell, Name, "Invalid sender email address.");
				}

				List<string> receivers = new List<string>(new string[] { mainReceiver });

				for (int i = 1; args.Either(out string receiver, "to" + i); ++i)
				{
					receivers.Add(receiver);
				}

				foreach (string receiver in receivers)
				{
					if (!ValidEmail(receiver))
					{
						return Format.Error(clientShell, Name, $"Invalid receiver email address for address: {receiver}");
					}
				}

				bool hasData = args.Either(out string data, "data");

				if (!hasData)
				{
					if (!args.Either<object>(out _, "block"))
					{
						return Format.Error(clientShell, Name, "No data/block argument detected, please refer to the help screen.");
					}
					else
					{
						// Get contents
						Multiline form = new Multiline();
						form.ShowDialog();

						// Form has finished...
						data = form.Value;
					}
				}

				clientShell.Listen = false; // Manual response management.

				try
				{
					clientShell.Send($"MAIL FROM:<{sender}>\r\n");

					// Expect 250 OK
					if (!clientShell.TryReceive(out string message) || new SMTPResponse(message).ResponseCode != SMTPResponse.Code.Success)
					{
						throw new SMTPException($"Invalid response: {message}" ?? "No response received on MAIL command.");
					}
					
					foreach (string receiver in receivers)
					{
						clientShell.Send($"RCPT TO:<{receiver}>\r\n");

						// Expect 250 OK
						if (!clientShell.TryReceive(out message) || new SMTPResponse(message).ResponseCode != SMTPResponse.Code.Success)
						{
							throw new SMTPException($"Invalid response: {message}" ?? $"No response received on RCPT command for address: {receiver}.");
						}
					}

					clientShell.Send("DATA\r\n");

					// Expect 354 End data with <CR><LF>.<CR><LF>
					if (!clientShell.TryReceive(out message) || new SMTPResponse(message).ResponseCode != SMTPResponse.Code.Redirect)
					{
						throw new SMTPException($"Invalid response: {message}" ?? "No response received on DATA command.");
					}

					string[] segments = data.Split('\n', '\r');

					foreach (string dataSegment in segments)
					{
						clientShell.Send(dataSegment + "\r\n");
					}

					clientShell.Send("\r\n.\r\n");

					// Expect 250 OK
					if (!clientShell.TryReceive(out message) || new SMTPResponse(message).ResponseCode != SMTPResponse.Code.Success)
					{
						throw new SMTPException($"Invalid response: {message}" ?? "No response received on . (end of DATA) command.");
					}
				}
				catch (IOException e)
				{
					return Format.Error(clientShell, Name, e.Message);
				}
				catch (SMTPException e)
				{
					return Format.Error(clientShell, Name, e.Message);
				}
				finally
				{
					clientShell.Listen = true;
				}

				return true;
			}
			else
			{
				return Format.Error(sourceShell, Name, "This shell type is not supported by this command, please use a ClientShell instead.");
			}
		}
	}
}
