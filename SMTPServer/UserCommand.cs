using Server;
using Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SMTPServer
{
	/// <summary>
	/// Allows to manage a set of users.
	/// </summary>
	class UserCommand : ICommand
	{
		/// <summary>
		/// The help section of the shell for this command.
		/// </summary>
		public string Help => Format.Name(Name, new Arg("add", "user_to_add", 'a', false), new Arg("remove", "user_to_remove", 'r', false), new Arg("list", "", required: false)) + Format.Text("Allows to manage a users set.");

		/// <summary>
		/// The name of the command (user).
		/// </summary>
		public string Name => "user";

		/// <summary>
		/// Executes the command.
		/// </summary>
		public bool Execute(IShell sourceShell, ParameterSet args = null)
		{
			if (sourceShell is ServerShell serverShell && serverShell.Program is SMTPServer server)
			{
				if (args == null)
				{
					return Format.Error(sourceShell, Name, $"No arguments provided.");
				}

				bool actionCreate = args.Either(out string createUser, "add", "a");
				bool actionRemove = args.Either(out string removeUser, "remove", "r");
				bool actionLister = args.Either(out object _, "list");

				if (actionCreate && !CreateUser(serverShell, server.Source, createUser))
				{
					return false;
				}

				if (actionRemove && !RemoveUser(serverShell, server.Source, removeUser))
				{
					return false;
				}

				if (actionLister && !ListAllUsers(serverShell, server.Source))
				{
					return false;
				}

				return true;
			}
			else
			{
				return Format.Error(sourceShell, Name, $"The {Name} command is not supported on this shell.");
			}
		}

		/// <summary>
		/// Creates a new user, if one does not exist.
		/// </summary>
		/// <param name="username">The user to create.</param>
		private bool CreateUser(ServerShell shell, SMTPData source, string username)
		{
			string[] components = username.Split(';');

			if (components.Length != 4)
			{
				return Format.Error(shell, Name, "Invalid username formatting, please use \"username;password;name;email\"");
			}

				   username = components[0];
			string password = components[1];
			string fullname = components[2];
			string emailadd = components[3];

			User generatedUser = new User()
			{
				Username = username,
				Password = password,
				Name = fullname,
				Email = new MailAddress(emailadd)
			};

			try
			{
				source.CreateUser(generatedUser);
				shell.Print(Name, Format.Output($"Username {components[0]} created."));
				return true;
			}
			catch (Exception e)
			{
				return Format.Error(shell, Name, "Could not create user: " + e.Message);
			}
		}

		/// <summary>
		/// Deletes a username, if it exists.
		/// </summary>
		/// <param name="username">The user to delete.</param>
		private bool RemoveUser(ServerShell shell, SMTPData source, string username)
		{
			try
			{
				source.DeleteUser(username);
				shell.Print(Name, Format.Output($"Username {username} deleted."));
				return true;
			}
			catch (Exception e)
			{
				return Format.Error(shell, Name, "Could not delete user: " + e.Message);
			}
		}

		/// <summary>
		/// Lists all available users.
		/// </summary>
		private bool ListAllUsers(ServerShell shell, SMTPData source)
		{
			try
			{
				foreach (User user in source.ListUsers())
				{
					shell.Print(Name, Format.Output(user.ToString()));
				}

				return true;
			}
			catch (Exception e)
			{
				return Format.Error(shell, Name, "Could not list all users: " + e.Message);
			}
		}
	}
}
