using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{  
    /// <summary>
    /// Entry point class.
    /// </summary>
    class Program
    {
        /// <summary>
        /// Entry point method.
        /// </summary>
        /// <param name="args">Optional argument list.</param>
        static void Main(string[] args)
        {
			ClientShell shell = new ClientShell();
			shell.Create(1000, 600);
			shell.Run();
        }
    }
}
