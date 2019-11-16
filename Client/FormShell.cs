using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
	/// <summary>
	/// A form-based shell system to take inputs and register outputs simultaneously.
	/// </summary>
    public partial class FormShell : Form
    {
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
		/// Sets whether or not this shell can be written to.
		/// </summary>
		/// <param name="value">True if the shell can be written to, false otherwise.</param>
		public void SetActive(bool value)
		{
			input.ReadOnly = !value;
		}

		/// <summary>
		/// Appends RTF-formatted output to the output view.
		/// </summary>
		/// <param name="rtf">The RTF-formatted output.</param>
		public void AppendOutput(string rtf)
		{
			output.SelectedRtf = rtf;
		}

		/// <summary>
		/// Constructs a new form shell and initialises its internal
		/// components.
		/// </summary>
        public FormShell()
        {
            InitializeComponent();
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
				input.Text = "";
			}
		}
	}
}
