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
			public const string SystemStatus = "211";
			public const string HelpResponse = "214";
			public const string SystemReady = "220";
			public const string SystemClosing = "221";
			public const string ServiceOK = "250";
			public const string UserNotLocal = "251";
		}

		/// <summary>
		/// Contains all redirect messages (3XX).
		/// </summary>
		public static class Redirection
		{
			public const string BeginMail = "354";
		}

		/// <summary>
		/// Contains any server related error code.
		/// </summary>
		public static class ServerError
		{
			public const string ServiceUnavailable = "421";
			public const string MailboxUnavailable = "450";
			public const string ErrorAbort = "451";
			public const string InsufficientStorage = "452";
			public const string ServiceError = "471";
		}

		/// <summary>
		/// Contains any client related error code.
		/// </summary>
		public static class ClientError
		{
			public const string SyntaxError = "500";
			public const string ParameterError = "501";
			public const string CommandNotImplemented = "502";
			public const string BadOrder = "503";
			public const string ParameterNotImplemented = "504";
			public const string MailboxUnavailable = "550";
			public const string UserNotLocal = "551";
			public const string AllocationExceeded = "552";
			public const string InvalidMailboxName = "553";
			public const string TransactionFailed = "554";
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
