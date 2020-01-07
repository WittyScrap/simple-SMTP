using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using VariableManagement;
using System.Security.Cryptography;
using System.Text;
using NetworkSecurity;
using Shell;

namespace SMTPServer
{
	/// <summary>
	/// Manages internal SMTP permanent data for mailboxes/audit logs.
	/// </summary>
	class SMTPData
	{
		/// <summary>
		/// The root of all SMTP data.
		/// </summary>
		public string DirectoryRoot { get; }

		/// <summary>
		/// The name of the users database.
		/// </summary>
		public string UsersDatabase { get; }

		/// <summary>
		/// The path to the users database.
		/// </summary>
		public string UsersPath => Path.Combine(DirectoryRoot, UsersDatabase);

		/// <summary>
		/// The direct path to all inboxes.
		/// </summary>
		public string InboxesFolder => Path.Combine(DirectoryRoot, "Inboxes");

		/// <summary>
		/// Contains an audit log of all actions from all clients.
		/// </summary>
		public string AuditLog { get; private set; }

		/// <summary>
		/// Initialises a connection to the users database.
		/// </summary>
		public SMTPData(string root, string databaseName, string auditLog)
		{
			DirectoryRoot = root;
			UsersDatabase = databaseName;
			AuditLog = auditLog;

			PrepareConfiguration();

			_usersData = Variables.Load(UsersPath);
		}

		/// <summary>
		/// Creates any missing folders and files.
		/// </summary>
		private void PrepareConfiguration()
		{
			if (!Directory.Exists(DirectoryRoot))
			{
				Directory.CreateDirectory(DirectoryRoot);
			}

			if (!File.Exists(UsersPath))
			{
				File.WriteAllText(UsersPath, "users\r\n{\r\n}\r\n");
			}
		}

		/// <summary>
		/// Creates an inbox folder if it does not exist and returns a direct path to it.
		/// </summary>
		private string GetInbox(string email)
		{
			if (!Directory.Exists(InboxesFolder))
			{
				Directory.CreateDirectory(InboxesFolder);
			}

			string fullPath = Path.Combine(InboxesFolder, email);

			if (!Directory.Exists(fullPath))
			{
				Directory.CreateDirectory(fullPath);
			}

			return fullPath;
		}

		/// <summary>
		/// Checks whether or not a username has been registered in the users data.
		/// </summary>
		public User Verify(string username)
		{
			if (_usersData.Root.GetObject("users")?.GetObject(username) != null)
			{
				VariablesObject user = _usersData.Root.GetObject("users").GetObject(username);

				return new User((string)user.GetField("salt"))
				{
					Username = username,
					Password = (string)user.GetField("password"),
					Name = (string)user.GetField("name"),
					Email = new MailAddress((string)user.GetField("email"))
				};
			}
			else
			{
				return null;
			}
		}

		/// <summary>
		/// Verifies an email address.
		/// </summary>
		public User VerifyMail(string email)
		{
			foreach (User user in ListUsers())
			{
				if (user.Email.Address == email)
				{
					return new User()
					{
						Username = user.Username,
						Name = user.Name,
						Email = user.Email
					};
				}
			}

			return null;
		}

		/// <summary>
		/// Stores an email.
		/// </summary>
		public void SaveMail(Mail mail)
		{
			string inbox = GetInbox(mail.Receiver);
			string emailName = EncryptionUtilities.CalculateHash($"{mail.Sender}{mail.Receiver}{User.GenerateSalt()}{DateTime.Now.Ticks.ToString()}.mail");
			string emailPath = Path.Combine(inbox, emailName);

			File.WriteAllText(emailPath, mail.Serialise());
		}

		/// <summary>
		/// Stores any changes made to the users list back into the users file.
		/// </summary>
		/// <param name="users">The updated users list.</param>
		private void SaveChanges(ICollection<User> users)
		{
			string data = "users\n{\n";

			foreach (User user in users)
			{
				data += $"\t{user.Username}\n\t{{\n";
				data += $"\t\tpassword: \"{user.Password}\";\n";
				data += $"\t\tsalt: \"{user.Salt}\";\n";
				data += $"\t\tname: \"{user.Name}\";\n";
				data += $"\t\temail: \"{user.Email.Address}\";\n";
				data += "\t}\n";
			}

			data += "}";

			File.WriteAllText(UsersPath, data);
			_usersData = Variables.Parse(data);
		}

		/// <summary>
		/// Creates a user, if one with the same name does not exist.
		/// </summary>
		/// <param name="username">The user to create.</param>
		public void CreateUser(User user)
		{
			if (Verify(user.Username) != null)
			{
				throw new InvalidOperationException($"User {user.Username} already exists.");
			}

			user.Password = EncryptionUtilities.HashPassword(user.Password, user.Salt);

			List<User> allUsers = ListUsers().ToList();
			allUsers.Add(user);

			SaveChanges(allUsers);
		}

		/// <summary>
		/// Deletes a user, if it exists.
		/// </summary>
		/// <param name="username">The user to be deleted.</param>
		public void DeleteUser(string username)
		{
			if (Verify(username) == null)
			{
				throw new InvalidOperationException($"User {username} does not exist, cannot delete.");
			}

			List<User> allUsers = ListUsers().ToList();
			allUsers.RemoveAll(userObject => userObject.Username == username);

			SaveChanges(allUsers);
		}

		/// <summary>
		/// Lists all available users.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<User> ListUsers()
		{
			VariablesObject users = _usersData.Root.GetObject("users");

			foreach (var examinedUser in users.GetAllObjects())
			{
				yield return new User((string)examinedUser.Value.GetField("salt"))
				{
					Username = examinedUser.Key,
					Name = (string)examinedUser.Value.GetField("name"),
					Email = new MailAddress((string)examinedUser.Value.GetField("email")),
					Password = (string)examinedUser.Value.GetField("password")
				};
			}
		}

		/// <summary>
		/// Logs an action within the <see cref="AuditLog"/> file.
		/// </summary>
		public void LogAction(string user, string action)
		{
			string header = $"[LOGGER::{DateTime.Now.ToString()}";
			string body = $"{user} : {action}";

			string hash = EncryptionUtilities.CalculateHash(body);

			File.AppendAllText(AuditLog, $"{header}?:{hash}] - {body}\r\n");
		}

		// Users data
		private Variables _usersData;
	}
}
