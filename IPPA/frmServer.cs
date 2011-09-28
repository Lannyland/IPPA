using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace IPPA
{
    public partial class frmServer : Form
    {
        #region Members

        //WinAPI-Declaration for SendMessage
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(
        IntPtr window, int message, int wparam, int lparam);
        const int WM_VSCROLL = 0x115;
        const int SB_BOTTOM = 7;

        // Private members
        private bool blnServerRunning = false;
        private List<PathPlanningRequest> lstRequestQueue = new List<PathPlanningRequest>();
        private PathPlanningServer myServer = new PathPlanningServer();
                
        #endregion

        #region Constructor, Destructor

        // Constructor
        public frmServer()
        {
            InitializeComponent();
        }

        // Destructor
        ~frmServer()
        {
            // Cleaning up
            lstRequestQueue.Clear();
            lstRequestQueue = null;
            myServer = null;
        }

        #endregion

        #region Message Handlers

        // When form loads
        private void frmServer_Load(object sender, EventArgs e)
        {
            lblServer.Text = "Intelligent Path Planning Algorithm Server takes path " +
                             "planning requests over the network. Requests will be " +
                             "queued and processed sequentially. Eacah request includes " +
                             "a matrix representing the probability distribution, the " +
                             "starting position, (optionally) the ending position, " +
                             "and the flight duration (in seconds), together with the " +
                             "UAV type (fix-wing or copter) and detection type (" +
                             "fixed amount, fixed amount in percentage, or fixed percentage).";
        }

        // When Load Test Module button is pressed
        private void btnLoadTestModule_Click(object sender, EventArgs e)
        {
            frmTestModule myForm = new frmTestModule(this) ;
            myForm.Show();
        }

        // When Start Server/Stop Server button is pressed
        private void btnServerSwitch_Click(object sender, EventArgs e)
        {
            if (!blnServerRunning)
            {
                btnServerSwitch.Text = "Stop Server";
                blnServerRunning = true;
            }
            else
            {
                btnServerSwitch.Text = "Start Server";
                blnServerRunning = false;
            }

            // Eventually have a monitor thread initiate path planning tasks. 
            // For now, just call from here directly.
            while (myServer.GetServerQueue().Count > 0)
            {
                // Do the path planning
                PathPlanningHandler newHandler = new PathPlanningHandler(myServer.GetServerQueue()[0].GetRequest());
                newHandler.Run();
                newHandler = null;

                // Log activities
                Log("Path planning using " + myServer.GetServerQueue()[0].GetRequest().AlgToUse.ToString() +
                    " algorithm completed successfully.\n\n");

                // TODO Send activity log back to TestModuleForm

                // Remove it from server queue
                myServer.GetServerQueue().RemoveAt(0);

                // Remove it from listbox queue
                lstQueue.Items.RemoveAt(0);
                lstRequestQueue.RemoveAt(0);
            }
        }

        // When the request queue item is clicked on, show details
        private void lstQueue_Click(object sender, EventArgs e)
        {
            int i = lstQueue.SelectedIndex;
            if (i > -1)
            {
                rtxtRequestDetails.AppendText(GetRequestDetail(i));
            }
        }

        // When the Delete button is pressed
        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Remove all selected items in listbox.
            while (lstQueue.SelectedIndices.Count > 0)
            {
                int i = lstQueue.SelectedIndices[0];
                Log("Removed " + lstQueue.SelectedItems[0].ToString() + "path planning request from queue.\n\n");
                lstQueue.Items.RemoveAt(i);
                lstRequestQueue.RemoveAt(i);
            }
            
            // Also clear details box
            rtxtRequestDetails.Clear();
        }

        // When the Clear Button is clicked.
        private void btnClear_Click(object sender, EventArgs e)
        {
            rtxtLog.Clear();
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

        // Function to add item to queue
        public void SubmitToRequestQueue(PathPlanningRequest newRequest)
        {
            // Add request item to queue
            lstQueue.Items.Add(newRequest.AlgToUse.ToString());
            lstRequestQueue.Add(newRequest);
            // For now add request from local host
            // TODO Later implement the real network calls
            ServerQueueItem newItem = new ServerQueueItem(newRequest, "127.0.0.1");
            myServer.AddRequest(newItem);
            
            // Log activity
            Log("New path planning request queued...\n");
            Log("----------------------------------------------\n");
            Log(GetRequestDetail(lstRequestQueue.Count-1)+"\n\n");
        }

        // Display path planning request details
        private string GetRequestDetail(int i)
        {
            string response = "";
            PathPlanningRequest curRequest;
            curRequest = lstRequestQueue[i];
            response+="Use distribution map: " + curRequest.UseDistributionMap.ToString() + "\n";
            response+="Use difficulty map: " + curRequest.UseTaskDifficultyMap.ToString() + "\n";
            response+="Use coarse-to-fine search: " + curRequest.UseCoarseToFineSearch.ToString() + "\n";
            response+="Use parallel processing: " + curRequest.UseParallelProcessing.ToString() + "\n";
            response+="UAV type: " + curRequest.VehicleType.ToString() + "\n";
            response+="Detection type: " + curRequest.DetectionType.ToString() + "\n";
            response+="Detection rate: " + curRequest.DetectionRate.ToString() + "\n";
            response+="Flight duration (time steps): " + curRequest.T.ToString() + "\n";
            response+=("Set end point: " + curRequest.UseEndPoint.ToString() + "\n");
            response+="Starting Point (row, column): " + curRequest.pStart.row.ToString() +
                ", " + curRequest.pStart.column.ToString() + "\n";
            response+="Ending Point (row, column): " + curRequest.pEnd.row.ToString() +
                ", " + curRequest.pEnd.column.ToString() + "\n";
            response+="Algorithm: " + curRequest.AlgToUse.ToString() + "\n";
            response+="Batch run: " + curRequest.BatchRun.ToString() + "\n";
            response+="Run times: " + curRequest.RunTimes.ToString() + "\n";     
            return response;
        }

        #endregion
    }
}
