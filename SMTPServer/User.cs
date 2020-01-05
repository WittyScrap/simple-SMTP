using System;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace SMTPServer
{
	/// <summary>
	/// Represents data about a single user.
	/// </summary>
	class User
	{
		/// <summary>
		/// The username.
		/// </summary>
		public string Username { get; set; }

		/// <summary>
		/// The full name of the user.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The hashed and salted password.
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// The randomly generated salt.
		/// </summary>
		public string Salt { get; }

		/// <summary>
		/// The user's email address.
		/// </summary>
		public MailAddress Email { get; set; }

		/// <summary>
		/// Generates a new user with a randomly generated salt for its password.
		/// </summary>
		public User()
		{
			Salt = Guid.NewGuid().ToString();
		}

		/// <summary>
		/// Creates a new user with a predefined salt.
		/// </summary>
		/// <param name="salt"></param>
		public User(string salt)
		{
			Salt = salt;
		}

		/// <summary>
		/// Converts a sequence of bytes into a readable string format.
		/// </summary>
		/// <param name="sourceBytes">The original byte array segment.</param>
		/// <returns>A string composed of the original array of bytes.</returns>
		public static string Decode(byte[] sourceBytes)
		{
			return Encoding.UTF8.GetString(sourceBytes).Replace("\0", "");
		}

		/// <summary>
		/// Converts this user to a readable string.
		/// </summary>
		public override string ToString()
		{
			return $"{Name} [{Username}] <{Email}>";
		}
	}
}
