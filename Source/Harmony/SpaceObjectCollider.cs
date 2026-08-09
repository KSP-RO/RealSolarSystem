using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace RealSolarSystem.Harmony
{
    [HarmonyPatch(typeof(SpaceObjectCollider))]
    internal class PatchSpaceObjectCollider
    {
        // Set true to log one line per generated space object.
        // It shows which mesh the colliders were actually built from.
        internal static bool LogColliderGen = false;

        private const float MaxGenFrames = 120f;

        // Generation is throttled by distance, and the stock thresholds are absolute metres. 2500m
        // sits barely clear of the surface of a kilometre-scale asteroid, so the whole approach
        // reads as "far away" and generation crawls at one collider per frame. RSS also raises the
        // load range for space objects to 100km, which makes the stall considerably longer.
        [HarmonyPrefix]
        [HarmonyPatch("Setup", new Type[]
        {
            typeof(PSpaceObject), typeof(Mesh), typeof(Vector3), typeof(Func<Transform, float>),
            typeof(float), typeof(float), typeof(Callback)
        })]
        internal static void Prefix_Setup(Mesh refMesh, ref float maxRange)
        {
            float radius = MeanRadius(refMesh);
            if (radius <= 0f)
            {
                return;
            }

            maxRange = Mathf.Max(maxRange, radius * 3f);

            if (LogColliderGen)
            {
                Debug.LogFormat("[RSS] SpaceObjectCollider: source {0} tris / {1} verts, mean radius {2:F1}m, maxRange {3:F1}m",
                    refMesh.triangles.Length / 3, refMesh.vertexCount, radius, maxRange);
            }
        }

        // The stock per-frame budgets are fixed item counts tuned for objects that decompose into a
        // few dozen solids. An RSS asteroid produces thousands, which at long range would trickle
        // out one collider per frame for the better part of a minute - so the object is still
        // generating when the vessel arrives, and there is nothing there to hit. Taking the larger
        // of the stock budget and a work-proportional one bounds the total frame count without ever
        // generating slower than stock.
        [HarmonyPostfix]
        [HarmonyPatch("UpdateGenRange")]
        internal static void Postfix_UpdateGenRange(float ___rangeScale, SpaceObjectCollider.Chunk[] ___chunks,
            List<SpaceObjectCollider.CompositeSolid> ___solids, ref int ___marchStepsPerFrame,
            ref int ___meshGensPerFrame, ref int ___colliderGensPerFrame)
        {
            // All zero means the object is close enough to generate in one frame without yielding.
            if (___marchStepsPerFrame == 0 && ___meshGensPerFrame == 0 && ___colliderGensPerFrame == 0)
            {
                return;
            }

            float frames = Mathf.Max(1f, Mathf.Lerp(1f, MaxGenFrames, ___rangeScale));
            int chunkCount = (___chunks != null) ? ___chunks.Length : 0;
            int solidCount = (___solids != null) ? ___solids.Count : 0;

            ___marchStepsPerFrame = Mathf.Max(___marchStepsPerFrame, Mathf.CeilToInt(chunkCount / frames));
            ___meshGensPerFrame = Mathf.Max(___meshGensPerFrame, Mathf.CeilToInt(solidCount / frames));
            ___colliderGensPerFrame = Mathf.Max(___colliderGensPerFrame, Mathf.CeilToInt(solidCount / frames));
        }

        // Stock links each triangle to its edge neighbours with a nested scan over every other
        // triangle. That is 7ms on the 1280-triangle proxy but 108ms on the 5120-triangle mesh the
        // colliders are now built from, on the main thread, per asteroid. An edge dictionary gives
        // identical neighbour assignments in linear time.
        [HarmonyPrefix]
        [HarmonyPatch("LinkChunks")]
        internal static bool Prefix_LinkChunks(SpaceObjectCollider.Chunk[] chunks)
        {
            int count = chunks.Length;
            Dictionary<long, int> edgeOwners = new Dictionary<long, int>(count * 2);
            for (int i = 0; i < count; i++)
            {
                LinkChunkEdge(chunks, edgeOwners, chunks[i].i0, chunks[i].i1, i);
                LinkChunkEdge(chunks, edgeOwners, chunks[i].i1, chunks[i].i2, i);
                LinkChunkEdge(chunks, edgeOwners, chunks[i].i0, chunks[i].i2, i);
            }
            return false;
        }

        private static void LinkChunkEdge(SpaceObjectCollider.Chunk[] chunks, Dictionary<long, int> edgeOwners,
            int vA, int vB, int index)
        {
            long edgeKey = GetEdgeKey(vA, vB);
            if (edgeOwners.TryGetValue(edgeKey, out int owner))
            {
                // SetNeighbour resolves the unordered pair against each chunk's own winding, so the
                // same pair sets the correct slot on both sides.
                chunks[index].SetNeighbour(vA, vB, chunks[owner]);
                chunks[owner].SetNeighbour(vA, vB, chunks[index]);
                edgeOwners.Remove(edgeKey);
            }
            else
            {
                edgeOwners.Add(edgeKey, index);
            }
        }

        private static long GetEdgeKey(int vA, int vB)
        {
            if (vA >= vB)
            {
                int swap = vA;
                vA = vB;
                vB = swap;
            }
            return ((long)vA << 32) + vB;
        }

        private static float MeanRadius(Mesh mesh)
        {
            if (mesh == null)
            {
                return 0f;
            }
            Vector3[] vertices = mesh.vertices;
            if (vertices.Length == 0)
            {
                return 0f;
            }
            double total = 0.0;
            for (int i = 0; i < vertices.Length; i++)
            {
                total += vertices[i].magnitude;
            }
            return (float)(total / vertices.Length);
        }
    }

    // Each cluster of surface triangles is closed into a solid by adding a single apex beneath the
    // surface, and the result is cooked as a convex hull. That apex has to sit below every surface
    // vertex in the cluster for the hull to have any depth. Stock searches for the lowest one, but
    // the comparison in that search tests an unrelated loop counter instead of the radius being
    // compared, so the search never narrows: the apex depth ends up taken from the centroid of
    // whichever triangle happens to be first in the cluster.
    //
    // On a surface whose radius varies by tens of percent - which is every RSS asteroid - that
    // centroid is often higher than the cluster's own lowest vertices. The apex then falls inside
    // their hull, contributes no depth, and the solid cooks as a zero-thickness sheet that a vessel
    // passes straight through. On level-5 geometry matching a 1.2km asteroid, roughly 15% of solids
    // came out degenerate this way.
    //
    // Taking the minimum over the cluster's actual surface vertices restores the intent and gives
    // every solid real inward thickness.
    [HarmonyPatch(typeof(SpaceObjectCollider.CompositeSolid))]
    internal class PatchCompositeSolid
    {
        [HarmonyPrefix]
        [HarmonyPatch("BuildCompositeMesh")]
        internal static bool Prefix_BuildCompositeMesh(SpaceObjectCollider.CompositeSolid __instance,
            Vector3[] srcVerts, Vector3[] srcNormals, Color c)
        {
            List<SpaceObjectCollider.Chunk> chunks = __instance.chunks;
            List<int> sourceIndices = new List<int>();
            Dictionary<int, int> localIndices = new Dictionary<int, int>();

            int[] tris = (chunks.Count == 1) ? new int[12] : new int[chunks.Count * 3 + 3];
            int write = 0;
            for (int i = 0; i < chunks.Count; i++)
            {
                tris[write] = MapVertex(sourceIndices, localIndices, chunks[i].i0);
                tris[write + 1] = MapVertex(sourceIndices, localIndices, chunks[i].i1);
                tris[write + 2] = MapVertex(sourceIndices, localIndices, chunks[i].i2);
                write += 3;
            }

            Vector3[] verts;
            Vector3[] normals;
            if (chunks.Count == 1)
            {
                verts = new Vector3[4]
                {
                    srcVerts[sourceIndices[0]],
                    srcVerts[sourceIndices[1]],
                    srcVerts[sourceIndices[2]],
                    chunks[0].srfRadial
                };
                verts[3] *= Mathf.Sqrt(Mathf.Min(Mathf.Min(verts[0].sqrMagnitude, verts[1].sqrMagnitude), verts[2].sqrMagnitude)) * 0.95f;
                normals = new Vector3[4]
                {
                    srcNormals[sourceIndices[0]],
                    srcNormals[sourceIndices[1]],
                    srcNormals[sourceIndices[2]],
                    -chunks[0].srfRadial
                };
                tris[write] = 3;
                tris[write + 1] = 0;
                tris[write + 2] = 1;
                write += 3;
                tris[write] = 3;
                tris[write + 1] = 1;
                tris[write + 2] = 2;
                write += 3;
                tris[write] = 3;
                tris[write + 1] = 2;
                tris[write + 2] = 0;
            }
            else
            {
                verts = new Vector3[sourceIndices.Count + 1];
                normals = new Vector3[sourceIndices.Count + 1];
                float minSqrMagnitude = float.MaxValue;
                for (int j = 0; j < verts.Length - 1; j++)
                {
                    verts[j] = srcVerts[sourceIndices[j]];
                    normals[j] = srcNormals[sourceIndices[j]];
                    minSqrMagnitude = Mathf.Min(minSqrMagnitude, verts[j].sqrMagnitude);
                }
                Vector3 srfRadial = __instance.GetSrfRadial();
                verts[verts.Length - 1] = srfRadial * Mathf.Sqrt(minSqrMagnitude) * 0.95f;
                normals[normals.Length - 1] = -srfRadial;

                // Stock closes the hull at index verts.Length - 1, which is inside the triangle list
                // it just wrote. The three reserved slots are at chunks.Count * 3.
                write = chunks.Count * 3;
                tris[write] = verts.Length - 1;
                tris[write + 1] = 0;
                tris[write + 2] = 1;
            }

            __instance.tris = tris;
            __instance.verts = verts;
            __instance.normals = normals;
            __instance.color = c;

            if (verts.Length <= 3)
            {
                Debug.LogError("Invalid Solid: " + verts.Length + " defined, but at least 4 are required.");
                return false;
            }

            Mesh mesh = new Mesh();
            mesh.vertices = verts;
            mesh.normals = normals;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            __instance.mesh = mesh;
            return false;
        }

        // Reproduces AddUnique followed by IndexOf, in first-seen order, without the quadratic scan.
        private static int MapVertex(List<int> sourceIndices, Dictionary<int, int> localIndices, int vertex)
        {
            if (localIndices.TryGetValue(vertex, out int local))
            {
                return local;
            }
            local = sourceIndices.Count;
            sourceIndices.Add(vertex);
            localIndices.Add(vertex, local);
            return local;
        }
    }
}
