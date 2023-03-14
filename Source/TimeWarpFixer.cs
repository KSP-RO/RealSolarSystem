using UnityEngine;

namespace RealSolarSystem
{
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class TimeWarpFixer : MonoBehaviour
    {
        public double lastTime = 0;
        public double currentTime = 0;
        public static bool fixedTimeWarp = false;

        public void Start()
        {
            fixedTimeWarp = false;

            GameSettings.KERBIN_TIME = false;
        }

        public void Update()
        {
            // Update the TimeWarp rates.

            if (!fixedTimeWarp && TimeWarp.fetch != null)
            {
                fixedTimeWarp = true;
                ConfigNode twNode = null;

                foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("REALSOLARSYSTEM"))
                    twNode = node.GetNode("timeWarpRates");

                Debug.Log("[RealSolarSystem] Setting TimeWarp rates...");

                if (twNode != null)
                {
                    for (int i = 1; i < 8; i++)
                    {
                        if (twNode.HasValue("rate" + i))
                            if (float.TryParse(twNode.GetValue("rate" + i), out float ftmp))
                                TimeWarp.fetch.warpRates[i] = ftmp;
                    }
                }

                Destroy(this);
            }
        }
    }
}
