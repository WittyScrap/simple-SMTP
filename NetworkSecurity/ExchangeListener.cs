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
	/// Manages the receiving end of the key exchange process.
	/// </summary>
	public class ExchangeListener : TransactionManager
	{
		/// <summary>
		/// Defines which state the exchange is in.
		/// </summary>
		enum ExchangeState
		{
			AwaitStarter,
			AwaitPublicKey,
			AwaitConfirmation,
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
				case ExchangeState.AwaitStarter:
					return OnStarterReceived(message);

				case ExchangeState.AwaitPublicKey:
					return OnPublicKeyReceived(message);

				case ExchangeState.AwaitConfirmation:
					return OnConfirmationReceived(message);

				case ExchangeState.Complete:
					return null; // But don't fail.

				default:
					HasFailed = true;
					return null;
			}
		}

		/// <summary>
		/// Calculates internal secret value, public key, and submits it.
		/// </summary>
		public string OnStarterReceived(string message)
		{
			if (message == null)
			{
				HasFailed = true;
				return null;
			}

			// Ensure confirmation is valid.
			PrimerExchanger primer = PrimerExchanger.Unpack(message);

			if (primer != null)
			{
				_state++;
				_initiator = primer;

				return message; // Confirm.
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

				_secret = Math.Abs(_randomizer.Next(0xFF, 0xFF + 0xFF));
				BigInteger publicKey = BigInteger.ModPow(_initiator.BaseValue, _secret, _initiator.PrimeValue);

				KeyExchanger keyExchanger = new KeyExchanger(publicKey);
				string response = keyExchanger.Pack();

				BigInteger theirPublicKey = theirKey.PublicKey;
				BigInteger encryptionKey = BigInteger.ModPow(theirPublicKey, _secret, _initiator.PrimeValue);

				_privateKey = encryptionKey.ToString("x2");

				return response;
			}
			else
			{
				HasFailed = true;
			}

			return null;
		}

		/// <summary>
		/// Ensures that a confirmation has been received.
		/// </summary>
		private string OnConfirmationReceived(string message)
		{
			if (message == null)
			{
				HasFailed = true;
				return null;
			}

			Variables ok;

			try
			{
				ok = Variables.Parse(message);
				
				if (ok.Get<string>("end.status") == "OK")
				{
					EncryptionKey = _privateKey;
					return null;
				}
				else
				{
					HasFailed = true;
					return null;
				}
			}
			catch (Exception)
			{
				HasFailed = true;
				return null;
			}
		}

		/// <summary>
		/// Sets this class type as initiator.
		/// </summary>
		public ExchangeListener() : base(ManagerRole.Listener)
		{
			_state = ExchangeState.AwaitStarter;
			_randomizer = new Random((int)DateTime.Now.Ticks);
		}

		/// <summary>
		/// The current state of the transaction.
		/// </summary>
		private ExchangeState _state;
		private int _secret;
		private Random _randomizer;
		private PrimerExchanger _initiator;
		private string _privateKey;
	}
}
