namespace IPPA
{
    partial class frmServer
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
            this.btnServerSwitch = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblServer = new System.Windows.Forms.Label();
            this.rtxtLog = new System.Windows.Forms.RichTextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnLoadTestModule = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.rtxtRequestDetails = new System.Windows.Forms.RichTextBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.lstQueue = new System.Windows.Forms.ListBox();
            this.btnExecute = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnServerSwitch
            // 
            this.btnServerSwitch.Location = new System.Drawing.Point(596, 24);
            this.btnServerSwitch.Name = "btnServerSwitch";
            this.btnServerSwitch.Size = new System.Drawing.Size(134, 38);
            this.btnServerSwitch.TabIndex = 0;
            this.btnServerSwitch.Text = "Start Server";
            this.btnServerSwitch.UseVisualStyleBackColor = true;
            this.btnServerSwitch.Click += new System.EventHandler(this.btnServerSwitch_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblServer);
            this.groupBox1.Controls.Add(this.btnServerSwitch);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(736, 122);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Server Description";
            // 
            // lblServer
            // 
            this.lblServer.AutoSize = true;
            this.lblServer.Location = new System.Drawing.Point(6, 24);
            this.lblServer.MaximumSize = new System.Drawing.Size(400, 80);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(201, 13);
            this.lblServer.TabIndex = 0;
            this.lblServer.Text = "Intelligent Path Planning Algorithm Server";
            // 
            // rtxtLog
            // 
            this.rtxtLog.Location = new System.Drawing.Point(6, 19);
            this.rtxtLog.Name = "rtxtLog";
            this.rtxtLog.Size = new System.Drawing.Size(584, 339);
            this.rtxtLog.TabIndex = 4;
            this.rtxtLog.TabStop = false;
            this.rtxtLog.Text = "";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnLoadTestModule);
            this.groupBox2.Controls.Add(this.btnClear);
            this.groupBox2.Controls.Add(this.rtxtLog);
            this.groupBox2.Location = new System.Drawing.Point(12, 401);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(736, 364);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Activity Log";
            // 
            // btnLoadTestModule
            // 
            this.btnLoadTestModule.Location = new System.Drawing.Point(596, 320);
            this.btnLoadTestModule.Name = "btnLoadTestModule";
            this.btnLoadTestModule.Size = new System.Drawing.Size(134, 38);
            this.btnLoadTestModule.TabIndex = 6;
            this.btnLoadTestModule.Text = "Load Test Module";
            this.btnLoadTestModule.UseVisualStyleBackColor = true;
            this.btnLoadTestModule.Click += new System.EventHandler(this.btnLoadTestModule_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(596, 19);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(134, 38);
            this.btnClear.TabIndex = 5;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnExecute);
            this.groupBox3.Controls.Add(this.rtxtRequestDetails);
            this.groupBox3.Controls.Add(this.btnDelete);
            this.groupBox3.Controls.Add(this.lstQueue);
            this.groupBox3.Location = new System.Drawing.Point(12, 140);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(736, 255);
            this.groupBox3.TabIndex = 1;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Path Planning Request Queues";
            // 
            // rtxtRequestDetails
            // 
            this.rtxtRequestDetails.Location = new System.Drawing.Point(214, 19);
            this.rtxtRequestDetails.Name = "rtxtRequestDetails";
            this.rtxtRequestDetails.ReadOnly = true;
            this.rtxtRequestDetails.Size = new System.Drawing.Size(376, 225);
            this.rtxtRequestDetails.TabIndex = 2;
            this.rtxtRequestDetails.TabStop = false;
            this.rtxtRequestDetails.Text = "";
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(596, 19);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(134, 38);
            this.btnDelete.TabIndex = 3;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // lstQueue
            // 
            this.lstQueue.FormattingEnabled = true;
            this.lstQueue.Location = new System.Drawing.Point(6, 19);
            this.lstQueue.Name = "lstQueue";
            this.lstQueue.Size = new System.Drawing.Size(201, 225);
            this.lstQueue.TabIndex = 1;
            this.lstQueue.Click += new System.EventHandler(this.lstQueue_Click);
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(596, 107);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(134, 106);
            this.btnExecute.TabIndex = 4;
            this.btnExecute.Text = "Process Queue";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // frmServer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(760, 777);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "frmServer";
            this.Text = "IPPA Control Panel";
            this.Load += new System.EventHandler(this.frmServer_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnServerSwitch;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label lblServer;
        private System.Windows.Forms.RichTextBox rtxtLog;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ListBox lstQueue;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.RichTextBox rtxtRequestDetails;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnLoadTestModule;
        private System.Windows.Forms.Button btnExecute;
    }
}

