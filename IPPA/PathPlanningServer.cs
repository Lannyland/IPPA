using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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

            while (true)
            {
                try
                {
                    // Data is send in bytes -- so we need to convert a C# string to a Byte[]
                    byte[] bytesFrom = new byte[10025];
                    // Blocks until a client sends a message
                    clientStream.Read(bytesFrom, 0, (int)clientSocket.ReceiveBufferSize);
                    string dataFromClient = System.Text.Encoding.ASCII.GetString(bytesFrom);
                    dataFromClient = dataFromClient.Substring(0, dataFromClient.IndexOf("$"));
                    theForm.Log(dataFromClient + "\n");
                    // Send server reponse
                    string serverResponse = "Responding to " + dataFromClient;
                    Byte[] sendBytes = Encoding.ASCII.GetBytes(serverResponse);
                    clientStream.Write(sendBytes, 0, sendBytes.Length);
                    clientStream.Flush();
                }
                catch
                {
                    //a socket error has occured
                    break;
                }
            }

            clientSocket.Close();
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
