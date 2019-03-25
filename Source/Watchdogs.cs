using UnityEngine;

namespace RealSolarSystem
{
    // The RSS watchdog is a general place to prevent RSS changes
    // from being reverted by other mods when our back is turned.

    [KSPAddon(KSPAddon.Startup.FlightAndKSC, false)]

    public class RSSWatchDog : MonoBehaviour
    {
        ConfigNode RSSSettings = null;
        double delayCounter = 0;
        const double initialDelay = 1; // 1 second wait before cam fixing

        bool watchdogRun = false;
        protected bool isCompatible = true;
        protected bool isSuborbital = false;

        public void Start()
        {
            if (!CompatibilityChecker.IsCompatible())
            {
                isCompatible = false;
                return;
            }

            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("REALSOLARSYSTEM"))
                RSSSettings = node;

            GameEvents.onVesselSOIChanged.Add(OnVesselSOIChanged);
            GameEvents.onVesselSituationChange.Add(OnVesselSituationChanged);
        }

        public void OnDestroy()
        {
            if (isCompatible)
            {
                GameEvents.onVesselSOIChanged.Remove(OnVesselSOIChanged);
                GameEvents.onVesselSituationChange.Remove(OnVesselSituationChanged);
            }
        }

        public void Update()
        {
            if (!isCompatible)
                return;

            if (watchdogRun)
                return;

            delayCounter += Time.deltaTime;

            if(delayCounter < initialDelay)
                return;

            watchdogRun = true;

            Camera[] cameras = Camera.allCameras;
            string bodyName = FlightGlobals.getMainBody().name;

            foreach (Camera cam in cameras)
            {
                float farClip = -1;
                float nearClip = -1;

                if (cam.name.Equals("Camera 00"))
                {
                    RSSSettings.TryGetValue("cam00FarClip", ref farClip);

                    if (RSSSettings.HasNode(bodyName))
                        RSSSettings.GetNode(bodyName).TryGetValue("cam00FarClip", ref farClip);

                    RSSSettings.TryGetValue("cam00NearClip", ref nearClip);

                    if (RSSSettings.HasNode(bodyName))
                        RSSSettings.GetNode(bodyName).TryGetValue("cam00NearClip", ref nearClip);
                }
                else if (cam.name.Equals("Camera 01"))
                {
                    RSSSettings.TryGetValue("cam01FarClip", ref farClip);

                    if (RSSSettings.HasNode(bodyName))
                        RSSSettings.GetNode(bodyName).TryGetValue("cam01FarClip", ref farClip);

                    RSSSettings.TryGetValue("cam01NearClip", ref nearClip);

                    if (RSSSettings.HasNode(bodyName))
                        RSSSettings.GetNode(bodyName).TryGetValue("cam01NearClip", ref nearClip);
                }
                else if (cam.name.Equals("Camera ScaledSpace"))
                {
                    RSSSettings.TryGetValue("camScaledSpaceFarClip", ref farClip);

                    if (RSSSettings.HasNode(bodyName))
                        RSSSettings.GetNode(bodyName).TryGetValue("camScaledSpaceFarClip", ref farClip);

                    RSSSettings.TryGetValue("camScaledSpaceNearClip", ref nearClip);

                    if (RSSSettings.HasNode(bodyName))
                        RSSSettings.GetNode(bodyName).TryGetValue("camScaledSpaceNearClip", ref nearClip);
                }

                if (nearClip > 0)
                {
                    cam.nearClipPlane = nearClip;

                    Debug.Log("[RealSolarSystem]: Watchdog: Setting camera " + cam.name + " near clip to " + nearClip + " so camera now has " + cam.nearClipPlane);
                }

                if (farClip > 0)
                {
                    cam.farClipPlane = farClip;

                    Debug.Log("[RealSolarSystem]: Watchdog: Setting camera " + cam.name + " far clip to " + farClip + " so camera now has " + cam.farClipPlane);
                }
            }
        }

        public void OnVesselSOIChanged(GameEvents.HostedFromToAction<Vessel, CelestialBody> evt)
        {
            watchdogRun = false;

            delayCounter = 0;
        }

        private void OnVesselSituationChanged(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> data)
        {
            Vessel curVessel = data.host;
            if (!curVessel.mainBody.isHomeWorld || !curVessel.isActiveVessel) return;

            if (data.from == Vessel.Situations.FLYING && data.to == Vessel.Situations.SUB_ORBITAL)
            {
                isSuborbital = true;
            }
            else if (isSuborbital && data.to == Vessel.Situations.FLYING)
            {
                isSuborbital = false;
                Debug.Log("[RealSolarSystem]: Calling StartUpSphere() to prevent missing PQ tiles");
                curVessel.mainBody.pqsController.StartUpSphere();
            }
        }
    }
}
