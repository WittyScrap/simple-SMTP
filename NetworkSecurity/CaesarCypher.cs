using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSecurity
{
	/// <summary>
	/// Simple caesar cypher to test encryption/decryption and key transportation.
	/// </summary>
	public class CaesarCypher : IEncryptor
	{
		/// <summary>
		/// "Encrypts" a message with a given key.
		/// </summary>
		/// <param name="data">The message to "encrypt".</param>
		/// <param name="key">The key.</param>
		public string Encrypt(string data, string key)
		{
			if (data == null)
			{
				return null;
			}

			StringBuilder encryptedMessage = new StringBuilder();

			foreach (char letter in data)
			{
				char nextLetter = letter;

				foreach (char keyElement in key)
				{
					nextLetter += keyElement;
				}

				encryptedMessage.Append(nextLetter);
			}

			return encryptedMessage.ToString();
		}

		/// <summary>
		/// Decrypts a message with a given key.
		/// </summary>
		/// <param name="data">The message to decrypt.</param>
		/// <param name="key">The key.</param>
		public string Decrypt(string data, string key)
		{
			if (data == null)
			{
				return null;
			}

			StringBuilder encryptedMessage = new StringBuilder();

			foreach (char letter in data)
			{
				char nextLetter = letter;

				foreach (char keyElement in key)
				{
					nextLetter -= keyElement;
				}

				encryptedMessage.Append(nextLetter);
			}

			return encryptedMessage.ToString();
		}
	}
}
