﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Threading.Tasks;

// Disambiguations
using Timer = System.Windows.Forms.Timer;


namespace Shell
{
	/// <summary>
	/// A form-based shell system to take inputs and register outputs simultaneously.
	/// </summary>
    public abstract partial class AsyncShell<TCommandSet> : Form, IShell where TCommandSet : ICommandSet, new()
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
		/// Delegate to define an input validation event.
		/// </summary>
		/// <param name="command">The command that has been typed in.</param>
		public delegate void InputEvent(string command);

		/// <summary>
		/// Event invoked when the command has been written and needs validation.
		/// </summary>
		public event InputEvent OnInput;

		/// <summary>
		/// Constructs a new form shell and initialises its internal
		/// components.
		/// </summary>
		public AsyncShell()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Sets whether or not this shell can be written to.
		/// </summary>
		/// <param name="value">True if the shell can be written to, false otherwise.</param>
		public void SetActive(bool value)
		{
			input.ReadOnly = !value;
		}

		/// <summary>
		/// Checks that this shell is active and can be written to.
		/// </summary>
		public bool IsActive => !input.ReadOnly;

		/// <summary>
		/// Appends RTF-formatted output to the output view.
		/// </summary>
		/// <param name="rtf">The RTF-formatted output.</param>
		public void AppendOutput(string rtf)
		{
			output.SelectedRtf = rtf;
		}

		/// <summary>
		/// Handles receiving text that should be shown to the console.
		/// </summary>
		/// <param name="output">The output that was received.</param>
		public virtual string OnOutputReceive(string source, string output)
		{
			return output;
		}

		/// <summary>
		/// Event method called when a command needs to be sent.
		/// </summary>
		/// <param name="command">The command to parse and send.</param>
		public virtual void OnCommandSend(string command, ParameterSet commandArgs)
		{
			if (_commandSet.CommandExists(command))
			{
				_commandSet[command].Execute(this, commandArgs);
			}
			else
			{
				Receive(entityMachine, @"Error:\i  Command \""" + command + @"\"" does not exist.\i0");
			}
		}

		/// <summary>
		/// The current state of the shell.
		/// </summary>
		/// <returns>An instance of the <see cref="ShellState"/>.</returns>
		public ShellState GetState()
		{
			return _shellState;
		}

		/// <summary>
		/// Sends a command to the remote host.
		/// </summary>
		/// <param name="command">The command to send to the remote host.</param>
		public async void SendCommand(string command)
		{
			if (!ParseCommand(command, out var parsedCommand))
			{
				string error;

				if (!string.IsNullOrEmpty(parsedCommand.header))
				{
					error = "Invalid format.";
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
				SetActive(false);
				Receive(entityUser, GetCommandFormat(parsedCommand.header, parsedCommand.args));
				Task commandTask = Task.Run(() => OnCommandSend(parsedCommand.header, parsedCommand.args));
				await commandTask;
				SetActive(true);
			}
		}

		/// <summary>
		/// Handles receiving some output that needs to be displayed to the console.
		/// </summary>
		/// <param name="output">The output to be displayed.</param>
		public void Receive(string source, string output)
		{
			output = OnOutputReceive(source, output);
			string rtf = @"{\rtf1\ansi {\colortbl ;\red255\green255\blue255;\red204\green204\blue204;\red0\green255\blue0;}\b "
						   + source + @"\b0: " + output + @"\line}";
			EnqueueCommand(() => AppendOutput(rtf));
		}

		/// <summary>
		/// Event method called when the shell is being created.
		/// </summary>
		/// <param name="width">The width of the window.</param>
		/// <param name="height">The height of the window.</param>
		/// <returns>True if the window can be created successfully, false otherwise.</returns>
		protected virtual bool OnShellCreation(int width, int height)
		{
			_windowCommandQueue = new ConcurrentQueue<Action>();
			_inputHistory = new LinkedList<string>();
			var consoleCreated = new ManualResetEvent(false);

			_formThread = new Thread(() =>
			{
				Show();
				_tickManager = new Timer();
				_tickManager.Interval = 10; // 10ms
				_tickManager.Tick += (s, e) => OnTick();
				_tickManager.Start();

				consoleCreated.Set();

				// Start main window loop
				Application.Run(this);
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
		protected virtual bool OnShellInit()
		{
			if (Visible)
			{
				_commandSet = new TCommandSet();
				EnqueueCommand(() => OnInput += SendCommand);
				EnqueueCommand(() => SetActive(true));

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
		protected virtual void OnShellDestruction()
		{
			if (!IsDisposed)
			{
				// Note: this will end the thread as well, as they are the only
				// processes running on it.
				EnqueueCommand(_tickManager.Stop);
				EnqueueCommand(Close);
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
				while (!_windowCommandQueue.TryDequeue(out nextAction)) ;
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
		private bool ParseCommand(string command, out (string header, ParameterSet args) parsedCommand)
		{
			parsedCommand.header = "";
			parsedCommand.args = null;
			string[] components = command.Split(' ');

			// Check for a header
			if (components.Length == 0 || components[0][0] == '-')
			{
				return false;
			}

			// Header detected!
			parsedCommand.header = components[0];

			if (components.Length == 1) // No arguments...
			{
				// But we still found the header.
				return true;
			}

			components = components.Skip(1).ToArray();
			parsedCommand.args = new ParameterSet();

			for (int arg = 0; arg < components.Length; ++arg)
			{
				string arg_key = components[arg];
				string arg_val = null;

				if (ExtractArgument(arg_key, out string arg_name))
				{
					if (arg + 1 < components.Length && !ExtractArgument(components[arg + 1], out string _))
					{
						arg_val = components[++arg];
					}

					parsedCommand.args.Add(new CommandArg(arg_name, arg_val));
				}
				else
				{
					return false;
				}
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

			if (key.Length == 0 || key[0] != '-' || key.Length == 3 && key[1] == '-' || key.Length > 2 && key[1] != '-')
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
		private string GetCommandFormat(string header, ParameterSet args)
		{
			// Format command for presentation.
			string commandFormat = @"\cf1\b " + header + @"\b0\cf3  ";

			if (args == null)
			{
				return commandFormat;
			}

			foreach (var arg in args)
			{
				string key = arg.Key;
				object val = arg.Value.Value;

				if (key.Length > 1)
				{
					key = "--" + key;
				}
				else
				{
					key = "-" + key;
				}

				commandFormat += @"\cf2\i " + key + @"\i0\cf3  ";

				if (val != null)
				{
					commandFormat += val.ToString() + " ";
				}
			}

			return commandFormat;
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
		/// Displays the help screen for the shell.
		/// </summary>
		public void DisplayHelp()
		{
			string format = @"\cf1\b Command List:\b0\cf3";

			foreach (ICommand command in _commandSet)
			{
				format += @"\line " + command.Help;
			}

			Receive(entityMachine, format);
		}

		/// <summary>
		/// Event triggered when any keyboard input is detected
		/// within the input text field.
		/// </summary>
		/// <param name="sender">The text field object.</param>
		/// <param name="e">Event data.</param>
		private void Input_KeyPressed(object sender, KeyPressEventArgs e)
		{
			if (e.KeyChar == (char)Keys.Return)
			{
				OnInput?.Invoke(input.Text);
				_inputHistory.AddLast(input.Text);
				_historyPlace = null;
				input.Text = "";
			}
		}

		/// <summary>
		/// Event triggered when a key has been pressed down.
		/// </summary>
		private void Input_KeyDown(object sender, KeyEventArgs e)
		{
			if (_inputHistory.Count == 0 || !IsActive)
			{
				return;
			}

			if (e.KeyCode == Keys.Up)
			{
				if (_historyPlace == null)
				{
					_historyPlace = _inputHistory.Last;
					input.Text = _historyPlace.Value;
				}
				else
				{
					if (_historyPlace.Previous != null)
					{
						_historyPlace = _historyPlace.Previous;
					}
					input.Text = _historyPlace.Value;
				}
			}

			if (e.KeyCode == Keys.Down)
			{
				if (_historyPlace == null)
				{
					return;
				}
				if (_historyPlace.Next != null)
				{
					_historyPlace = _historyPlace.Next;
				}
				input.Text = _historyPlace.Value;
			}
		}


		/* ------------ */
		/* --- Data --- */
		/* ------------ */

		// -- Shell management -- //

		private ConcurrentQueue<Action> _windowCommandQueue;
		private LinkedListNode<string> _historyPlace;
		private LinkedList<string> _inputHistory;
		private TCommandSet _commandSet;
		private ShellState _shellState;
		private Thread _formThread;
		private Timer _tickManager;

		// -- Entities management -- //

		/// <summary>
		/// The local machine.
		/// </summary>
		protected virtual string entityMachine => "local";

		/// <summary>
		/// The local or remote machine.
		/// </summary>
		protected virtual string entityHost => entityMachine;

		/// <summary>
		/// User input.
		/// </summary>
		protected virtual string entityUser => "user";
	}
}
