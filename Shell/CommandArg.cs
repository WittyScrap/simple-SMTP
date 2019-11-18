namespace Shell
{
	/// <summary>
	/// Represents a single command argument.
	/// </summary>
	public struct CommandArg
	{
		/// <summary>
		/// The name of the argument.
		/// </summary>
		public string Key { get; }

		/// <summary>
		/// The value of the argument.
		/// </summary>
		public object Value { get; }

		/// <summary>
		/// Whether or not this command argument is
		/// a single CHAR or a full WORD.
		/// </summary>
		public bool IsWord => Key.Length > 1;

		/// <summary>
		/// Creates a new command argument.
		/// </summary>
		/// <param name="key">The key of the argument.</param>
		/// <param name="value">The value of the argument.</param>
		public CommandArg(string key, object value)
		{
			Key = key;
			Value = value;
		}
	}
}
