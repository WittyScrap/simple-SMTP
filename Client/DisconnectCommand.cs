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
				return @"\cf1\b disconnect:\b0\cf2\i  Disconnects the shell from the remote host, if it is connected.\i0\cf3";
			}
		}

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
			sourceShell.Receive("disconnect", errorMessage);
			return false;
		}
	}
}
