using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shell;

namespace Client
{
	/// <summary>
	/// Command to disconnect the <see cref="ClientShell"/> from its
	/// remote host, if it is connected to one.
	/// </summary>
	class DisconnectCommand : ICommand
	{
		/// <summary>
		/// Help section.
		/// </summary>
		public string Help {
			get
			{
				return Format.Name(Name) + Format.Text("Disconnects this shell from the remote host, if a connection exists.");
			}
		}

		/// <summary>
		/// The name of this command.
		/// </summary>
		public string Name => "disconnect";

		/// <summary>
		/// Safely disconnect the shell from the remote host.
		/// </summary>
		public bool Execute(IShell sourceShell, ParameterSet args = null)
		{
			if (sourceShell is ClientShell)
			{
				ClientShell shell = sourceShell as ClientShell;

				if (!shell.IsConnected)
				{
					return Error(sourceShell, "Shell not connected, type \"connect\" before trying to disconnect.");
				}

				shell.Disconnect();
				return true;
			}
			else
			{
				return Error(sourceShell, "The disconnect command is not supported on this shell.");
			}
		}

		/// <summary>
		/// Compresses two method calls into one.
		/// </summary>
		private bool Error(IShell sourceShell, string errorMessage)
		{
			sourceShell.Print("disconnect", errorMessage);
			return false;
		}
	}
}
