using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPServer
{
	/// <summary>
	/// Contains a collection of all valid SMTP commands.
	/// </summary>
	static class SMTPCommandLookup
	{
		/// <summary>
		/// The dictionary of all available SMTP commands.
		/// </summary>
		private static HashSet<string> static_AllowedCommands = new HashSet<string>()
		{
			"HELO",
			"MAIL",
			"RCPT",
			"DATA",
			"RSET",
			"SEND",
			"SOML",
			"SAML",
			"VRFY",
			"EXPN",
			"HELP",
			"NOOP",
			"QUIT",
			"TURN"
		};

		/// <summary>
		/// Checks whether a command exists within the RFC 821 and RFC 5321 standards.
		/// </summary>
		public static bool CommandExists(string command)
		{
			return static_AllowedCommands.Contains(command);
		}
	}
}
