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


        private RtwMatrix CurMap;
        private frmMap map;

        #endregion

        #region Constructor, Destructor

        public frmTestModule()
        {
            InitializeComponent();
        }

        #endregion

        #region Message Handlers

        // When the form is loading
        private void frmTestModule_Load(object sender, EventArgs e)
        {
            txtFileName1.Text = ProjectConstants.MapsDir + @"\5_modal.csv";

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
            CurMap = MISCLib.ReadInMap(txtFileName1.Text);

            // Convert matrix from RGB to HSI

            // Scale so RGB range [0,255]
            ImgLib.ScaleImageValues(ref CurMap);

            // Convert matrix to image
            Bitmap CurBMP = new Bitmap(CurMap.Columns, CurMap.Rows);
            ImgLib.MatrixToImage(ref CurMap, ref CurBMP);

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

        private void btnTest_Click(object sender, EventArgs e)
        {
            DateTime startTime = DateTime.Now;
            CountDistModes myCount = new CountDistModes(ref CurMap);
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
