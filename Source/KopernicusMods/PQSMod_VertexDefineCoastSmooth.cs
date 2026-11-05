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

        // Legacy fixed-width ramp, ignored in adaptive mode. One width is either too soft on flat
        // coasts or so steep that every vertex lands on a plateau, which puts the waterline on the
        // vertex grid and turns it into a staircase.
        public double slopeScale;

        // Half-width of the land/water transition, in vertex spacings of the quad being built.
        // Positive enables adaptive mode, where the ramp is sized from the local height map
        // gradient so the coast crosses sea level over this distance whatever the terrain does.
        // Zero keeps the fixed slopeScale ramp.
        // Spacings and not metres: spacing doubles with every subdivision level below the maximum,
        // and the maximum moves with the terrain detail preset, so a width in metres would only be
        // right in a shell around the camera.
        public double coastSpacings;

        // Half-width of the central difference used to measure that gradient, in height map texels.
        public double gradientStencil;

        private double minHeight;
        private double maxHeight;
        private double invDepth;
        private double invRise;
        private bool isActive;

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

        // False when the mod has nothing valid to do, so the BurstPQS mod can skip it as well.
        public bool IsActive => isActive;

        // Resolved adaptive setup, read by the BurstPQS mod so both paths use the same map and the
        // same constants. Only meaningful while IsAdaptive is true.
        public bool IsAdaptive => isAdaptive;
        public MapSO AdaptiveHeightMap => heightMap;
        public double AdaptiveMapDeformity => mapDeformity;
        public double AdaptiveStencilU => du;
        public double AdaptiveStencilV => dv;
        public double AdaptiveMetresPerU => metresPerU;
        public double AdaptiveMetresPerV => metresPerV;

        /// <summary>
        /// Ramp half-width in metres of ground for a quad at the given subdivision level.
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
            isActive = false;

            if (minHeightOffset >= 0.0 || maxHeightOffset <= 0.0)
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

            // Reset() only runs in the editor, so an unset slopeScale is 0 and not the default
            // above. A zero ramp maps the whole band onto sea level, which is worse than nothing.
            if (!isAdaptive && slopeScale <= 0.0)
            {
                Debug.LogWarning($"[RealSolarSystem] VertexDefineCoastSmooth on {sphere.name} is inactive: adaptive mode is off and slopeScale is {slopeScale}");
                return;
            }

            isActive = true;

            // With different amplitudes either side, the mesh crosses sea level nearer the
            // shallower one, biasing the waterline off the height map contour by tens of metres.
            if (isAdaptive && Math.Abs(maxHeightOffset + minHeightOffset) > 1E-6)
            {
                Debug.LogWarning($"[RealSolarSystem] VertexDefineCoastSmooth on {sphere.name}:"
                    + $" asymmetric band [{minHeightOffset}, {maxHeightOffset}] displaces the waterline"
                    + " off the height map contour; prefer equal offsets");
            }
        }

        public override void OnVertexBuildHeight(PQS.VertexBuildData data)
        {
            if (!isActive)
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
                // Grading over a fixed number of vertex spacings gives low-detail quads further out
                // a proportionally wider ramp, instead of collapsing them onto the plateaus.
                // buildQuad is null on the GetSurfaceHeight path, which has no mesh to grade.
                double spacing = maxLevelSpacing;
                if (data.buildQuad != null)
                {
                    int level = data.buildQuad.subdivision;
                    if (level >= 0 && level < levelSpacing.Length)
                    {
                        spacing = levelSpacing[level];
                    }
                }

                // Height the terrain gains over that distance. Capping at the band makes the ramp
                // finish exactly on the plateau. The product is grouped to match the Burst path,
                // which folds it into a per-quad width; float multiply is not associative.
                double window = GetLocalGradient(data) * (coastSpacings * spacing);
                window = Math.Min(window, height < 0.0 ? -minHeightOffset : maxHeightOffset);
                // Not Math.Sign, which throws on NaN, and a NaN height passes the band test above.
                t = window > 0.0 ? height / window : (height < 0.0 ? -1.0 : (height > 0.0 ? 1.0 : 0.0));
            }
            else
            {
                // slopeScale below 1 leaves a step at the band edges.
                t = (height < 0.0 ? height * invDepth : height * invRise) * slopeScale;
            }
            t = Math.Min(Math.Max(-1.0, t), 1.0);

            // Odd extension of the 7th order smoothstep onto [-1, 1], i.e. 2 * S((t + 1) / 2) - 1.
            // Sea level is an exact fixed point, so the waterline stays on the height map contour.
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
            // GetPixelFloat wraps both axes. Right for longitude, but in v it would jump the pole,
            // so keep the stencil inside the map.
            double v = Math.Min(Math.Max(data.v, dv), 1.0 - dv);

            double dHdu = mapDeformity * invTwoDu *
                (heightMap.GetPixelFloat(data.u + du, v) - heightMap.GetPixelFloat(data.u - du, v));
            double dHdv = mapDeformity * invTwoDv *
                (heightMap.GetPixelFloat(data.u, v + dv) - heightMap.GetPixelFloat(data.u, v - dv));

            // u spans the circumference, v pole to pole, and meridians converge with latitude.
            // directionFromCenter is a unit radial, so its horizontal length is exactly cos(lat).
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
        /// Vertex spacing per subdivision level. A quad at level L spans (pi * R / 2) / 2^L across
        /// cacheSideVertCount vertices, so spacing doubles for each level below the maximum.
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
        /// Finds the VertexHeightMap the coastline comes from. Sampling the map instead of
        /// differencing the mesh keeps the gradient independent of subdivision level, so the
        /// waterline stays put as quads split and only the steepness either side changes.
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
                Debug.LogWarning($"[RealSolarSystem] VertexDefineCoastSmooth on {sphere.name}: coastSpacings is set but no VertexHeightMap was found, falling back to fixed slopeScale");
                return false;
            }

            heightMap = best.heightMap;
            // The stock mod adds offset + deformity * pixel. The offset is constant and cancels in
            // a difference, so only the deformity scales the gradient.
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
