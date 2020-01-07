using Client;
using Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPClient
{
	/// <summary>
	/// Closes an existing SMTP connection with a remote host (SMTP equivalent: QUIT).
	/// </summary>
	class QuitCommand : ICommand
	{
		/// <summary>
		/// The help screen text.
		/// </summary>
		public string Help => Format.Name(Name) + Format.Text("Closes an existing SMTP connection with a remote SMTP server (SMTP equivalent: QUIT).");

		/// <summary>
		/// The name of the command (quit).
		/// </summary>
		public string Name => "quit";

		/// <summary>
		/// Runs a QUIT command on the remote connected host.
		/// </summary>
		public bool Execute(IShell sourceShell, ParameterSet args)
		{
			if (sourceShell is ClientShell)
			{
				ClientShell clientShell = sourceShell as ClientShell;

				try
				{
					clientShell.Send("QUIT\r\n", true);
					SMTPResponse response = new SMTPResponse(clientShell.WaitForResponse());

					if (response.ResponseCode == SMTPResponse.Code.Success)
					{
						clientShell.Disconnect();
					}
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
