using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPServer
{
	/// <summary>
	/// Contains parsing information for SMTP commands.
	/// </summary>
	static class SMTPParser
	{
		/// <summary>
		/// Parses a command.
		/// </summary>
		public static bool Parse(string command, out ISMTPCommand parsedCommand)
		{
			parsedCommand = null;

			// C M N D <SP> //...
			if (command.Length < 4)
			{
				return false;
			}

			switch(command.Substring(0, 4))
			{
				case "HELO":
					parsedCommand = new HELOCommand(command);
					break;

				case "MAIL":
					parsedCommand = new MAILCommand(command);
					break;

				case "RCPT":
					parsedCommand = new RCPTCommand(command);
					break;

				case "DATA":
					parsedCommand = new DATACommand(command);
					break;

				case "RSET":
					parsedCommand = new RSETCommand(command);
					break;

				case "NOOP":
					parsedCommand = new NOOPCommand(command);
					break;

				case "HELP":
					parsedCommand = new HELPCommand(command);
					break;

				case "VRFY":
					parsedCommand = new VRFYCommand(command);
					break;

				case "LOGI":
					parsedCommand = new LOGICommand(command);
					break;

				case "QUIT":
					parsedCommand = new QUITCommand(command);
					break;
			}

			return parsedCommand != null;
		}
	}
}
