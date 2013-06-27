using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

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
            int byteRead = 0;
            int totalByteRead = 0;
            bool blnReadMore = true;

            try
            {
                while (blnReadMore)
                {
                    // See if data is available to read. If not, then wait. 
                    int smallCounter = 0;
                    while (!clientStream.DataAvailable && smallCounter < TCPIPTest.ProjectConstants.MaxWaitTime + 1)
                    {
                        Console.WriteLine("Data not available, wait...");
                        Thread.Sleep(10);
                        smallCounter++;
                        Console.WriteLine(clientStream.DataAvailable);
                    }
                    if (smallCounter > TCPIPTest.ProjectConstants.MaxWaitTime)
                    {
                        Exception ex = new SocketException();
                        throw ex;
                    }

                    // When data is ready to be read, read as much as possible (up to buffer size)
                    byteRead = clientStream.Read(bytesFrom, 0, 8192);
                    Console.WriteLine("Read " + byteRead + " bytes this time.");

                    // First time also get the data size
                    if (counter < 1)
                    {
                        // Get data size
                        size = BitConverter.ToInt32(bytesFrom, 0);
                        // Alocate enough memory to store data
                        byteFinal = new byte[size + 4];
                    }

                    // Append data read to our data store
                    Array.Copy(bytesFrom, 0, byteFinal, totalByteRead, byteRead);


                    // Increase total counter.
                    totalByteRead += byteRead;


                    // See if all data is read
                    if (size + 4 <= totalByteRead)
                    {
                        blnReadMore = false;
                        // No need to loop any more
                        break;
                    }

                    // Increase counter
                    counter++;
                    // Thread.Sleep(500);
                }

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

        // Method to receive full data. Read multiple times if necessary
        public static byte[] RecieveDataBackup(TcpClient clientSocket, NetworkStream clientStream)
        {
            // Data is send in bytes -- so we need to allocate memory to read in bytes
            byte[] bytesFrom = new byte[10025];

            // Loop to make sure all data is read
            int counter = 0;
            int bufferSize = (int)clientSocket.ReceiveBufferSize;
            int size = 0;
            byte[] byteFinal = null;
            bool blnReadMore = true;

            try
            {
                while (blnReadMore)
                {
                    int startIndex = bufferSize * counter;
                    int totalSize = bufferSize * (counter + 1);
                    int smallCounter = 0;
                    while (!clientStream.DataAvailable && smallCounter < 10)
                    {
                        Console.WriteLine("Data not available, wait...");
                        Thread.Sleep(10);
                        smallCounter++;
                        Console.WriteLine(clientStream.DataAvailable);
                    }
                    if (smallCounter > 9)
                    {
                        throw new SocketException();
                    }

                    int bytecount = clientStream.Read(bytesFrom, 0, bufferSize);
                    Console.WriteLine("Read " + bytecount + " bytes this time.");

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
                        blnReadMore = false;
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
                            blnReadMore = false;
                        }
                    }
                    counter++;
                    // Thread.Sleep(500);
                    Console.WriteLine("Counter = " + counter + " last byte = " + byteFinal[36446] + " " + clientStream.DataAvailable);
                }

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
