using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPServer
{
	/// <summary>
	/// Manages a HELP command.
	/// </summary>
	class HELPCommand : SMTPCommand
	{
		/// <summary>
		/// The name of the command (HELP).
		/// </summary>
		public override string Name => "HELP";

		/// <summary>
		/// The code to display upon successful parsing.
		/// </summary>
		protected override string SuccessCode => SMTPCodes.Status.HELP;

		/// <summary>
		/// The specific command the user is requesting help for.
		/// </summary>
		private string SpecificCommand { get; } = null;

		/// <summary>
		/// The message to be sent in a success case.
		/// </summary>
		protected override string SuccessMessage 
		{
			get
			{
				if (SpecificCommand == null)
				{
					return $"OK, command help reference: https://tools.ietf.org/html/rfc5321 .";
				}
				else
				{
					return $"OK, specific command help not available, please refer to: https://tools.ietf.org/html/rfc5321 .";
				}
			}
		}

		/// <summary>
		/// Interprets a HELP command.
		/// </summary>
		public HELPCommand(string source) : base(source)
		{
			source = source.Substring(0, source.Length - 2); // Remove <CRLF>

			if (source.Length > 5)
			{
				SpecificCommand = source.Substring(5);
			}
		}
	}
}
