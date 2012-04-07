using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using rtwmatrix;
using System.Net;
using System.Net.Sockets;
using System.IO;


namespace TCPIPTest
{
    public partial class frmTestModule : Form
    {
        #region Members
        
        private RtwMatrix CurDistMap;
        private RtwMatrix CurDiffMap;
        private frmMap frmDistMap;
        private frmMap frmDiffMap;
        private Point Start = new Point(0,0);
        private Point End = new Point(0,0);

        #endregion

        #region Constructor, Destructor

        // Constructor
        public frmTestModule()
        {            
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

        // Plan Flight Path button is pressed
        private void btnExecute_Click(object sender, EventArgs e)
        {
            // Sanity check
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
            PathPlanningRequest newRequest = new PathPlanningRequest();
            if (!BuildRequest(newRequest))
            {
                return;
            }
            
            // Build Path Request into Object
            // Object via protocol buffer
            byte[] outStream = RequestToByteArray(newRequest);

            // Add header to indicate data size
            byte[] outStreamFinal = outStreamFinal = ProtoBuffer.TCPHandler.AddDataSizeHeader(outStream);
            
            // Send request
            try
            {
                TcpClient clientSocket = new TcpClient();
                clientSocket.Connect("127.0.0.1", 8888);

                NetworkStream serverStream = clientSocket.GetStream();

                // Send data over socket connection
                serverStream.Write(outStreamFinal, 0, outStreamFinal.Length);
                serverStream.Flush();

                // Get server response
                byte[] inStream = ProtoBuffer.TCPHandler.RecieveData(clientSocket, serverStream);
                
                // Convert byte array to things we want
                if (inStream.Length == 0)
                {
                    System.Windows.Forms.MessageBox.Show("Server exception occurred. No data received!");
                }
                else
                {
                    Log("Efficiency = " + BitConverter.ToDouble(inStream, 0) + "\n");
                    Log("Run Time = " + BitConverter.ToDouble(inStream, 8) + "\n");
                    int[] curP = new int[inStream.Length - 16];
                    Log("Path = ");
                    for (int i = 16; i < inStream.Length; i = i + 4)
                    {
                        Log(BitConverter.ToInt32(inStream, i).ToString() + " ");
                        if ((i / 4) % 2 == 1)
                        {
                            Log(", ");
                        }
                    }
                }
            }
            catch (System.Net.Sockets.SocketException ex)
            {
                if (ex.ErrorCode == 10061)
                {
                    System.Windows.Forms.MessageBox.Show("Server not available!");
                }
                else
                {
                    Log(ex.Message);
                }                
            }

            //TODO Deal with path response
            // First 4 bytes size of data
            // Next 4 bytes Efficiency
            // Rest path
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

        // Building the path planning request object and does sanity check
        private bool BuildRequest(PathPlanningRequest newRequest)
        {
            bool goodRequest = true;

            ListViewItem item = lvQueue.Items[0];
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
                newRequest.TopN = Convert.ToInt32(ntxtTopNCount.Value);
            }

            if (!newRequest.SanityCheck())
            {
                System.Windows.Forms.MessageBox.Show(newRequest.GetLog());
                goodRequest = false;
            }

            // Clear task queue
            lvQueue.Clear();
            lvQueue.Columns.Add("Task");
            lvQueue.Columns[0].Width = lvQueue.Width - 5;
            return goodRequest;
        }

        // Construct byte array for Server Queue Item
        private byte[] RequestToByteArray(PathPlanningRequest newRequest)
        {
            byte[] bytes;

            ProtoBuffer.PathPlanningRequest.Builder newPBRequest = ProtoBuffer.PathPlanningRequest.CreateBuilder();
            newPBRequest.SetUseDistributionMap(newRequest.UseDistributionMap)
                      .SetUseTaskDifficultyMap(newRequest.UseTaskDifficultyMap)
                      .SetUseHiararchy(newRequest.UseHiararchy)
                      .SetUseCoarseToFineSearch(newRequest.UseCoarseToFineSearch)
                      .SetUseParallelProcessing(newRequest.UseParallelProcessing)
                      .SetVehicleType((ProtoBuffer.PathPlanningRequest.Types.UAVType)newRequest.VehicleType)
                      .SetDetectionType((ProtoBuffer.PathPlanningRequest.Types.DType)newRequest.DetectionType)
                      .SetDetectionRate(newRequest.DetectionRate)
                      .SetUseEndPoint(newRequest.UseEndPoint)
                      .SetT(newRequest.T)
                      .SetPStart(DistPointToPBDistPoint(newRequest.pStart))
                      .SetPEnd(DistPointToPBDistPoint(newRequest.pEnd))
                      .SetAlgToUse((ProtoBuffer.PathPlanningRequest.Types.AlgType)newRequest.AlgToUse)
                      .SetBatchRun(newRequest.BatchRun)
                      .SetRunTimes(newRequest.RunTimes)
                      .SetMaxDifficulty(newRequest.MaxDifficulty)
                      .SetDrawPath(newRequest.DrawPath)
                      .SetD(newRequest.d)
                      .SetTopNCount(newRequest.TopN);
            // Have to deal with DiffRates array separately
            if (newRequest.DiffRates != null)
            {
                for (int i = 0; i < newRequest.DiffRates.Length; i++)
                {
                    newPBRequest.AddDiffRate(newRequest.DiffRates[i]);
                }
            }
            //Here do the matrix thing
            //// Debug Code
            //// Test with small matrix
            //// int d = 40;
            //int d = 100;
            //int v = 1;
            //RtwMatrix testM = new RtwMatrix(d, d);
            //for (int i = 0; i < d; i++)
            //{
            //    for (int j = 0; j < d; j++)
            //    {
            //        testM[i, j] = v;
            //    }
            //}
            //newPBRequest.SetDistMap(RtwMatrixToPBMatrix(testM));
            if (newRequest.DistMap != null)
            {
                newPBRequest.SetDistMap(RtwMatrixToPBMatrix(newRequest.DistMap));
            }
            if (newRequest.DiffMap != null)
            {
                newPBRequest.SetDiffMap(RtwMatrixToPBMatrix(newRequest.DiffMap));
            }
            ProtoBuffer.PathPlanningRequest Request = newPBRequest.Build();
            newRequest = null;
            // Finally the ServerQueueItem
            ProtoBuffer.ServerQueueItem.Builder newServerQueueItem = new ProtoBuffer.ServerQueueItem.Builder();
            newServerQueueItem.SetCallerIP("127.0.0.1")
                              .SetCurRequest(Request);
            ProtoBuffer.ServerQueueItem ServerQueueItem = newServerQueueItem.Build();
            newServerQueueItem = null;
            bytes = ServerQueueItem.ToByteArray();

            return bytes;
        }

        // Method to convert DistPoint to Protocal Buffer version of DistPoint
        private static ProtoBuffer.PathPlanningRequest.Types.DistPoint DistPointToPBDistPoint(DistPoint p)
        {
            ProtoBuffer.PathPlanningRequest.Types.DistPoint.Builder newPBuilder = ProtoBuffer.PathPlanningRequest.Types.DistPoint.CreateBuilder();
            newPBuilder.SetRow(p.row)
                    .SetColumn(p.column);
            ProtoBuffer.PathPlanningRequest.Types.DistPoint newP = newPBuilder.Build();
            newPBuilder = null;
            return newP;
        }

        // Method to convert RtwMatrix to Protocal Buffer version of Matrix
        private static ProtoBuffer.PathPlanningRequest.Types.Matrix RtwMatrixToPBMatrix(RtwMatrix m)
        {
            ProtoBuffer.PathPlanningRequest.Types.Matrix.Builder newMBuilder = ProtoBuffer.PathPlanningRequest.Types.Matrix.CreateBuilder();
            for (int i = 0; i < m.Rows; i++)
            {
                ProtoBuffer.PathPlanningRequest.Types.MatrixRow.Builder rowBuilder = ProtoBuffer.PathPlanningRequest.Types.MatrixRow.CreateBuilder();
                for (int j = 0; j < m.Columns; j++)
                {
                    rowBuilder.AddCell(m[i, j]);
                }
                ProtoBuffer.PathPlanningRequest.Types.MatrixRow newRow = rowBuilder.Build();
                rowBuilder = null;
                newMBuilder.AddRow(newRow);
            }
            ProtoBuffer.PathPlanningRequest.Types.Matrix newM = newMBuilder.Build();
            newMBuilder = null;
            return newM;
        }

        // Code to display logs in log rich text box (refresh and scroll to bottom)
        private void Log(string str)
        {
            rtxtLog.AppendText(str);
            rtxtLog.Refresh();
        }

        #endregion        
    }
}
