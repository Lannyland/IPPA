using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace ProtoBuffer
{
    class TCPHandler
    {
        #region Constructor, Destructor
        
        // Constructor
        public TCPHandler()
        {
        }

        // Destructor
        ~TCPHandler()
        {
            // Cleaning up
        }

        #endregion

        #region functions

        // Method to add a header to data stream indicating size of data (not including header)
        public static byte[] AddDataSizeHeader(byte[] outStream)
        {
            byte[] outStreamFinal;
            // Count byte size and conver to byte[4]
            int size = outStream.Length;
            byte[] header = BitConverter.GetBytes(size);

            // Include header in stream
            outStreamFinal = new byte[size + 4];
            Array.Copy(header, 0, outStreamFinal, 0, header.Length);
            Array.Copy(outStream, 0, outStreamFinal, 4, size);
            return outStreamFinal;
        }

        // Method to receive full data. Read multiple times if necessary
        public static byte[] RecieveData(TcpClient clientSocket, NetworkStream clientStream)
        {
            // Data is send in bytes -- so we need to allocate memory to read in bytes
            byte[] bytesFrom = new byte[10025];

            // Loop to make sure all data is read
            int counter = 0;
            int bufferSize = (int)clientSocket.ReceiveBufferSize;
            int size = 0;
            byte[] byteFinal = null;

            try
            {
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
                return byteTrimmed;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                return null;
            }
        }
        
        #endregion
    }
}
