using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IPPA
{
    public partial class frmServer : Form
    {
        public frmServer()
        {
            InitializeComponent();
        }

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

        private void btnLoadTestModule_Click(object sender, EventArgs e)
        {
            frmTestModule myForm = new frmTestModule() ;
            myForm.Show();
        }
    }
}
