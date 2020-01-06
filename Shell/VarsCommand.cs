using System;
using VariableManagement;

namespace Shell
{
	/// <summary>
	/// Lists all environmental variables as loaded in memory.
	/// </summary>
	public class VarsCommand : ICommand
	{
		/// <summary>
		/// Displays the help section for this command.
		/// </summary>
		public string Help => @"\cf1\b " + Name + @" \b0\cf2\i [--set/-s\cf3\i0  var_name\cf2\i  <--value/-v\cf3\i0  var_value\cf2\i > [--show]]\i0\b\cf1 :\b0\cf2\i  Displays all environmental variables as currently loaded in memory.\i0";

		/// <summary>
		/// The name of this command.
		/// </summary>
		public string Name => "vars";

		/// <summary>
		/// Displays all environmental variables.
		/// </summary>
		public bool Execute(IShell sourceShell, ParameterSet args = null)
		{
			if (sourceShell.Variables == null)
			{
				return Format.Error(sourceShell, Name, "Variables could not be parsed, please check the environment.vars file for errors.");
			}

			string variableName = null;
			bool hasSet = args != null && args.Either(out variableName, "s", "set");

			if (!hasSet)
			{
				string formattedOutput = "";

				DisplayVariableBlock(sourceShell.Variables.Root, ref formattedOutput);
				sourceShell.Print("vars", @"\cf2\i Variables will be displayed below.\i0" + formattedOutput + @"\cf3");

				return true;
			}
			else
			{
				if (variableName == null)
				{
					return Format.Error(sourceShell, Name, "Incomplete vars command: set flag was found, but no variable name was provided.");
				}

				bool hasValue = args.Either(out string variableValue, "v", "value");
				bool shouldShow = args.Either<object>(out _, "show");

				if (!hasValue)
				{
					return Format.Error(sourceShell, Name, "Incomplete vars command: set flag was found, but no value was provided.");
				}

				VariablesParser.Parse(variableValue, out object parsedValue);

				try
				{
					sourceShell.Variables[variableName] = parsedValue;
				}
				catch (Exception e)
				{
					return Format.Error(sourceShell, Name, e.Message);
				}

				if (shouldShow)
				{
					string formattedOutput = "";

					DisplayVariableBlock(sourceShell.Variables.Root, ref formattedOutput);
					sourceShell.Print("vars", @"\cf2\i Variables will be displayed below.\i0" + formattedOutput + @"\cf3");
				}

				return true;
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
				formatOutput += @"\line" + tabs + @"\cf1\b " + varsField.Key + @":\b0\i\cf2  " + varsField.Value.ToString() + @"\i0";
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
				tabs += "    ";
			}

			return tabs;
		}
	}
}
