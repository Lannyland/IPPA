using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Drawing;
using rtwmatrix;

namespace IPPA
{
    class PathPlanningServer
    {
        #region Members

        private List<ServerQueueItem> ServerQueue = new List<ServerQueueItem>();
        private TcpListener serverSocket;
        private Thread listenThread;
        private Thread clientThread;
        private frmServer theForm;
        private volatile bool blnServerRunning = false;

        #endregion

        #region Constructor, Destructor

        // Constructor
        public PathPlanningServer(frmServer _theForm)
        {
            theForm = _theForm;

            if (theForm.blnServerRunning)
            {
                // Setup a listener at all network cards, port 8888
                this.serverSocket = new TcpListener(IPAddress.Any, 8888);
                this.listenThread = new Thread(new ThreadStart(ListenForClients));
                this.listenThread.Name = "listenThread";
                this.listenThread.Start();
            }
        }

        // Destructor
        ~PathPlanningServer()
        {
            // Cleaning up
            ServerQueue.Clear();
            ServerQueue = null;
            // Close connection
            if (serverSocket != null)
            {
                serverSocket.Stop();
            }
            serverSocket = null;
            if (listenThread != null)
            {
                this.listenThread.Abort();
            }
            listenThread = null;
        }

        #endregion

        #region Other Functions

        // Method to set up a TCPIP server and wait for connections
        private void ListenForClients()
        {
            // Start listening for connections.
            blnServerRunning = true;
            this.serverSocket.Start(1);   // Start(1) would only allow one connection.

            try
            {
                while (blnServerRunning)
                {
                    Console.WriteLine("listenThread: working...");
                    // The program is suspended while waiting for an incoming connection.
                    // This is a synchronous TCP application
                    TcpClient clientSocket = default(TcpClient);
                    clientSocket = serverSocket.AcceptTcpClient();

                    clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                    clientThread.Name = "clientThread";
                    clientThread.Start(clientSocket);
                }
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }

            this.serverSocket.Stop();
            Console.WriteLine("listenThread: terminating gracefully.");
        }

        private void HandleClientComm(object client)
        {
            TcpClient clientSocket = (TcpClient)client;

            if (!blnServerRunning)
            {
                clientSocket.Close();

                return;
            }

            // Obtain a stream object for reading and writing
            NetworkStream clientStream = clientSocket.GetStream();

            // Object via protocol buffer
            while (true)
            {
                // Blocks until a client sends a message
                Console.WriteLine("clientThread: working...");
                try
                {
                    // Get data over TCP
                    byte[] byteTrimmed = ProtoBuffer.TCPHandler.RecieveData(clientSocket, clientStream);

                    theForm.Invoke(theForm.dLogCallBack, new object[] { "Received path planning request over the network.\n\n" });

                    // Construct Path Planning Request object from byte array
                    PathPlanningRequest curRequest = ByteArrayToRequest(byteTrimmed);

                    // Add request to queue
                    theForm.Invoke(theForm.dSubmitToRequestQueueCallBack, new object[] {curRequest});

                    // Do the path planning
                    PathPlanningHandler newHandler = new PathPlanningHandler(curRequest);
                    newHandler.Run();
                    double curEfficiency = newHandler.GetAvgEfficiency();
                    double curRunTime = newHandler.GetAvgRunTime();
                    List<Point> curPath = newHandler.GetPath();
                    newHandler = null;
                    theForm.Invoke(theForm.dLogCallBack, new object[] { curRequest.GetLog() });

                    // Convert result to byte array
                    byte[] curE = BitConverter.GetBytes(curEfficiency);
                    byte[] curR = BitConverter.GetBytes(curRunTime);
                    // Convert to int array first
                    int[] temp = new int[curPath.Count * 2];
                    for (int i = 0; i < curPath.Count; i++)
                    {
                        temp[i * 2] = curPath[i].X;
                        temp[i * 2 + 1] = curPath[i].Y;
                    }
                    // Convert int array to byte array
                    byte[] curP = new byte[temp.Length * sizeof(int)];
                    Buffer.BlockCopy(temp, 0, curP, 0, curP.Length);
                    // Combine them together
                    byte[] result = new byte[curE.Length + curR.Length + curP.Length];
                    Array.Copy(curE, 0, result, 0, curE.Length);
                    Array.Copy(curR, 0, result, curE.Length, curR.Length);
                    Array.Copy(curP, 0, result, curE.Length + curR.Length, curP.Length);
                    byte[] outStream = ProtoBuffer.TCPHandler.AddDataSizeHeader(result);

                    // Remove request from queue
                    theForm.Invoke(theForm.dRemoveFromRequestQueueCallBack, new object[] { 0 });

                    // Send server reponse
                    clientStream.Write(outStream, 0, outStream.Length);
                    clientStream.Flush();

                    theForm.Invoke(theForm.dLogCallBack, new object[] { "Path planning result sent back to client.\n\n" });

                    break;
                }
                catch (Exception e)
                {
                    // A socket error has occured
                    // theForm.Log(e.Message);
                    System.Windows.Forms.MessageBox.Show(e.Message);
                    break;
                }
            }

            clientStream.Close();
            clientSocket.Close();
            Console.WriteLine("clientThread: terminating gracefully.");
        }

        // Method to construct Path Planning Request object from byte array
        private static PathPlanningRequest ByteArrayToRequest(byte[] byteTrimmed)
        {
            // Construct Protocol Buffer object form byte array
            ProtoBuffer.ServerQueueItem restored = ProtoBuffer.ServerQueueItem.CreateBuilder().MergeFrom(byteTrimmed).Build();

            // Construct PathPlanningRequest object
            ProtoBuffer.PathPlanningRequest PBRequest = restored.CurRequest;
            PathPlanningRequest curRequest = new PathPlanningRequest();
            // Easy stuff first
            curRequest.UseDistributionMap = PBRequest.UseDistributionMap;
            curRequest.UseTaskDifficultyMap = PBRequest.UseTaskDifficultyMap;
            curRequest.UseHiararchy = PBRequest.UseHiararchy;
            curRequest.UseCoarseToFineSearch = PBRequest.UseCoarseToFineSearch;
            curRequest.UseParallelProcessing = PBRequest.UseParallelProcessing;
            curRequest.VehicleType = (UAVType)PBRequest.VehicleType;
            curRequest.DetectionType = (DType)PBRequest.DetectionType;
            curRequest.DetectionRate = PBRequest.DetectionRate;
            curRequest.UseEndPoint = PBRequest.UseEndPoint;
            curRequest.T = PBRequest.T;
            curRequest.pStart = new DistPoint(PBRequest.PStart.Row, PBRequest.PStart.Column);
            curRequest.pEnd = new DistPoint(PBRequest.PEnd.Row, PBRequest.PEnd.Column);
            curRequest.AlgToUse = (AlgType)PBRequest.AlgToUse;
            curRequest.BatchRun = PBRequest.BatchRun;
            curRequest.RunTimes = PBRequest.RunTimes;
            curRequest.MaxDifficulty = PBRequest.MaxDifficulty;
            curRequest.DrawPath = PBRequest.DrawPath;
            curRequest.d = PBRequest.D;
            curRequest.TopN = PBRequest.TopNCount;
            // DiffRates array next
            double[] DiffRates = null;
            if (PBRequest.DiffRateList.Count > 0)
            {
                DiffRates = new double[PBRequest.DiffRateList.Count];
            }
            for (int i = 0; i < PBRequest.DiffRateList.Count; i++)
            {
                DiffRates[i] = PBRequest.DiffRateList[i];
            }
            curRequest.DiffRates = DiffRates;
            // Last the matrices
            curRequest.DistMap = PBMatrixToRtwMatrix(PBRequest.DistMap);
            curRequest.DiffMap = PBMatrixToRtwMatrix(PBRequest.DiffMap);

            return curRequest;
        }

        // Method to convert Protocal Buffer version of Matrix to RtwMatrix
        public static RtwMatrix PBMatrixToRtwMatrix(ProtoBuffer.PathPlanningRequest.Types.Matrix mPB)
        {
            RtwMatrix mMap = null;
            if (mPB.RowList.Count > 0)
            {
                mMap = new RtwMatrix(mPB.RowList.Count, mPB.RowList[0].CellList.Count);
                for (int i = 0; i < mPB.RowList.Count; i++)
                {
                    for (int j = 0; j < mPB.RowList[0].CellList.Count; j++)
                    {
                        mMap[i, j] = mPB.RowList[i].CellList[j];
                    }
                }            
            }
            return mMap;
        }

        // Method to stop server
        public void Stop()
        {
            //TODO set flags to stop services
            blnServerRunning = false;
            if (clientThread != null)
            {
                clientThread.Abort();
            }
            Thread.Sleep(50);
            if (serverSocket != null)
            {
                this.serverSocket.Stop();
            }
            if (listenThread != null)
            {
                this.listenThread.Abort();
            }
        }

        // Method to add server queue items
        public void AddRequest(ServerQueueItem item)
        {
            ServerQueue.Add(item);
        }

        #region Getters
        public List<ServerQueueItem> GetServerQueue()
        {
            return ServerQueue;
        }
        #endregion

        #endregion
    }
}
