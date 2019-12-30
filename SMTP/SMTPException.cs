using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTP
{
	/// <summary>
	/// Manages a generic SMTP exception.
	/// </summary>
	class SMTPException : Exception
	{
		/// <summary>
		/// The exception's message.
		/// </summary>
		public override string Message => "SMTP: " + base.Message;

		/// <summary>
		/// Creates a new SMTP Exception.
		/// </summary>
		public SMTPException(string message) : base(message)
		{ }

		/// <summary>
		/// Creates a new SMTP Exception from a SMTP Response.
		/// </summary>
		/// <param name="response"></param>
		public SMTPException(SMTPResponse response) : base(response.Response + " :: " + response.Message)
		{ }
	}
}
