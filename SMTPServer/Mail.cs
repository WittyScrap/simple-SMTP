using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VariableManagement;

namespace SMTPServer
{
	/// <summary>
	/// Represents a simple serializable email.
	/// </summary>
	class Mail
	{
		/// <summary>
		/// The email sender.
		/// </summary>
		public string Sender { get; }

		/// <summary>
		/// The specific receiving user of this mail.
		/// </summary>
		public string Receiver { get; }

		/// <summary>
		/// The contents of the email.
		/// </summary>
		public string Data { get; }

		/// <summary>
		/// Returns the data for this email one line at a time.
		/// </summary>
		/// <returns>The data for this email one line at a time.</returns>
		public IEnumerable<string> GetLines()
		{
			string[] lines = Data.Split(new string[] { "\r\n" }, StringSplitOptions.None);

			foreach (string line in lines)
			{
				yield return line;
			}
		}

		/// <summary>
		/// Deserializes an email from a given source.
		/// </summary>
		/// <param name="source">Where to deserialize the email from.</param>
		/// <returns>An email object equivalent to the deserialized file.</returns>
		public static Mail Deserialize(string source)
		{
			Variables mailData;

			try
			{
				mailData = Variables.Parse(source);
			}
			catch (Exception) // Parsing failed.
			{
				return null;
			}

			return new Mail(
				mailData.Get<string>("mail.sender"),
				mailData.Get<string>("mail.receiver"),
				mailData.Get<string>("mail.data")
			);
		}

		/// <summary>
		/// Serialises this email into a string.
		/// </summary>
		public string Serialise()
		{
			string data = "mail {\r\n";

			data += $"\tsender: \"{Sender}\"\r\n";
			data += $"\treceiver: \"{Receiver}\"\r\n";
			data += $"\tdata: \r\n\"{Data}\"\r\n";

			data += "}";

			return data;
		}

		/// <summary>
		/// Creates a new mail.
		/// </summary>
		/// <param name="sender">The mail sender.</param>
		/// <param name="receiver">The mail receiver.</param>
		/// <param name="data">The mail contents.</param>
		public Mail(string sender, string receiver, string data)
		{
			Sender = sender;
			Receiver = receiver;
			Data = data;
		}
	}
}
