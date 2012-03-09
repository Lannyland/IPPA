using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace TCPIPTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            TcpClient clientSocket = new TcpClient();
            clientSocket.Connect("127.0.0.1", 8888);

            NetworkStream serverStream = clientSocket.GetStream();

            //// Plain text
            //byte[] outStream = System.Text.Encoding.ASCII.GetBytes(txtSendText.Text + "$");

            // Object via protocol buffer
            byte[] outStream = PrepareServerQueueItem();
            for (int i = 0; i < outStream.Length; i++)
            {
                rtxtReceiveText.AppendText(outStream[i] + " ");
            }
            rtxtReceiveText.AppendText("\n\n");

            // Count byte size and conver to byte[4]
            int size = outStream.Length;
            byte[] header = BitConverter.GetBytes(size);
            
            // Include header in stream
            byte[] outStreamFinal = new byte[size + 4];
            Array.Copy(header, 0, outStreamFinal, 0, header.Length);
            Array.Copy(outStream, 0, outStreamFinal, 4, size);

            // Send data over socket connection
            serverStream.Write(outStreamFinal, 0, outStreamFinal.Length);
            serverStream.Flush();

            // Get server response
            byte[] inStream = new byte[10025];
            serverStream.Read(inStream, 0, (int)clientSocket.ReceiveBufferSize);
            string returndata = System.Text.Encoding.ASCII.GetString(inStream);
            rtxtReceiveText.AppendText(returndata + "   \n" + "   \n");
        }

        private byte[] PrepareServerQueueItem()
        {
            byte[] bytes;
            // First DistPoints
            ProtoBuffer.PathPlanningRequest.Types.DistPoint.Builder newStart = ProtoBuffer.PathPlanningRequest.Types.DistPoint.CreateBuilder();
            newStart.SetRow(0)
                    .SetColumn(0);
            ProtoBuffer.PathPlanningRequest.Types.DistPoint Start = newStart.Build();
            newStart = null;
            ProtoBuffer.PathPlanningRequest.Types.DistPoint.Builder newEnd = ProtoBuffer.PathPlanningRequest.Types.DistPoint.CreateBuilder();
            newEnd.SetRow(59)
                    .SetColumn(59);
            ProtoBuffer.PathPlanningRequest.Types.DistPoint End = newEnd.Build();
            newEnd = null;
            // Then PathPlanningRequest
            ProtoBuffer.PathPlanningRequest.Builder newRequest = ProtoBuffer.PathPlanningRequest.CreateBuilder();
            newRequest.SetUseDistributionMap(true)
                      .SetUseTaskDifficultyMap(true)
                      .SetUseHiararchy(false)
                      .SetUseCoarseToFineSearch(false)
                      .SetUseParallelProcessing(false)
                      .SetVehicleType(ProtoBuffer.PathPlanningRequest.Types.UAVType.Copter)
                      .SetDetectionType(ProtoBuffer.PathPlanningRequest.Types.DType.FixPercentage)
                      .SetDetectionRate(1)
                      .SetUseEndPoint(false)
                      .SetT(150)
                      .SetPStart(Start)
                      .SetPEnd(End)
                      .SetAlgToUse(ProtoBuffer.PathPlanningRequest.Types.AlgType.TopN)
                      .SetBatchRun(false)
                      .SetRunTimes(0)
                      .SetMaxDifficulty(3)
                      .SetDrawPath(false)
                      .SetD(0)
                      .SetTopNCount(5);
            newRequest.AddDiffRate(0);
            newRequest.AddDiffRate(0.25);
            newRequest.AddDiffRate(0.5);
            newRequest.AddDiffRate(0.75);
            ProtoBuffer.PathPlanningRequest Request = newRequest.Build();
            newRequest = null;
            // Finally the ServerQueueItem
            ProtoBuffer.ServerQueueItem.Builder newServerQueueItem = new ProtoBuffer.ServerQueueItem.Builder();
            newServerQueueItem.SetCallerIP("127.0.0.1")
                              .SetCurRequest(Request);
            ProtoBuffer.ServerQueueItem ServerQueueItem = newServerQueueItem.Build();
            newServerQueueItem = null;
            bytes = ServerQueueItem.ToByteArray();

            ProtoBuffer.ServerQueueItem restored = ProtoBuffer.ServerQueueItem.CreateBuilder().MergeFrom(bytes).Build();
            return bytes;
        }
    }
}
