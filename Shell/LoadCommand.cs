using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Shell;
using System.Reflection;

namespace Shell
{
	/// <summary>
	/// Loads a custom command set.
	/// </summary>
	public class LoadCommand<TDestinationSet> : ICommand where TDestinationSet : IExpandableCommandSet, new()
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
						int found = 0;
						
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
									return Format.Error(sourceShell, Name, "Command pack could not be integrated, perhaps it has already been added?");
								}

								found++;
							}
						}

						if (found == 0)
						{
							return Format.Error(sourceShell, Name, "No ICommand instances could be found in given DLL pack.");
						}
						else
						{
							asyncShell.Print(Name, $"Successfully loaded {found} commands.");
						}

						return true;
					}
					else
					{
						return Format.Error(sourceShell, Name, "Pack file does not exist in the current directory.");
					}
				}
				else
				{
					return Format.Error(sourceShell, Name, "Pack name argument missing (--pack/-p)!");
				}
			}
			else
			{
				return Format.Error(sourceShell, Name, "This shell is not supported by the load command.");
			}
		}
	}
}
