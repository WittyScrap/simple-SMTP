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
	class HELOCommand : ISMTPCommand
	{
		/// <summary>
		/// The name of the command (HELO).
		/// </summary>
		public string Name => "HELO";

		/// <summary>
		/// The domain that is being identified.
		/// </summary>
		public string Domain { get; }

		/// <summary>
		/// Whether or not the command is correctly formatted.
		/// </summary>
		public bool IsFormatted { get; }

		/// <summary>
		/// Regular expression to match a given domain name.
		/// </summary>
		private Regex DomainMatch { get; } = new Regex(@"([a-z0-9]+\.)*[a-z0-9]+\.[a-z]+", RegexOptions.Compiled);

		/// <summary>
		/// The response to this command.
		/// </summary>
		public string Response 
		{ 
			get
			{
				if (!IsFormatted)
				{
					return SMTPCodes.Compose(SMTPCodes.ClientError.PSTX, "Invalid command syntax.");
				}

				if (Domain == null)
				{
					return SMTPCodes.Compose(SMTPCodes.ClientError.PSTX, "Invalid domain provided.");
				}
				
				return SMTPCodes.Compose(SMTPCodes.Status.SVOK, $"Welcome, {Domain}!");
			}
		}

		/// <summary>
		/// Parses a HELO command from a given source string.
		/// </summary>
		/// <param name="source">The source from which to parse the command.</param>
		public HELOCommand(string source)
		{
			if (source.Substring(source.Length - 2) != "\r\n")
			{
				IsFormatted = false;
			}
			else
			{
				IsFormatted = true;
			}

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
