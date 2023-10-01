using UnityEngine;

namespace RealSolarSystem
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class HarmonyPatcher : MonoBehaviour
    {
        internal void Start()
        {
            var harmony = new HarmonyLib.Harmony("RSS.HarmonyPatcher");
            harmony.PatchAll();
        }
    }
}
