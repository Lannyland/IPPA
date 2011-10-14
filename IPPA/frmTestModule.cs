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
        private frmMap frmDistMap;
        private frmMap frmDiffMap;
        private Point Start = new Point(0,0);
        private Point End = new Point(0,0);

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
            frmDistMap = null;
            frmDiffMap = null;
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
            rbtnFixedAmountPercent.Checked = true;
            rbtnFixedPercent.Checked = false;
            ntxtDetectionRate.Value = 1;

            txtFileName1.Text = ProjectConstants.DefaultDistMap;
            txtFileName2.Text = ProjectConstants.DefaultDiffMap;

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
            ntxtGWCount.Value = ProjectConstants.GWCount;
            ntxtConvCount.Value = ProjectConstants.ConvCount;
            ntxtPFCount.Value = ProjectConstants.PFCount;

            lstAlg.Items.Add("CC");
            lstAlg.Items.Add("LHC-GW-CONV");
            lstAlg.Items.Add("LHC-GW-PF");
            lstAlg.Items.Add("LHC-Random");
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

            chkBatchRun.Checked = false;
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
            fdlg.InitialDirectory = ProjectConstants.MapsDir+@"\DistMaps";
            fdlg.Filter = "CSV files (*.CSV)|*.CSV|All files (*.*)|*.*";
            fdlg.FilterIndex = 1;
            fdlg.RestoreDirectory = false;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                txtFileName1.Text = fdlg.FileName;
            }
        }

        // When the Load button for distribution is clicked.
        private void btnLoad1_Click(object sender, EventArgs e)
        {
            // Read in map file to matrix
            CurDistMap = MISCLib.ReadInMap(txtFileName1.Text);

            // Scale so RGB range [0,255]
            ImgLib.ScaleImageValues(ref CurDistMap);

            // Convert matrix to image
            Bitmap CurBMP = new Bitmap(CurDistMap.Columns, CurDistMap.Rows);
            ImgLib.MatrixToImage(ref CurDistMap, ref CurBMP);

            // Showing map in map form
            frmDistMap = new frmMap(this);
            frmDistMap.Text = "Probability Distribution Map";
            frmDistMap.setImage(CurBMP);
            frmDistMap.Show();
        }

        // When the Browse button for task-difficulty map is clicked.
        private void btnBrowse2_Click(object sender, EventArgs e)
        {
            OpenFileDialog fdlg = new OpenFileDialog();
            fdlg.Title = "Open Task-Difficulty Map File";
            fdlg.InitialDirectory = ProjectConstants.MapsDir + @"\DiffMaps";
            fdlg.Filter = "CSV files (*.CSV)|*.CSV|All files (*.*)|*.*";
            fdlg.FilterIndex = 1;
            fdlg.RestoreDirectory = false;
            if (fdlg.ShowDialog() == DialogResult.OK)
            {
                txtFileName2.Text = fdlg.FileName;
            }
        }

        // When the Load button for task-difficulty map is clicked.
        private void btnLoad2_Click(object sender, EventArgs e)
        {
            // Read in map file to matrix
            CurDiffMap = MISCLib.ReadInMap(txtFileName2.Text);

            // Scale so RGB range [0,255]
            RtwMatrix TempDiffMap = CurDiffMap.Clone();
            ImgLib.ScaleImageValues(ref TempDiffMap);

            // Convert matrix to image
            Bitmap CurBMP = new Bitmap(CurDiffMap.Columns, CurDiffMap.Rows);
            ImgLib.MatrixToImage(ref TempDiffMap, ref CurBMP);

            // Showing map in map form
            frmDiffMap = new frmMap();
            frmDiffMap.Text = "Task-Difficulty Map";
            frmDiffMap.setImage(CurBMP);
            frmDiffMap.Show();

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
            #region Sanity Check

            // Make sure there are tasks in queue
            if (lvQueue.Items.Count < 1)
            {
                System.Windows.Forms.MessageBox.Show("Please select a task first!");
                return;
            }

            // Make sure maps are loaded
            if (chkUseDist.Checked && CurDistMap == null)
            {
                System.Windows.Forms.MessageBox.Show("Please load a probability distribution map first!");
                return;
            }
            if (chkUseDiff.Checked && CurDiffMap == null)
            {
                System.Windows.Forms.MessageBox.Show("Please load a task-difficulty map first!");
                return;
            }

            // Sanity check to make sure distribution map and task-difficulty map have same size
            if (chkUseDiff.Checked && chkUseDist.Checked)
            {
                if (CurDistMap.Rows != CurDiffMap.Rows || CurDistMap.Columns != CurDiffMap.Columns)
                {
                    System.Windows.Forms.MessageBox.Show("Please make sure the distribution map and the " +
                    "task-difficulty map must be the same size!");
                    return;
                }                    
            }

            // Use default distribution map or task-difficulty map if only one is checked
            if (chkUseDiff.Checked && !chkUseDist.Checked)
            {
                CurDistMap = new RtwMatrix(CurDiffMap.Rows, CurDiffMap.Columns);
            }
            if (!chkUseDiff.Checked && chkUseDist.Checked)
            {
                CurDiffMap = new RtwMatrix(CurDistMap.Rows, CurDistMap.Columns);
            }

            #endregion

            // Generate path planning requests
            foreach (ListViewItem item in lvQueue.Items)
            {
                PathPlanningRequest newRequest = new PathPlanningRequest();
                
                #region Setting Request Object Properties
                
                newRequest.UseDistributionMap = chkUseDist.Checked;
                newRequest.UseTaskDifficultyMap = chkUseDiff.Checked;
                newRequest.UseHiararchy = chkHiararchy.Checked;
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
                newRequest.DetectionRate = Convert.ToDouble(ntxtDetectionRate.Value);
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
                        case "LHC-Random":
                            newRequest.AlgToUse = AlgType.LHCRandom_E;
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
                        case "LHC-Random":
                            newRequest.AlgToUse = AlgType.LHCRandom;
                            break;
                        case "PF":
                            newRequest.AlgToUse = AlgType.PF;
                            break;
                        case "EA":
                            newRequest.AlgToUse = AlgType.EA;
                            break;
                    }
                }
                newRequest.DrawPath = chkShowPath.Checked;
                if (chkBatchRun.Checked)
                {
                    newRequest.BatchRun = true;
                    newRequest.RunTimes = Convert.ToInt16(ntxtRunTimes.Value);
                }
                
                // Find max task-difficulty and compute diff rates only once
                if (chkUseDiff.Checked)
                {
                    newRequest.MaxDifficulty = Convert.ToInt32(CurDiffMap.MinMaxValue()[1]);
                    // Set task-difficulty rates
                    double[] DiffRates = new double[newRequest.MaxDifficulty + 1];
                    double rate = 1.0 / (newRequest.MaxDifficulty + 1);
                    for (int i = 0; i < newRequest.MaxDifficulty + 1; i++)
                    {
                        DiffRates[i] = 1 - i * rate;
                    }
                    newRequest.DiffRates = DiffRates;
                }

                if (!newRequest.SanityCheck())
                {
                    System.Windows.Forms.MessageBox.Show(newRequest.GetLog());
                    return;
                }
                #endregion

                // Add to server queue and pass alone the object
                frmParent.SubmitToRequestQueue(newRequest);
            }

            // Clear task queue
            lvQueue.Clear();
            lvQueue.Columns.Add("Task");
            lvQueue.Columns[0].Width = lvQueue.Width - 5;
        }

        // Starting Point x numerical box value is changed
        private void ntxtSX_ValueChanged(object sender, EventArgs e)
        {
            Start.X = Convert.ToInt32(ntxtSX.Value);
            Start.Y = Convert.ToInt32(ntxtSY.Value);
            SetDestPoint(true, Start);
        }

        // Starting Point y numerical box value is changed
        private void ntxtSY_ValueChanged(object sender, EventArgs e)
        {
            Start.X = Convert.ToInt32(ntxtSX.Value);
            Start.Y = Convert.ToInt32(ntxtSY.Value);
            SetDestPoint(true, Start);
        }

        // Ending Point x numerical box value is changed
        private void ntxtEX_ValueChanged(object sender, EventArgs e)
        {
            End.X = Convert.ToInt32(ntxtEX.Value);
            End.Y = Convert.ToInt32(ntxtEY.Value);
            SetDestPoint(false, End);
        }

        // Ending Point y numerical box value is changed
        private void ntxtEY_ValueChanged(object sender, EventArgs e)
        {
            End.X = Convert.ToInt32(ntxtEX.Value);
            End.Y = Convert.ToInt32(ntxtEY.Value);
            SetDestPoint(false, End);
        }

        // GW Iterations numerical box value is changed
        private void ntxtGWCount_ValueChanged(object sender, EventArgs e)
        {
            ProjectConstants.GWCount = Convert.ToInt32(ntxtGWCount.Value);
        }

        // Conv Iterations numerical box value is changed
        private void ntxtConvCount_ValueChanged(object sender, EventArgs e)
        {
            ProjectConstants.ConvCount = Convert.ToInt32(ntxtConvCount.Value);
        }

        // PF Iterations numerical box value is changed
        private void ntxtPFCount_ValueChanged(object sender, EventArgs e)
        {
            ProjectConstants.PFCount = Convert.ToInt32(ntxtPFCount.Value);
        }
        
        // View Reachable Area button is pressed
        private void btnView_Click(object sender, EventArgs e)
        {
            if (CurDistMap == null)
            {
                System.Windows.Forms.MessageBox.Show("Please load a distribution map first!");
                return;
            }
            DrawReachableArea();
        }

        // Function to draw reachable area on map
        private void DrawReachableArea()
        {
            // Redraw map to only show areas UAV can reach
            RtwMatrix mReachableRegion = new RtwMatrix(CurDistMap.Rows, CurDistMap.Columns);
            if (!chkUseEndPoint.Checked)
            {
                for (int y = 0; y < mReachableRegion.Rows; y++)
                {
                    for (int x = 0; x < mReachableRegion.Columns; x++)
                    {
                        int dist = MISCLib.ManhattanDistance(x, y, Start.X, Start.Y);
                        if (dist <= trbFlightTime.Value)
                        {
                            // Mark 1 as possible
                            mReachableRegion[y, x] = 1;
                        }
                    }
                }
            }
            else
            {
                int dist = MISCLib.ManhattanDistance(Start.X, Start.Y, End.X, End.Y);
                if (dist > ntxtFlightTime.Value)
                {
                    // Impossible to get from A to B in allowed flight time
                    System.Windows.Forms.MessageBox.Show("Impossible! Extend flight time!");
                    return;
                }

                if (ntxtFlightTime.Value % 2 != dist % 2)
                {
                    // Impossible to get from A to B in the exact allowed flight time
                    System.Windows.Forms.MessageBox.Show("Impossible to reach end point at time T! Add 1 or minus 1!");
                    return;
                }

                for (int y = 0; y < mReachableRegion.Rows; y++)
                {
                    for (int x = 0; x < mReachableRegion.Columns; x++)
                    {
                        int dist_AC = MISCLib.ManhattanDistance(x, y, Start.X, Start.Y);
                        int dist_BC = MISCLib.ManhattanDistance(x, y, End.X, End.Y);
                        if ((dist_AC + dist_BC) <= ntxtFlightTime.Value)
                        {
                            // Mark 1 as possible
                            mReachableRegion[y, x] = 1;
                        }
                    }
                }
            }

            if (frmDistMap != null)
            {
                frmDistMap.resetImage();
                frmDistMap.DrawingStartEndPoints();
                frmDistMap.DrawReachableRegion(mReachableRegion);
                frmDistMap.Refresh();
            }
            if (frmDiffMap != null)
            {
                frmDiffMap.resetImage();
                frmDiffMap.DrawingStartEndPoints();
                frmDiffMap.DrawReachableRegion(mReachableRegion);
                frmDiffMap.Refresh();
            }
        }


        // When the test button is pressed (Count how many modes)
        private void btnTest_Click(object sender, EventArgs e)
        {
            DateTime startTime = DateTime.Now;
            CountDistModes myCount = new CountDistModes(CurDistMap);
            Log(myCount.GetCount().ToString()+"\n");
            DateTime stopTime = DateTime.Now;
            TimeSpan duration = stopTime - startTime;
            Log("Computation took " + duration.ToString() + " seconds.\n");
            myCount = null;
        }

        
        
        
        #endregion

        #region Other Functions

        // Drawing starting point or ending point on display map
        private void SetDestPoint(bool start, Point p)
        {
            if (frmDistMap != null)
            {
                frmDistMap.resetImage();
                frmDistMap.setPoint(start, p);
                frmDistMap.DrawingStartEndPoints();
            }
            if(frmDiffMap != null)
            {
                frmDiffMap.resetImage();
                frmDiffMap.setPoint(start, p);
                frmDiffMap.DrawingStartEndPoints();
            }
        }

        // Set starting point ending point coordinates
        public void SetStartEndPoints(Point Start, Point End)
        {
            ntxtSX.Value = Start.X;
            ntxtSY.Value = Start.Y;
            ntxtEX.Value = End.X;
            ntxtEY.Value = End.Y;
        }

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
