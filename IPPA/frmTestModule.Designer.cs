namespace IPPA
{
    partial class frmTestModule
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
            this.btnLoad1 = new System.Windows.Forms.Button();
            this.btnBrowse1 = new System.Windows.Forms.Button();
            this.txtFileName1 = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.chkUseDist = new System.Windows.Forms.CheckBox();
            this.chkParallel = new System.Windows.Forms.CheckBox();
            this.chkCoaseToFine = new System.Windows.Forms.CheckBox();
            this.chkUseDiff = new System.Windows.Forms.CheckBox();
            this.gboxDetectionType = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.ntxtDetectionRate = new System.Windows.Forms.NumericUpDown();
            this.rbtnFixedPercent = new System.Windows.Forms.RadioButton();
            this.rbtnFixedAmountPercent = new System.Windows.Forms.RadioButton();
            this.rbtnFixedAmount = new System.Windows.Forms.RadioButton();
            this.gboxUAVType = new System.Windows.Forms.GroupBox();
            this.rbtnCopter = new System.Windows.Forms.RadioButton();
            this.rbtnFixWing = new System.Windows.Forms.RadioButton();
            this.chkUseEndPoint = new System.Windows.Forms.CheckBox();
            this.ntxtEY = new System.Windows.Forms.NumericUpDown();
            this.ntxtEX = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.btnView = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.ntxtFlightTime = new System.Windows.Forms.NumericUpDown();
            this.trbFlightTime = new System.Windows.Forms.TrackBar();
            this.ntxtSY = new System.Windows.Forms.NumericUpDown();
            this.ntxtSX = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtFileName2 = new System.Windows.Forms.TextBox();
            this.btnBrowse2 = new System.Windows.Forms.Button();
            this.btnLoad2 = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.chkShowPath = new System.Windows.Forms.CheckBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.ntxtRunTimes = new System.Windows.Forms.NumericUpDown();
            this.chkBatchRun = new System.Windows.Forms.CheckBox();
            this.btnExecute = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.lvQueue = new System.Windows.Forms.ListView();
            this.lstAlg = new System.Windows.Forms.ListBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.btnTest = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.rtxtLog = new System.Windows.Forms.RichTextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.gboxDetectionType.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ntxtDetectionRate)).BeginInit();
            this.gboxUAVType.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ntxtEY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ntxtEX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ntxtFlightTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trbFlightTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ntxtSY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ntxtSX)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ntxtRunTimes)).BeginInit();
            this.groupBox5.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnLoad1
            // 
            this.btnLoad1.Location = new System.Drawing.Point(540, 32);
            this.btnLoad1.Name = "btnLoad1";
            this.btnLoad1.Size = new System.Drawing.Size(75, 23);
            this.btnLoad1.TabIndex = 5;
            this.btnLoad1.Text = "&Load Map";
            this.btnLoad1.UseVisualStyleBackColor = true;
            this.btnLoad1.Click += new System.EventHandler(this.btnLoad1_Click);
            // 
            // btnBrowse1
            // 
            this.btnBrowse1.Location = new System.Drawing.Point(458, 32);
            this.btnBrowse1.Name = "btnBrowse1";
            this.btnBrowse1.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse1.TabIndex = 4;
            this.btnBrowse1.Text = "Bro&wse";
            this.btnBrowse1.UseVisualStyleBackColor = true;
            this.btnBrowse1.Click += new System.EventHandler(this.btnBrowse1_Click);
            // 
            // txtFileName1
            // 
            this.txtFileName1.Location = new System.Drawing.Point(10, 32);
            this.txtFileName1.Name = "txtFileName1";
            this.txtFileName1.Size = new System.Drawing.Size(442, 20);
            this.txtFileName1.TabIndex = 3;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.groupBox6);
            this.groupBox1.Controls.Add(this.gboxDetectionType);
            this.groupBox1.Controls.Add(this.gboxUAVType);
            this.groupBox1.Location = new System.Drawing.Point(12, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(626, 139);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.chkUseDist);
            this.groupBox6.Controls.Add(this.chkParallel);
            this.groupBox6.Controls.Add(this.chkCoaseToFine);
            this.groupBox6.Controls.Add(this.chkUseDiff);
            this.groupBox6.Location = new System.Drawing.Point(6, 10);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(216, 122);
            this.groupBox6.TabIndex = 23;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Options";
            // 
            // chkUseDist
            // 
            this.chkUseDist.AutoSize = true;
            this.chkUseDist.Location = new System.Drawing.Point(6, 19);
            this.chkUseDist.Name = "chkUseDist";
            this.chkUseDist.Size = new System.Drawing.Size(176, 17);
            this.chkUseDist.TabIndex = 15;
            this.chkUseDist.Text = "Load probability distribution map";
            this.chkUseDist.UseVisualStyleBackColor = true;
            // 
            // chkParallel
            // 
            this.chkParallel.AutoSize = true;
            this.chkParallel.Location = new System.Drawing.Point(6, 88);
            this.chkParallel.Name = "chkParallel";
            this.chkParallel.Size = new System.Drawing.Size(149, 17);
            this.chkParallel.TabIndex = 22;
            this.chkParallel.Text = "Enable parallel processing";
            this.chkParallel.UseVisualStyleBackColor = true;
            // 
            // chkCoaseToFine
            // 
            this.chkCoaseToFine.AutoSize = true;
            this.chkCoaseToFine.Location = new System.Drawing.Point(6, 65);
            this.chkCoaseToFine.Name = "chkCoaseToFine";
            this.chkCoaseToFine.Size = new System.Drawing.Size(161, 17);
            this.chkCoaseToFine.TabIndex = 21;
            this.chkCoaseToFine.Text = "Enable coarse-to-fine search";
            this.chkCoaseToFine.UseVisualStyleBackColor = true;
            // 
            // chkUseDiff
            // 
            this.chkUseDiff.AutoSize = true;
            this.chkUseDiff.Location = new System.Drawing.Point(6, 42);
            this.chkUseDiff.Name = "chkUseDiff";
            this.chkUseDiff.Size = new System.Drawing.Size(137, 17);
            this.chkUseDiff.TabIndex = 16;
            this.chkUseDiff.Text = "Load task-difficulty map";
            this.chkUseDiff.UseVisualStyleBackColor = true;
            // 
            // gboxDetectionType
            // 
            this.gboxDetectionType.Controls.Add(this.label10);
            this.gboxDetectionType.Controls.Add(this.ntxtDetectionRate);
            this.gboxDetectionType.Controls.Add(this.rbtnFixedPercent);
            this.gboxDetectionType.Controls.Add(this.rbtnFixedAmountPercent);
            this.gboxDetectionType.Controls.Add(this.rbtnFixedAmount);
            this.gboxDetectionType.Location = new System.Drawing.Point(351, 9);
            this.gboxDetectionType.Name = "gboxDetectionType";
            this.gboxDetectionType.Size = new System.Drawing.Size(264, 123);
            this.gboxDetectionType.TabIndex = 19;
            this.gboxDetectionType.TabStop = false;
            this.gboxDetectionType.Text = "Detection Type";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 96);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(82, 13);
            this.label10.TabIndex = 21;
            this.label10.Text = "Detection Rate:";
            // 
            // ntxtDetectionRate
            // 
            this.ntxtDetectionRate.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.ntxtDetectionRate.Location = new System.Drawing.Point(96, 94);
            this.ntxtDetectionRate.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.ntxtDetectionRate.Name = "ntxtDetectionRate";
            this.ntxtDetectionRate.Size = new System.Drawing.Size(73, 20);
            this.ntxtDetectionRate.TabIndex = 20;
            this.ntxtDetectionRate.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // rbtnFixedPercent
            // 
            this.rbtnFixedPercent.AutoSize = true;
            this.rbtnFixedPercent.Location = new System.Drawing.Point(6, 64);
            this.rbtnFixedPercent.Name = "rbtnFixedPercent";
            this.rbtnFixedPercent.Size = new System.Drawing.Size(220, 17);
            this.rbtnFixedPercent.TabIndex = 19;
            this.rbtnFixedPercent.TabStop = true;
            this.rbtnFixedPercent.Text = "Fixed percentage (e.g., 50% of remaining)";
            this.rbtnFixedPercent.UseVisualStyleBackColor = true;
            // 
            // rbtnFixedAmountPercent
            // 
            this.rbtnFixedAmountPercent.AutoSize = true;
            this.rbtnFixedAmountPercent.Location = new System.Drawing.Point(6, 41);
            this.rbtnFixedAmountPercent.Name = "rbtnFixedAmountPercent";
            this.rbtnFixedAmountPercent.Size = new System.Drawing.Size(244, 17);
            this.rbtnFixedAmountPercent.TabIndex = 18;
            this.rbtnFixedAmountPercent.TabStop = true;
            this.rbtnFixedAmountPercent.Text = "Fixed amount in percentage (e.g., 25% always)";
            this.rbtnFixedAmountPercent.UseVisualStyleBackColor = true;
            // 
            // rbtnFixedAmount
            // 
            this.rbtnFixedAmount.AutoSize = true;
            this.rbtnFixedAmount.Location = new System.Drawing.Point(6, 19);
            this.rbtnFixedAmount.Name = "rbtnFixedAmount";
            this.rbtnFixedAmount.Size = new System.Drawing.Size(147, 17);
            this.rbtnFixedAmount.TabIndex = 17;
            this.rbtnFixedAmount.TabStop = true;
            this.rbtnFixedAmount.Text = "Fixed amount (e.g., 5 unit)";
            this.rbtnFixedAmount.UseVisualStyleBackColor = true;
            // 
            // gboxUAVType
            // 
            this.gboxUAVType.Controls.Add(this.rbtnCopter);
            this.gboxUAVType.Controls.Add(this.rbtnFixWing);
            this.gboxUAVType.Location = new System.Drawing.Point(238, 9);
            this.gboxUAVType.Name = "gboxUAVType";
            this.gboxUAVType.Size = new System.Drawing.Size(96, 123);
            this.gboxUAVType.TabIndex = 18;
            this.gboxUAVType.TabStop = false;
            this.gboxUAVType.Text = "UAV Type";
            // 
            // rbtnCopter
            // 
            this.rbtnCopter.AutoSize = true;
            this.rbtnCopter.Location = new System.Drawing.Point(6, 41);
            this.rbtnCopter.Name = "rbtnCopter";
            this.rbtnCopter.Size = new System.Drawing.Size(56, 17);
            this.rbtnCopter.TabIndex = 18;
            this.rbtnCopter.TabStop = true;
            this.rbtnCopter.Text = "Copter";
            this.rbtnCopter.UseVisualStyleBackColor = true;
            // 
            // rbtnFixWing
            // 
            this.rbtnFixWing.AutoSize = true;
            this.rbtnFixWing.Location = new System.Drawing.Point(6, 19);
            this.rbtnFixWing.Name = "rbtnFixWing";
            this.rbtnFixWing.Size = new System.Drawing.Size(66, 17);
            this.rbtnFixWing.TabIndex = 17;
            this.rbtnFixWing.TabStop = true;
            this.rbtnFixWing.Text = "Fix-Wing";
            this.rbtnFixWing.UseVisualStyleBackColor = true;
            // 
            // chkUseEndPoint
            // 
            this.chkUseEndPoint.AutoSize = true;
            this.chkUseEndPoint.Location = new System.Drawing.Point(16, 65);
            this.chkUseEndPoint.Name = "chkUseEndPoint";
            this.chkUseEndPoint.Size = new System.Drawing.Size(160, 17);
            this.chkUseEndPoint.TabIndex = 3;
            this.chkUseEndPoint.Text = "Set ending point for the path";
            this.chkUseEndPoint.UseVisualStyleBackColor = true;
            // 
            // ntxtEY
            // 
            this.ntxtEY.Location = new System.Drawing.Point(572, 52);
            this.ntxtEY.Name = "ntxtEY";
            this.ntxtEY.Size = new System.Drawing.Size(43, 20);
            this.ntxtEY.TabIndex = 13;
            this.ntxtEY.ValueChanged += new System.EventHandler(this.ntxtEY_ValueChanged);
            // 
            // ntxtEX
            // 
            this.ntxtEX.Location = new System.Drawing.Point(505, 52);
            this.ntxtEX.Name = "ntxtEX";
            this.ntxtEX.Size = new System.Drawing.Size(43, 20);
            this.ntxtEX.TabIndex = 11;
            this.ntxtEX.ValueChanged += new System.EventHandler(this.ntxtEX_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(554, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(12, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "y";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(487, 54);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(12, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "x";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(411, 54);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(70, 13);
            this.label6.TabIndex = 9;
            this.label6.Text = "Ending Point:";
            // 
            // btnView
            // 
            this.btnView.Location = new System.Drawing.Point(474, 82);
            this.btnView.Name = "btnView";
            this.btnView.Size = new System.Drawing.Size(143, 23);
            this.btnView.TabIndex = 14;
            this.btnView.Text = "View Reachable Area";
            this.btnView.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(11, 24);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(140, 13);
            this.label7.TabIndex = 0;
            this.label7.Text = "Flight Duration (in time step):";
            // 
            // ntxtFlightTime
            // 
            this.ntxtFlightTime.Location = new System.Drawing.Point(351, 22);
            this.ntxtFlightTime.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.ntxtFlightTime.Name = "ntxtFlightTime";
            this.ntxtFlightTime.Size = new System.Drawing.Size(52, 20);
            this.ntxtFlightTime.TabIndex = 2;
            this.ntxtFlightTime.ValueChanged += new System.EventHandler(this.ntxtFlightTime_ValueChanged);
            // 
            // trbFlightTime
            // 
            this.trbFlightTime.Location = new System.Drawing.Point(157, 22);
            this.trbFlightTime.Maximum = 30;
            this.trbFlightTime.Name = "trbFlightTime";
            this.trbFlightTime.Size = new System.Drawing.Size(188, 45);
            this.trbFlightTime.TabIndex = 1;
            this.trbFlightTime.TickStyle = System.Windows.Forms.TickStyle.None;
            this.trbFlightTime.Scroll += new System.EventHandler(this.trbFlightTime_Scroll);
            // 
            // ntxtSY
            // 
            this.ntxtSY.Location = new System.Drawing.Point(572, 22);
            this.ntxtSY.Name = "ntxtSY";
            this.ntxtSY.Size = new System.Drawing.Size(43, 20);
            this.ntxtSY.TabIndex = 8;
            this.ntxtSY.ValueChanged += new System.EventHandler(this.ntxtSY_ValueChanged);
            // 
            // ntxtSX
            // 
            this.ntxtSX.Location = new System.Drawing.Point(505, 22);
            this.ntxtSX.Name = "ntxtSX";
            this.ntxtSX.Size = new System.Drawing.Size(43, 20);
            this.ntxtSX.TabIndex = 6;
            this.ntxtSX.ValueChanged += new System.EventHandler(this.ntxtSX_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(554, 24);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(12, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "y";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(487, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(12, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "x";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(411, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Starting Point:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.txtFileName2);
            this.groupBox2.Controls.Add(this.btnBrowse2);
            this.groupBox2.Controls.Add(this.btnLoad2);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.txtFileName1);
            this.groupBox2.Controls.Add(this.btnBrowse1);
            this.groupBox2.Controls.Add(this.btnLoad1);
            this.groupBox2.Location = new System.Drawing.Point(12, 140);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(626, 123);
            this.groupBox2.TabIndex = 7;
            this.groupBox2.TabStop = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(9, 65);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(164, 13);
            this.label9.TabIndex = 10;
            this.label9.Text = "Specify task-difficulty map to use:";
            // 
            // txtFileName2
            // 
            this.txtFileName2.Location = new System.Drawing.Point(12, 81);
            this.txtFileName2.Name = "txtFileName2";
            this.txtFileName2.Size = new System.Drawing.Size(442, 20);
            this.txtFileName2.TabIndex = 7;
            // 
            // btnBrowse2
            // 
            this.btnBrowse2.Location = new System.Drawing.Point(460, 81);
            this.btnBrowse2.Name = "btnBrowse2";
            this.btnBrowse2.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse2.TabIndex = 8;
            this.btnBrowse2.Text = "Bro&wse";
            this.btnBrowse2.UseVisualStyleBackColor = true;
            // 
            // btnLoad2
            // 
            this.btnLoad2.Location = new System.Drawing.Point(542, 81);
            this.btnLoad2.Name = "btnLoad2";
            this.btnLoad2.Size = new System.Drawing.Size(75, 23);
            this.btnLoad2.TabIndex = 9;
            this.btnLoad2.Text = "&Load Map";
            this.btnLoad2.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 16);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(203, 13);
            this.label8.TabIndex = 6;
            this.label8.Text = "Specify probability distribution map to use:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.chkShowPath);
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.chkUseEndPoint);
            this.groupBox3.Controls.Add(this.ntxtSX);
            this.groupBox3.Controls.Add(this.ntxtEY);
            this.groupBox3.Controls.Add(this.ntxtSY);
            this.groupBox3.Controls.Add(this.ntxtEX);
            this.groupBox3.Controls.Add(this.trbFlightTime);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.ntxtFlightTime);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.btnView);
            this.groupBox3.Location = new System.Drawing.Point(12, 264);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(626, 120);
            this.groupBox3.TabIndex = 8;
            this.groupBox3.TabStop = false;
            // 
            // chkShowPath
            // 
            this.chkShowPath.AutoSize = true;
            this.chkShowPath.Location = new System.Drawing.Point(16, 88);
            this.chkShowPath.Name = "chkShowPath";
            this.chkShowPath.Size = new System.Drawing.Size(196, 17);
            this.chkShowPath.TabIndex = 15;
            this.chkShowPath.Text = "Draw path planned for each request";
            this.chkShowPath.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.ntxtRunTimes);
            this.groupBox4.Controls.Add(this.chkBatchRun);
            this.groupBox4.Controls.Add(this.btnExecute);
            this.groupBox4.Controls.Add(this.btnRemove);
            this.groupBox4.Controls.Add(this.btnAdd);
            this.groupBox4.Controls.Add(this.lvQueue);
            this.groupBox4.Controls.Add(this.lstAlg);
            this.groupBox4.Location = new System.Drawing.Point(12, 390);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(626, 124);
            this.groupBox4.TabIndex = 9;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Path Planning Algorithms";
            // 
            // ntxtRunTimes
            // 
            this.ntxtRunTimes.Location = new System.Drawing.Point(563, 19);
            this.ntxtRunTimes.Maximum = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.ntxtRunTimes.Name = "ntxtRunTimes";
            this.ntxtRunTimes.Size = new System.Drawing.Size(52, 20);
            this.ntxtRunTimes.TabIndex = 6;
            this.ntxtRunTimes.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // chkBatchRun
            // 
            this.chkBatchRun.AutoSize = true;
            this.chkBatchRun.Location = new System.Drawing.Point(474, 19);
            this.chkBatchRun.Name = "chkBatchRun";
            this.chkBatchRun.Size = new System.Drawing.Size(72, 17);
            this.chkBatchRun.TabIndex = 5;
            this.chkBatchRun.Text = "Batch run";
            this.chkBatchRun.UseVisualStyleBackColor = true;
            // 
            // btnExecute
            // 
            this.btnExecute.Location = new System.Drawing.Point(473, 45);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(144, 69);
            this.btnExecute.TabIndex = 4;
            this.btnExecute.Text = "Plan Flight Path(s)";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(203, 70);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(60, 23);
            this.btnRemove.TabIndex = 2;
            this.btnRemove.Text = "&Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(203, 41);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(60, 23);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "&Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // lvQueue
            // 
            this.lvQueue.GridLines = true;
            this.lvQueue.Location = new System.Drawing.Point(269, 19);
            this.lvQueue.Name = "lvQueue";
            this.lvQueue.Size = new System.Drawing.Size(183, 95);
            this.lvQueue.TabIndex = 3;
            this.lvQueue.UseCompatibleStateImageBehavior = false;
            this.lvQueue.DoubleClick += new System.EventHandler(this.lvQueue_DoubleClick);
            // 
            // lstAlg
            // 
            this.lstAlg.FormattingEnabled = true;
            this.lstAlg.Location = new System.Drawing.Point(9, 19);
            this.lstAlg.Name = "lstAlg";
            this.lstAlg.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstAlg.Size = new System.Drawing.Size(188, 95);
            this.lstAlg.TabIndex = 0;
            this.lstAlg.DoubleClick += new System.EventHandler(this.lstAlg_DoubleClick);
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.btnTest);
            this.groupBox5.Controls.Add(this.btnClear);
            this.groupBox5.Controls.Add(this.rtxtLog);
            this.groupBox5.Location = new System.Drawing.Point(12, 520);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(626, 227);
            this.groupBox5.TabIndex = 10;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Path Planning Log";
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(474, 138);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(143, 83);
            this.btnTest.TabIndex = 4;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(474, 17);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(143, 23);
            this.btnClear.TabIndex = 3;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // rtxtLog
            // 
            this.rtxtLog.Location = new System.Drawing.Point(9, 19);
            this.rtxtLog.Name = "rtxtLog";
            this.rtxtLog.Size = new System.Drawing.Size(445, 202);
            this.rtxtLog.TabIndex = 0;
            this.rtxtLog.Text = "";
            // 
            // frmTestModule
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(652, 758);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "frmTestModule";
            this.Text = "Test Module";
            this.Load += new System.EventHandler(this.frmTestModule_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.gboxDetectionType.ResumeLayout(false);
            this.gboxDetectionType.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ntxtDetectionRate)).EndInit();
            this.gboxUAVType.ResumeLayout(false);
            this.gboxUAVType.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ntxtEY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ntxtEX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ntxtFlightTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trbFlightTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ntxtSY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ntxtSX)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ntxtRunTimes)).EndInit();
            this.groupBox5.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnLoad1;
        private System.Windows.Forms.Button btnBrowse1;
        private System.Windows.Forms.TextBox txtFileName1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkUseEndPoint;
        private System.Windows.Forms.NumericUpDown ntxtEY;
        private System.Windows.Forms.NumericUpDown ntxtEX;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnView;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown ntxtFlightTime;
        private System.Windows.Forms.TrackBar trbFlightTime;
        private System.Windows.Forms.NumericUpDown ntxtSY;
        private System.Windows.Forms.NumericUpDown ntxtSX;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkUseDist;
        private System.Windows.Forms.CheckBox chkUseDiff;
        private System.Windows.Forms.GroupBox gboxDetectionType;
        private System.Windows.Forms.RadioButton rbtnFixedPercent;
        private System.Windows.Forms.RadioButton rbtnFixedAmountPercent;
        private System.Windows.Forms.RadioButton rbtnFixedAmount;
        private System.Windows.Forms.GroupBox gboxUAVType;
        private System.Windows.Forms.RadioButton rbtnCopter;
        private System.Windows.Forms.RadioButton rbtnFixWing;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtFileName2;
        private System.Windows.Forms.Button btnBrowse2;
        private System.Windows.Forms.Button btnLoad2;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.ListView lvQueue;
        private System.Windows.Forms.ListBox lstAlg;
        private System.Windows.Forms.CheckBox chkShowPath;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.RichTextBox rtxtLog;
        private System.Windows.Forms.CheckBox chkParallel;
        private System.Windows.Forms.CheckBox chkCoaseToFine;
        private System.Windows.Forms.NumericUpDown ntxtRunTimes;
        private System.Windows.Forms.CheckBox chkBatchRun;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown ntxtDetectionRate;
    }
}