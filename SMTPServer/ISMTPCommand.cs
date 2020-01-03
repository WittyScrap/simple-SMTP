using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPServer
{
	/// <summary>
	/// Represents a generic SMTP command.
	/// </summary>
	interface ISMTPCommand
	{
		/// <summary>
		/// The name of the command.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Whether or not the command is correctly formatted.
		/// </summary>
		bool IsFormatted { get; }

		/// <summary>
		/// The full response to this command.
		/// </summary>
		string Response { get; }
	}
}
