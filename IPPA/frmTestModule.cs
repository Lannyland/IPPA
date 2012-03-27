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

        PathPlanningRequest test;

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
            trbFlightTime.Minimum = ProjectConstants.MinFlightTime;
            trbFlightTime.Maximum = ProjectConstants.MaxFlightTime;
            trbFlightTime.Value = ProjectConstants.DefaultFlightTime;
            ntxtFlightTime.Minimum = ProjectConstants.MinFlightTime;
            ntxtFlightTime.Maximum = ProjectConstants.MaxFlightTime;
            ntxtFlightTime.Value = ProjectConstants.DefaultFlightTime;
            ntxtGWCount.Value = ProjectConstants.GWCount;
            ntxtConvCount.Value = ProjectConstants.ConvCount;
            ntxtPFCount.Value = ProjectConstants.PFCount;
            ntxtTop2Iterations.Value = ProjectConstants.SearchResolution;
            ntxtTopNCount.Value = ProjectConstants.TopNCount;

            lstAlg.Items.Add("CC");
            lstAlg.Items.Add("LHC-GW-CONV");
            lstAlg.Items.Add("LHC-GW-PF");
            lstAlg.Items.Add("LHC-Random");
            lstAlg.Items.Add("Random");
            lstAlg.Items.Add("CONV");
            lstAlg.Items.Add("PF");
            lstAlg.Items.Add("TopTwo");
            lstAlg.Items.Add("TopN");
            lstAlg.Items.Add("EA-Path");

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
            // Also show start end points
            Start.X = Convert.ToInt32(ntxtSX.Value);
            Start.Y = Convert.ToInt32(ntxtSY.Value);
            SetDestPoint(true, Start);
            End.X = Convert.ToInt32(ntxtEX.Value);
            End.Y = Convert.ToInt32(ntxtEY.Value);
            SetDestPoint(false, End);
            frmDistMap.DrawingStartEndPoints();
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
            frmDiffMap = new frmMap(this);
            frmDiffMap.Text = "Task-Difficulty Map";
            frmDiffMap.setImage(CurBMP);
            // Also show start end points
            Start.X = Convert.ToInt32(ntxtSX.Value);
            Start.Y = Convert.ToInt32(ntxtSY.Value);
            SetDestPoint(true, Start);
            End.X = Convert.ToInt32(ntxtEX.Value);
            End.Y = Convert.ToInt32(ntxtEY.Value);
            SetDestPoint(false, End);
            frmDiffMap.DrawingStartEndPoints();
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
                newRequest.UseHierarchy = chkHierarchy.Checked;
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
                        case "Random":
                            newRequest.AlgToUse = AlgType.Random_E;
                            break;
                        case "CONV":
                            newRequest.AlgToUse = AlgType.CONV_E;
                            break;
                        case "PF":
                            newRequest.AlgToUse = AlgType.PF_E;
                            break;
                        case "TopTwo":
                            newRequest.AlgToUse = AlgType.TopTwo_E;
                            break;
                        case "TopN":
                            newRequest.AlgToUse = AlgType.TopN_E;
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
                        case "Random":
                            newRequest.AlgToUse = AlgType.Random;
                            break;
                        case "CONV":
                            newRequest.AlgToUse = AlgType.CONV;
                            break;
                        case "PF":
                            newRequest.AlgToUse = AlgType.PF;
                            break;
                        case "TopTwo":
                            newRequest.AlgToUse = AlgType.TopTwo;
                            break;
                        case "TopN":
                            newRequest.AlgToUse = AlgType.TopN;
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

                // Remember TopN parameter for TopTwo and TopN algorithms
                if (newRequest.AlgToUse == AlgType.TopTwo || newRequest.AlgToUse == AlgType.TopTwo_E)
                {
                    newRequest.TopN = 2;
                }
                if (newRequest.AlgToUse == AlgType.TopN || newRequest.AlgToUse == AlgType.TopN_E)
                {                    
                    newRequest.TopN = Convert.ToInt32(ntxtGWCount.Value);
                }

                if (!newRequest.SanityCheck())
                {
                    System.Windows.Forms.MessageBox.Show(newRequest.GetLog());
                    return;
                }
                #endregion

                // Debug: Test
                test = newRequest;

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

        // GW Iterations numerical box value is changed (also manages TopTwo SearchResolution)
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

        // Top 2 Iteration box value is changed
        private void ntxtTop2Iterations_ValueChanged(object sender, EventArgs e)
        {
            ProjectConstants.SearchResolution = Convert.ToInt32(ntxtTop2Iterations.Value);
        }

        // Top N Count box value is changed
        private void ntxtTopNCount_ValueChanged(object sender, EventArgs e)
        {
            ProjectConstants.TopNCount = Convert.ToInt32(ntxtTopNCount.Value);
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

                if (!rbtnCopter.Checked && ntxtFlightTime.Value % 2 != dist % 2)
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
            #region Test Mode Count
            //DateTime startTime = DateTime.Now;
            //CountDistModes myCount;
            //if (chkUseDiff.Checked)
            //{
            //    RtwMatrix mDistReachable = test.DistMap.Clone();
            //    RtwMatrix mDiffReachable = test.DiffMap.Clone();

            //    RtwMatrix mRealModes = new RtwMatrix(CurDiffMap.Rows, CurDiffMap.Columns);
            //    for (int i = 0; i < mRealModes.Rows; i++)
            //    {
            //        for (int j = 0; j < mRealModes.Columns; j++)
            //        {
            //            mRealModes[i, j] = mDistReachable[i, j] *
            //                (float)test.DiffRates[Convert.ToInt32(mDiffReachable[i, j])];
            //        }
            //    }
            //    myCount = new CountDistModes(mRealModes);

            //    // Debug code: Showing the product of dist and diff
            //    frmMap mapReal = new frmMap();
            //    Bitmap CurBMPReal = new Bitmap(mRealModes.Columns, mRealModes.Rows);
            //    ImgLib.MatrixToImage(ref mRealModes, ref CurBMPReal);
            //    mapReal.Text = "Real probability distribution with respect to difficulty map";
            //    mapReal.setImage(CurBMPReal);
            //    mapReal.Show();
            //    mapReal.resetImage();
            //}
            //else
            //{
            //    myCount = new CountDistModes(CurDiffMap);
            //}

            //Log(myCount.GetCount().ToString() + "\n");
            //DateTime stopTime = DateTime.Now;
            //TimeSpan duration = stopTime - startTime;
            //Log("Computation took " + duration.ToString() + " seconds.\n");
            //// Show mode nodes
            //RtwMatrix myModes = myCount.GetModes().Clone();
            //for (int i = 0; i < myModes.Rows; i++)
            //{
            //    for (int j = 0; j < myModes.Columns; j++)
            //    {
            //        if (myModes[i, j] > 0)
            //        {
            //            myModes[i, j] = 255;
            //        }
            //    }
            //}
            //// Convert matrix to image
            //Bitmap CurBMP = new Bitmap(myModes.Columns, myModes.Rows);
            //ImgLib.MatrixToImage(ref myModes, ref CurBMP);
            //// Showing map in map form
            //frmMap myModesForm = new frmMap(this);
            //myModesForm.Text = "Modes Map";
            //myModesForm.setImage(CurBMP);
            //myModesForm.Show();

            //myCount = null;
            #endregion 

            #region Test permutation
            
            //int[] intInput = { 1, 2, 3, 4};
            //Log(ShowPermutations<int>(intInput, 3));

            //string[] stringInput = { "Hello", "World", "Foo" };
            //Log(ShowPermutations<string>(stringInput, 2));
            
            #endregion

            #region Test MATLAB
            ////////////////////
            // Input Parameters
            ////////////////////

            // create an array ar for the real part of "a"
            System.Array ar = new double[2];
            ar.SetValue(11, 0);
            ar.SetValue(12, 1);

            // create an array ai for the imaginary part of "a"
            System.Array ai = new double[2];
            ai.SetValue(1, 0);
            ai.SetValue(2, 1);

            // create an array br for the real part of "b"
            System.Array br = new double[2];
            br.SetValue(21, 0);
            br.SetValue(22, 1);

            // create an array bi for the imaginary part of "b"
            System.Array bi = new double[2];
            bi.SetValue(3, 0);
            bi.SetValue(4, 1);

            /////////////////////
            // Output Parameters
            /////////////////////

            // initialize variables for return value from ML
            System.Array cr = new double[2];
            System.Array ci = new double[2];
            System.Array dr = new double[2];
            System.Array di = new double[2];


            ////////////////////////
            // Call MATLAB function
            ////////////////////////
            // call appropriate function/method based on Mode
            // use MATLAB engine
            UseEngine(ar, ai, br, bi, ref cr, ref ci, ref dr, ref di);

            Log("ar = " + ar.GetValue(0).ToString() + " " + ar.GetValue(1).ToString() + "\n");
            Log("ai = " + ai.GetValue(0).ToString() + " " + ai.GetValue(1).ToString() + "\n");
            Log("br = " + br.GetValue(0).ToString() + " " + br.GetValue(1).ToString() + "\n");
            Log("bi = " + bi.GetValue(0).ToString() + " " + bi.GetValue(1).ToString() + "\n");
            Log("cr = " + cr.GetValue(0).ToString() + " " + cr.GetValue(1).ToString() + "\n");
            Log("ci = " + ci.GetValue(0).ToString() + " " + ci.GetValue(1).ToString() + "\n");
            Log("dr = " + dr.GetValue(0).ToString() + " " + dr.GetValue(1).ToString() + "\n");
            Log("di = " + di.GetValue(0).ToString() + " " + di.GetValue(1).ToString() + "\n");
            #endregion
        }

        //// Print out the permutations of the input 
        //static string ShowPermutations<T>(IEnumerable<T> input, int count)
        //{
        //    string s = "";
        //    foreach (IEnumerable<T> permutation in PermuteUtils.Permute<T>(input, count))
        //    {
        //        foreach (T i in permutation)
        //        {
        //            s += " " + i.ToString();
        //        }
        //        s += "\n";
        //    }
        //    return s;
        //}

        static private void UseEngine(Array ar, Array ai, Array br,
            Array bi, ref Array cr, ref Array ci, ref Array dr, ref Array di)
        {
            /*
             * This function calls the math_by_numbers routine inside
             * MATLAB using the MATLAB Engine's com interface
             */

            // Instantiate MATLAB Engine Interface through com
            MLApp.MLAppClass matlab = new MLApp.MLAppClass();

            // Using Engine Interface, put the matrix "a" into 
            // the base workspace.
            // "a" is a complex variable with a real part of ar,
            // and an imaginary part of ai
            matlab.PutFullMatrix("a", "base", ar, ai);

            // Using Engine Interface, put the matrix "b" into 
            // the base workspace.
            // "b" is a complex variable with a real part of br,
            // and an imaginary part of bi
            matlab.PutFullMatrix("b", "base", br, bi);

            // Put test array into base workspace
            double[,] SR = new double[2, 2];
            double[,] SI = new double[2, 2];
            SR[0, 0] = 1;
            SR[0, 1] = 2;
            SR[1, 0] = 3;
            SR[1, 1] = 4;

            SI[0, 0] = 0;
            SI[0, 1] = 0;
            SI[1, 0] = 0;
            SI[1, 1] = 0;

            System.Array TR = new double[2];
            System.Array TI = new double[2];
            TR.SetValue(1, 0);
            TR.SetValue(2, 1);
            TI.SetValue(0, 0);
            TI.SetValue(0, 1);

            // Test List
            List<double[]> samples = new List<double[]>();
            double[] aaa = new double[2];
            double[] bbb = new double[2];
            aaa[0] = 5;
            aaa[1] = 6;
            bbb[0] = 7;
            bbb[1] = 8;
            samples.Add(aaa);
            samples.Add(bbb);
            double[,] arrSamplesR = new double[samples.Count, 2];
            double[,] arrSamplesI = new double[samples.Count, 2];
            for (int i = 0; i < samples.Count; i++)
            {
                double[] b = samples[i];
                arrSamplesR[i, 0] = b[0];
                arrSamplesR[i, 1] = b[1];
                arrSamplesI[i, 0] = 0;
                arrSamplesI[i, 1] = 0;
            }
            matlab.PutFullMatrix("ss", "base", arrSamplesR, arrSamplesI);

            // Using Engine Interface, execute the ML command
            // contained in quotes.
            matlab.Execute("cd 'C:\\Lanny\\MAMI\\ZPlayGround\\C# Calling MATLAB\\CSharp_MATLAB\\CSharp_MATLAB\\mcode';");
            matlab.Execute("open math_on_numbers.m");
            matlab.Execute("dbstop in math_on_numbers.m");
            matlab.Execute("[c, d] = math_on_numbers(a,b);");
            matlab.Execute("com.mathworks.mlservices.MLEditorServices.closeAll");
            //matlab.Execute("dbquit all");

            // Using Engine Interface, get the matrix "c" from
            // the base workspace.
            // "c" is a complex variable with a real part of cr,
            // and an imaginary part of ci
            matlab.GetFullMatrix("c", "base", ref cr, ref ci);

            // using engine interface, get the matrix "c" from
            // the base workspace.
            // "d" is a complex variable with a real part of dr,
            // and an imaginary part of di
            matlab.GetFullMatrix("d", "base", ref dr, ref di);

            System.Array resultsR = new double[2, 2];
            System.Array resultsI = new double[2, 2];
            matlab.GetFullMatrix("ss", "base", ref resultsR, ref resultsI);
            Console.WriteLine(resultsR.GetValue(0, 0).ToString() + " "
                            + resultsR.GetValue(0, 1).ToString() + " "
                            + resultsR.GetValue(1, 0).ToString() + " "
                            + resultsR.GetValue(1, 1).ToString());
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
