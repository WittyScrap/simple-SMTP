using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shell
{
    /// <summary>
    /// Clears the console.
    /// </summary>
    public class ClearCommand : ICommand
    {
        /// <summary>
        /// The messsage displayed in the help screen.
        /// </summary>
        public string Help => Format.Name("clear") + Format.Text("Clears the console.");

        /// <summary>
        /// Clear the console as per instruction.
        /// </summary>
        public bool Execute(IShell sourceShell, ParameterSet args = null)
        {
            sourceShell.Clear();
            return true;
        }
    }
}
