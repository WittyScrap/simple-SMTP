using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPServer
{
	/// <summary>
	/// Manages a RCPT command.
	/// </summary>
	class RCPTCommand : MAILCommand
	{
		/// <summary>
		/// The name of the command (RCPT).
		/// </summary>
		public override string Name => "RCPT";

		/// <summary>
		/// The section that needs verification for the command.
		/// </summary>
		protected override string Formatter { get => "TO:"; }

		/// <summary>
		/// Initialises a new RCPT command.
		/// </summary>
		/// <param name="source"></param>
		public RCPTCommand(string source) : base(source)
		{ }
	}
}
