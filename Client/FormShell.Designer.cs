namespace Client
{
    partial class FormShell
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.output = new System.Windows.Forms.RichTextBox();
			this.input = new System.Windows.Forms.TextBox();
			this.commandLabel = new System.Windows.Forms.Label();
			this.outputBody = new System.Windows.Forms.Panel();
			this.outputBody.SuspendLayout();
			this.SuspendLayout();
			// 
			// output
			// 
			this.output.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.output.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.output.Dock = System.Windows.Forms.DockStyle.Fill;
			this.output.Location = new System.Drawing.Point(0, 0);
			this.output.Margin = new System.Windows.Forms.Padding(4);
			this.output.Name = "output";
			this.output.ReadOnly = true;
			this.output.Size = new System.Drawing.Size(761, 469);
			this.output.TabIndex = 0;
			this.output.Text = "";
			// 
			// input
			// 
			this.input.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.input.Location = new System.Drawing.Point(93, 487);
			this.input.Name = "input";
			this.input.ReadOnly = true;
			this.input.Size = new System.Drawing.Size(680, 22);
			this.input.TabIndex = 1;
			this.input.WordWrap = false;
			this.input.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Input_KeyPressed);
			// 
			// commandLabel
			// 
			this.commandLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.commandLabel.AutoSize = true;
			this.commandLabel.Location = new System.Drawing.Point(12, 490);
			this.commandLabel.Name = "commandLabel";
			this.commandLabel.Size = new System.Drawing.Size(75, 17);
			this.commandLabel.TabIndex = 2;
			this.commandLabel.Text = "Command:";
			// 
			// outputBody
			// 
			this.outputBody.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.outputBody.Controls.Add(this.output);
			this.outputBody.Location = new System.Drawing.Point(12, 12);
			this.outputBody.Name = "outputBody";
			this.outputBody.Size = new System.Drawing.Size(761, 469);
			this.outputBody.TabIndex = 3;
			// 
			// FormShell
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(785, 518);
			this.Controls.Add(this.commandLabel);
			this.Controls.Add(this.input);
			this.Controls.Add(this.outputBody);
			this.Margin = new System.Windows.Forms.Padding(4);
			this.Name = "FormShell";
			this.Text = "Shell";
			this.outputBody.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox output;
		private System.Windows.Forms.TextBox input;
		private System.Windows.Forms.Label commandLabel;
		private System.Windows.Forms.Panel outputBody;
	}
}