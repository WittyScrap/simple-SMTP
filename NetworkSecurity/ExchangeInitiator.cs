using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSecurity
{
	/// <summary>
	/// Handles managing transactions for exchanging encryption keys.
	/// This half of the class will be the initiator.
	/// </summary>
	public class ExchangeInitiator : TransactionManager
	{
		/// <summary>
		/// Defines which state the exchange is in.
		/// </summary>
		enum ExchangeState
		{ 
			ShareStarter,
			AwaitConfirmation,
			AwaitPublicKey,
			Complete
		}

		/// <summary>
		/// Evaluates an incoming message from a remote host to
		/// determine if the next action in the key agreement process
		/// can be made.
		/// </summary>
		/// <param name="message">The message to evaluate.</param>
		/// <returns>The appropriate response, if none is needed NULL will be returned.</returns>
		public override string Evaluate(string message)
		{
			switch (_state)
			{
				case ExchangeState.AwaitConfirmation:
					return OnConfirmationReceived(message);

				case ExchangeState.AwaitPublicKey:
					return OnPublicKeyReceived(message);

				case ExchangeState.Complete:
					return null; // But don't fail.

				default:
					HasFailed = true;
					return null;
			}
		}

		/// <summary>
		/// Calculates the square root of a <see cref="BigInteger"/>.
		/// </summary>
		private static BigInteger Sqrt(BigInteger n)
		{
			if (n == 0)
			{
				return 0;
			}

			if (n > 0)
			{
				int bitLength = Convert.ToInt32(Math.Ceiling(BigInteger.Log(n, 2)));
				BigInteger root = BigInteger.One << (bitLength / 2);

				while (!IsSqrt(n, root))
				{
					root += n / root;
					root /= 2;
				}

				return root;
			}

			throw new ArithmeticException("NaN");
		}

		/// <summary>
		/// Checks if a BigInteger is a perfect square.
		/// </summary>
		private static bool IsSqrt(BigInteger n, BigInteger root)
		{
			BigInteger lowerBound = root * root;
			BigInteger upperBound = (root + 1) * (root + 1);

			return (n >= lowerBound && n < upperBound);
		}

		/// <summary>
		/// Checks whether or not a number is prime.
		/// </summary>
		private static bool IsPrime(BigInteger number)
		{
			BigInteger root = Sqrt(number);

			for (BigInteger i = 2; i <= root; i++)
			{
				if (number % i == 0)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Ensures that the passed integer is prime.
		/// </summary>
		private static void EnsurePrime(ref BigInteger number)
		{
			while (!IsPrime(number))
			{
				number--;
			}
		}

		/// <summary>
		/// Checks whether or not a number is prime.
		/// </summary>
		private static bool IsPrime(int number)
		{
			int root = (int)Math.Sqrt(number);

			for (int i = 2; i <= root; i++)
			{
				if (number % i == 0)
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Ensures that the passed normal integer is prime.
		/// </summary>
		private static void EnsurePrime(ref int number)
		{
			while (!IsPrime(number))
			{
				number--;
			}
		}

		/// <summary>
		/// Submits the pre-agreed p and g values and moves on to the next state.
		/// </summary>
		public string Initiate()
		{
			_state++;
			const int bits = 32;

			_randomizer = new Random((int)DateTime.Now.Ticks);
			byte[] pData = new byte[bits / 8];
			_randomizer.NextBytes(pData);

			// Initial value.
			BigInteger p = BigInteger.Abs(new BigInteger(pData));

			// Base value.
			int g = _randomizer.Next(0xFF, 0xFF * 2);

			// Make prime.
			EnsurePrime(ref p);
			EnsurePrime(ref g);

			// Create packet and submit.
			_initiator = new PrimerExchanger(p, g);
			return _initiator.Pack();
		}

		/// <summary>
		/// Calculates internal secret value, public key, and submits it.
		/// </summary>
		public string OnConfirmationReceived(string message)
		{
			if (message == null)
			{
				HasFailed = true;
				return null;
			}

			// Ensure confirmation is valid.
			PrimerExchanger primer = PrimerExchanger.Unpack(message);

			if (primer != null && primer.Equals(_initiator))
			{
				_state++;

				_secret = Math.Abs(_randomizer.Next(0xFF, 0xFF * 2));
				BigInteger publicKey = BigInteger.ModPow(primer.BaseValue, _secret, primer.PrimeValue);

				KeyExchanger keyExchanger = new KeyExchanger(publicKey);
				return keyExchanger.Pack();
			}
			else
			{
				HasFailed = true;
				return null;
			}
		}

		/// <summary>
		/// Calculates private key, stores it, ends transaction.
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		public string OnPublicKeyReceived(string message)
		{
			if (message == null)
			{
				HasFailed = true;
				return null;
			}

			// Check message integrity.
			KeyExchanger theirKey = KeyExchanger.Unpack(message);

			if (theirKey != null)
			{
				_state++;

				BigInteger publicKey = theirKey.PublicKey;
				BigInteger encryptionKey = BigInteger.ModPow(publicKey, _secret, _initiator.PrimeValue);

				_privateKey = encryptionKey.ToString("x2");

				return "end{status:\"OK\";}";
			}
			else
			{
				HasFailed = true;
				return null;
			}
		}

		/// <summary>
		/// Applies the internal private key to the Encryption key.
		/// </summary>
		public void Apply()
		{
			EncryptionKey = _privateKey;
			_privateKey = null;
		}

		/// <summary>
		/// Resets this initiator.
		/// </summary>
		public void Reset()
		{
			_state = ExchangeState.ShareStarter;
			EncryptionKey = null;
			_privateKey = null;
		}

		/// <summary>
		/// Sets this class type as initiator.
		/// </summary>
		public ExchangeInitiator() : base(ManagerRole.Initiator)
		{
			_state = ExchangeState.ShareStarter;
		}

		/// <summary>
		/// Whether or not the internal private key is ready to be applied.
		/// </summary>
		public bool ReadyToApply { get => _privateKey != null; }

		/// <summary>
		/// The current state of the transaction.
		/// </summary>
		private ExchangeState _state;
		private PrimerExchanger _initiator;
		private int _secret;
		private Random _randomizer;
		private string _privateKey;
	}
}
