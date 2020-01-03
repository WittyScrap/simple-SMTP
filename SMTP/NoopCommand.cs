using Shell;
using Client;
using System.IO;

namespace SMTPClient
{
	/// <summary>
	/// Performs no operation but expects a response (SMTP equivalent: NOOP).
	/// </summary>
	class NoopCommand : ICommand
	{
		/// <summary>
		/// Help text to be displayed in help screen.
		/// </summary>
		public string Help => Format.Name(Name) + Format.Text("Performs no operation, expects a response from the SMTP server (SMTP equivalent: NOOP).");

		/// <summary>
		/// The name of the command (nop).
		/// </summary>
		public string Name => "nop";

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
					clientShell.Send("NOOP\r\n", true);
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
