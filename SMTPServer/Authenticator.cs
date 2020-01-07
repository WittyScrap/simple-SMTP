using NetworkSecurity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPServer
{
	/// <summary>
	/// Manages authenticating a user.
	/// </summary>
	class Authenticator
	{
		/// <summary>
		/// The username that this session is being identified as.
		/// </summary>
		public string Username { get; private set; } = null;

		/// <summary>
		/// A flag that checks whether or not the authentication has been carried out successfully.
		/// </summary>
		public bool IsAuthenticated => Username != null;

		/// <summary>
		/// Creates a new authenticator.
		/// </summary>
		/// <param name="data">Reference permanent data.</param>
		public Authenticator(SMTPData data)
		{
			_data = data;
		}

		/// <summary>
		/// Attempts to perform a log in.
		/// </summary>
		/// <param name="messageData">The data sent from the user.</param>
		/// <returns>True if the login was successful, false otherwise.</returns>
		public string TryLogin(string messageData)
		{
			if (!SMTPParser.Parse(messageData, out ISMTPCommand command))
			{
				return SMTPCodes.Compose(SMTPCodes.ClientError.SyntaxError, "Invalid syntax.");
			}

			if (!(command is LOGICommand logiCommand))
			{
				return SMTPCodes.Compose(SMTPCodes.ClientError.BadOrder, "Bad senquence of commands; you must login.");
			}

			if (!logiCommand.IsValid)
			{
				return SMTPCodes.Compose(SMTPCodes.ClientError.ParameterError, "Invalid argument syntax.");
			}

			string username = logiCommand.Username;
			string password = logiCommand.Password;

			// Check that username exists
			User user = _data.Verify(username);

			if (user != null)
			{
				if (!CheckPassword(password, user))
				{
					return SMTPCodes.Compose(SMTPCodes.ClientError.MailboxUnavailable, "Login failed; invalid password.");
				}
				else
				{
					_data.LogAction(username, "Logged in.");
					Username = username;
					return null;
				}
			}
			else
			{
				return SMTPCodes.Compose(SMTPCodes.ClientError.MailboxUnavailable, "Login failed; invalid username.");
			}
		}
		
		/// <summary>
		/// Checks if the password matches the user's hashed password.
		/// </summary>
		private bool CheckPassword(string password, User user)
		{
			password = EncryptionUtilities.HashPassword(password, user.Salt);
			return password == user.Password;
		}

		/// <summary>
		/// Removes any authentication.
		/// </summary>
		public void Logout()
		{
			Username = null;
		}

		//
		// Data
		//

		private SMTPData _data;
	}
}
