using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using VariableManagement;

namespace NetworkSecurity
{
	/// <summary>
	/// Structure that aids in performing a Diffie-Hellman key exchange.
	/// </summary>
	public class KeyExchanger
	{
		/// <summary>
		/// The public key being exchanged.
		/// </summary>
		public BigInteger PublicKey { get; }

		/// <summary>
		/// Creates a new Key exchanger.
		/// </summary>
		/// <param name="publicKey"></param>
		public KeyExchanger(BigInteger publicKey)
		{
			PublicKey = publicKey;
		}

		/// <summary>
		/// Converts this key exchanger instance into a transmittable source.
		/// </summary>
		public string Pack()
		{
			return
			"exchange" +
			"{" +
				"key:" + PublicKey + ";" +
			"}";
		}

		/// <summary>
		/// Attempts to unpack an incoming packed key exchanger.
		/// </summary>
		public static KeyExchanger Unpack(string message)
		{
			Variables parser;

			try
			{
				parser = Variables.Parse(message);
			}
			catch (Exception)
			{
				return null;
			}

			return new KeyExchanger(new BigInteger(Convert.ToInt64(parser["exchange.key"])));
		}
	}
}
