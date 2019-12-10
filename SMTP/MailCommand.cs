using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shell;
using Client;

namespace SMTP
{
    /// <summary>
    /// Combines MAIL FROM, RECPT, and DATA into one command.
    /// </summary>
    public class MailCommand : ICommand
    {
        /// <summary>
        /// The help message to be displayed.
        /// </summary>
        public string Help => Format.Name(Name, new Arg("from", "sender_address"), new Arg("to", "recipient_address")) + Format.Text("Sends an email to a given recipient from a given sender.");

        /// <summary>
        /// The name of the command.
        /// </summary>
        public string Name => "mail";

        /// <summary>
        /// Sends an email to a connected network shell.
        /// </summary>
        public bool Execute(IShell sourceShell, ParameterSet args = null)
        {
            if (sourceShell is ClientShell)
            {
                return true;
            }
            else
            {
                return Error(sourceShell, "The mail command is not supported on this shell.");
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
            shell.Print(Name, @"\b\cf1Error:\b0\cf2\i  " + message);
            return false;
        }
    }
}
