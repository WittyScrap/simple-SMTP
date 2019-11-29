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
    /// Form to freely allow entering multiline text.
    /// </summary>
    public partial class Multiline : Form
    {
        /// <summary>
        /// The value of this form.
        /// </summary>
        public string Value
        {
            get
            {
                return messageBox.Text;
            }
        }

        /// <summary>
        /// Allows freely entering multiline text.
        /// </summary>
        public Multiline()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Sends all text in the source box.
        /// </summary>
        private void SendButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
