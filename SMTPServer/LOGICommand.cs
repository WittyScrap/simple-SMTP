using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPServer
{
	/// <summary>
	/// Manages a LOGI command.
	/// </summary>
	class LOGICommand : SMTPCommand
	{
		/// <summary>
		/// The name of the command (LOGI).
		/// </summary>
		public override string Name => "LOGI";

		/// <summary>
		/// Whether or not this is a valid LOGI command (must have username and password arguments).
		/// </summary>
		public bool IsValid { get; }

		/// <summary>
		/// The username parameter.
		/// </summary>
		public string Username { get; }

		/// <summary>
		/// The password parameter.
		/// </summary>
		public string Password { get; }

		/// <summary>
		/// The code to be displayed on successful formatting.
		/// </summary>
		protected override string SuccessCode { get; }

		/// <summary>
		/// The message to be displayed on successful formatting.
		/// </summary>
		protected override string SuccessMessage { get; }

		/// <summary>
		/// Checks whether an incoming message is a valid LOGI command.
		/// </summary>
		public bool CheckCommand(string messageData)
		{
			if (!IsFormatted)
			{
				return false;
			}

			string[] components = messageData.Substring(0, messageData.Length - 2).Split(' ');

			if (components.Length == 3 && components[0] == "LOGI")
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Creates a new LOGI command.
		/// </summary>
		public LOGICommand(string source) : base(source)
		{
			IsValid = CheckCommand(source);

			if (!IsValid)
			{
				SuccessCode = SMTPCodes.ClientError.SNTX;
				SuccessMessage = "Invalid command syntax.";
			}
			else
			{
				string[] components = source.Split(' ');

				Username = components[1];
				Password = components[2].Substring(0, components[2].Length - 2);
			}
		}
	}
}
