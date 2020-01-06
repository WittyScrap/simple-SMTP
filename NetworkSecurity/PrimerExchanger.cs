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
	/// Exchanges initial information to perform a Diffie-hellman key exchange.
	/// </summary>
	public class PrimerExchanger : IEquatable<PrimerExchanger>
	{
		/// <summary>
		/// The large prime value.
		/// </summary>
		public BigInteger PrimeValue { get; }

		/// <summary>
		/// The smaller base value.
		/// </summary>
		public int BaseValue { get; }

		/// <summary>
		/// Creates a new primer exchanger.
		/// </summary>
		public PrimerExchanger(BigInteger primeValue, int baseValue)
		{
			PrimeValue = primeValue;
			BaseValue = baseValue;
		}

		/// <summary>
		/// Converts this key exchanger instance into a transmittable source.
		/// </summary>
		public string Pack()
		{
			return
			"exchange" +
			"{" +
				"base:" + BaseValue + ";" +
				"prime:" + PrimeValue + ";" +
			"}";
		}

		/// <summary>
		/// Attempts to unpack an incoming packed key exchanger.
		/// </summary>
		public static PrimerExchanger Unpack(string message)
		{
			Variables parser;

			try
			{
				parser = Variables.Parse(message);
				return new PrimerExchanger(new BigInteger(Convert.ToInt64(parser["exchange.prime"])), parser.Get<int>("exchange.base"));
			}
			catch (Exception)
			{
				return null;
			}
		}

		/// <summary>
		/// Checks that this exchanger matches the target exchanger.
		/// </summary>
		public bool Equals(PrimerExchanger target)
		{
			return BaseValue.Equals(target.BaseValue) &&
				   PrimeValue.Equals(target.PrimeValue);
		}
	}
}
