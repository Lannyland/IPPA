using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace IPPA
{
    class PathPlanningServer
    {
        #region Members

        private List<ServerQueueItem> ServerQueue = new List<ServerQueueItem>();
        private TcpListener serverSocket;
        private Thread listenThread;
        private frmServer theForm;

        #endregion

        #region Constructor, Destructor

        // Constructor
        public PathPlanningServer(frmServer _theForm)
        {
            theForm = _theForm;
            // Setup a listener at all network cards, port 8888
            this.serverSocket = new TcpListener(IPAddress.Any, 8888);
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
        }

        // Destructor
        ~PathPlanningServer()
        {
            // Cleaning up
            ServerQueue.Clear();
            ServerQueue = null;
            // Close connection
            this.listenThread.Abort();
        }

        #endregion

        #region Other Functions

        // Method to set up a TCPIP server and wait for connections
        private void ListenForClients()
        {
            // Start listening for connections.
            this.serverSocket.Start(1);   // Start(1) would only allow one connection.

            try
            {
                while (true)
                {
                    // The program is suspended while waiting for an incoming connection.
                    // This is a synchronous TCP application
                    TcpClient clientSocket = default(TcpClient);
                    clientSocket = serverSocket.AcceptTcpClient();

                    Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                    clientThread.Start(clientSocket);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught Exception: {0}", e.ToString());
            }

            this.serverSocket.Stop();
        }

        private void HandleClientComm(object client)
        {
            TcpClient clientSocket = (TcpClient)client;
            // Obtain a stream object for reading and writing
            NetworkStream clientStream = clientSocket.GetStream();

            // Object via protocol buffer
            while (true)
            {
                try
                {
                    // Data is send in bytes -- so we need to convert a C# string to a Byte[]
                    byte[] bytesFrom = new byte[10025];
                    // Blocks until a client sends a message

                    // Loop to make sure all data is read
                    int counter = 0;
                    int bufferSize = (int)clientSocket.ReceiveBufferSize;
                    int size = 0;
                    byte[] byteFinal = null;
                    do
                    {
                        int startIndex = bufferSize * counter;
                        int totalSize = bufferSize * (counter + 1);
                        clientStream.Read(bytesFrom, 0, bufferSize);
                        if (counter == 0)
                        {
                            // First time also get the data size
                            size = BitConverter.ToInt32(bytesFrom, 0);
                            byteFinal = new byte[size + 4];
                        }
                        if (size < bufferSize)
                        {
                            // One read is enough to read all data
                            Array.Copy(bytesFrom, 0, byteFinal, 0, size + 4);
                        }
                        else
                        {
                            // Lots of data and requires multiple reads
                            if (size >= totalSize)
                            {
                                // Still need to read more
                                Array.Copy(bytesFrom, 0, byteFinal, startIndex, bufferSize);
                            }
                            else
                            {
                                Array.Copy(bytesFrom, 0, byteFinal, startIndex, size + 4 - startIndex);
                            }
                        }
                        counter++;
                    }
                    while (clientStream.DataAvailable);

                    byte[] byteTrimmed = new byte[size];
                    Array.Copy(byteFinal, 4, byteTrimmed, 0, size);

                    //AddressBook restored = AddressBook.CreateBuilder().MergeFrom(bytesTrimmed).Build();
                    //Person p1 = restored.GetPerson(0);
                    //theForm.Log(p1.Id + " " + p1.Name + " " + p1.PhoneList[0] + " " + p1.Email + "\n");

                    ProtoBuffer.ServerQueueItem restored = ProtoBuffer.ServerQueueItem.CreateBuilder().MergeFrom(byteTrimmed).Build();
                    theForm.Log("UseDistributionMap = " + restored.CurRequest.UseDistributionMap + "\n");
                    theForm.Log("UseTaskDifficultyMap = " + restored.CurRequest.UseTaskDifficultyMap + "\n");
                    theForm.Log("UseHiararchy = " + restored.CurRequest.UseHiararchy + "\n");
                    theForm.Log("VehicleType = " + restored.CurRequest.VehicleType + "\n");
                    theForm.Log("pStart = (" + restored.CurRequest.PStart.Column + "," + restored.CurRequest.PStart.Row + "\n");
                    theForm.Log("pEnd = (" + restored.CurRequest.PEnd.Column + "," + restored.CurRequest.PEnd.Row + "\n");
                    theForm.Log("DiffRate = " + restored.CurRequest.GetDiffRate(0) + ","
                                              + restored.CurRequest.GetDiffRate(1) + ","
                                              + restored.CurRequest.GetDiffRate(2) + ","
                                              + restored.CurRequest.GetDiffRate(3) + "\n");

                    //TODO Construct PathPlanningRequest object
                    PathPlanningRequest curRequest = new PathPlanningRequest(); 

                    // Add request to queue
                    theForm.SubmitToRequestQueue(curRequest);
                                        
                    // Wait three seconds
                    Thread.Sleep(3000);

                    //TODO Perform path planning

                    // Remove request from queue


                    // Send server reponse
                    string serverResponse = "Responding!" + "\n";
                    Byte[] sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                    clientStream.Write(sendBytes, 0, sendBytes.Length);
                    clientStream.Flush();
                }
                catch(Exception e)
                {
                    //a socket error has occured
                    theForm.Log(e.Message);
                    break;
                }
            }

            clientSocket.Close();
        }

        // Method to stop server
        public void Stop()
        {
            //TODO set flags to stop services
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
