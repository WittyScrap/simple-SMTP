using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    /// <summary>
    /// Shell framework system.
    /// </summary>
    interface IShell
    {
        /// <summary>
        /// Runs the console.
        /// </summary>
        void Run();

        /// <summary>
        /// Interrupts the console.
        /// </summary>
        void Stop();

        /// <summary>
        /// Parses a command sent by the console's input system.
        /// </summary>
        /// <param name="command">The raw command to be sent and parsed.</param>
        void SendCommand(string command);
    }
}
