﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPServer
{
	/// <summary>
	/// Manages a VRFY command.
	/// </summary>
	class VRFYCommand : SMTPCommand
	{
		/// <summary>
		/// The name of the command (VRFY).
		/// </summary>
		public override string Name => "VRFY";

		/// <summary>
		/// The message to display on successful parsing.
		/// </summary>
		protected override string SuccessMessage => IsComplete ? "" : "Invalid syntax.";

		/// <summary>
		/// The code to be returned upon successful parsing.
		/// </summary>
		protected override string SuccessCode => IsComplete ? "" : SMTPCodes.ClientError.ParameterError;

		/// <summary>
		/// Whether or not the command includes the username argument.
		/// </summary>
		public bool IsComplete => Username != null;

		/// <summary>
		/// The stored username.
		/// </summary>
		public string Username { get; } = null;

		/// <summary>
		/// Manages a new VRFY command.
		/// </summary>
		public VRFYCommand(string source) : base(source)
		{
			if (!IsFormatted)
			{
				return;
			}

			source = source.Substring(0, source.Length - 2);

			if (source.Length > 5 && source[4] == ' ')
			{
				string userName = source.Substring(5);

				if (!string.IsNullOrWhiteSpace(userName))
				{
					Username = userName;
				}
			}
		}
	}
}
