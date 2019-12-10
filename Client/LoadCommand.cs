using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Shell;
using System.Reflection;

namespace Client
{
	/// <summary>
	/// Loads a custom command set.
	/// </summary>
	class LoadCommand<TDestinationSet> : ICommand where TDestinationSet : IExpandableCommandSet, new()
	{
		/// <summary>
		/// Displays the help screen.
		/// </summary>
		public string Help => Format.Name(Name, new Arg("pack", "pack_name", 'p')) + Format.Text("Loads a command pack from a DLL file with the given pack name.");

		/// <summary>
		/// The name of this command.
		/// </summary>
		public string Name => "load";

		/// <summary>
		/// Executes the load command.
		/// </summary>
		public bool Execute(IShell sourceShell, ParameterSet args = null)
		{
			if (sourceShell is AsyncShell<TDestinationSet>)
			{
				AsyncShell<TDestinationSet> asyncShell = sourceShell as AsyncShell<TDestinationSet>;
				TDestinationSet commandSet = asyncShell.Commands;

				if (args != null && args.Either(out object packName, "pack", "p") && packName != null)
				{
					string packFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ((string)packName) + ".dll");

					if (File.Exists(packFile))
					{
						Assembly packAssembly = Assembly.LoadFile(packFile);
						
						foreach (Type packType in packAssembly.GetTypes())
						{
							if (typeof(ICommand).IsAssignableFrom(packType))
							{
								ICommand instancedCommand = (ICommand)Activator.CreateInstance(packType);

								try
								{
									commandSet.Expand(instancedCommand.Name, instancedCommand);
								}
								catch (ArgumentException)
								{
									return Error(asyncShell, "Command pack could not be integrated, perhaps it has already been added?");
								}
							}
						}

						return true;
					}
					else
					{
						return Error(asyncShell, "Pack file does not exist in the current directory.");
					}
				}
				else
				{
					return Error(asyncShell, "Pack name argument missing (--pack/-p)!");
				}
			}
			else
			{
				return Error(sourceShell, "This shell is not supported by the load command.");
			}
		}

		/// <summary>
		/// Displays an error message and returns the default error
		/// value of false.
		/// </summary>
		/// <param name="shell">The shell to display the message on.</param>
		/// <param name="message">The message to display.</param>
		/// <returns>A constant value of false.</returns>
		private bool Error(IShell shell, string message)
		{
			shell.Print("load", @"\b\cf1Error:\b0\cf2\i  " + message);
			return false;
		}
	}
}
