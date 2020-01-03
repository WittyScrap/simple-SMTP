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
	class QUITCommand : SMPLCommand
	{
		/// <summary>
		/// The name of the command.
		/// </summary>
		public override string Name => "QUIT";

		/// <summary>
		/// Creates a new QUIT command.
		/// </summary>
		public QUITCommand(string source) : base(source)
		{ }
	}
}
