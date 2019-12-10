using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shell
{
	/// <summary>
	/// A command set that can be expanded with a new list of commands.
	/// </summary>
	public interface IExpandableCommandSet : ICommandSet
	{
		/// <summary>
		/// Merges this command set with the provided command.
		/// </summary>
		void Expand(string commandName, ICommand command);
	}
}
