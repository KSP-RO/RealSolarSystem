using UnityEngine;

namespace RealSolarSystem
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]

    public class TimeWarpFixer : MonoBehaviour
    {
        public double lastTime = 0;
        public double currentTime = 0;
        public static bool fixedTimeWarp = false;
        protected bool isCompatible = true;

        public void Start()
        {
            isCompatible &= CompatibilityChecker.IsCompatible ();
            fixedTimeWarp = false;

            GameSettings.KERBIN_TIME = false;
            PQSCache.PresetList.SetPreset(PQSCache.PresetList.presets.Count - 1);
        }

        public void Update()
        {
            if (!isCompatible)
                return;

            // Update the TimeWarp rates.

            if (!fixedTimeWarp && TimeWarp.fetch != null)
            {
                fixedTimeWarp = true;
                ConfigNode twNode = null;

                foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("REALSOLARSYSTEM"))
                    twNode = node.GetNode("timeWarpRates");

                Debug.Log("[RealSolarSystem]: Setting TimeWarp rates...");

                float ftmp;

                if (twNode != null)
                {
                    for (int i = 1; i < 8; i++)
                    {
                        if (twNode.HasValue("rate" + i))
                            if (float.TryParse(twNode.GetValue("rate" + i), out ftmp))
                                TimeWarp.fetch.warpRates[i] = ftmp;
                    }
                }

                Destroy(this);
            }
        }
    }
}
