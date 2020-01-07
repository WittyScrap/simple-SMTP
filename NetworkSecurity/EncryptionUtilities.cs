using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSecurity
{
	/// <summary>
	/// Contains utilities for general cryptography.
	/// </summary>
	public static class EncryptionUtilities
	{
		/// <summary>
		/// Calculates a SHA256 hash from a given source.
		/// </summary>
		/// <param name="source">The source string.</param>
		/// <returns>A SHA256 hashed version of the source string.</returns>
		public static string CalculateHash(string source)
		{
			using (SHA256 hash = SHA256.Create())
			{
				byte[] hashedValues = hash.ComputeHash(Encoding.UTF8.GetBytes(source));
				StringBuilder builder = new StringBuilder();

				foreach (byte point in hashedValues)
				{
					builder.Append(point.ToString("x2"));
				}

				return builder.ToString();
			}
		}

		/// <summary>
		/// Hashes a password using SHA256.
		/// </summary>
		/// <param name="password">The password to hash.</param>
		/// <param name="salt">The salt to add prior to salting.</param>
		/// <returns></returns>
		public static string HashPassword(string password, string salt)
		{
			return CalculateHash(password + salt);
		}
	}
}
