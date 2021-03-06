﻿using System;
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
		public string Help => @"\cf1\b " + Name + @" \b0\cf2\i <--data/-d\cf1\i0  | \cf2\i --block/-b\cf3\i0  message\cf2\i >\i0\b\cf1 :\b0\cf2\i  Sends a message to a remote connected host.\i0";

		/// <summary>
		/// The name of this command.
		/// </summary>
		public string Name => "send";

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

                if (!clientShell.IsConnected)
                {
                    return Format.Error(clientShell, Name, "Cannot send data through a disconnected shell, use the connect command before using the send command.");
                }

				if (args == null || args.Count != 1)
				{
					return Format.Error(clientShell, Name, "Invalid argument count.");
				}

                bool hasData    = args.Either(out object packedMessage, "data", "d");
                bool wantsBlock = args.Either(out object _, "block", "b");

				if (!hasData && !wantsBlock)
				{
					return Format.Error(clientShell, Name, "Missing data/block argument, please refer to the help screen for more information.");
				}

                string message;

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
                    Logger.Message(Logger.MSG, message);
                    sourceShell.Print("send", @"Sending message:\line " + message.Replace("\n", @"\line ") + " ");
                }

				try
				{
					clientShell.Send(message);
				}
				catch (Exception e)
				{
					return Format.Error(clientShell, Name, e.Message);
				}

				return true;
			}
			else
			{
				return Format.Error(sourceShell, Name, "Send is not supported on this shell.");
			}
		}
	}
}
