using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPServer
{
	/// <summary>
	/// Handles a DATA command.
	/// </summary>
	class DATACommand : SMPLCommand
	{
		/// <summary>
		/// The name of the command (DATA).
		/// </summary>
		public override string Name => "DATA";

		/// <summary>
		/// The success code (354).
		/// </summary>
		protected override string SuccessCode => SMTPCodes.Redirection.MAIL;

		/// <summary>
		/// The success message.
		/// </summary>
		protected override string SuccessMessage => "Start mail input; end with <CRLF>.<CRLF>";

		/// <summary>
		/// Constructs a new DATA command.
		/// </summary>
		public DATACommand(string source) : base(source)
		{ }
	}
}
