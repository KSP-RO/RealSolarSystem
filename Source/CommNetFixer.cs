using CommNet;
using System;
using System.Linq;
using UnityEngine;

namespace RealSolarSystem
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]
    public class RSSCommNetSettings : MonoBehaviour
    {
        public void Start()
        {
            try
            {
                bool enableExtraGroundStations = true;
                bool overrideCommNetParams = true;

                float occlusionMultiplierInAtm = 1.0f;
                float occlusionMultiplierInVac = 1.0f;

                Debug.Log("[RealSolarSystem] Checking for custom CommNet settings...");

                ConfigNode node = GameDatabase.Instance.GetConfigNodes("REALSOLARSYSTEM").FirstOrDefault();

                if (node != null)
                {
                    node.TryGetValue("overrideCommNetParams", ref overrideCommNetParams);
                    node.TryGetValue("enableGroundStations", ref enableExtraGroundStations);
                    node.TryGetValue("occlusionMultiplierAtm", ref occlusionMultiplierInAtm);
                    node.TryGetValue("occlusionMultiplierVac", ref occlusionMultiplierInVac);

                    if (overrideCommNetParams)
                    {
                        //  Set the default CommNet parameters for RealSolarSystem.

                        Debug.Log("[RealSolarSystem] Updating the CommNet settings...");

                        HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().enableGroundStations = enableExtraGroundStations;
                        HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().occlusionMultiplierAtm = occlusionMultiplierInAtm;
                        HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().occlusionMultiplierVac = occlusionMultiplierInVac;
                    }
                }
            }
            catch (Exception exceptionStack)
            {
                Debug.Log("[RealSolarSystem] RSSCommNetSettings.Start() caught an exception: " + exceptionStack);
            }
            finally
            {
                Destroy(this);
            }
        }
    }
}
