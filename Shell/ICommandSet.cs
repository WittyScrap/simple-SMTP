using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shell
{
	/// <summary>
	/// Represents a set of commands.
	/// </summary>
	public interface ICommandSet : IReadOnlyDictionary<string, ICommand>
	{
		/// <summary>
		/// Checks if the command is supported by this set.
		/// </summary>
		/// <param name="commandHeader">The command to check for.</param>
		/// <returns>True if the command exists, false otherwise.</returns>
		bool CommandExists(string commandHeader);

		/// <summary>
		/// Enumerates across the single commands.
		/// </summary>
		/// <returns></returns>
		new IEnumerator<ICommand> GetEnumerator();
	}
}
