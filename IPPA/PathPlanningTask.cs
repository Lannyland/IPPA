using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;
using System.Drawing;

namespace IPPA
{
    // Launched from PathPlanningHandler
    // Performs one of the batch path planning tasks
    // Calls the right algorithm based on PathPlanningRequest
    class PathPlanningTask
    {
        #region Members

        // Private members
        private PathPlanningRequest curRequest;
        private int ModeCount = 0;
        private RtwMatrix mDistReachable;
        private RtwMatrix mDiffReachable;
        private double Efficiency_UB = 0;
        private double Efficiency = 0;
        private double RunTime = 0;
        private List<Point> Path;
        AlgPathPlanning curAlg;

        #endregion

        #region Constructor, Destructor

        // Constructor
        public PathPlanningTask(PathPlanningRequest _curRequest, int _ModeCount,
            RtwMatrix _mDistReachable, RtwMatrix _mDiffReachable, double _Efficiency_UB)
        {
            curRequest = _curRequest;
            ModeCount = _ModeCount;
            mDistReachable = _mDistReachable;
            mDiffReachable = _mDiffReachable;
            Efficiency_UB = _Efficiency_UB;
        }

        // Destructor
        ~PathPlanningTask()
        {
            // Cleaning up
            curRequest = null;
            mDistReachable = null;
            mDiffReachable = null;
            curAlg = null;
        }

        #endregion

        #region Other Functions

        // Performing the path planning task
        public void Run()
        {
            // Use the right algorithm
            switch (curRequest.AlgToUse)
            {
                case AlgType.CC:
                    curAlg = new AlgCC(curRequest, mDistReachable, mDiffReachable, Efficiency_UB);
                    curAlg.PlanPath();
                    break;
                case AlgType.CC_E:
                    curAlg = new AlgSearchReverse(curRequest, ModeCount, mDistReachable, mDiffReachable, Efficiency_UB);
                    curAlg.PlanPath();
                    break;
                case AlgType.LHCGWCONV:
                    curAlg = new AlgGlobalWarming(curRequest, ModeCount, mDistReachable, mDiffReachable, Efficiency_UB);
                    curAlg.PlanPath();
                    break;
                case AlgType.LHCGWCONV_E:
                    curAlg = new AlgSearchReverse(curRequest, ModeCount, mDistReachable, mDiffReachable, Efficiency_UB);
                    curAlg.PlanPath();
                    break;
                case AlgType.LHCGWPF:
                    curAlg = new AlgGlobalWarming(curRequest, ModeCount, mDistReachable, mDiffReachable, Efficiency_UB);
                    curAlg.PlanPath();
                    break;
                case AlgType.LHCGWPF_E:
                    curAlg = new AlgSearchReverse(curRequest, ModeCount, mDistReachable, mDiffReachable, Efficiency_UB);
                    curAlg.PlanPath();
                    break;
                case AlgType.LHCRandom:
                    curAlg = new AlgLHCRandom(curRequest, mDistReachable, mDiffReachable, Efficiency_UB);
                    curAlg.PlanPath();
                    break;
                case AlgType.LHCRandom_E:
                    curAlg = new AlgSearchReverse(curRequest, ModeCount, mDistReachable, mDiffReachable, Efficiency_UB);
                    curAlg.PlanPath();
                    break;
                case AlgType.Random:
                    curAlg = new AlgRandom(curRequest, mDistReachable, mDiffReachable, Efficiency_UB);
                    curAlg.PlanPath();
                    break;
                case AlgType.Random_E:
                    curAlg = new AlgSearchReverse(curRequest, ModeCount, mDistReachable, mDiffReachable, Efficiency_UB);
                    curAlg.PlanPath();
                    break;
                case AlgType.PF:
                    curAlg = new AlgPFLooper(curRequest, mDistReachable, mDiffReachable, Efficiency_UB);
                    curAlg.PlanPath();
                    break;
                case AlgType.PF_E:
                    curAlg = new AlgSearchReverse(curRequest, ModeCount, mDistReachable, mDiffReachable, Efficiency_UB);
                    curAlg.PlanPath();
                    break;
                case AlgType.EA:
                    curAlg = new AlgEA(curRequest, ModeCount, mDistReachable, mDiffReachable, Efficiency_UB);
                    curAlg.PlanPath();
                    break;
                case AlgType.EA_E:
                    curAlg = new AlgSearchReverse(curRequest, ModeCount, mDistReachable, mDiffReachable, Efficiency_UB);
                    curAlg.PlanPath();
                    break;
            }

            // Set things ready for getters
            Efficiency = curAlg.GetEfficiency();
            RunTime = curAlg.GetRunTime();
            Path = curAlg.GetPath();
            curAlg.Shout();

            // Debug code, show actual path
            if (curRequest.DrawPath)
            {
                //// Draw coverage
                //Bitmap CurBMP = new Bitmap(mDistReachable.Columns, mDistReachable.Rows);
                //ImgLib.MatrixToImage(ref mDistReachable, ref CurBMP);
                //frmMap map = new frmMap();
                //map.Text = "Actual UAV path";
                //map.setImage(CurBMP);
                //map.Show();
                //map.resetImage();
                //map.DrawCoverage(Path);
                //map.Refresh();

                //// Draw path
                //Bitmap CurBMP2 = new Bitmap(mDistReachable.Columns, mDistReachable.Rows);
                //ImgLib.MatrixToImage(ref mDistReachable, ref CurBMP2);
                //frmMap map2 = new frmMap();
                //map2.Text = "UAV trajectory simulation";
                //map2.setImage(CurBMP2);
                //map2.Show();
                //map2.resetImage();
                //map2.DrawPath(Path);
                //map2.Refresh();

                // Draw path with map remains
                Bitmap CurBMP3 = new Bitmap(mDistReachable.Columns, mDistReachable.Rows);
                ImgLib.MatrixToImage(ref mDistReachable, ref CurBMP3);
                frmMap map3 = new frmMap();
                map3.Text = "UAV trajectory and coverage";
                map3.setImage(CurBMP3);
                map3.Show();
                map3.resetImage();
                List<float> remains = curAlg.ShowCoverage();
                Color c = Color.FromArgb(255, 0, 0);
                for (int i=0; i<Path.Count; i++)
                {
                    Point p = Path[i];
                    map3.setPointColor(p, c);
                    map3.Refresh();
                    map3.setPointColor(p, remains[i]);
                    map3.Refresh();
                }                

                // Drawing real path
                MISCLib.ShowImage(MISCLib.DrawPath(Path), "Real Path");
            }

            // Log results
            curRequest.SetLog("Run time: " + curAlg.GetRunTime() + "\n");
            curRequest.SetLog("Best CDF: " + curAlg.GetCDF() + "\n");
            curRequest.SetLog("Best Efficiency: " + curAlg.GetEfficiency() + "\n");
        }

        #region Getters
        public double GetEfficiency()
        {
            return Efficiency;
        }
        public double GetRunTime()
        {
            return RunTime;
        }
        public List<Point> GetPath()
        {
            return Path;
        }
        #endregion

        #endregion




    }
}
