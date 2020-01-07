using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPClient
{
	/// <summary>
	/// Represents all necessary data for an SMTP response.
	/// </summary>
	class SMTPResponse
	{
		/// <summary>
		/// The SMTP response code.
		/// </summary>
		public enum Code
		{
			Invalid,
			Info,
			Success,
			Redirect,
			ServerError,
			ClientError
		}

		/// <summary>
		/// The code of this message's response.
		/// </summary>
		public Code ResponseCode { get; }

		/// <summary>
		/// The actual raw response code.
		/// </summary>
		public string Response { get; }

		/// <summary>
		/// The response message.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// Parses an SMTP response.
		/// </summary>
		/// <param name="source">The SMTP source message.</param>
		public SMTPResponse(string source)
		{
			if (source.Length < 5)
			{
				ResponseCode = Code.Invalid;
				Response = "000";
				Message = source;

				return;
			}

			string code = source.Substring(0, 3);
			string body = source.Substring(4);

			if (int.TryParse(code[0].ToString(), out int codeNumber))
			{
				ResponseCode = (Code)codeNumber;
				Response = code;
				Message = body;
			}
			else
			{
				ResponseCode = Code.Invalid;
				Response = "000";
				Message = source;
			}
		}
	}
}
