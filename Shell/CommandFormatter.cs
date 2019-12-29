namespace Shell
{
	/// <summary>
	/// Text-only argument for displaying a command's help.
	/// </summary>
	public class Arg
	{
		/// <summary>
		/// Creates a new help arg.
		/// </summary>
		/// <param name="name">The name of the argument in the --name format.</param>
		/// <param name="friendlyName">The friendly name of the argument (argument value hint).</param>
		/// <param name="shortName">The short name of the argument in the -n format.</param>
		public Arg(string name, string friendlyName, char shortName = '\0', bool required = true)
		{
			Name = name;
			FriendlyName = friendlyName;
			ShortName = shortName;
			Required = required;
		}

		/// <summary>
		/// The name of the argument.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// The friendly name for the argument.
		/// </summary>
		public string FriendlyName { get; }

		/// <summary>
		/// The short version of the argument, char 0 indicates no short name is provided.
		/// </summary>
		public char ShortName { get; }

		/// <summary>
		/// Whether or not the argument is required.
		/// </summary>
		public bool Required { get; }
	}

	/// <summary>
	/// Provides command formatting tools.
	/// </summary>
	public static class Format
	{
		/// <summary>
		/// Returns the correct type of brackets to use for this argument.
		/// </summary>
		private static string GetBracketSet(Arg arg)
		{
			if (arg.Required)
			{
				return "<>";
			}
			else
			{
				return "[]";
			}
		}

		/// <summary>
		/// Unpacks an arguments array into a string.
		/// </summary>
		private static string UnpackArguments(Arg[] args)
		{
			if (args.Length == 0)
			{
				return "";
			}

			string output = "";

			foreach (Arg arg in args)
			{
				string bracket = GetBracketSet(arg);
				output += Text($" {bracket[0]}--") + Text(arg.Name);
				if (arg.ShortName != '\0')
				{
					output += Text("/-") + Text(arg.ShortName.ToString());
				}
				output += " " + arg.FriendlyName + Text($"{bracket[1]}");
			}

			return output;
		}

		/// <summary>
		/// Format for displaying the command's name.
		/// </summary>
		public static string Name(string commandName, params Arg[] args)
		{
			return @"\cf1\b " + commandName + @"\b0 " + UnpackArguments(args) + @"\cf1\b :\b0\cf3  ";
		}

		/// <summary>
		/// Format for displaying simple text.
		/// </summary>
		public static string Text(string text)
		{
			return $@"\cf2\i {text}\i0\cf3 ";
		}

		/// <summary>
		/// Format for displaying user input in the shell.
		/// </summary>
		public static string Output(string text)
		{
			return $@"\cf1\b {text}\b0\cf3 ";
		}

		/// <summary>
		/// Prompts an error message on the given shell.
		/// </summary>
		/// <param name="sourceShell"></param>
		/// <param name="errorMessage"></param>
		/// <returns></returns>
		public static bool Error(IShell sourceShell, string command, string errorMessage)
		{
			sourceShell.Print(command, errorMessage);
			return false;
		}
	}
}
