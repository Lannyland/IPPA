using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;
using System.Drawing;

namespace IPPA
{
    // Performs the Global Warming Search
    class AlgGlobalWarming : AlgPathPlanning
    {
        #region Members

        // Private variables
        private int ModeCount = 0;

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgGlobalWarming(PathPlanningRequest _curRequest, int _ModeCount, 
            RtwMatrix _mDistReachable, RtwMatrix _mDiffReachable, double _Efficiency_LB)
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_LB)
        {
            ModeCount = _ModeCount;
        }

        // Destructor
        ~AlgGlobalWarming()
        {
            // Cleaning up
        }

        #endregion

        #region Other Functions

        protected override void DoPathPlanning()
        {
            // If no Coarse-to-fine and no parallel
            if (!curRequest.UseCoarseToFineSearch && !curRequest.UseParallelProcessing)
            {
                GWExtensiveSearch();
            }
            
            // If yes Coarse-to-fine and no parallel
            if (curRequest.UseCoarseToFineSearch && !curRequest.UseParallelProcessing)
            {
                GWCoarseToFineSearch();
            }

            // If no Coarse-to-fine and yes parallel
            if (!curRequest.UseCoarseToFineSearch && curRequest.UseParallelProcessing)
            {
                GWParallelSearch();
            }

            // If yes Coarse-to-fine and yes parallel
            if (curRequest.UseCoarseToFineSearch && curRequest.UseParallelProcessing)
            {
                GWCoarseToFineAndParallelSearch();
            }

            // Debug code, show map remain (especially for partial detection)
            // Re-enact the flight with real distmap
            RtwMatrix DistMapRemain = mDist.Clone();
            foreach (Point p in Path)
            {
                DistMapRemain[p.Y, p.X] = VacuumProbability(p, DistMapRemain);
            }
            Bitmap CurBMP = new Bitmap(DistMapRemain.Columns, DistMapRemain.Rows);
            ImgLib.MatrixToImage(ref DistMapRemain, ref CurBMP);
            frmMap map = new frmMap();
            map.setImage(CurBMP);
            map.Show();
            map.resetImage();
            map.Refresh();
        }

        // Search every GW
        private void GWExtensiveSearch()
        {
            int GWCount = ProjectConstants.GWCount;

            // Make copy of map
            RtwMatrix mGW = mDist.Clone();

            // Find max value
            float[] minmax = mGW.MinMaxValue();
            float max = minmax[1];
            float rise = max / (GWCount + 1);

            // Loop many times
            for (int i = 0; i < GWCount; i++)
            {
                //Console.Write("i=" + i + " ");
                // Don't rise ocean for first search
                // Log("Orean rise " + i.ToString() + "\n");
                if (i > 0)
                {
                    // Ocean rises
                    for (int a = 0; a < mGW.Rows; a++)
                    {
                        for (int b = 0; b < mGW.Columns; b++)
                        {
                            mGW[a, b] = mGW[a, b] - rise;
                            if (mGW[a, b] < 0)
                            {
                                mGW[a, b] = 0;
                            }
                        }
                    }
                }

                // After ocean rises
                if (curRequest.AlgToUse == AlgType.LHCGWCONV)
                {
                    // If LHCGWCONV, search multiple convolution kernal sizes
                    int dim = Math.Max(mDist.Rows, mDist.Columns);
                    for (int j = 3; j < dim; j += (int)(dim / ProjectConstants.ConvCount))
                    {
                        //Console.Write("j=" + j + "\n");
                        AlgLHCGWCONV myAlg = new AlgLHCGWCONV(curRequest, mGW, mDiff, Efficiency_LB, j);
                        myAlg.PlanPath();

                        // I am recalculating BestCDF because the Global Warming effect lowered the probabilities
                        double RealCDF = GetTrueCDF(myAlg.GetPath());
                        //// Log RealCDF for each GW run
                        //curRequest.SetLog(RealCDF.ToString() + ", ");

                        if (CDF < RealCDF)
                        {
                            CDF = RealCDF;
                            Path = myAlg.GetPath();
                        }

                        // Cleaning up                        
                        myAlg = null;

                        // If we already have the best path, then no need to continue
                        if (Math.Abs(Efficiency_LB - CDF) < 0.001)
                        {
                            i = GWCount;
                            j = dim;
                        }
                    }
                    //// Print one GW per line (3 conv each line)
                    //curRequest.SetLog("\n");
                }
                if (curRequest.AlgToUse == AlgType.LHCGWPF)
                {
                    // If LHCGWPF, search three convolution kernal sizes
                    int dim = Math.Max(mDist.Rows, mDist.Columns);
                    int Sigma = 0;
                    for (int j = 0; j < ProjectConstants.PFCount; j++)
                    {
                        //Console.Write("j=" + j + "\n");
                        Sigma += Convert.ToInt16(dim / 3);
                        AlgLHCGWCONV myAlg = new AlgLHCGWCONV(curRequest, mGW, mDiff, Efficiency_LB, Sigma);
                        myAlg.PlanPath();

                        // I am recalculating BestCDF because the Global Warming effect lowered the probabilities
                        double RealCDF = GetTrueCDF(myAlg.GetPath());
                        //// Log RealCDF for each GW run
                        //curRequest.SetLog(RealCDF.ToString() + ", ");

                        if (CDF < RealCDF)
                        {
                            CDF = RealCDF;
                            Path = myAlg.GetPath();
                        }

                        // Cleaning up                        
                        myAlg = null;

                        // If we already have the best path, then no need to continue
                        if (Math.Abs(Efficiency_LB - CDF) < 0.001)
                        {
                            i = GWCount;
                            j = ProjectConstants.PFCount;
                        }
                    }
                    // Print one GW per line (3 conv each line)
                    //curRequest.SetLog("\n");
                }
            }

            // Cleaning up                        
            mGW = null;
        }

        // Search GW intelligently
        private void GWCoarseToFineSearch()
        {
            throw new NotImplementedException();
        }

        // Search multiple GW simutaneously
        private void GWParallelSearch()
        {
            throw new NotImplementedException();
        }

        // Search GW intelligently and simutaneously
        private void GWCoarseToFineAndParallelSearch()
        {
            throw new NotImplementedException();
        }

        // TODO Turn this into a seperate file so we can consider task-difficulty map and detection type
        // Function to calculate true cummulative probability using original map
        private double GetTrueCDF(List<Point> curPath)
        {
            double curCDF = 0;

            // Fly through the map
            //for (int i = 0; i < curRequest.T + 1; i++)
            //{
            //    curCDF += GetPartialDetection(curPath[i]);                
            //    mCurDist[curPath[i].Y, curPath[i].X] = VacuumProbability(curPath[i]);
            //}

            //// Cleaning up
            //mCurDist = mDiff.Clone();
            RtwMatrix mCDF = mDist.Clone();
            for (int i = 0; i < curRequest.T + 1; i++)
            {
                curCDF += GetPartialDetection(curPath[i], mCDF);
                mCDF[curPath[i].Y, curPath[i].X] = VacuumProbability(curPath[i], mCDF);
            }

            // Cleaning up
            mCDF = null;

            return curCDF;
        }

        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgGlobalWarming!");
        }

        #endregion
    }
}
