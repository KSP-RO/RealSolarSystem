using System;
using BurstPQS;
using BurstPQS.Map;
using RealSolarSystem;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[BatchPQSMod(typeof(PQSMod_VertexDefineCoastSmooth))]
public class BatchPQSMod_VertexDefineCoastSmooth : BatchPQSMod<PQSMod_VertexDefineCoastSmooth>
{
    private bool _mapSupported;

    public BatchPQSMod_VertexDefineCoastSmooth(PQSMod_VertexDefineCoastSmooth mod) : base(mod)
    {
    }

    public override void OnSetup()
    {
        base.OnSetup();

        // The stock mod has already resolved which height map the coastline comes from; all this
        // has to decide is whether BurstPQS can read it. If it cannot, fall back to the fixed
        // slopeScale ramp and say so, because silently dropping adaptive mode here would make the
        // terrain depend on whether BurstPQS happens to be installed.
        _mapSupported = Mod.IsAdaptive
            && Mod.AdaptiveHeightMap != null
            && BurstMapSO.IsSupported(Mod.AdaptiveHeightMap);

        if (Mod.IsAdaptive && !_mapSupported)
        {
            Debug.LogWarning("[RealSolarSystem] VertexDefineCoastSmooth: height map"
                + $" {Mod.AdaptiveHeightMap?.GetType().Name ?? "(none)"} is not supported by BurstPQS,"
                + " falling back to fixed slopeScale");
        }
    }

    public override void OnQuadPreBuild(PQ quad, BatchPQSJobSet jobSet)
    {
        base.OnQuadPreBuild(quad, jobSet);

        double minHeightOffset = Mod.minHeightOffset;
        double maxHeightOffset = Mod.maxHeightOffset;
        if (minHeightOffset >= 0.0 || maxHeightOffset <= 0.0)
        {
            // Band does not straddle sea level; the stock mod logs this and goes inert.
            return;
        }

        BuildJob job = new BuildJob
        {
            minHeightOffset = minHeightOffset,
            maxHeightOffset = maxHeightOffset,
            invDepth = -1.0 / minHeightOffset,
            invRise = 1.0 / maxHeightOffset,
            slopeScale = Mod.slopeScale,
        };

        if (_mapSupported)
        {
            double du = Mod.AdaptiveStencilU;
            double dv = Mod.AdaptiveStencilV;

            job.isAdaptive = true;
            job.heightMap = BurstMapSO.Create(Mod.AdaptiveHeightMap);
            job.mapDeformity = Mod.AdaptiveMapDeformity;
            job.du = du;
            job.dv = dv;
            job.invTwoDu = 1.0 / (2.0 * du);
            job.invTwoDv = 1.0 / (2.0 * dv);
            job.metresPerU = Mod.AdaptiveMetresPerU;
            job.metresPerV = Mod.AdaptiveMetresPerV;

            // The subdivision level is known here, so the ramp width is resolved once per quad
            // instead of once per vertex the way the stock path has to do it.
            job.rampWidth = Mod.AdaptiveRampWidth(quad.subdivision);
        }

        jobSet.Add(job);
    }

    [BurstCompile]
    struct BuildJob : IBatchPQSHeightJob, IDisposable
    {
        public double minHeightOffset;
        public double maxHeightOffset;
        public double invDepth;
        public double invRise;
        public double slopeScale;

        public bool isAdaptive;
        public BurstMapSO heightMap;
        public double mapDeformity;
        public double du;
        public double dv;
        public double invTwoDu;
        public double invTwoDv;
        public double metresPerU;
        public double metresPerV;
        public double rampWidth;

        public void BuildHeights(in BuildHeightsData data)
        {
            double radius = data.sphere.radius;
            double minHeight = radius + minHeightOffset;
            double maxHeight = radius + maxHeightOffset;

            // Each of these properties returns the span by value, so hoist them out of the loop.
            var vertHeight = data.vertHeight;
            var uCoord = data.u;
            var vCoord = data.v;
            var direction = data.directionFromCenter;

            for (int i = 0; i < data.VertexCount; ++i)
            {
                double height = vertHeight[i];
                if (height <= minHeight || height >= maxHeight)
                {
                    continue;
                }

                height -= radius;

                // Signed position within the band: sea level at 0, the two band edges at -1 and 1.
                double t;
                if (isAdaptive)
                {
                    // Raw height the terrain gains over rampWidth metres of ground. Capping it at
                    // the band keeps the ramp finishing exactly on the plateau, with no step.
                    double window = Gradient(uCoord[i], vCoord[i], direction[i]) * rampWidth;
                    window = Math.Min(window, height < 0.0 ? -minHeightOffset : maxHeightOffset);
                    t = window > 0.0 ? height / window : (height < 0.0 ? -1.0 : (height > 0.0 ? 1.0 : 0.0));
                }
                else
                {
                    t = (height < 0.0 ? height * invDepth : height * invRise) * slopeScale;
                }
                t = Math.Min(Math.Max(-1.0, t), 1.0);

                // Odd extension of the 7th order smoothstep onto [-1, 1], i.e. 2 * S((t + 1) / 2) - 1.
                // Sea level is an exact fixed point of this, so the waterline stays on the height
                // map's own contour instead of drifting.
                double x = (t + 1.0) * 0.5;
                double x2 = x * x;
                double s = 2.0 * (x2 * x2 * (35.0 - 84.0 * x + 70.0 * x2 - 20.0 * x2 * x)) - 1.0;

                vertHeight[i] = radius + (s < 0.0 ? -s * minHeightOffset : s * maxHeightOffset);
            }
        }

        /// <summary>
        /// Magnitude of the height map's slope at this vertex, in metres of rise per metre
        /// travelled. Must stay in step with PQSMod_VertexDefineCoastSmooth.LocalGradient.
        /// </summary>
        private double Gradient(double u, double v, Vector3d dir)
        {
            // GetPixelFloat wraps both axes, which is right for longitude but would jump across the
            // pole in v, so keep the stencil inside the map vertically.
            v = Math.Min(Math.Max(v, dv), 1.0 - dv);

            double dHdu = mapDeformity * invTwoDu *
                (heightMap.GetPixelFloat(u + du, v) - heightMap.GetPixelFloat(u - du, v));
            double dHdv = mapDeformity * invTwoDv *
                (heightMap.GetPixelFloat(u, v + dv) - heightMap.GetPixelFloat(u, v - dv));

            // directionFromCenter is a unit radial, so the length of its horizontal part is exactly
            // cos(latitude), which is cheaper than Math.Cos and avoids needing the latitude span.
            double cosLat = Math.Sqrt(dir.x * dir.x + dir.z * dir.z);
            if (cosLat < 1E-3)
            {
                cosLat = 1E-3;
            }

            double gu = dHdu / (metresPerU * cosLat);
            double gv = dHdv / metresPerV;
            return Math.Sqrt(gu * gu + gv * gv);
        }

        public void Dispose()
        {
            if (isAdaptive)
            {
                heightMap.Dispose();
            }
        }
    }
}
