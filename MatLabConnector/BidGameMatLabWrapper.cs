using BidGameMatLab;
using MathWorks.MATLAB.NET.Arrays;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MatLabConnector
{
    public class BidGameMatLabWrapper
    {
        private static readonly Lazy<BidGameMatLabWrapper> instance = new Lazy<BidGameMatLabWrapper>(() => new BidGameMatLabWrapper());
        private object _lock = new object();


        private Random random;
        private BidGame m;        

        public static BidGameMatLabWrapper Instance
        {
            get { return instance.Value; }
        }

        public BidGameMatLabWrapper()
        {
            random = new Random();
            m = new BidGame();
        }

        private BidGame M
        {
            get
            {
                return m;
            }
        }

        public double GetEstimate(string function)
        {
            return ((MWNumericArray)M.Estimate(function, random.NextDouble())).ToScalarDouble();
        }

        public void GetRobotValues(string function, int numofplayers, double p, double r, out double zj, out double bj)
        {
            double qj = random.NextDouble();
            zj = ((MWNumericArray)M.Estimate(function, qj)).ToScalarDouble();
            bj = ((MWNumericArray)M.RobotBid(function, GetZ0(function, p, r), zj, p, r, numofplayers)).ToScalarDouble();
        }

        public double GetZ0(string function, double p, double r)
        {
            return ((MWNumericArray)M.vbar(function, p, r)).ToScalarDouble();
        }

        public double[,] ProbabilityChart(string function, int min, int max)
        {
            MWNumericArray chart = null;

            chart = (MWNumericArray)M.ProbabilityChart(function, min, max);

            // Convert the magic square array to a two dimensional native double array
            double[,] nativeArray = (double[,])chart.ToArray(MWArrayComponent.Real);

            return nativeArray;
        }

        public Bitmap ProbabilityGraph(string function, int min, int max)
        {
            MWNumericArray imageData = null;
            try
            {
                imageData = (MWNumericArray)M.ProbabilityGraph(function, min, max);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return generateBitMap(imageData);
        }

        public Bitmap ProbabilityDensityGraph(string function, int min, int max)
        {
            MWNumericArray imageData = null;
            try
            {
                imageData = (MWNumericArray)M.ProbabilityDensityGraph(function, min, max);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return generateBitMap(imageData);
        }

        public double[,] RobotBidChart(string function, int min, int max, int numofplayers, double p, double r, int steps)
        {
            MWNumericArray chart = null;

            chart = (MWNumericArray)M.RobotBidChart(function, min, max, numofplayers, p, GetZ0(function, p, r), r, steps);

            // Convert the magic square array to a two dimensional native double array
            double[,] nativeArray = (double[,])chart.ToArray(MWArrayComponent.Real);

            return nativeArray;
        }

        public Bitmap RobotBidGraph(string function, int min, int max, int numofplayers, double p, double r)
        {
            MWNumericArray imageData = null;
            try
            {
                imageData = (MWNumericArray)M.RobotBidGraph(function, min, max, numofplayers, p, GetZ0(function, p, r), r);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return generateBitMap(imageData);
        }

        protected static Bitmap generateBitMap(MWNumericArray imageData)
        {
            Bitmap m = new Bitmap(imageData.Dimensions[1], imageData.Dimensions[0]);
            //Build up the bitmap from the MATLAB intesity data
            //MATLAB is using the default RGB color map with 
            //an alpha channel of 1.0 (255), so just shove the
            //data straight in

            BitmapData bmd = m.LockBits(new Rectangle(0, 0, imageData.Dimensions[1], imageData.Dimensions[0]),
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                m.PixelFormat);

            try
            {
                byte[,,] a = (byte[,,])imageData.ToArray(MWArrayComponent.Real);
                int pixelSize = 4;
                for (int idx = 0; idx < a.GetLength(1); idx++)
                {
                    for (int jdx = 0; jdx < a.GetLength(2); jdx++)
                    {
                        //swap x,y to account for MATLAB column/row orientation
                        Marshal.WriteByte(bmd.Scan0, (bmd.Stride * idx) + (pixelSize * jdx), a[2, idx, jdx]);
                        Marshal.WriteByte(bmd.Scan0, (bmd.Stride * idx) + (pixelSize * jdx + 1), a[1, idx, jdx]);
                        Marshal.WriteByte(bmd.Scan0, (bmd.Stride * idx) + (pixelSize * jdx + 2), a[0, idx, jdx]);
                        Marshal.WriteByte(bmd.Scan0, (bmd.Stride * idx) + (pixelSize * jdx + 3), 255);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            m.UnlockBits(bmd);

            return m;
        }


        public double[,] GenerateGameTable(int r, int z1, int z2, int z3, string f1, string f2, string f3)
        {
            MWNumericArray table = null;

            table = (MWNumericArray)M.GenerateGameTable(f1, f2, f3, z1, z2, z3, r);

            // Convert the magic square array to a two dimensional native double array
            double[,] nativeArray = (double[,])table.ToArray(MWArrayComponent.Real);

            return nativeArray;
        }

        public Bitmap RobotBid2Graph(double[,] data)
        {
            MWNumericArray imageData = null;
            try
            {
                MWNumericArray table = new MWNumericArray(data);
                imageData = (MWNumericArray)M.RobotBid2Graph(table);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return generateBitMap(imageData);
        }

        public Bitmap ProbabilityDensityGraph2(string f1, string f2, string f3, int z1, int z2, int z3)
        {
            MWNumericArray imageData = null;
            try
            {
                imageData = (MWNumericArray)M.ProbabilityDensityGraph2(f1, f2, f3, z1, z2, z3);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return generateBitMap(imageData);
        }

        public Bitmap ProbabilityGraph2(string f1, string f2, string f3, int z1, int z2, int z3)
        {
            MWNumericArray imageData = null;
            try
            {
                imageData = (MWNumericArray)M.ProbabilityGraph2(f1, f2, f3, z1, z2, z3);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return generateBitMap(imageData);
        }

        public double[,] ProbabilityChart2(string f1, string f2, string f3, int z1, int z2, int z3)
        {
            MWNumericArray chart = null;

            chart = (MWNumericArray)M.ProbabilityChart2(f1, f2, f3, z1, z2, z3);

            // Convert the magic square array to a two dimensional native double array
            double[,] nativeArray = (double[,])chart.ToArray(MWArrayComponent.Real);

            return nativeArray;
        }

    }
}