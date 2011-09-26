using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using rtwmatrix;
using System.Runtime.InteropServices;

namespace IPPA
{
    public partial class frmTestModule : Form
    {
        #region Members
        
        //WinAPI-Declaration for SendMessage
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(
        IntPtr window, int message, int wparam, int lparam);
        const int WM_VSCROLL = 0x115;
        const int SB_BOTTOM = 7;


        private RtwMatrix CurDistMap;
        private RtwMatrix CurDiffMap;
        private frmServer frmParent;
        private frmMap map;

        #endregion

        #region Constructor, Destructor

        // Constructor
        public frmTestModule(frmServer _frmParent)
        {
            frmParent = _frmParent;
            InitializeComponent();
        }

        // Destructor
        ~frmTestModule()
        {
            // Cleaning up
            CurDistMap = null;
            map = null;
        }

        #endregion

        #region Message Handlers

        // When the form is loading
        private void frmTestModule_Load(object sender, EventArgs e)
        {
            chkUseDist.Checked = true;
            chkUseDiff.Checked = false;
            chkCoaseToFine.Checked = false;
            chkParallel.Checked = false;
            rbtnFixWing.Checked = true;
            rbtnCopter.Checked = false;
            rbtnFixedAmount.Checked = false;
            rbtnFixedAmountPercent.Checked = false;
            rbtnFixedPercent.Checked = true;
            ntxtDetectionRate.Value = 1;

            txtFileName1.Text = ProjectConstants.MapsDir + @"\DistMaps\5_modal.csv";
            txtFileName2.Text = ProjectConstants.MapsDir + @"\DiffMaps\test.csv";

            ntxtSX.Minimum = 0;
            ntxtSX.Maximum = ProjectConstants.DefaultDimension - 1;
            ntxtSY.Minimum = 0;
            ntxtSY.Maximum = ProjectConstants.DefaultDimension - 1;
            trbFlightTime.Minimum = 0;
            trbFlightTime.Maximum = ProjectConstants.MaxFlightTime;
            trbFlightTime.Value = ProjectConstants.MaxFlightTime;
            ntxtFlightTime.Minimum = 0;
            ntxtFlightTime.Maximum = ProjectConstants.MaxFlightTime;
            ntxtFlightTime.Value = 150;

            lstAlg.Items.Add("CC");
            lstAlg.Items.Add("LHC-GW-CONV");
            lstAlg.Items.Add("LHC-GW-PF");
            lstAlg.Items.Add("PF");
            // lstAlg.Items.Add("EA-Dir");
            lstAlg.Items.Add("EA-Path");
            //// Group of algorithms for set destination
            //lstAlg.Items.Add("CC_E");
            //lstAlg.Items.Add("CC_E Reversed");
            //lstAlg.Items.Add("LHCGWConv_E");
            //lstAlg.Items.Add("LHCGWConv_E Reversed");
            //lstAlg.Items.Add("LHCGWPF_E");
            //lstAlg.Items.Add("LHCGWPF_E Reversed");
            //lstAlg.Items.Add("PF_E");
            //lstAlg.Items.Add("PF_E Reversed");
            //lstAlg.Items.Add("EA_E");

            lvQueue.Clear();
            lvQueue.View = View.Details;
            lvQueue.LabelEdit = false;
            lvQueue.AllowColumnReorder = false;
            lvQueue.FullRowSelect = true;
            lvQueue.GridLines = false;
            lvQueue.Columns.Add("Task");
            lvQueue.Columns[0].Width = lvQueue.Width - 5;

            chkBatchRun.Checked = true;
            ntxtRunTimes.Value = 10;
        }
        
        // When the Clear Button is clicked.
        private void btnClear_Click(object sender, EventArgs e)
        {
            rtxtLog.Clear();
        }

        // When the Browse button for distribution is clicked.
        private void btnBrowse1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Open Distribution Map File";
            fdlg.InitialDirectory = ProjectConstants.MapsDir;
            fdlg.Filter = "CSV files (*.CSV)|*.CSV|All files (*.*)|*.*";
            fdlg.FilterIndex = 1;
            fdlg.RestoreDirectory = false;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                txtFileName1.Text = fdlg.FileName;
            }
        }

        // When the load button for distribution is clicked.
        private void btnLoad1_Click(object sender, EventArgs e)
        {
            // Read in map file to matrix
            CurDistMap = MISCLib.ReadInMap(txtFileName1.Text);

            // Convert matrix from RGB to HSI

            // Scale so RGB range [0,255]
            ImgLib.ScaleImageValues(ref CurDistMap);

            // Convert matrix to image
            Bitmap CurBMP = new Bitmap(CurDistMap.Columns, CurDistMap.Rows);
            ImgLib.MatrixToImage(ref CurDistMap, ref CurBMP);

            // Showing map in map form
            map = new frmMap();
            map.setImage(CurBMP);
            map.Show();
        }

        // Add button is pressed
        private void btnAdd_Click(object sender, EventArgs e)
        {
            foreach (string s in lstAlg.SelectedItems)
            {
                ListViewItem i = new ListViewItem(s);
                lvQueue.Items.Add(i);
            }
        }

        // Remove button is pressed
        private void btnRemove_Click(object sender, EventArgs e)
        {
            while (lvQueue.SelectedIndices.Count > 0)
            {
                int i = lvQueue.SelectedIndices[0];
                lvQueue.Items.RemoveAt(i);
            }
        }

        // Flight Time trackbar is scrolled
        private void trbFlightTime_Scroll(object sender, EventArgs e)
        {
            ntxtFlightTime.Value = Convert.ToDecimal(trbFlightTime.Value);
        }

        // Flight Time numerical box value is changed
        private void ntxtFlightTime_ValueChanged(object sender, EventArgs e)
        {
            trbFlightTime.Value = Convert.ToInt16(ntxtFlightTime.Value);
        }

        // Algorithms listbox is double clicked
        private void lstAlg_DoubleClick(object sender, EventArgs e)
        {
            ListViewItem i = new ListViewItem((string)lstAlg.SelectedItem);
            lvQueue.Items.Add(i);

        }

        // Queue listview is double clicked
        private void lvQueue_DoubleClick(object sender, EventArgs e)
        {
            int i = lvQueue.SelectedIndices[0];
            lvQueue.Items.RemoveAt(i);
        }

        // Plan Flight Path button is pressed
        private void btnExecute_Click(object sender, EventArgs e)
        {
            // Make sure there are tasks in queue
            if (lvQueue.Items.Count < 1)
            {
                System.Windows.Forms.MessageBox.Show("Please select a task first!");
                return;
            }

            // Generate path planning requests
            foreach (ListViewItem item in lvQueue.Items)
            {
                PathPlanningRequest newRequest = new PathPlanningRequest();

                newRequest.UseDistributionMap = chkUseDist.Checked;
                newRequest.UseTaskDifficultyMap = chkUseDiff.Checked;
                newRequest.UseCoarseToFineSearch = chkCoaseToFine.Checked;
                newRequest.UseParallelProcessing = chkParallel.Checked;
                if (rbtnFixWing.Checked)
                {
                    newRequest.VehicleType = UAVType.FixWing;
                }
                if (rbtnCopter.Checked)
                {
                    newRequest.VehicleType = UAVType.Copter;
                }
                if (rbtnFixedAmount.Checked)
                {
                    newRequest.DetectionType = DType.FixAmount;
                }
                if (rbtnFixedAmountPercent.Checked)
                {
                    newRequest.DetectionType = DType.FixAmountInPercentage;
                }
                if (rbtnFixedPercent.Checked)
                {
                    newRequest.DetectionType = DType.FixPercentage;
                }
                newRequest.DetectionRate = (float)ntxtDetectionRate.Value;
                newRequest.DistMap = CurDistMap;
                newRequest.DiffMap = CurDiffMap;
                newRequest.UseEndPoint = chkUseEndPoint.Checked;
                newRequest.T = trbFlightTime.Value;
                newRequest.pStart.column = Convert.ToInt16(ntxtSX.Value);
                newRequest.pStart.row = Convert.ToInt16(ntxtSY.Value);
                newRequest.pEnd.column = Convert.ToInt16(ntxtEX.Value);
                newRequest.pEnd.row = Convert.ToInt16(ntxtEY.Value);
                if (chkUseEndPoint.Checked)
                {
                    switch (item.Text)
                    {
                        case "CC":
                            newRequest.AlgToUse = AlgType.CC_E;
                            break;
                        case "LHC-GW-CONV":
                            newRequest.AlgToUse = AlgType.LHCGWCONV_E;
                            break;
                        case "LHC-GW-PF":
                            newRequest.AlgToUse = AlgType.LHCGWPF_E;
                            break;
                        case "PF":
                            newRequest.AlgToUse = AlgType.PF_E;
                            break;
                        case "EA":
                            newRequest.AlgToUse = AlgType.EA_E;
                            break;
                    }
                }
                else
                {
                    switch (item.Text)
                    {
                        case "CC":
                            newRequest.AlgToUse = AlgType.CC;
                            break;
                        case "LHC-GW-CONV":
                            newRequest.AlgToUse = AlgType.LHCGWCONV;
                            break;
                        case "LHC-GW-PF":
                            newRequest.AlgToUse = AlgType.LHCGWPF;
                            break;
                        case "PF":
                            newRequest.AlgToUse = AlgType.PF;
                            break;
                        case "EA":
                            newRequest.AlgToUse = AlgType.EA;
                            break;
                    }
                }
                if (chkBatchRun.Checked)
                {
                    newRequest.BatchRun = true;
                    newRequest.RunTimes = Convert.ToInt16(ntxtRunTimes.Value);
                }

                // Add to server queue and pass alone the object
                frmParent.SubmitToRequestQueue(newRequest);
            }

            // Clear task queue
            lvQueue.Clear();
            lvQueue.Columns.Add("Task");
            lvQueue.Columns[0].Width = lvQueue.Width - 5;
        }




        // When the test button is pressed (Count how many modes)
        private void btnTest_Click(object sender, EventArgs e)
        {
            DateTime startTime = DateTime.Now;
            CountDistModes myCount = new CountDistModes(ref CurDistMap);
            Log(myCount.GetCount().ToString()+"\n");
            DateTime stopTime = DateTime.Now;
            TimeSpan duration = stopTime - startTime;
            Log("Computation took " + duration.ToString() + " seconds.\n");
            myCount = null;
        }

        
        
        
        #endregion

        #region Other Functions

        // Code to display logs in log rich text box (refresh and scroll to bottom)
        private void Log(string str)
        {
            rtxtLog.AppendText(str);
            rtxtLog.Refresh();
            SendMessage(rtxtLog.Handle, WM_VSCROLL, SB_BOTTOM, 0);
        }

        #endregion
        
    }
}
