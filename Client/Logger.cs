using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    /// <summary>
    /// Streamlines writing to the default console.
    /// </summary>
    static class Logger
    {
        /// <summary>
        /// Writes a message to the console.
        /// </summary>
        /// <param name="messageContents"></param>
        public static void Message(string type, string contents)
        {
            Console.WriteLine(type + ": " + contents);
        }


        /* ------------ */
        /* --- Data --- */
        /* ------------ */


        public const string MSG = "Log";
        public const string WRN = "Warning";
        public const string ERR = "Error";
    }
}
