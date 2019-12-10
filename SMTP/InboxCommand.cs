using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shell;
using Client;

namespace SMTP
{
	/// <summary>
	/// Shows any incoming emails.
	/// </summary>
	class InboxCommand : ICommand
	{
		/// <summary>
		/// The help message.
		/// </summary>
		public string Help => Format.Name(Name, new Arg("inbox", "account_name", 'i')) + Format.Text("Shows any incoming emails into the provided account.");

		/// <summary>
		/// The name of the command.
		/// </summary>
		public string Name => "inbox";

		/// <summary>
		/// Checks for emails.
		/// </summary>
		public bool Execute(IShell sourceShell, ParameterSet args = null)
		{
			return true;
		}
	}
}
