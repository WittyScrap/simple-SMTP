using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

// Disambiguations
using Timer = System.Windows.Forms.Timer;

namespace Client
{
	/// <summary>
	/// An asynchronous shell system to better handle
	/// over-the-network connections. More specialised
	/// versions of this can be made for specific uses.
	/// </summary>
	class NetworkShell : AsyncShell
	{
		/// <summary>
		/// Sends a command to the remote host.
		/// </summary>
		/// <param name="command">The command to send to the remote host.</param>
		public override void SendCommand(string command)
		{
			if (!ParseCommand(command, out string header, out IReadOnlyDictionary<string, string> args))
			{
				string error;

				if (!string.IsNullOrEmpty(header))
				{
					error = "Invalid arguments detected.";
				}
				else
				{
					error = "No command detected, note that commands cannot be prefixed by - or --.";
				}

				Receive(entityUser, command);
				Receive(entityMachine, @"Error: \i " + error + @"\i0");
			}
			else
			{
				Receive(entityUser, GetCommandFormat(header, args));
			}
		}

		/// <summary>
		/// Handles receiving some output that needs to be displayed to the console.
		/// </summary>
		/// <param name="output">The output to be displayed.</param>
		public override void Receive(string source, string output)
		{
			string rtf = @"{\rtf1\ansi \b " + source + @"\b0: " + output + @"\line}";
			EnqueueCommand(() => _formShell.AppendOutput(rtf));
		}

		/// <summary>
		/// Event method called when the shell is being created.
		/// </summary>
		/// <param name="width">The width of the window.</param>
		/// <param name="height">The height of the window.</param>
		/// <returns>True if the window can be created successfully, false otherwise.</returns>
		protected override bool OnShellCreation(int width, int height)
		{
			_windowCommandQueue = new ConcurrentQueue<Action>();
			var consoleCreated = new ManualResetEvent(false);

			_formThread = new Thread(() =>
			{
				_formShell = new FormShell
				{
					Width = width,
					Height = height
				};

				_formShell.Show();
				_tickManager = new Timer();
				_tickManager.Interval = 10; // 10ms
				_tickManager.Tick += (s, e) => OnTick();
				_tickManager.Start();

				consoleCreated.Set();

				// Start main window loop
				Application.Run(_formShell);
			});

			_formThread.Start();
			consoleCreated.WaitOne();
			consoleCreated.Dispose();

			return true;
		}

		/// <summary>
		/// Event method invoked when the shell is initialised.
		/// </summary>
		/// <returns>True if the initialisation works as intended, false otherwise.</returns>
		protected override bool OnShellInit()
		{
			if (_formShell != null)
			{
				EnqueueCommand(() => _formShell.OnInput += SendCommand);
				EnqueueCommand(() => _formShell.SetActive(true));

				Receive(entityMachine, @"Console ready, type \i help \i0 for more information.");
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Handles destroying the internal window and closing the shell.
		/// </summary>
		protected override void OnShellDestruction()
		{
			if (_formShell != null && !_formShell.IsDisposed)
			{
				// Note: this will end the thread as well, as they are the only
				// processes running on it.
				EnqueueCommand(_tickManager.Stop);
				EnqueueCommand(_formShell.Close);
			}
		}

		/// <summary>
		/// Event invoked by the internal thread on each window tick.
		/// </summary>
		protected virtual void OnTick()
		{
			// Execute everything in the command queue.
			while (!_windowCommandQueue.IsEmpty)
			{
				Action nextAction;
				while (!_windowCommandQueue.TryDequeue(out nextAction));
				nextAction();
			}
		}

		/// <summary>
		/// Queues in the next command to execute by the
		/// form's working thread.
		/// </summary>
		/// <param name="action">The action to perform next.</param>
		protected void EnqueueCommand(Action action)
		{
			_windowCommandQueue.Enqueue(action);
		}

		/// <summary>
		/// Parses a command into a header and a set of keys and values
		/// for argument names and values.
		/// </summary>
		/// <param name="command">The source command.</param>
		/// <param name="header">The extrapolated header.</param>
		/// <param name="args">The argument list.</param>
		/// <returns>True if the command could be parsed, false if the command has errors.</returns>
		private bool ParseCommand(string command, out string header, out IReadOnlyDictionary<string, string> args)
		{
			header = "";
			args = null;
			string[] components = command.Split(' ');

			// Check for a header
			if (components.Length == 0 || components[0][0] == '-')
			{
				return false;
			}

			// Header detected!
			header = components[0];

			if (components.Length == 1) // No arguments...
			{
				// But we still found the header.
				return true;
			}

			components = components.Skip(1).ToArray();
			
			if (components.Length % 2 != 0) // Uneven argument count? Either an extra value or a missing key, error...
			{
				// The header will still be set, which will 
				// give us information on the error type.
				return false;
			}

			var argList = new Dictionary<string, string>();
			args = argList;

			// Key: every even index, will look for elements with patterns: '-a' or '-word_or_sentence'.
			// Value: every odd index, will look for raw values with no specific pattern.
			// Each indexer will advance by two to keep their parity.
			for (int key = 0, value = 1; value < components.Length; key += 2, value += 2)
			{
				string arg_key = components[key];
				string arg_val = components[value];

				if (!ExtractArgument(arg_key, out string arg_name))
				{
					return false;
				}

				argList[arg_name] = arg_val;
			}

			return true;
		}

		/// <summary>
		/// Extracts the argument name from a key in either format:
		/// -a
		/// -word_or_sentence.
		/// </summary>
		/// <param name="key">The raw key.</param>
		/// <param name="argName">The extracted name.</param>
		/// <returns>True if the key is in the correct format, false otherwise.</returns>
		private bool ExtractArgument(string key, out string argName)
		{
			argName = "";

			if (key[0] != '-' || key.Length > 2 && key[1] != '-')
			{
				return false;
			}

			if (key.Length > 2)
			{
				argName = key.Substring(2);
				return true;
			}
			else
			{
				argName = key[1].ToString();
				return true;
			}
		}

		/// <summary>
		/// Returns an RTF formatted string containing a clean representation
		/// of the raw command's header and arguments.
		/// </summary>
		/// <remarks>
		/// <paramref name="args"/> can be null, and if it is no arguments will
		/// be shown.
		/// </remarks>
		/// <param name="header">The header of the command.</param>
		/// <param name="args">The arguments to the command.</param>
		/// <returns>The formatted string.</returns>
		private string GetCommandFormat(string header, IReadOnlyDictionary<string, string> args)
		{
			// Format command for presentation.
			string commandFormat = @"\i " + header + @"\i0  ";

			if (args == null)
			{
				return commandFormat;
			}

			foreach (var arg in args)
			{
				string key = arg.Key;
				string val = arg.Value;

				if (key.Length > 1)
				{
					key = "--" + key;
				}
				else
				{
					key = "-" + key;
				}

				commandFormat += @"\i " + key + @"\i0  " + val + " ";
			}

			return commandFormat;
		}

		/* ------------ */
		/* --- Data --- */
		/* ------------ */

		// -- Shell management -- //

		private ConcurrentQueue<Action> _windowCommandQueue;
		private FormShell _formShell;
		private Thread _formThread;
		private Timer _tickManager;

		// -- Entities management -- //

		/// <summary>
		/// The local or remote machine.
		/// </summary>
		private string entityMachine => "local";

		/// <summary>
		/// User input.
		/// </summary>
		private string entityUser => "user";

		// -- Network management -- //


	}
}
