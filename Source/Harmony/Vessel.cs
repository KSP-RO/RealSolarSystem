using FinePrint.Utilities;
using HarmonyLib;

namespace RealSolarSystem.Harmony
{
    [HarmonyPatch(typeof(Vessel))]
    internal class PatchVessel
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        internal static void Postfix_Start(Vessel __instance)
        {
            if (__instance.vesselType == VesselType.SpaceObject ||
                (__instance.vesselType != VesselType.Debris &&
                VesselUtilities.VesselHasPartName("PotatoRoid", __instance)))
            {
                ClobberVesselRanges(__instance.vesselRanges.orbit);
                ClobberVesselRanges(__instance.vesselRanges.subOrbital);
                ClobberVesselRanges(__instance.vesselRanges.escaping);
                ClobberVesselRanges(__instance.vesselRanges.flying);
                ClobberVesselRanges(__instance.vesselRanges.splashed);
                ClobberVesselRanges(__instance.vesselRanges.landed);
            }
        }

        private static void ClobberVesselRanges(VesselRanges.Situation vr)
        {
            vr.load = 100000f;
            vr.unload = 101000f;
            vr.pack = 2600f;
            vr.unpack = 2500f;
        }
    }
}
