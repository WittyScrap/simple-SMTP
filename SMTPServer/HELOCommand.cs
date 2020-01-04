using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SMTPServer
{
	/// <summary>
	/// Manages a HELO command.
	/// </summary>
	class HELOCommand : SMTPCommand
	{
		/// <summary>
		/// The name of the command (HELO).
		/// </summary>
		public override string Name => "HELO";

		/// <summary>
		/// The domain that is being identified.
		/// </summary>
		public string Domain { get; }

		/// <summary>
		/// Regular expression to match a given domain name.
		/// </summary>
		private Regex DomainMatch { get; } = new Regex(@"([a-z0-9]+\.)*[a-z0-9]+\.[a-z]+", RegexOptions.Compiled);

		/// <summary>
		/// The message to be displayed on successful parsing.
		/// </summary>
		protected override string SuccessMessage => Domain == null ? "Invalid domain provided." : $"Welcome, {Domain}!";

		/// <summary>
		/// The response code upon successful parsing.
		/// </summary>
		protected override string SuccessCode => Domain != null ? base.SuccessCode : SMTPCodes.ClientError.PSTX;

		/// <summary>
		/// Parses a HELO command from a given source string.
		/// </summary>
		/// <param name="source">The source from which to parse the command.</param>
		public HELOCommand(string source) : base(source)
		{
			source = source.Substring(0, source.Length - 2);

			if (source.Length > 5)
			{
				Domain = source.Substring(5);

				if (!DomainMatch.IsMatch(Domain))
				{
					Domain = null;
				}
			}
			else
			{
				Domain = null;
			}
		}
	}
}
