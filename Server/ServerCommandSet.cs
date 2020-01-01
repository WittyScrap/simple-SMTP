using Shell;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
	/// <summary>
	/// The command set to be used by the server.
	/// </summary>
	class ServerCommandSet : IExpandableCommandSet
	{
		/// <summary>
		/// Saves all related commands.
		/// </summary>
		public ServerCommandSet()
		{
			_commands = new Dictionary<string, ICommand>
			{
				["help"] = new HelpCommand(),
				["clear"] = new ClearCommand(),
				["vars"] = new VarsCommand(),
				["set"] = new ServerCommand(),
				["start"] = new StartCommand(),
				["stop"] = new StopCommand(),
				["load"] = new LoadCommand<ServerCommandSet>()
			};
		}

		/// <summary>
		/// Retrieves the command.
		/// </summary>
		public ICommand this[string key] {
			get
			{
				return _commands[key];
			}
		}

		/// <summary>
		/// Retrieves a list of all the source commands.
		/// </summary>
		public IEnumerable<string> Keys {
			get
			{
				return _commands.Keys;
			}
		}

		/// <summary>
		/// Retrieves a list of all the commands.
		/// </summary>
		public IEnumerable<ICommand> Values {
			get
			{
				return _commands.Values;
			}
		}

		/// <summary>
		/// The amount of commands in this set.
		/// </summary>
		public int Count => _commands.Count;

		/// <summary>
		/// Checks that a command is supported by this command set.
		/// </summary>
		/// <param name="commandHeader">The command's header.</param>
		/// <returns>True if the command is supported by this set, false otherwise.</returns>
		public bool CommandExists(string commandHeader)
		{
			return _commands.ContainsKey(commandHeader);
		}

		/// <summary>
		/// Alias to CommandExists.
		/// </summary>
		/// <param name="key">The command to look for.</param>
		/// <returns>True if the command exists, false otherwise.</returns>
		public bool ContainsKey(string key)
		{
			return CommandExists(key);
		}

		/// <summary>
		/// Retrieves all pairs of commands and names.
		/// </summary>
		public IEnumerator<ICommand> GetEnumerator()
		{
			foreach (KeyValuePair<string, ICommand> command in (IReadOnlyDictionary<string, ICommand>)this)
			{
				yield return command.Value;
			}
		}

		/// <summary>
		/// Attempts to extract a command, sets to null if the command
		/// does not exist.
		/// </summary>
		/// <param name="key">The command to search.</param>
		/// <param name="value">The command object, if found.</param>
		/// <returns>True if the command exists, false otherwise.</returns>
		public bool TryGetValue(string key, out ICommand value)
		{
			return _commands.TryGetValue(key, out value);
		}

		/// <summary>
		/// Non-generic version of <see cref="GetEnumerator"/>.
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Cycles through all command/command-name pairs.
		/// </summary>
		IEnumerator<KeyValuePair<string, ICommand>> IEnumerable<KeyValuePair<string, ICommand>>.GetEnumerator()
		{
			return _commands.GetEnumerator();
		}

		/// <summary>
		/// Expands this command set.
		/// </summary>
		/// <param name="commandName">The name of the command to be added.</param>
		/// <param name="command">The command to be added.</param>
		public void Expand(string commandName, ICommand command)
		{
			if (_commands.ContainsKey(commandName))
			{
				throw new ArgumentException("Command already has a name for: " + commandName);
			}

			_commands[commandName] = command;
		}


		/* ------------ */
		/* --- Data --- */
		/* ------------ */


		// The actual commands list.
		private readonly Dictionary<string, ICommand> _commands;
	}
}
