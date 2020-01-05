using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSecurity
{
	/// <summary>
	/// Represents an encapsulation of an encryption algorithm.
	/// </summary>
	public interface IEncryptor
	{
		/// <summary>
		/// Encrypts the given data string using the given key string.
		/// </summary>
		string Encrypt(string data, string key);

		/// <summary>
		/// Decrypts the given data string using the given key string.
		/// </summary>
		string Decrypt(string data, string key);
	}
}
