using System;
using System.Collections.Generic;
using System.Text;

namespace IPPA
{
    class ProjectConstants
    {
        // Whether to print debug iformation to server activity log
        public static bool DebugMode = true;
        
        // Whether to print out CDF for graphs and charts
        public static bool GraphCDF = false;

        // Max number of times to run Accord.net if it fails
        public static int MaxAccordRun = 3;
        
        // General parameters
        public static int DefaultDimension = 100;
        public static int DefaultWidth = 100;
        public static int DefaultHeight = 100;
        public static int MinFlightTime = 10;
        public static int MaxFlightTime = 3600;
        public static int DefaultFlightTime = 1800;
        public static string MapsDir = @"C:\Lanny\MAMI\IPPA\Maps";
        // public static string DefaultDistMap = @"C:\Lanny\MAMI\IPPA\Maps\DistMaps\Simple_BimodalFar.csv";
        // public static string DefaultDistMap = @"C:\Lanny\MAMI\IPPA\Maps\DistMaps\TestDistMap.csv";
        // public static string DefaultDistMap = @"C:\Lanny\MAMI\IPPA\Maps\DistMaps\Smoothed_Small_HikerPaulDist.csv";
        // public static string DefaultDistMap = @"C:\Lanny\MAMI\IPPA\Maps\DistMaps\Smoothed_Small_NewYork53Dist.csv";
        // public static string DefaultDistMap = @"C:\Lanny\MAMI\IPPA\Maps\DistMaps\TestDistMap100.csv";
        public static string DefaultDistMap = @"C:\Lanny\MAMI\PathPlanner\PathPlanner\UserStudyData\TestDistMap1.csv";

        // public static string DefaultDiffMap = @"C:\Lanny\MAMI\IPPA\Maps\DiffMaps\TestDiffMap.csv";
        // public static string DefaultDiffMap = @"C:\Lanny\MAMI\IPPA\Maps\DiffMaps\Small_HikerPaulDiff.csv";
        // public static string DefaultDiffMap = @"C:\Lanny\MAMI\IPPA\Maps\DiffMaps\Small_NewYork53Diff.csv";
        // public static string DefaultDiffMap = @"C:\Lanny\MAMI\IPPA\Maps\DiffMaps\TestDiffMap100_012.csv";
        public static string DefaultDiffMap = @"C:\Lanny\MAMI\PathPlanner\PathPlanner\UserStudyData\TestDiffMap1.csv";
        
        public static int DefaultStartX = 50;
        public static int DefaultStartY = 50;
        
        // Down sample scale in CountDistModes class
        public static int DownSampleScale = 1;

        // Global Warming parameters
        public static int GWCount = 20;             // How many extensive GWs to search for
        public static int ConvCount = 3 ;           // How many convolution kernals to use
        public static int PFCount = 6;              // How many PF discount functions to use
        public static int CTFGWCoraseLevel = 4;     // How many GW to perform at each coarse-to-fine level
        public static int CTFGWLevelCount = 3;      // How many coarse-to-fine levels to search for

        // PF parameters
        public static int PFLoop = 12;              // How many time we should run PF with different sigmas
        public static int PFStep = 6;               // How much to increase sigma in each step

        // EA parameters
        public static int Count_CC = 5;
        public static int Count_LHCGWCONV = 5;
        public static int Count_LHCGWPF = 0;
        public static int Count_LHCRandom = 10;
        public static int Count_TopTwoH = 5;
        public static int Count_TopNH = 5;

        public static int EA_Population = 100;
        public static float EA_ReplacementRate = 0.3f;
        public static float EA_MutationRate = 0.50f;
        public static int EA_BestToKeep = 3;
        public static int EA_Crossover_MidSize = 5;
        public static int EA_Minimum_Run = 300;
        public static int EA_Maximum_Run = 1000;
        public static int EA_Epsilon_Run = 100;

        // EA_E parameters
        public static int EA_E_Population = 100;
        public static float EA_E_ReplacementRate = 0.3f;
        public static float EA_E_MutationRate = 0.50f;
        public static int EA_E_BestToKeep = 3;
        public static int EA_E_Crossover_MidSize = 5;
        public static int EA_E_Minimum_Run = 300;
        public static int EA_E_Maximum_Run = 1000;
        public static int EA_E_Epsilon_Run = 100;
        public static int EA_E_Mutate_MidSize = 5;

        // TopTwo parameters
        public static int SearchResolution = 40;    // How many searches
        public static int CTFTTCoraseLevel = 4;     // How many searches to perform at each coarse-to-fine level
        public static int CTFTTLevelCount = 6;      // How many coarse-to-fine levels to search for
        public static bool LogDistanceRatio = true; // Whether to use Log for distance ratio when identifying top regions
        public static bool UseAccordProportions = true; // Whether to use the proportions values generated from Accord.NET GMM
        public static bool UseAreaInHeuristic = true;  // Whether to use area in Mode Goodness Ratio heuristic.

        // TopN parameters
        public static int TopNCount = 3;            // Default N
        public static int Kernel_Size = 5;          // What size kenel for convolution       
        public static int Max_N = 5;                // Maximum number of Gaussians to fit 

        // When called from self, because scale to 255, so set to 25.
        // When called from PathPlanner Unity, because scale to 4, so set to 0.4;
        public static float DownSample_Rate = 0.4f;     // Only use a percentage of the samples (from probability distribution map)

        // Max wait time (*10)
        public static int MaxWaitTime = 1500;            // 15000 milisecond, so 15 seconds.
    }
}
