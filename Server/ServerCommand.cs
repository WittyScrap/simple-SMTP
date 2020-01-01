using Shell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	/// <summary>
	/// Command that allows to load any custom server program into a server shell.
	/// </summary>
	class ServerCommand : ICommand
	{
		/// <summary>
		/// Help screen contents.
		/// </summary>
		public string Help => Format.Name(Name, new Arg("load", "server_dll_name", 'l')) + Format.Text("Loads a server program from the given DLL pack.");

		/// <summary>
		/// The name of the command (server).
		/// </summary>
		public string Name => "server";

		/// <summary>
		/// Executes the command.
		/// </summary>
		public bool Execute(IShell sourceShell, ParameterSet args = null)
		{
			if (sourceShell is ServerShell)
			{
				ServerShell serverShell = sourceShell as ServerShell;

				if (args != null && args.Either(out object packName, "load", "l") && packName != null)
				{
					string packFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ((string)packName) + ".dll");

					if (File.Exists(packFile))
					{
						Assembly packAssembly = Assembly.LoadFile(packFile);
						string serverName = null;

						foreach (Type packType in packAssembly.GetTypes())
						{
							if (typeof(IServerProgram).IsAssignableFrom(packType))
							{
								IServerProgram serverProgram = (IServerProgram)Activator.CreateInstance(packType);
								serverShell.LoadServer(serverProgram);

								serverName = packType.Name;
								break;
							}
						}

						if (serverName == null)
						{
							return Format.Error(serverShell, Name, "No IServerProgram class found within given DLL.");
						}
						else
						{
							serverShell.Print(Name, $"Succesfully loaded server: {serverName}.");
						}

						return true;
					}
					else
					{
						return Format.Error(serverShell, Name, "Server pack file does not exist in the current directory.");
					}
				}
				else
				{
					return Format.Error(serverShell, Name, "Server name missing (--load/-l)!");
				}
			}
			else
			{
				return Format.Error(sourceShell, Name, "This shell is not supported by the program command.");
			}
		}
	}
}
