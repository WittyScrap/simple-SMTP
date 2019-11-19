using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shell;
using VariableManagement;

namespace Client
{
	/// <summary>
	/// Lists all environmental variables as loaded in memory.
	/// </summary>
	class VarsCommand : ICommand
	{
		/// <summary>
		/// Displays the help section for this command.
		/// </summary>
		public string Help => @"\cf1\b vars:\b0\cf2\i  Displays all environmental variables as currently loaded in memory.\i0";

		/// <summary>
		/// Displays all environmental variables.
		/// </summary>
		public bool Execute(IShell sourceShell, ParameterSet args = null)
		{
			if (sourceShell is ClientShell)
			{
				string formattedOutput = "";
				ClientShell clientShell = sourceShell as ClientShell;
				DisplayVariableBlock(clientShell.Variables.Root, ref formattedOutput);
				clientShell.Receive("vars", @"\cf2\i Variables will be displayed below.\i0" + formattedOutput);
				return true;
			}
			else
			{
				sourceShell.Receive("vars", "The vars command is not supported on this shell.");
				return false;
			}
		}

		/// <summary>
		/// Displays a variable block from a VarsObject instance.
		/// </summary>
		/// <param name="root">The block of variables to display.</param>
		/// <param name="tabsCount">How many tabs to add to each line.</param>
		private void DisplayVariableBlock(VariablesObject root, ref string formatOutput, int tabsCount = 0)
		{
			string tabs = GetTabs(tabsCount);

			// First all of the values...
			foreach (var varsField in root.GetAllFields())
			{
				formatOutput += @"\line" + tabs + @"+ \cf1\b " + varsField.Key + @":\b0\i\cf2  " + varsField.Value.ToString() + @"\i0";
			}

			// Then, recursively, all of the sub objects as well...
			foreach (var varsObject in root.GetAllObjects())
			{
				formatOutput += @"\line" + tabs + @"\cf1\b " + varsObject.Key + @":\b0";
				DisplayVariableBlock(varsObject.Value, ref formatOutput, tabsCount + 1);
			}
		}

		/// <summary>
		/// Returns a string containing the desired amount of tabs.
		/// </summary>
		/// <param name="tabsCount">The amount of tab chars to use in the string.</param>
		/// <returns>A string entirely composed of the desired amount of tab chars.</returns>
		private string GetTabs(int tabsCount)
		{
			string tabs = "";

			while (tabsCount-- > 0)
			{
				tabs += @"\tab";
			}

			return tabs;
		}
	}
}
