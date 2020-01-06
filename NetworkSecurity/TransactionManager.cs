using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkSecurity
{
	/// <summary>
	/// Manages transactions in a secure manner and provides tools to handle a key exchange sequence.
	/// </summary>
	public abstract class TransactionManager
	{
		/// <summary>
		/// Lists the possible roles that a manager can have.
		/// 
		/// Initiator: sends out the initial p and g values;
		/// Listener: waits for the initial p and g values to be sent;
		/// 
		/// </summary>
		public enum ManagerRole
		{ 
			Initiator,
			Listener
		}

		/// <summary>
		/// Creates a new transaction manager.
		/// </summary>
		protected TransactionManager(ManagerRole role)
		{
			Role = role;
		}

		/// <summary>
		/// Evaluates an incoming message from a remote host to
		/// determine if the next action in the key agreement process
		/// can be made.
		/// </summary>
		public abstract string Evaluate(string message);

		/// <summary>
		/// The agreed encryption key.
		/// </summary>
		public string EncryptionKey { get; protected set; } = null;

		/// <summary>
		/// Whether or not this transaction manager has finished its operations.
		/// </summary>
		public virtual bool IsReady { get => EncryptionKey != null; }

		/// <summary>
		/// Whether or not the key exchange sequence has failed.
		/// </summary>
		public bool HasFailed { get; protected set; }

		/// <summary>
		/// The role of this manager.
		/// </summary>
		public ManagerRole Role { get; }
	}
}
