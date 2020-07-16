/* 
 * This code is adapted from KopernicusExpansion-Continued
 * Available from https://github.com/StollD/KopernicusExpansion-Continued
 */

using System;
using UnityEngine;

namespace RealSolarSystem
{
    /// <summary>
    /// A PQSMod that defines coastlines in a smoother way than stock VertexDefineCoast
    /// </summary>
    public class PQSMod_VertexDefineCoastSmooth : PQSMod
    {
        public double minHeightOffset;
        public double maxHeightOffset;
        public double slopeScale;

        private double minHeight;
        private double maxHeight;

        private void Reset()
        {
            minHeightOffset = -1.0;
            maxHeightOffset = 1.0;
            slopeScale = 1.0;
        }

        public override void OnSetup()
        {
            requirements = PQS.ModiferRequirements.MeshCustomNormals;
            minHeight = sphere.radius + minHeightOffset;
            maxHeight = sphere.radius + maxHeightOffset;
        }

        public override void OnVertexBuildHeight(PQS.VertexBuildData data)
        {
            if (data.vertHeight > minHeight && data.vertHeight < maxHeight)
            {
                // 7th order polynomial smoothstep.
                double x = (data.vertHeight - minHeight) / (maxHeight - minHeight);
                x = Math.Min(Math.Max(0.0, (x - 0.5) * slopeScale + 0.5), 1.0); // No Math.clamp?
                double y = -20.0 * Math.Pow(x, 7.0) + 70 * Math.Pow(x, 6.0) - 84.0 * Math.Pow(x, 5.0) + 35.0 * Math.Pow(x, 4.0);
                data.vertHeight = y * (maxHeight - minHeight) + minHeight;
            }
        }

        public override double GetVertexMaxHeight()
        {
            return maxHeightOffset;
        }

        public override double GetVertexMinHeight()
        {
            return minHeightOffset;
        }
    }
}
