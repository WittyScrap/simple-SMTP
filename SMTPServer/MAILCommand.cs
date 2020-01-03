using System.Net.Mail;

namespace SMTPServer
{
	/// <summary>
	/// Manages a MAIL command.
	/// </summary>
	class MAILCommand : ISMTPCommand
	{
		/// <summary>
		/// The name of the command (MAIL).
		/// </summary>
		public virtual string Name => "MAIL";

		/// <summary>
		/// The sender's address.
		/// </summary>
		public string Address { get; }

		/// <summary>
		/// Whether or not the command is properly formatted.
		/// </summary>
		public bool IsFormatted { get; }

		/// <summary>
		/// The response to the command.
		/// </summary>
		public string Response { get; }

		/// <summary>
		/// The component that needs to be verified.
		/// </summary>
		protected virtual string Formatter { get => "FROM:"; }

		/// <summary>
		/// Creates a new MAIL command instance.
		/// </summary>
		public MAILCommand(string source)
		{
			// M A I L <SP> F R O M : <SP> < <CHAR> @ <CHAR> . <CHAR> > <CRLF>
			if (source.Length < 18 || source.Substring(source.Length - 2) != "\r\n")
			{
				IsFormatted = false;
				Address = null;
				Response = SMTPCodes.Compose(SMTPCodes.ClientError.PSTX, "Incorrect syntax.");
			}
			else
			{
				source = source.Substring(0, source.Length - 2);

				string[] components = source.Split(' ');

				if (components.Length != 3 || components[1] != Formatter)
				{
					IsFormatted = false;
					Address = null;
					Response = SMTPCodes.Compose(SMTPCodes.ClientError.PSTX, "Incorrect syntax.");

					return;
				}

				string address = components[2];

				if (address[0] != '<' || address[address.Length - 1] != '>')
				{
					IsFormatted = false;
					Address = null;
					Response = SMTPCodes.Compose(SMTPCodes.ClientError.PSTX, "Incorrect email address formatting.");

					return;
				}

				address = address.Substring(1, address.Length - 2);

				// Verify email
				if (!EmailValid(address))
				{
					IsFormatted = false;
					Address = null;
					Response = SMTPCodes.Compose(SMTPCodes.ClientError.PSTX, "Invalid email address.");

					return;
				}

				IsFormatted = true;
				Address = address;
				Response = null; // No response needed here, will be processed later.
			}
		}

		/// <summary>
		/// Checks that an email is in a valid format.
		/// </summary>
		private static bool EmailValid(string email)
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
	}
}
