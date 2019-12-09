using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Shell;

namespace Client
{
	/// <summary>
	/// Command used to connect a <see cref="ClientShell"/> to a remote host.
	/// </summary>
	class ConnectCommand : ICommand
	{
		/// <summary>
		/// Usage instructions.
		/// </summary>
		public string Help {
			get
			{
				return Format.Name("connect", new Arg("host", "host_name", 'h'), new Arg("port", "port_number", 'p')) + Format.Text("Connects the shell to the remote host on the defined port.");
			}
		}

		/// <summary>
		/// Executes this command and connects the shell appropriately.
		/// </summary>
		public bool Execute(IShell sourceShell, ParameterSet args = null)
		{
			if (sourceShell is ClientShell)
			{
				ClientShell shell = sourceShell as ClientShell;

				if (shell.IsConnected)
				{
					return Error(sourceShell, "Already connected, type \"disconnect\" before connecting again.");
				}

				if (args == null || args.Count != 2)
				{
					return Error(sourceShell, "Invalid argument count, type \"help\" for more information.");
				}

				bool hasHost = args.Either(out object hostName, "h", "host");
				bool hasPort = args.Either(out object portNum, "p", "port");

				if (!hasHost || hostName == null)
				{
					return Error(sourceShell, "Invalid arguments: no hostname found (-h/--host).");
				}

				if (!hasPort || portNum == null)
				{
					return Error(sourceShell, "Invalid argumens: no port numebr found (-p/--port).");
				}

				string parsedHost = (string)hostName;
				string parsedPort = (string)portNum;

				if (!int.TryParse(parsedPort, out int portNumber))
				{
					return Error(sourceShell, "Invalid port number: ensure only digits are used.");
				}

				if (portNumber < 0 || portNumber > 65535)
				{
					return Error(sourceShell, "Invalid port number: port number must be between 0 and 65535.");
				}

				try
				{
					shell.Connect(parsedHost, portNumber);
				}
				catch (Exception e)
				{
					return Error(sourceShell, e.Message);
				}

				return true;
			}
			else
			{
				return Error(sourceShell, "The connect command is not supported on this shell.");
			}
		}

		/// <summary>
		/// Compresses two method calls into one.
		/// </summary>
		private bool Error(IShell sourceShell, string errorMessage)
		{
			sourceShell.Print("connect", errorMessage);
			return false;
		}
	}
}
