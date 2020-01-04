using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPServer
{
	/// <summary>
	/// Manages a NOOP command.
	/// </summary>
	class NOOPCommand : SMTPCommand
	{
		/// <summary>
		/// The name of the command (NOOP).
		/// </summary>
		public override string Name => "NOOP";

		/// <summary>
		/// Creates a new NOOP command.
		/// </summary>
		public NOOPCommand(string source) : base(source)
		{ }
	}
}
