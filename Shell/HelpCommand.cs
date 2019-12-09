using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shell
{
	/// <summary>
	/// Displays the help screen on the console.
	/// </summary>
	public class HelpCommand : ICommand
	{
		/// <summary>
		/// Help's screen on the help command. I mean...
		/// </summary>
		public string Help {
			get
			{
				return Format.Name("help") + Format.Text("Displays this help screen.");
			}
		}

		/// <summary>
		/// Asks the console to display the help screen.
		/// </summary>
		public bool Execute(IShell sourceShell, ParameterSet args = null)
		{
			sourceShell.DisplayHelp();
			return true;
		}
	}
}
