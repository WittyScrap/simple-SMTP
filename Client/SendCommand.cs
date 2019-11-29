using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shell;

namespace Client
{
	/// <summary>
	/// Command used to send data to a connected socket.
	/// </summary>
	class SendCommand : ICommand
	{
		/// <summary>
		/// Displayed in the help page of the shell.
		/// </summary>
		public string Help => @"\cf1\b send \b0\cf2\i <--data/-d\cf3\i0 | \cf2\i --block/-b\cf3\i0 message\cf2\i >\i0\b\cf1 :\b0\cf2\i  Sends a message to a remote connected host.\i0";

		/// <summary>
		/// Sends a message to the remote connected host.
		/// </summary>
		/// <param name="sourceShell">The shell to use as sending agent.</param>
		/// <param name="args">The arguments to the command.</param>
		/// <returns>True on successful completion, false on error.</returns>
		public bool Execute(IShell sourceShell, ParameterSet args = null)
		{
			if (sourceShell is ClientShell)
			{
				ClientShell clientShell = sourceShell as ClientShell;

				if (args == null || args.Count != 1)
				{
					return Error(clientShell, "Invalid argument count.");
				}

                bool hasData    = args.Either(out object packedMessage, "data", "d");
                bool wantsBlock = args.Either(out object _, "block", "b");

				if (!hasData && !wantsBlock)
				{
					return Error(clientShell, "Missing data/block argument, please refer to the help screen for more information.");
				}

                string message = "";

                if (hasData)
                {
                    message = (string)packedMessage;
                }
                else
                {
                    Multiline form = new Multiline();
                    form.ShowDialog();

                    // Form has finished...
                    message = form.Value;
                }

				try
				{
					clientShell.Send(message);
				}
				catch (Exception e)
				{
					return Error(clientShell, e.Message);
				}

				return true;
			}
			else
			{
				return Error(sourceShell, "Send is not supported on this shell.");
			}
		}

		/// <summary>
		/// Displays an error message and returns the default error
		/// value of false.
		/// </summary>
		/// <param name="shell">The shell to display the message on.</param>
		/// <param name="message">The message to display.</param>
		/// <returns>A constant value of false.</returns>
		private bool Error(IShell shell, string message)
		{
			shell.Print("send", @"\b\cf1Error:\b0\cf2\i  " + message);
			return false;
		}
	}
}
