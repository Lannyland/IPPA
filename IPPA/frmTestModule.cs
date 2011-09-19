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
            fdlg.InitialDirectory = ProjectConstants.LabDir;
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
