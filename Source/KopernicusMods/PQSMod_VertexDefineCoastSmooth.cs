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

        // Legacy fixed slopeScale behaviour. Has no effect in adaptive mode.
        // This is unavoidably either too soft on flat coasts or steep enough to quantise every vertex onto the two
        // plateaus - and once that happens the waterline snaps to the vertex grid's edge midpoints
        // and turns into a staircase.
        public double slopeScale;

        // Target half-width of the land/water transition, in vertex spacings of the quad being
        // built. When positive the mod runs in adaptive mode: the ramp is sized from the local
        // height map gradient so that the coast always crosses sea level over roughly this
        // distance, whatever the terrain does. Zero (the default) keeps the fixed slopeScale behaviour.
        //
        // Spacings rather than metres because a quad's vertex spacing doubles for every subdivision
        // level below the maximum, and the maximum itself moves with the terrain detail preset. A
        // width fixed in metres is only correct in a shell around the camera.
        public double coastSpacings;

        // Half-width of the central difference used to measure that gradient, in height map texels.
        public double gradientStencil;

        private double minHeight;
        private double maxHeight;
        private double invDepth;
        private double invRise;
        private bool bandCrossesSeaLevel;

        private bool isAdaptive;
        private MapSO heightMap;
        private double mapDeformity;
        private double du;
        private double dv;
        private double invTwoDu;
        private double invTwoDv;
        private double metresPerU;
        private double metresPerV;
        private double[] levelSpacing;
        private double maxLevelSpacing;

        // Resolved adaptive setup. Exposed so the Burst path can mirror this mod exactly instead of
        // repeating the height map search and re-deriving the constants - the two implementations
        // have to agree vertex for vertex or terrain changes depending on whether BurstPQS is
        // installed. Only meaningful while IsAdaptive is true.
        public bool IsAdaptive => isAdaptive;
        public MapSO AdaptiveHeightMap => heightMap;
        public double AdaptiveMapDeformity => mapDeformity;
        public double AdaptiveStencilU => du;
        public double AdaptiveStencilV => dv;
        public double AdaptiveMetresPerU => metresPerU;
        public double AdaptiveMetresPerV => metresPerV;

        /// <summary>
        /// Half-width of the ramp in metres of ground for a quad at the given subdivision level.
        /// The Burst path resolves this once per quad rather than per vertex, since it is handed
        /// the quad up front.
        /// </summary>
        public double AdaptiveRampWidth(int subdivision)
        {
            if (levelSpacing == null)
            {
                return 0.0;
            }

            double spacing = subdivision >= 0 && subdivision < levelSpacing.Length
                ? levelSpacing[subdivision]
                : maxLevelSpacing;
            return coastSpacings * spacing;
        }

        private void Reset()
        {
            minHeightOffset = -1.0;
            maxHeightOffset = 1.0;
            slopeScale = 1.0;
            coastSpacings = 0.0;
            gradientStencil = 1.0;
        }

        public override void OnSetup()
        {
            requirements = PQS.ModiferRequirements.MeshCustomNormals;
            minHeight = sphere.radius + minHeightOffset;
            maxHeight = sphere.radius + maxHeightOffset;
            isAdaptive = false;

            // The two halves of the band are scaled independently, so the offsets need not be
            // symmetric. Dropping the seabed further than the land rises steepens the waterline
            // without turning coastal lowland into a mesa - the deep side is hidden under the ocean.
            bandCrossesSeaLevel = minHeightOffset < 0.0 && maxHeightOffset > 0.0;
            if (!bandCrossesSeaLevel)
            {
                Debug.LogWarning($"[RealSolarSystem] VertexDefineCoastSmooth on {sphere.name} is inactive: band [{minHeightOffset}, {maxHeightOffset}] does not cross sea level");
                return;
            }

            invDepth = -1.0 / minHeightOffset;
            invRise = 1.0 / maxHeightOffset;

            if (coastSpacings > 0.0)
            {
                isAdaptive = BindHeightMap();
                if (isAdaptive)
                {
                    requirements |= PQS.ModiferRequirements.VertexMapCoords;
                    BuildSpacingTable();
                }
            }

            // The rendered waterline sits where the mesh crosses sea level, and with different
            // amplitudes either side the crossing is pulled towards the shallower one - a constant
            // bias of tens of metres that no ramp width fixes. Keep the band symmetric.
            if (isAdaptive && Math.Abs(maxHeightOffset + minHeightOffset) > 1E-6)
            {
                Debug.LogWarning($"[RealSolarSystem] VertexDefineCoastSmooth on {sphere.name}:"
                    + $" asymmetric band [{minHeightOffset}, {maxHeightOffset}] displaces the waterline"
                    + " off the height map contour; prefer equal offsets");
            }
        }

        public override void OnVertexBuildHeight(PQS.VertexBuildData data)
        {
            if (!bandCrossesSeaLevel)
            {
                return;
            }

            if (data.vertHeight <= minHeight || data.vertHeight >= maxHeight)
            {
                return;
            }

            double height = data.vertHeight - sphere.radius;

            // Signed position within the band: sea level at 0, the two band edges at -1 and 1.
            double t;
            if (isAdaptive)
            {
                // Grade the coast over a fixed number of vertex spacings of whatever quad is being
                // built, so distant low-detail quads get a proportionally wider ramp instead of
                // collapsing onto the plateaus. buildQuad is null on the GetSurfaceHeight path,
                // which has no mesh to grade, so answer those at the finest level.
                double spacing = maxLevelSpacing;
                if (data.buildQuad != null)
                {
                    int level = data.buildQuad.subdivision;
                    if (level >= 0 && level < levelSpacing.Length)
                    {
                        spacing = levelSpacing[level];
                    }
                }

                // Raw height the terrain gains over that distance of ground. Capping it at the band
                // keeps the ramp finishing exactly on the plateau, with no step at the edge.
                // Grouped so this is the same product the Burst path folds into its per-quad ramp
                // width; multiplication is not associative in floating point and the two
                // implementations have to agree to the last bit.
                double window = GetLocalGradient(data) * (coastSpacings * spacing);
                window = Math.Min(window, height < 0.0 ? -minHeightOffset : maxHeightOffset);
                // Not Math.Sign, which throws on NaN - and a NaN height slips through the band test
                // above, since every comparison against NaN is false.
                t = window > 0.0 ? height / window : (height < 0.0 ? -1.0 : (height > 0.0 ? 1.0 : 0.0));
            }
            else
            {
                // slopeScale below 1 leaves a step at the band edges, same as it always has.
                t = (height < 0.0 ? height * invDepth : height * invRise) * slopeScale;
            }
            t = Math.Min(Math.Max(-1.0, t), 1.0);

            // Odd extension of the 7th order smoothstep onto [-1, 1], i.e. 2 * S((t + 1) / 2) - 1.
            // Sea level is an exact fixed point of this, so the waterline stays on the height
            // map's own contour instead of drifting.
            double x = (t + 1.0) * 0.5;
            double x2 = x * x;
            double s = 2.0 * (x2 * x2 * (35.0 - 84.0 * x + 70.0 * x2 - 20.0 * x2 * x)) - 1.0;

            data.vertHeight = sphere.radius + (s < 0.0 ? -s * minHeightOffset : s * maxHeightOffset);
        }

        public override double GetVertexMaxHeight()
        {
            return maxHeightOffset;
        }

        public override double GetVertexMinHeight()
        {
            return minHeightOffset;
        }

        /// <summary>
        /// Magnitude of the height map's slope at this vertex, in metres of rise per metre travelled.
        /// </summary>
        private double GetLocalGradient(PQS.VertexBuildData data)
        {
            // GetPixelFloat wraps both axes, which is right for longitude but would jump across the
            // pole in v, so keep the stencil inside the map vertically.
            double v = Math.Min(Math.Max(data.v, dv), 1.0 - dv);

            double dHdu = mapDeformity * invTwoDu *
                (heightMap.GetPixelFloat(data.u + du, v) - heightMap.GetPixelFloat(data.u - du, v));
            double dHdv = mapDeformity * invTwoDv *
                (heightMap.GetPixelFloat(data.u, v + dv) - heightMap.GetPixelFloat(data.u, v - dv));

            // u spans the full circumference, v spans pole to pole; meridians converge with latitude.
            // directionFromCenter is a unit radial, so the length of its horizontal part is exactly cos(latitude)
            Vector3d dir = data.directionFromCenter;
            double cosLat = Math.Sqrt(dir.x * dir.x + dir.z * dir.z);
            if (cosLat < 1E-3)
            {
                cosLat = 1E-3;
            }

            double gu = dHdu / (metresPerU * cosLat);
            double gv = dHdv / metresPerV;
            return Math.Sqrt(gu * gu + gv * gv);
        }

        /// <summary>
        /// Vertex spacing for every subdivision level. A quad at level L spans (pi * R / 2) / 2^L
        /// and carries cacheSideVertCount vertices per side, so spacing doubles for each level
        /// below the maximum - which is exactly how far a coast has to be graded to stay smooth.
        /// </summary>
        private void BuildSpacingTable()
        {
            int intervals = PQS.cacheSideVertCount > 1 ? PQS.cacheSideVertCount - 1 : 1;
            double rootEdge = Math.PI * sphere.radius * 0.5;

            levelSpacing = new double[sphere.maxLevel + 1];
            for (int level = 0; level <= sphere.maxLevel; level++)
            {
                levelSpacing[level] = rootEdge / (1 << level) / intervals;
            }
            maxLevelSpacing = levelSpacing[sphere.maxLevel];
        }

        /// <summary>
        /// Locates the VertexHeightMap this body's coastline comes from, so the ramp can be sized
        /// from the map's own gradient. Sampling the map rather than differencing the mesh keeps the
        /// measured gradient independent of subdivision level, so the waterline itself stays put as
        /// quads split - only the steepness either side of it changes.
        /// </summary>
        private bool BindHeightMap()
        {
            PQSMod_VertexHeightMap best = null;
            foreach (PQSMod_VertexHeightMap mod in sphere.GetComponentsInChildren<PQSMod_VertexHeightMap>(true))
            {
                // Mirror the filtering PQS itself applies when it assembles the mod list.
                if (mod.heightMap == null || !mod.modEnabled || !mod.gameObject.activeSelf)
                {
                    continue;
                }
                if (best == null || mod.order < best.order)
                {
                    best = mod;
                }
            }

            if (best == null)
            {
                Debug.LogWarning($"[RealSolarSystem] VertexDefineCoastSmooth on {sphere.name}: coastWidth is set but no VertexHeightMap was found, falling back to fixed slopeScale");
                return false;
            }

            heightMap = best.heightMap;
            // The stock mod adds heightMapOffset + heightMapDeformity * pixel; the offset is constant
            // and drops out of a difference, so only the deformity scales the gradient.
            mapDeformity = best.heightMapDeformity;

            double stencil = gradientStencil > 0.0 ? gradientStencil : 1.0;
            du = stencil / heightMap.Width;
            dv = stencil / heightMap.Height;
            invTwoDu = 1.0 / (2.0 * du);
            invTwoDv = 1.0 / (2.0 * dv);
            metresPerU = 2.0 * Math.PI * sphere.radius;
            metresPerV = Math.PI * sphere.radius;
            return true;
        }
    }
}
