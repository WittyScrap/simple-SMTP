using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shell;

namespace Server
{
	/// <summary>
	/// Starts a server on a server shell.
	/// </summary>
	class StartCommand : ICommand
	{
		/// <summary>
		/// Help screen string.
		/// </summary>
		public string Help => Format.Name(Name, new Arg("host", "host_name", 'h', false), new Arg("port", "port_number", 'p', false)) + Format.Text("Initialises a server on the given host and port.");

		/// <summary>
		/// The name of the command (start).
		/// </summary>
		public string Name => "start";

		/// <summary>
		/// Starts a server on the given host name and port.
		/// </summary>
		public bool Execute(IShell sourceShell, ParameterSet args = null)
		{
			if (sourceShell is ServerShell)
			{
				ServerShell shell = sourceShell as ServerShell;

				string hostName = null;
				string portNum = null;

				bool hasHost = args?.Either(out hostName, "h", "host") ?? false;
				bool hasPort = args?.Either(out portNum, "p", "port") ?? false;

				if (!hasHost || hostName == null)
				{
					hostName = shell.Variables.Get<string>("network.default_host");
				}

				if (!hasPort || portNum == null)
				{
					portNum = shell.Variables.Get<int>("network.default_port").ToString();
				}

				if (!int.TryParse(portNum, out int portNumber))
				{
					return Format.Error(sourceShell, Name, "Invalid port number: ensure only digits are used.");
				}

				if (portNumber < 0 || portNumber > 65535)
				{
					return Format.Error(sourceShell, Name, "Invalid port number: port number must be between 0 and 65535.");
				}

				try
				{
					shell.StartServer(hostName, portNumber);
				}
				catch (Exception e)
				{
					return Format.Error(sourceShell, Name, e.Message);
				}

				return true;
			}
			else
			{
				return Format.Error(sourceShell, Name, "The start command is not supported on this shell.");
			}
		}
	}
}
