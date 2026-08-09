using HarmonyLib;
using UnityEngine;

namespace RealSolarSystem.Harmony
{
    // Asteroid and comet colliders are built by decomposing a mesh into convex solids. Stock feeds
    // that decomposition the crude convexSphere proxy while discarding the detailed colliderSphere
    // mesh it generated moments earlier - PSpaceObject.Setup hands CreateCollider the convex mesh,
    // and CreateConvexCollider, which is what the proxy was built for, is never called at all.
    // At stock asteroid sizes the resulting mismatch is centimetres. On an RSS-scale asteroid the
    // same relative error is tens of metres of collider sitting below the visible surface.
    // Patching CreateCollider rather than Setup keeps convexColliderMesh pointing at the proxy, so
    // the proxy is still released by OnDestroy and ModuleComet still reads the bounds it expects.
    [HarmonyPatch(typeof(PSpaceObject))]
    internal class PatchPSpaceObject
    {
        [HarmonyPrefix]
        [HarmonyPatch("CreateCollider")]
        internal static void Prefix_CreateCollider(ref Mesh colliderMesh, Mesh ___colliderMesh, Mesh ___visualMesh)
        {
            // Setup assigns both fields before reaching CreateCollider. colliderSphere is unset on
            // some prefabs, in which case the visual mesh is the most detailed thing available.
            Mesh source = ___colliderMesh != null ? ___colliderMesh : ___visualMesh;
            if (source != null)
            {
                colliderMesh = source;
            }
        }
    }
}
