using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPServer
{
	/// <summary>
	/// A response to an SMTP command.
	/// </summary>
	class SMTPResponse
	{
		/// <summary>
		/// Whether or not the response was successful.
		/// </summary>
		public bool IsSuccessful { get; }

		/// <summary>
		/// The response code.
		/// </summary>
		public string Code { get; }

		/// <summary>
		/// The response message.
		/// </summary>
		public string Message { get; }

		/// <summary>
		/// The full response to be sent to the remote client.
		/// </summary>
		public string Response => Code + " " + Message;

		/// <summary>
		/// Creates a new SMTP response.
		/// </summary>
		/// <param name="success">Whether this is a successful response.</param>
		/// <param name="code">The response code.</param>
		/// <param name="message">The contents of the response.</param>
		public SMTPResponse(bool success, string code, string message)
		{
			IsSuccessful = success;
			Code = code;
			Message = message;
		}
	}
}
