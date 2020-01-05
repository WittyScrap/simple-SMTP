using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using VariableManagement;
using System.Security.Cryptography;
using System.Text;

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
		/// Initialises a connection to the users database.
		/// </summary>
		public SMTPData(string root, string databaseName)
		{
			DirectoryRoot = root;
			UsersDatabase = databaseName;

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
			string emailName = $"mail{mail.Sender}{mail.Receiver}data.mail";

			string emailPath = Path.Combine(inbox, emailName);
			string submitPath = emailPath;

			int offset = 0;

			while (File.Exists(submitPath))
			{
				submitPath = emailPath + "#" + ++offset;
			}

			File.WriteAllText(submitPath, mail.Serialise());
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

			// Hash password.
			using (SHA256 hash = SHA256.Create())
			{
				byte[] hashedValues = hash.ComputeHash(Encoding.UTF8.GetBytes(user.Password + user.Salt));
				StringBuilder builder = new StringBuilder();

				foreach (byte point in hashedValues)
				{
					builder.Append(point.ToString("x2"));
				}

				user.Password = builder.ToString();
			}

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

		// Users data
		private Variables _usersData;
	}
}
