namespace TCPIPTest
{
    partial class Form1
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
            this.txtSendText = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.rtxtReceiveText = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // txtSendText
            // 
            this.txtSendText.Location = new System.Drawing.Point(24, 13);
            this.txtSendText.Name = "txtSendText";
            this.txtSendText.Size = new System.Drawing.Size(412, 20);
            this.txtSendText.TabIndex = 0;
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(455, 13);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 23);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // rtxtReceiveText
            // 
            this.rtxtReceiveText.Location = new System.Drawing.Point(24, 51);
            this.rtxtReceiveText.Name = "rtxtReceiveText";
            this.rtxtReceiveText.Size = new System.Drawing.Size(412, 380);
            this.rtxtReceiveText.TabIndex = 2;
            this.rtxtReceiveText.Text = "";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(545, 451);
            this.Controls.Add(this.rtxtReceiveText);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.txtSendText);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtSendText;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.RichTextBox rtxtReceiveText;
    }
}

