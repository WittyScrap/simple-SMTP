using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPServer
{
	/// <summary>
	/// Source command for any command that only contians a header.
	/// </summary>
	abstract class SMTPCommand : ISMTPCommand
	{
		/// <summary>
		/// The header of the command.
		/// </summary>
		public abstract string Name { get; }

		/// <summary>
		/// Ensures that the <CRLF> is present at the end of the command.
		/// </summary>
		public bool IsFormatted { get; }

		/// <summary>
		/// The response for this command.
		/// </summary>
		public string Response => IsFormatted ? SMTPCodes.Compose(SuccessCode, SuccessMessage) : SMTPCodes.Compose(SMTPCodes.ClientError.PSTX, "Incorrect syntax.");

		/// <summary>
		/// The code to be displayed on success.
		/// </summary>
		protected virtual string SuccessCode => SMTPCodes.Status.SVOK;

		/// <summary>
		/// The message to be displayed on success.
		/// </summary>
		protected virtual string SuccessMessage => "Ok.";

		/// <summary>
		/// Creates a new command.
		/// </summary>
		public SMTPCommand(string source)
		{
			IsFormatted = source.Length >= 6 && source.Substring(source.Length - 2) == "\r\n";
		}
	}
}
