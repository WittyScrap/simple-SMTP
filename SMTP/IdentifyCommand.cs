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
	/// A simple identifier command (RFC821 equivalent: HELO).
	/// </summary>
	class IdentifyCommand : ICommand
	{
		/// <summary>
		/// The help string to display.
		/// </summary>
		public string Help => Format.Name(Name, new Arg("domain", "domain_name", 'd')) + Format.Text("Identifies your domain with the remote host and opens an SMTP state (SMTP equivalent: HELO).");

		/// <summary>
		/// The name of the command (identify).
		/// </summary>
		public string Name => "identify";

		/// <summary>
		/// Sends a HELO command through a connected service, displays an error if the shell is not connected.
		/// </summary>
		public bool Execute(IShell sourceShell, ParameterSet args = null)
		{
			if (sourceShell is ClientShell)
			{
				ClientShell clientShell = sourceShell as ClientShell;

				string domain = null;
				bool hasDomain = args?.Either(out domain, "domain", "d") ?? false;

				if (!hasDomain)
				{
					return Format.Error(sourceShell, Name, "Domain argument not found, please refer to help screen.");
				}

				try
				{
					clientShell.Send($"HELO {domain}\r\n");
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
