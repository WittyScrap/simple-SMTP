using Shell;
using Client;
using System.IO;

namespace SMTP
{
	/// <summary>
	/// Sends a command to reset the remote SMTP server's state (SMTP equivalent: RSET).
	/// </summary>
	class ResetCommand : ICommand
	{
		/// <summary>
		/// Help text to be displayed in help screen.
		/// </summary>
		public string Help => Format.Name(Name) + Format.Text("Sends a command to reset the remote SMTP server's state (SMTP equivalent: RSET).");

		/// <summary>
		/// The name of the command (reset).
		/// </summary>
		public string Name => "reset";

		/// <summary>
		/// Sends a NOOP command to the connected SMTP server.
		/// </summary>
		public bool Execute(IShell sourceShell, ParameterSet args)
		{
			if (sourceShell is ClientShell)
			{
				ClientShell clientShell = sourceShell as ClientShell;

				try
				{
					clientShell.Send("RSET\r\n");
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
