using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shell;

namespace Server
{
	/// <summary>
	/// Stops a running server.
	/// </summary>
	class StopCommand : ICommand
	{
		/// <summary>
		/// Provides a help screen for the stop command.
		/// </summary>
		public string Help => Format.Name(Name) + Format.Text("Stops a running server.");

		/// <summary>
		/// The name of the command (stop).
		/// </summary>
		public string Name => "stop";

		/// <summary>
		/// Stops a running server and kills all connections to it.
		/// </summary>
		public bool Execute(IShell sourceShell, ParameterSet args = null)
		{
			if (sourceShell is ServerShell)
			{
				ServerShell shell = sourceShell as ServerShell;

				try
				{
					shell.StopServer();
				}
				catch (Exception e)
				{
					return Format.Error(sourceShell, Name, e.Message);
				}

				return true;
			}
			else
			{
				return Format.Error(sourceShell, Name, "The stop command is not supported on this shell.");
			}
		}
	}
}
