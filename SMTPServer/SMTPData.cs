using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMTPServer
{
	/// <summary>
	/// Manages internal SMTP permanent data for mailboxes/audit logs.
	/// </summary>
	static class SMTPData
	{
		/// <summary>
		/// The name of the root folder.
		/// </summary>
		const string ROOT = "SMTP";

		/// <summary>
		/// Returns all users inside the main ROOT folder.
		/// </summary>
		/// <returns>All users inside the root folder.</returns>
		public static IEnumerable<string> GetUsers()
		{
			if (!Directory.Exists(ROOT))
			{
				return Enumerable.Empty<string>();
			}

			return Directory.EnumerateDirectories(ROOT);
		}

		/// <summary>
		/// Checks that a username exists.
		/// </summary>
		/// <param name="username">The username to verify.</param>
		/// <returns>True if the username's mailbox could be found, false otherwise.</returns>
		public static bool UsernameExists(string username)
		{
			username = ROOT + Path.DirectorySeparatorChar + username;

			foreach (string user in GetUsers())
			{
				if (user == username)
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Returns the path to use to reach a given username.
		/// </summary>
		/// <param name="username">The username to be reached.</param>
		/// <returns>The full path to reach the username's inbox.</returns>
		private static string GetUserPath(string username)
		{
			return ROOT + Path.DirectorySeparatorChar + username;
		}

		/// <summary>
		/// Returns all mail for a given username.
		/// If the user does not exist, no mail will be returned.
		/// </summary>
		/// <param name="username">The username to check any mail for.</param>
		/// <returns>All mail in the user's inbox.</returns>
		public static IEnumerable<Mail> GetInbox(string username)
		{
			if (!UsernameExists(username))
			{
				yield break;
			}

			string path = GetUserPath(username);

			foreach (string mail in Directory.EnumerateFiles(path))
			{
				string mailFile = File.ReadAllText(mail);
				Mail mailObject = Mail.Deserialize(mailFile);

				if (mailObject != null)
				{
					yield return mailObject;
				}
			}
		}

		/// <summary>
		/// Saves a mail into its appropriate inbox.
		/// </summary>
		/// <param name="mail">The mail to be saved.</param>
		public static void SaveMail(Mail mail)
		{
			string mailData = mail.Serialise();
			string inbox = mail.Receiver;

			if (!UsernameExists(inbox))
			{
				return;
			}

			DateTime foo = DateTime.UtcNow;
			long unixTime = ((DateTimeOffset)foo).ToUnixTimeSeconds();

			string path = GetUserPath(inbox) + Path.DirectorySeparatorChar;
			string baseName = $"mail#{mail.Sender}#{mail.Receiver}#{unixTime}";
			string fileName = baseName;

			int userPadding = 0;

			while (File.Exists(path + fileName))
			{
				fileName = baseName + $"#{++userPadding}";
			}

			fileName = path + fileName + "#data.mail";

			File.WriteAllText(fileName, mailData);
		}
	}
}
