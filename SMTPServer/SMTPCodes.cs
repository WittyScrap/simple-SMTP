using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPServer
{
	/// <summary>
	/// Contains all SMTP response codes.
	/// </summary>
	static class SMTPCodes
	{
		/// <summary>
		/// Contains all status messages (2XX).
		/// </summary>
		public static class Status
		{
			public const string SYSM = "211";
			public const string HELP = "214";
			public const string REDY = "220";
			public const string CLSE = "221";
			public const string SVOK = "250";
			public const string USNL = "251";
		}

		/// <summary>
		/// Contains all redirect messages (3XX).
		/// </summary>
		public static class Redirection
		{
			public const string MAIL = "354";
		}

		/// <summary>
		/// Contains any server related error code.
		/// </summary>
		public static class ServerError
		{
			public const string NOSV = "421";
			public const string NOMB = "450";
			public const string LERR = "451";
			public const string STRG = "452";
			public const string SVER = "471";
		}

		/// <summary>
		/// Contains any client related error code.
		/// </summary>
		public static class ClientError
		{
			public const string SNTX = "500";
			public const string PSTX = "501";
			public const string NIMP = "502";
			public const string ORDR = "503";
			public const string PNIM = "504";
			public const string NOMB = "550";
			public const string USNL = "551";
			public const string STRG = "552";
			public const string MBNM = "553";
			public const string FAIL = "554";
		}

		/// <summary>
		/// Composes a response message.
		/// </summary>
		public static string Compose(string code, string message)
		{
			return $"{code} {message}\r\n";
		}
	}
}
