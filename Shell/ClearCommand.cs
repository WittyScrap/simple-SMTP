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
        public string Help => @"\cf1\b clear: \b0\i\cf2 Clears the console.\i0\b0\cf3";

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
