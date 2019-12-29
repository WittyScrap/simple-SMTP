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

		/// <summary>
		/// Returns a command arg that matches any of the keys.
		/// </summary>
		/// <param name="validKeys">The list of valid keys to match.</param>
		/// <returns>The command that matches any of the keys.</returns>
		public bool Either<T>(out T arg, params string[] validKeys)
		{
			arg = default;

			foreach (string key in validKeys)
			{
				if (ContainsKey(key))
				{
					arg = (T)this[key].Value;
					return true;
				}
			}

			return false;
		}
	}
}
