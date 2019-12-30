using Client;
using Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTP
{
	/// <summary>
	/// Asks the remote SMTP server to provide its HELP command (SMTP equivalent: HELP).
	/// </summary>
	class HelpCommand : ICommand
	{
		/// <summary>
		/// The help string to be displayed in the local shell's help screen.
		/// </summary>
		public string Help => Format.Name(Name, new Arg("command", "help_on_command_name", 'c', false)) + Format.Text("Sends a HELP command to a remote SMTP shell (SMTP equivalent: HELP).");

		/// <summary>
		/// The name of the command (shelp).
		/// </summary>
		public string Name => "shelp";

		/// <summary>
		/// Sends a HELP command to the connected SMTP shell.
		/// </summary>
		public bool Execute(IShell sourceShell, ParameterSet args = null)
		{
			if (sourceShell is ClientShell)
			{
				ClientShell clientShell = sourceShell as ClientShell;

				string command = null;
				args?.Either(out command, "command", "c");

				try
				{
					clientShell.Send($"HELP {command}\r\n");
				}
				catch (IOException e)
				{
					return Format.Error(clientShell, Name, e.Message);
				}

				return true;
			}
			else
			{
				return Format.Error(sourceShell, Name, "This shell type is not supported by this command, please use a ClientShell instead.");
			}
		}
	}
}
