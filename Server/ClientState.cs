using System.Net.Sockets;

namespace Server
{
	/// <summary>
	/// Represents information about a connected client.
	/// </summary>
	internal class ClientState
	{
		/// <summary>
		/// The reference connection socket.
		/// </summary>
		public Socket Connection { get; }

		/// <summary>
		/// The read buffer.
		/// </summary>
		public byte[] ReadBuffer { get; }

		/// <summary>
		/// Creats a new client state.
		/// </summary>
		/// <param name="bufferSize">The agreed buffer size.</param>
		public ClientState(Socket connection, int bufferSize)
		{
			Connection = connection;
			ReadBuffer = new byte[bufferSize];
		}
	}
}
