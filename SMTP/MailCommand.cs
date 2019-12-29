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
		public string Help => Format.Name(Name, new Arg("from", "sender_address"), new Arg("to[1, 2, ...]", "receiver_address"), new Arg("data/block", "data (or none)"));

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

				for (int i = 1; !args.Either(out string receiver, "to" + i); ++i)
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

				string data = null;
				bool hasData = args.Either(out data, "data");

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

				try
				{
					clientShell.Send($"MAIL FROM:<{sender}>");
					
					foreach (string receiver in receivers)
					{
						clientShell.Send($"RCPT TO:<{receiver}>");
					}

					clientShell.Send("DATA");
					clientShell.Send(data);
					clientShell.Send("\r\n.\r\n");
				}
				catch (IOException e)
				{
					return Format.Error(clientShell, Name, e.Message);
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
