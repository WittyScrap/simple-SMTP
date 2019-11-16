using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    /// <summary>
    /// A shell that can receive a stream of output whilst
    /// taking input asynchronously.
    /// </summary>
    abstract class AsyncShell : IShell
    {
        /// <summary>
        /// Defines the state of the shell.
        /// </summary>
        public enum ShellState
        {
            Closed = 0,
            Created = 1,
            Ready = 2
        }

        /// <summary>
        /// Creates the shell.
        /// </summary>
        /// <param name="width">Width of the shell window.</param>
        /// <param name="height">Height of the shell window.</param>
        public void Create(int width, int height)
        {
            if (_shellState != ShellState.Closed)
            {
                Logger.Message(Logger.WRN, "The shell has already been created, skipping...");
                return;
            }

            if (OnShellCreation(width, height))
            {
                _shellState = ShellState.Created;
            }
            else
            {
                Logger.Message(Logger.ERR, "The shell could not be created!");
            }
        }

        /// <summary>
        /// Sets the shell in the ready state.
        /// </summary>
        public void Run()
        {
            if (_shellState != ShellState.Created)
            {
                // Provide correct error/warning message.
                if (_shellState < ShellState.Created)
                {
                    Logger.Message(Logger.ERR, "The console cannot run before it's created!");
                }
                else if (_shellState > ShellState.Created)
                {
                    Logger.Message(Logger.WRN, "The console is already running, skipping...");
                }

                return;
            }

            if (OnShellInit())
            {
                _shellState = ShellState.Ready;
            }
            else
            {
                Logger.Message(Logger.ERR, "The shell could not be stated!");
            }
        }

        /// <summary>
        /// Quits the shell.
        /// </summary>
        public void Stop()
        {
			if (_shellState > ShellState.Closed)
			{
				OnShellDestruction();
			}
			else
			{
				Logger.Message(Logger.WRN, "The shell is not running, skipping...");
			}
        }

        /// <summary>
        /// Sends a command to be parsed by the shell.
        /// </summary>
        /// <param name="command">The command to parse and send.</param>
        public abstract void SendCommand(string command);

		/// <summary>
		/// Handles receiving text that should be shown to the console.
		/// </summary>
		/// <param name="output">The output that was received.</param>
		public abstract void Receive(string source, string output);

        /// <summary>
        /// The current state of the shell.
        /// </summary>
        /// <returns>An instance of the <see cref="ShellState"/>.</returns>
        public ShellState GetState()
        {
            return _shellState;
        }

        /// <summary>
        /// Called when the shell needs to be created.
        /// </summary>
        /// <param name="width">The width of the shell window.</param>
        /// <param name="height">The height of the shell window.</param>
        protected abstract bool OnShellCreation(int width, int height);

        /// <summary>
        /// Called when the shell needs to switch to its ready state.
        /// </summary>
        protected abstract bool OnShellInit();

		/// <summary>
		/// Called when the shell is about to be closed.
		/// </summary>
		protected abstract void OnShellDestruction();


        /* ------------ */
        /* --- Data --- */
        /* ------------ */


        // The current state of the shell.
        private ShellState _shellState;
    }
}
