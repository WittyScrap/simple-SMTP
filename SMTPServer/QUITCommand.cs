using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPServer
{
	/// <summary>
	/// Manages a QUIT command.
	/// </summary>
	class QUITCommand : SMTPCommand
	{
		/// <summary>
		/// The name of the command.
		/// </summary>
		public override string Name => "QUIT";

		/// <summary>
		/// Overrides success code for QUIT (221).
		/// </summary>
		protected override string SuccessCode => SMTPCodes.Status.CLSE;

		/// <summary>
		/// The message to display as a response to a QUIT message.
		/// </summary>
		protected override string SuccessMessage => "Service closing transmission channel.";

		/// <summary>
		/// Creates a new QUIT command.
		/// </summary>
		public QUITCommand(string source) : base(source)
		{ }
	}
}
