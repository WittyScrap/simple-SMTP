using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPServer
{
	/// <summary>
	/// Manages a RSET command.
	/// </summary>
	class RSETCommand : SMPLCommand
	{
		/// <summary>
		/// The name of the command (RSET).
		/// </summary>
		public override string Name => "RSET";

		/// <summary>
		/// Creates a new RSET command.
		/// </summary>
		public RSETCommand(string source) : base(source)
		{ }
	}
}
