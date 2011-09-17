using System;
using System.Collections.Generic;
using System.Text;
using rtwmatrix;
using System.Drawing;

namespace IPPA
{
    class ImgLib
    {

        // Static function to convert bitmap image object to matrix
        public static RtwMatrix ImageToMatrix(ref Bitmap bm)
        {
            RtwMatrix m = new RtwMatrix(bm.Height, bm.Width);
            for (int y = 0; y < bm.Size.Height; y++)
            {
                for (int x = 0; x < bm.Size.Width; x++)
                {
                    Color c = bm.GetPixel(x, y);
                    m[y, x] = (float)c.R;
                }
            }
            return m;
        }

        // Static function to convert matrix back to gray bitmap image
        public static void MatrixToImage(ref RtwMatrix m, ref Bitmap bm)
        {
            for (int y = 0; y < m.Rows; y++)
            {
                for (int x = 0; x < m.Columns; x++)
                {
                    int grayScale = Convert.ToInt32(m[y, x]);
                    
                    Color c = Color.FromArgb(grayScale, grayScale, grayScale);
                    bm.SetPixel(x, y, c);
                }
            }
        }

        /*
        public static RGB HSVtoRGB(HSV HSV)
        {
            // HSV contains values scaled as in the color wheel:
            // that is, all from 0 to 255.

            // for ( this code to work, HSV.Hue needs
            // to be scaled from 0 to 360 (it//s the angle of the selected
            // point within the circle). HSV.Saturation and HSV.value must be
            // scaled to be between 0 and 1.

            double h;
            double s;
            double v;

            double r = 0;
            double g = 0;
            double b = 0;

            // Scale Hue to be between 0 and 360. Saturation
            // and value scale to be between 0 and 1.
            h = ((double)HSV.Hue / 255 * 360) % 360;
            s = (double)HSV.Saturation / 255;
            v = (double)HSV.value / 255;

            if (s == 0)
            {
                // If s is 0, all colors are the same.
                // This is some flavor of gray.
                r = v;
                g = v;
                b = v;
            }
            else
            {
                double p;
                double q;
                double t;

                double fractionalSector;
                int sectorNumber;
                double sectorPos;

                // The color wheel consists of 6 sectors.
                // Figure out which sector you//re in.
                sectorPos = h / 60;
                sectorNumber = (int)(Math.Floor(sectorPos));

                // get the fractional part of the sector.
                // That is, how many degrees into the sector
                // are you?
                fractionalSector = sectorPos - sectorNumber;

                // Calculate values for the three axes
                // of the color.
                p = v * (1 - s);
                q = v * (1 - (s * fractionalSector));
                t = v * (1 - (s * (1 - fractionalSector)));

                // Assign the fractional colors to r, g, and b
                // based on the sector the angle is in.
                switch (sectorNumber)
                {
                    case 0:
                        r = v;
                        g = t;
                        b = p;
                        break;

                    case 1:
                        r = q;
                        g = v;
                        b = p;
                        break;

                    case 2:
                        r = p;
                        g = v;
                        b = t;
                        break;

                    case 3:
                        r = p;
                        g = q;
                        b = v;
                        break;

                    case 4:
                        r = t;
                        g = p;
                        b = v;
                        break;

                    case 5:
                        r = v;
                        g = p;
                        b = q;
                        break;
                }
            }
            // return an RGB structure, with values scaled
            // to be between 0 and 255.
            return new RGB((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }
        */

        // Scale image values between 0 and 255
        public static void ScaleImageValues(ref RtwMatrix imgin)
        {
            float[] MinMax = imgin.MinMaxValue();
            float max = MinMax[1];
            if (max != 0)
            {
                for (int y = 0; y < imgin.Rows; y++)
                {
                    for (int x = 0; x < imgin.Columns; x++)
                    {
                        imgin[y, x] = imgin[y, x] / max * 255;
                    }
                }
            }
            else
            {
                for (int y = 0; y < imgin.Rows; y++)
                {
                    for (int x = 0; x < imgin.Columns; x++)
                    {
                        imgin[y, x] = imgin[y, x] / max * 255;
                    }
                }
            }
        }
    }
}
