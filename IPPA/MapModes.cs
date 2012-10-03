using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using rtwmatrix;
using Accord.MachineLearning;
using MathNet.Numerics.LinearAlgebra.Double;

namespace IPPA
{
    class MapMode : IComparable
    {
        public Point Mode = new Point();
        public double GoodnessRating;

        // Sort MapMode in descending order with best GoodnessRating on top
        public int CompareTo(object obj)
        {
            MapMode Compare = (MapMode)obj;
            int result = this.GoodnessRating.CompareTo(Compare.GoodnessRating);
            return result;
        }

        // Constructor
        public MapMode(Point _Mode, double _GoodnessRating)
        {
            Mode = _Mode;
            GoodnessRating = _GoodnessRating;
        }
    }

    class MapModes
    {
        #region Members

        // Private variables
        private int ModeCount;
        private RtwMatrix mModes;
        private PathPlanningRequest curRequest;
        private int N;
        private int GCount = ProjectConstants.Max_N;
        private RtwMatrix mDist;
        private RtwMatrix mDiff;
        private RtwMatrix mRealMap;
        private List<Point> lstCentroids = new List<Point>();

        // Public variables

        #endregion

        #region Constructor, Destructor

        // Constructor
        public MapModes(int _ModeCount, RtwMatrix _mModes, PathPlanningRequest _curRequest)
        {
            ModeCount = _ModeCount;
            mModes = _mModes;
            curRequest = _curRequest;
            N = curRequest.TopN;
            FindTopNModes();
        }
        public MapModes(int _ModeCount, RtwMatrix _mModes, PathPlanningRequest _curRequest, RtwMatrix _mDist, RtwMatrix _mDiff)
        {
            mDist = _mDist;
            mDiff = _mDiff;
            ModeCount = _ModeCount;
            mModes = _mModes;
            curRequest = _curRequest;
            N = curRequest.TopN;
            FindTopNModes();
        }
        public MapModes(int _GCount, int _ModeCount, RtwMatrix _mModes, PathPlanningRequest _curRequest, RtwMatrix _mDist, RtwMatrix _mDiff)
        {
            GCount = _GCount;
            mDist = _mDist;
            mDiff = _mDiff;
            ModeCount = _ModeCount;
            mModes = _mModes;
            curRequest = _curRequest;
            N = curRequest.TopN;
            FindTopNModes();
        }

        // Destructor
        ~MapModes()
        {
            // Cleaning up
            mModes = null;
            mDist = null;
            mDiff = null;
            mRealMap = null;
            curRequest = null;
        }

        #endregion

        #region Other Functions

        // Method to to find top N modes, called at object construction time
        private void FindTopNModes()
        {
            #region Debug Code
            // Debug code: show modes
            // Show mode nodes
            RtwMatrix mModes2 = mModes.Clone();
            for (int i = 0; i < mModes2.Rows; i++)
            {
                for (int j = 0; j < mModes2.Columns; j++)
                {
                    if (mModes2[i, j] > 0)
                    {
                        mModes2[i, j] = 255;
                    }
                }
            }

            if (curRequest.DrawPath)
            {
                // Convert matrix to image
                Bitmap CurBMP = new Bitmap(mModes2.Columns, mModes2.Rows);
                ImgLib.MatrixToImage(ref mModes2, ref CurBMP);
                // Showing map in map form
                frmMap myModesForm = new frmMap();
                myModesForm.Text = "Modes Map";
                myModesForm.setImage(CurBMP);
                myModesForm.Show();
            }
            #endregion

            // Identify centroids for all modes
            FindModeCentroids();

            // Sanity check: make sure we do have that many modes
            if (N > ModeCount)
            {
                System.Windows.Forms.MessageBox.Show("You want top " + N + " modes, but there are only " + ModeCount + " modes! Reducing N to " + ModeCount + ".");
                curRequest.TopN = ModeCount;
                N = ModeCount;
            }
            else if (N == ModeCount)
            {
                // Do nothing
            }
            else if (N < ModeCount)
            {
                //Compute goodness of modes, but not when TopN = ModeCount
                ComputeModeGoodnessRatio();
            }
        }

        // Method to find mode centroids for all modes
        private void FindModeCentroids()
        {
            // Method to find mode centroids for all modes
            List<List<Point>> AllModes = new List<List<Point>>();
            // First create one list for each mode
            for (int i = 0; i < ModeCount; i++)
            {
                List<Point> curModeList = new List<Point>();
                AllModes.Add(curModeList);
            }
            // Now populate each mode list
            for (int i = 0; i < mModes.Rows; i++)
            {
                for (int j = 0; j < mModes.Columns; j++)
                {
                    if (mModes[i, j] > 0)
                    {
                        AllModes[Convert.ToInt16(mModes[i, j])-1].Add(new Point(j, i));
                    }
                }
            }
            // Now compute centroid for each mode list
            for (int i = 0; i < AllModes.Count; i++)
            {
                // Sum up all the x and y
                int x_sum = 0;
                int y_sum = 0;
                foreach (Point p in AllModes[i])
                {
                    x_sum += p.X;
                    y_sum += p.Y;
                }
                int c_x = Convert.ToInt16(Math.Round(Convert.ToDouble(x_sum) / AllModes[i].Count));
                int c_y = Convert.ToInt16(Math.Round(Convert.ToDouble(y_sum) / AllModes[i].Count));
                lstCentroids.Add(new Point(c_x, c_y));
            }            
        }

        // Method to rate mode goodness and then sort (use first mode as base)
        private void ComputeModeGoodnessRatio()
        {
            // Use no more than Max_N (which is 5 I believe)
            int n = lstCentroids.Count;     // find as many Gaussians as modes. Later find topN Gaussians as Hiariarchical Search
            // Don't do too many Gaussians because it will take a long time and perform poorly
            if (n > GCount)
            {
                n = GCount;
            }

            // Multiple dist map and diff map if necessary
            //DateTime startTime = DateTime.Now;
            ComputeRealMap();
            //DateTime stopTime = DateTime.Now;
            //TimeSpan duration = stopTime - startTime;
            //double RunTime = duration.TotalSeconds;
            //System.Windows.Forms.MessageBox.Show("ComputeRealMap Run time " + RunTime + " seconds!");
            
            // Allocate memory for output matrices
            Array arrModes = new double[n];
            Array arrProportions = new double[n];
            Array arrMUs = new double[n, 2];
            Array arrSigmaXSigmaY = new double[n];
            Array junkModes = new double[n];
            Array junkMUs = new double[n, 2];
            Array junkSigmaXSigmaY = new double[n];
            
            #region Using MATLAB for GMM

            ////startTime = DateTime.Now;
            //double[,] arrSamplesR;
            //double[,] arrSamplesI;

            //// Generate samples from map to get ready to perform mixed Gaussian fitting
            //PrepareSamples(out arrSamplesR, out arrSamplesI);
            ////stopTime = DateTime.Now;
            ////duration = stopTime - startTime;
            ////RunTime = duration.TotalSeconds;
            ////System.Windows.Forms.MessageBox.Show("PrepareSamples Run time " + RunTime + " seconds!");

            //// Perform mixed Gaussian fitting and get parameters
            ////startTime = DateTime.Now;            
            //// Using MATLAB to do GMM
            //GaussianFitting(n, arrSamplesR, arrSamplesI, ref arrModes, ref arrMUs, ref arrSigmaXSigmaY, ref junkModes, ref junkMUs, ref junkSigmaXSigmaY);
            ////stopTime = DateTime.Now;
            ////duration = stopTime - startTime;
            ////RunTime = duration.TotalSeconds;
            ////System.Windows.Forms.MessageBox.Show("GaussianFitting Run time " + RunTime + " seconds!");
            //// Debug
            //// curRequest.SetLog("\nMATLAB GMM results\n");

            #endregion

            #region Using C# for GMM

            // Prepare samples into the format needed
            double[][] arrSamples;

            // Generate samples from map to get ready to perform mixed Gaussian fitting
            PrepareSamplesAccord(out arrSamples);

            // Using Accord.net library to do GMM
            GaussianMixtureModel gmm = new GaussianMixtureModel(n);
            List<MapMode> lstGaussians = new List<MapMode>();           // Results stored in a list of MapModes for sorting

            // If Accord.net library fails, try it again up to 3 times
            for (int ii = 0; ii < ProjectConstants.MaxAccordRun; ii++)
            {
                try
                {
                    gmm.Compute(arrSamples, 10);
                    // Getting arrays ready                
                    for (int i = 0; i < n; i++)
                    {
                        // Means
                        arrMUs.SetValue(gmm.Gaussians[i].Mean[0], i, 0);
                        arrMUs.SetValue(gmm.Gaussians[i].Mean[1], i, 1);
                        // Area
                        DenseMatrix m = new DenseMatrix(gmm.Gaussians[i].Covariance);
                        System.Numerics.Complex[] d = m.Evd().EigenValues().ToArray();
                        double SigmaXSigmaY = Math.Sqrt(d[0].Real) * Math.Sqrt(d[1].Real);
                        arrSigmaXSigmaY.SetValue(SigmaXSigmaY, i);
                        // Modes
                        arrModes.SetValue(gmm.Gaussians[i].GetDistribution().ProbabilityDensityFunction(gmm.Gaussians[i].Mean), i);
                        // Scales
                        arrProportions.SetValue(gmm.Gaussians[i].Proportion, i);
                    }
                    // Debug
                    // curRequest.SetLog("\nAccord GMM results\n");

                    ////Debug code
                    //for (int i = 0; i < arrMUs.Length / 2; i++)
                    //{
                    //    curRequest.SetLog(arrMUs.GetValue(i, 0) + "," + arrMUs.GetValue(i, 1) + "  ");
                    //}
                    //curRequest.SetLog("\n");
                    //for (int i = 0; i < arrSigmaXSigmaY.Length; i++)
                    //{
                    //    curRequest.SetLog(arrSigmaXSigmaY.GetValue(i) + "  ");
                    //}
                    //curRequest.SetLog("\n");
                    //for (int i=0; i< arrModes.Length; i++)
                    //{
                    //    curRequest.SetLog(arrModes.GetValue(i) + " ");
                    //}
                    //curRequest.SetLog("\n");

                    //startTime = DateTime.Now;
                    // Match centroids to Gaussians
                    MatchCentroidsToGaussians(arrMUs, lstGaussians);
                    //stopTime = DateTime.Now;
                    //duration = stopTime - startTime;
                    //RunTime = duration.TotalSeconds;
                    //System.Windows.Forms.MessageBox.Show("MatchCentroidsToGaussians Run time " + RunTime + " seconds!");
                    ii = ProjectConstants.MaxAccordRun;
                }
                catch
                {
                    Console.WriteLine("Something went wrong with Accord.net library.");
                    // System.Windows.Forms.MessageBox.Show("Something went wrong with Accord.net library.");
                }
            }

            #endregion

            //startTime = DateTime.Now;
            // Evaluate Goodness Rating
            EvaluateGoodnessRatings(arrModes, arrSigmaXSigmaY, lstGaussians, arrProportions);
            //stopTime = DateTime.Now;
            //duration = stopTime - startTime;
            //RunTime = duration.TotalSeconds;
            //System.Windows.Forms.MessageBox.Show("EvaluateGoodnessRatings Run time " + RunTime + " seconds!");

            // Find top N modes
            lstGaussians.Sort();
            lstGaussians.Reverse();
            ////Debug
            //for (int i = 0; i < n; i++)
            //{
            //    curRequest.SetLog("Mode (x,y): " + lstGaussians[i].Mode.X + "," + lstGaussians[i].Mode.Y + " MGR:" + lstGaussians[i].GoodnessRating + "\n");
            //}            
            lstGaussians.RemoveRange(N, lstGaussians.Count - N);

            // Rebuild lstCentroids
            lstCentroids.Clear();
            foreach (MapMode mm in lstGaussians)
            {
                lstCentroids.Add(mm.Mode);
            }
        }

        // Taking into considertation diff map before Gaussian fitting.
        private void ComputeRealMap()
        {
            // If Diff map is used, multiply first
            if (curRequest.UseTaskDifficultyMap)
            {
                mRealMap = new RtwMatrix(mDist.Rows, mDist.Columns);
                for (int i = 0; i < mRealMap.Rows; i++)
                {
                    for (int j = 0; j < mRealMap.Columns; j++)
                    {
                        mRealMap[i, j] = mDist[i, j] *
                            (float)curRequest.DiffRates[Convert.ToInt32(mDiff[i, j])];
                    }
                }
            }
            else
            {
                mRealMap = mDist;
            }
        }

        // Turning probability maps back into sample points
        private void PrepareSamples(out double[,] arrSamplesR, out double[,] arrSamplesI)
        {
            // Build Samples
            List<double[]> lstSamples = new List<double[]>();
            for (int i = 0; i < mRealMap.Rows; i++)
            {
                for (int j = 0; j < mRealMap.Columns; j++)
                {
                    for (int k = 0; k < Convert.ToInt16(System.Math.Round(mRealMap[i, j])) / 30; k++)
                    {
                        double[] sample = new double[2];
                        sample[0] = i;
                        sample[1] = j;
                        lstSamples.Add(sample);
                    }
                }
            }
            arrSamplesR = new double[lstSamples.Count, 2];
            arrSamplesI = new double[lstSamples.Count, 2];
            for (int i = 0; i < lstSamples.Count; i++)
            {
                double[] sample = lstSamples[i];
                arrSamplesR[i, 0] = sample[0];
                arrSamplesR[i, 1] = sample[1];
                arrSamplesI[i, 0] = 0;
                arrSamplesI[i, 1] = 0;
            }
        }

        // Turning probability maps back into sample points
        private void PrepareSamplesAccord(out double[][] arrSamples)
        {
            // Build Samples
            List<double[]> lstSamples = new List<double[]>();
            for (int i = 0; i < mRealMap.Rows; i++)
            {
                for (int j = 0; j < mRealMap.Columns; j++)
                {
                    for (int k = 0; k < Convert.ToInt16(System.Math.Round(mRealMap[i, j])) / ProjectConstants.DownSample_Rate; k++)
                    {
                        double[] sample = new double[2];
                        sample[0] = i;
                        sample[1] = j;
                        lstSamples.Add(sample);
                    }
                }
            }

            arrSamples = new double[lstSamples.Count][];
            for (int i = 0; i < lstSamples.Count; i++)
            {
                double[] sample = lstSamples[i];
                arrSamples[i] = sample;
            }
        }

        // Calling MATLAB function file to perform Gaussian fitting
        private void GaussianFitting(int n, double[,] arrSamplesR, double[,] arrSamplesI, ref Array arrModes, ref Array arrMUs, ref Array arrSigmaXSigmaY, ref Array junkModes, ref Array junkMUs, ref Array junkSigmaXSigmaY)
        {
            // Instantiate MATLAB Engine Interface through com
            MLApp.MLAppClass matlab = new MLApp.MLAppClass();

            //DateTime startTime = DateTime.Now;
            // Set input matrices
            matlab.PutFullMatrix("samples", "base", arrSamplesR, arrSamplesI);
            //DateTime stopTime = DateTime.Now;
            //TimeSpan duration = stopTime - startTime;
            //double RunTime = duration.TotalSeconds;
            //System.Windows.Forms.MessageBox.Show("Set input paramters " + RunTime + " seconds!");
            double[] NR = new double[1];
            double[] NI = new double[1];
            NR[0] = n;
            NI[0] = 0;
            matlab.PutFullMatrix("N", "base", NR, NI);

            //startTime = DateTime.Now;
            // Using Engine Interface, execute ML script file
            string appPath = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            matlab.Execute("cd " + appPath);
            matlab.Execute("cd ..\\..\\..\\Scripts");
            matlab.Execute("[modes, MUs, SigmaXSigmaY] = IPPAGaussianFitting(samples, N);");
            //stopTime = DateTime.Now;
            //duration = stopTime - startTime;
            //RunTime = duration.TotalSeconds;
            //System.Windows.Forms.MessageBox.Show("Execute ML Script " + RunTime + " seconds!");

            //startTime = DateTime.Now;
            // Using Engine Interface, get matrices from the base workspace.
            matlab.GetFullMatrix("modes", "base", ref arrModes, ref junkModes);
            matlab.GetFullMatrix("MUs", "base", ref arrMUs, ref junkMUs);
            matlab.GetFullMatrix("SigmaXSigmaY", "base", ref arrSigmaXSigmaY, ref junkSigmaXSigmaY);
            //stopTime = DateTime.Now;
            //duration = stopTime - startTime;
            //RunTime = duration.TotalSeconds;
            //System.Windows.Forms.MessageBox.Show("Retrieve matrices " + RunTime + " seconds!");
        }

        // Matching centroids to Gaussians because they might differ.
        private void MatchCentroidsToGaussians(Array arrMUs, List<MapMode> lstGaussians)
        {
            // Loop through Gaussian modes to find closest centroid
            for (int i = 0; i < arrMUs.Length/2; i++)
            {
                int MU_x = Convert.ToInt16(Math.Round(Convert.ToDouble(arrMUs.GetValue(i, 1))));
                int MU_y = Convert.ToInt16(Math.Round(Convert.ToDouble(arrMUs.GetValue(i, 0))));
                Point MU = new Point(MU_x, MU_y);
                // Set it high and find lesser ones to replace
                double minDist = curRequest.DistMap.Rows + curRequest.DistMap.Columns;
                int minIndex = 0;
                // Loop through centroids to match each Gaussian
                Point closestCentroid = lstCentroids[0];
                for (int j = 0; j < lstCentroids.Count; j++)
                {
                    // No need to compute if only one centroid left
                    if (lstCentroids.Count == 1)
                    {
                        closestCentroid = lstCentroids[0];
                        minIndex = j;
                        break;
                    }
                    // More than one centroids left
                    Point centroid = lstCentroids[j];
                    double curDist = MISCLib.EuclidianDistance(MU, centroid);
                    if (curDist <= minDist)
                    {
                        minDist = curDist;
                        closestCentroid = lstCentroids[j];
                        minIndex = j;
                    }
                    // No need to continue loop if we have found the right centroid
                    if (minDist == 0)
                    {
                        break;
                    }
                }
                // Once we have the closest centroid, remember it
                MapMode GaussianMode = new MapMode(closestCentroid, 1);
                lstGaussians.Add(GaussianMode);
                // Remove the centroid so not to use it again.
                lstCentroids.RemoveAt(minIndex);
            }
            // Now we have a list of centroids for Gaussian modes matching the MUs list.
        }

        // Compute goodness rating for each Gaussian
        private void EvaluateGoodnessRatings(Array arrModes, Array arrSigmaXSigmaY, List<MapMode> lstGaussians, Array arrProportions)
        {
            // Compute the common denominator (based on mode 1)
            double denom = ComputeGoodness(lstGaussians, arrModes, arrSigmaXSigmaY, 0, arrProportions);
            // Make first MapMode goodness ratio 1
            lstGaussians[0].GoodnessRating = 1;
            for (int i = 1; i < lstGaussians.Count; i++)
            {
                lstGaussians[i].GoodnessRating = ComputeGoodness(lstGaussians, arrModes, arrSigmaXSigmaY, i, arrProportions) / denom;
            }

            //// Debug code
            //for (int i = 0; i < arrSigmaXSigmaY.Length; i++)
            //{
            //    Console.Write(arrSigmaXSigmaY.GetValue(i) + "  ");
            //}
            //Console.Write("\n");
        }

        // Compute the numerator or denominator of the goodness rating formula
        private double ComputeGoodness(List<MapMode> lstGaussians, Array arrModes, Array arrSigmaXSigmaY, int i, Array arrProportions)
        {
            // Compute distance ratio            
            double d_ratio = ComputeDistRatio(lstGaussians, i);
            // Compute scale
            double scale = 0;
            if (ProjectConstants.UseAccordProportions)
            {
                scale = Convert.ToDouble(arrProportions.GetValue(i));
            }
            else
            {
                scale = mRealMap[lstGaussians[i].Mode.Y, lstGaussians[i].Mode.X] / Convert.ToDouble(arrModes.GetValue(i));
            }
            // Debug
            // curRequest.SetLog("Scale: " + scale + "\n");
            
            double goodness = 0;

            if (ProjectConstants.UseAreaInHeuristic)
            {
                goodness = d_ratio * scale / Convert.ToDouble(arrSigmaXSigmaY.GetValue(i));
            }
            else
            {
                goodness = d_ratio * scale;
            }

            //// Debug code
            //Console.WriteLine("i = " + i);
            //Console.WriteLine("d_ratio = " + d_ratio);
            //Console.WriteLine("scale = " + mRealMap[lstGaussians[i].Mode.Y, lstGaussians[i].Mode.X] + " / " + Convert.ToDouble(arrModes.GetValue(i)) + " = " + scale);
            //Console.WriteLine("area = " + Convert.ToDouble(arrSigmaXSigmaY.GetValue(i)));
            //Console.WriteLine("goodness = " + goodness);

            return goodness;
        }

        // Compute the distance ratio factor of the goodness rating formula for a Gaussian
        private double ComputeDistRatio(List<MapMode> lstGaussians, int i)
        {
            int d_mi = MISCLib.ManhattanDistance(lstGaussians[i].Mode, new Point(curRequest.pStart.column, curRequest.pStart.row)); // Distance to Start
            if (curRequest.UseEndPoint)
            {
                d_mi += MISCLib.ManhattanDistance(lstGaussians[i].Mode, new Point(curRequest.pEnd.column, curRequest.pEnd.row)); // Distance to End
            }
            if (ProjectConstants.LogDistanceRatio)
            {
                return Math.Log((curRequest.T / (d_mi + 1)), 10);
            }
            else
            {
                return curRequest.T / (d_mi + 1);
            }
        }

        // Getters
        public int GetModeCount()
        {
            return ModeCount;
        }
        public List<Point> GetModeCentroids()
        {
            return lstCentroids;
        }

        #endregion
    }
}
