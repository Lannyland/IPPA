using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using rtwmatrix;
using System.Drawing;

namespace IPPA
{
    class MISCLib
    {
        // Read in a csv file and then store that into matrix
        public static RtwMatrix ReadInMap(string FileInName)
        {
            // Read file one line at a time and store to list.
            FileStream file = new FileStream(FileInName, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(file);
            string strLine;
            List<string[]> lstRow = new List<string[]>();
            // Loop through lines
            while (sr.Peek() >= 0)
            {
                strLine = sr.ReadLine().Trim();
                if (strLine.Length > 1)
                {
                    string[] splitData = strLine.Split(',');
                    lstRow.Add(splitData);
                }
            }
            sr.Close();
            file.Close();
            // Create matrix
            RtwMatrix mGrid = new RtwMatrix(lstRow.Count, lstRow[0].Length);
            for (int i = 0; i < mGrid.Rows; i++)
            {
                for (int j = 0; j < mGrid.Columns; j++)
                {
                    mGrid[i, j] = (float)(Convert.ToDouble(lstRow[i][j]));
                }
            }
            return mGrid;
        }
    }
}
