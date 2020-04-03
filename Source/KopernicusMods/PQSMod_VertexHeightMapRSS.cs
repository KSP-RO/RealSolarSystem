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
    public class PQSMod_VertexHeightMapRSS : PQSMod_VertexHeightMap
    {
        public override void OnVertexBuildHeight(PQS.VertexBuildData data)
        {
            // Get the HeightAlpha, not the Float-Value from the Map
            MapSO.HeightAlpha ha = heightMap.GetPixelHeightAlpha(data.u, data.v);
                
            // Get the height data from the terrain
            Double height = (ha.height + ha.alpha * (Double)Byte.MaxValue) / (Double)(Byte.MaxValue + 1);
                
            // Apply it
            data.vertHeight += heightMapOffset + heightMapDeformity * height;
        }
    }
}
