using CommNet;
using System;
using UnityEngine;

namespace RealSolarSystem
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, true)]

    public class RSSCommNetSettings : MonoBehaviour
    {
        void Start()
        {
            try
            {
                bool enableExtraGroundStations = true;
                bool overrideCommNetParams = true;

                float occlusionMultiplierInAtm = 1.0f;
                float occlusionMultiplierInVac = 1.0f;

                Debug.Log("[RealSolarSystem]: Checking for custom CommNet settings...");

                foreach (ConfigNode RSSSettings in GameDatabase.Instance.GetConfigNodes("REALSOLARSYSTEM"))
                {
                    RSSSettings.TryGetValue("overrideCommNetParams", ref overrideCommNetParams);
                    RSSSettings.TryGetValue("enableGroundStations", ref enableExtraGroundStations);
                    RSSSettings.TryGetValue("occlusionMultiplierAtm", ref occlusionMultiplierInAtm);
                    RSSSettings.TryGetValue("occlusionMultiplierVac", ref occlusionMultiplierInVac);
                }

                if (overrideCommNetParams)
                {
                    //  Set the default CommNet parameters for RealSolarSystem.

                    Debug.Log("[RealSolarSystem]: Updating the CommNet settings...");

                    HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().enableGroundStations = enableExtraGroundStations;
                    HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().occlusionMultiplierAtm = occlusionMultiplierInAtm;
                    HighLogic.CurrentGame.Parameters.CustomParams<CommNetParams>().occlusionMultiplierVac = occlusionMultiplierInVac;
                }
            }
            catch (Exception exceptionStack)
            {
                Debug.Log("[RealSolarSystem]: RSSCommNetSettings.Start() caught an exception: " + exceptionStack);
            }
            finally
            {
                Destroy(this);
            }
        }
    }
}
