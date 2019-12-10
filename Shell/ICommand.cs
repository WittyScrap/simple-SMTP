using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shell
{
	/// <summary>
	/// Interface for a console command.
	/// </summary>
	public interface ICommand
	{
		/// <summary>
		/// Executes the command on the given shell with the given
		/// set of optional arguments.
		/// </summary>
		/// <param name="sourceShell">The shell to execute the command on.</param>
		/// <param name="args">The argument list (optional).</param>
		/// <returns>True on successful execution, false on error.</returns>
		bool Execute(IShell sourceShell, ParameterSet args = null);

		/// <summary>
		/// The help summary for this command.
		/// </summary>
		string Help { get; }

		/// <summary>
		/// The name of the command.
		/// </summary>
		string Name { get; }
	}
}
