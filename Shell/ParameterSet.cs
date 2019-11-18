using System.Collections.Generic;

namespace Shell
{
	/// <summary>
	/// Marker class (alias) for hash set of command arguments.
	/// </summary>
	public class ParameterSet : Dictionary<string, CommandArg>
	{
		/// <summary>
		/// Allows directly adding a command arg.
		/// </summary>
		/// <param name="arg"></param>
		public void Add(CommandArg arg)
		{
			this[arg.Key] = arg;
		}
	}
}
