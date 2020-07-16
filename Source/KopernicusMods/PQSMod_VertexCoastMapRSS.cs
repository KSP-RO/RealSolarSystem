/* 
 * This code is adapted from KopernicusExpansion-Continued
 * Available from https://github.com/StollD/KopernicusExpansion-Continued
 */

using System;
using UnityEngine;

namespace RealSolarSystem
{
    /// <summary>
    /// A heightmap PQSMod that can parse encoded 16bpp textures
    /// </summary>
    public class PQSMod_VertexCoastMapRSS : PQSMod_VertexHeightMap
    {
        private double[] xcoeffs = new double[4];
        private double[] ycoeffs = new double[4];

        public override void OnVertexBuildHeight(PQS.VertexBuildData data)
        {
            try
            {
                // Lanczos resample
                double xpos = data.u * (heightMap.Width - 1);
                double ypos = data.v * (heightMap.Height - 1);
                for (int i = -1; i <= 2; ++i)
                {
                    double x = xpos - (Math.Floor(xpos) + i);
                    if (x == 0.0)
                        xcoeffs[i + 1] = 1.0;
                    else if (x >= -2.0 && x < 2)
                        xcoeffs[i + 1] = -2.0 * Math.Sin(Math.PI * x) * Math.Sin(Math.PI * x / -2) / (Math.PI * Math.PI * x * x);
                    else
                        xcoeffs[i + 1] = 0;

                    double y = ypos - (Math.Floor(ypos) + i);
                    if (y == 0.0)
                        ycoeffs[i + 1] = 1.0;
                    else if (y >= -2.0 && y < 2)
                        ycoeffs[i + 1] = -2.0 * Math.Sin(Math.PI * y) * Math.Sin(Math.PI * y / -2) / (Math.PI * Math.PI * y * y);
                    else
                        ycoeffs[i + 1] = 0;
                }

                // Get the height data from the terrain
                double height = 0.0;
                double sum = 0.0;
                for (int y = -1; y <= 2; ++y)
                {
                    for (int x = -1; x <= 2; ++x)
                    {
                        double h = heightMap.GetPixelFloat((int)xpos + x, (int)ypos + y);
                        h *= xcoeffs[x + 1] * ycoeffs[y + 1];
                        sum += xcoeffs[x + 1] * ycoeffs[y + 1];
                        height += h;
                    }
                }

                height /= sum;

                // Apply it
                data.vertHeight += heightMapOffset + heightMapDeformity * height;
            }
            catch (NullReferenceException e)
            {
                Debug.LogError("Caught NRE in VertexCoastMapRSS");
            }
        }
    }
}
