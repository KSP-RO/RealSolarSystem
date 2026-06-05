using System;
using BurstPQS;
using BurstPQS.Util;
using RealSolarSystem;
using Unity.Burst;

[BurstCompile]
[BatchPQSMod(typeof(PQSMod_VertexDefineCoastSmooth))]
public class BatchPQSMod_VertexDefineCoastSmooth : BatchPQSMod<PQSMod_VertexDefineCoastSmooth>
{
    public BatchPQSMod_VertexDefineCoastSmooth(PQSMod_VertexDefineCoastSmooth mod) : base(mod)
    {
    }

    public override void OnQuadPreBuild(PQ quad, BatchPQSJobSet jobSet)
    {
        base.OnQuadPreBuild(quad, jobSet);
        jobSet.Add(new BuildJob
        {
            minHeightOffset = Mod.minHeightOffset,
            maxHeightOffset = Mod.maxHeightOffset,
            slopeScale = Mod.slopeScale,
        });
    }

    [BurstCompile]
    struct BuildJob : IBatchPQSHeightJob
    {
        public double minHeightOffset;
        public double maxHeightOffset;
        public double slopeScale;

        public void BuildHeights(in BuildHeightsData data)
        {
            var minHeight = data.sphere.radius + minHeightOffset;
            var maxHeight = data.sphere.radius + maxHeightOffset;

            for (int i = 0; i < data.VertexCount; ++i)
            {
                if (data.vertHeight[i] > minHeight && data.vertHeight[i] < maxHeight)
                {
                    // 7th order polynomial smoothstep.
                    double x = (data.vertHeight[i] - minHeight) / (maxHeight - minHeight);
                    x = MathUtil.Clamp01((x - 0.5) * slopeScale + 0.5);
                    double y = -20.0 * Math.Pow(x, 7.0) + 70 * Math.Pow(x, 6.0) - 84.0 * Math.Pow(x, 5.0) +
                               35.0 * Math.Pow(x, 4.0);
                    data.vertHeight[i] = y * (maxHeight - minHeight) + minHeight;
                }
            }
        }
    }
}