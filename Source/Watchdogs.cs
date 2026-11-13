using System;
using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;

namespace RealSolarSystem
{
    /// <summary>
    /// The RSS watchdog is a general place to prevent RSS changes
    /// from being reverted by other mods when our back is turned.
    /// </summary>
    [KSPAddon(KSPAddon.Startup.FlightAndKSC, false)]
    public class RSSWatchDog : MonoBehaviour
    {
        private const double initialDelay = 1; // 1 second wait before cam fixing

        public static RSSWatchDog Instance { get; private set; }

        private ConfigNode rssSettings = null;
        private double delayCounter = 0;
        private bool watchdogRun = false;
        private bool isSuborbital = false;

        // The camera that renders PQS terrain gets its far clip plane extended
        // so that it always reaches the farthest visible terrain. Below the altitude where
        // that is closer than baseFarClip, the far clip stays at baseFarClip.
        private const float defaultFarClipHorizonMult = 1.2f;
        private Camera dynamicFarClipCam;
        private float baseFarClip = -1;
        private bool dynamicFarClipEnabled = false;
        private float farClipHorizonMult = defaultFarClipHorizonMult;
        private float maxFarClip = -1;

        private static bool IsDX11 =>
            SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D11 ||
            SystemInfo.graphicsDeviceType == GraphicsDeviceType.Direct3D12;

        // On DX11 KSP uses a single local camera. On other APIs Camera 01 renders the far range.
        public static string DynamicFarClipCamName => IsDX11 ? "Camera 00" : "Camera 01";

        public bool DynamicFarClipEnabled => dynamicFarClipEnabled;
        public float CurrentFarClip => dynamicFarClipCam != null ? dynamicFarClipCam.farClipPlane : -1;
        public float BaseFarClip => baseFarClip;
        public float FarClipHorizonMult => farClipHorizonMult;

        public void Awake()
        {
            Instance = this;
        }

        public void Start()
        {
            rssSettings = GameDatabase.Instance.GetConfigNodes("REALSOLARSYSTEM").FirstOrDefault(n => n.HasNode("ClipPlanes"));

            GameEvents.onVesselSOIChanged.Add(OnVesselSOIChanged);
            GameEvents.onVesselSituationChange.Add(OnVesselSituationChanged);
        }

        public void OnDestroy()
        {
            GameEvents.onVesselSOIChanged.Remove(OnVesselSOIChanged);
            GameEvents.onVesselSituationChange.Remove(OnVesselSituationChanged);

            if (Instance == this)
                Instance = null;
        }

        public void Update()
        {
            if (watchdogRun)
                return;

            delayCounter += Time.deltaTime;

            if (delayCounter < initialDelay)
                return;

            watchdogRun = true;

            Camera[] cameras = Camera.allCameras;
            string bodyName = FlightGlobals.getMainBody().name;

            if (rssSettings == null) return;

            ConfigNode clipPlaneSettings = rssSettings.GetNode("ClipPlanes")?.GetNode(IsDX11 ? "DX11" : "Other");

            if (clipPlaneSettings == null) return;

            dynamicFarClipEnabled = false;
            farClipHorizonMult = defaultFarClipHorizonMult;
            maxFarClip = -1;
            ReadValue(clipPlaneSettings, bodyName, "dynamicFarClip", ref dynamicFarClipEnabled);
            ReadValue(clipPlaneSettings, bodyName, "farClipHorizonMult", ref farClipHorizonMult);
            ReadValue(clipPlaneSettings, bodyName, "maxFarClip", ref maxFarClip);

            foreach (Camera cam in cameras)
            {
                float farClip = -1;
                float nearClip = -1;

                if (cam.name.Equals("Camera 00"))
                {
                    ReadValue(clipPlaneSettings, bodyName, "cam00FarClip", ref farClip);
                    ReadValue(clipPlaneSettings, bodyName, "cam00NearClip", ref nearClip);
                }
                else if (cam.name.Equals("Camera 01"))
                {
                    ReadValue(clipPlaneSettings, bodyName, "cam01FarClip", ref farClip);
                    ReadValue(clipPlaneSettings, bodyName, "cam01NearClip", ref nearClip);
                }
                else if (cam.name.Equals("Camera ScaledSpace"))
                {
                    ReadValue(clipPlaneSettings, bodyName, "camScaledSpaceFarClip", ref farClip);
                    ReadValue(clipPlaneSettings, bodyName, "camScaledSpaceNearClip", ref nearClip);
                }

                if (nearClip > 0)
                {
                    cam.nearClipPlane = nearClip;
                    Debug.Log($"[RealSolarSystem] Watchdog: Setting camera {cam.name} near clip to {nearClip} so camera now has {cam.nearClipPlane}");
                }

                if (farClip > 0)
                {
                    cam.farClipPlane = farClip;
                    Debug.Log($"[RealSolarSystem] Watchdog: Setting camera {cam.name} far clip to {farClip} so camera now has {cam.farClipPlane}");
                }

                if (cam.name.Equals(DynamicFarClipCamName))
                {
                    dynamicFarClipCam = cam;
                    // Without a configured value, use the camera's own value. Only do that once
                    // because afterwards the camera holds the dynamically adjusted value.
                    if (farClip > 0)
                        baseFarClip = farClip;
                    else if (baseFarClip <= 0)
                        baseFarClip = cam.farClipPlane;
                }
            }

            if (dynamicFarClipEnabled)
                Debug.Log($"[RealSolarSystem] Watchdog: Dynamic far clip enabled for {DynamicFarClipCamName}, base {baseFarClip}, horizon mult {farClipHorizonMult}, max {maxFarClip}");
        }

        public void LateUpdate()
        {
            if (!dynamicFarClipEnabled || dynamicFarClipCam == null || baseFarClip <= 0 ||
                !HighLogic.LoadedSceneIsFlight || MapView.MapIsEnabled)
                return;

            dynamicFarClipCam.farClipPlane = GetDynamicFarClip();
        }

        /// <summary>
        /// Turns the dynamic far clip on or off. When turned off, the camera goes back to the base far clip.
        /// </summary>
        public void SetDynamicFarClipEnabled(bool enabled)
        {
            dynamicFarClipEnabled = enabled;
            if (!enabled && dynamicFarClipCam != null && baseFarClip > 0)
                dynamicFarClipCam.farClipPlane = baseFarClip;
        }

        /// <summary>
        /// Sets the far clip value that the dynamic far clip will not go below.
        /// Used by the debug GUI so that manually applied values are not overwritten on the next frame.
        /// </summary>
        public void SetBaseFarClip(Camera cam, float farClip)
        {
            if (cam == dynamicFarClipCam && farClip > 0)
                baseFarClip = farClip;
        }

        private float GetDynamicFarClip()
        {
            CelestialBody body = FlightGlobals.currentMainBody;
            PQS pqs = body?.pqsController;
            if (pqs == null || !pqs.isActive)
                return baseFarClip;

            Vector3d camPos = dynamicFarClipCam.transform.position;
            double camDist = (camPos - body.position).magnitude;
            double radius = body.Radius;
            if (camDist <= radius)
                return baseFarClip;

            double horizonDist = Math.Sqrt(camDist * camDist - radius * radius);

            // Terrain above sea level stays visible past the sea level horizon.
            // The farthest visible point is a peak of max terrain height just behind the horizon.
            double radiusMax = Math.Max(pqs.radiusMax, radius);
            double overHorizonDist = Math.Sqrt(radiusMax * radiusMax - radius * radius);

            // Scatterer skips its effects on pixels close to the far plane, which shows up as
            // a dark line on the horizon. The multiplier keeps the terrain away from the far plane.
            double farClip = Math.Max(baseFarClip, (horizonDist + overHorizonDist) * farClipHorizonMult);
            if (maxFarClip > 0)
                farClip = Math.Min(farClip, maxFarClip);

            return (float)farClip;
        }

        private static void ReadValue(ConfigNode settings, string bodyName, string key, ref float value)
        {
            settings.TryGetValue(key, ref value);
            settings.GetNode(bodyName)?.TryGetValue(key, ref value);
        }

        private static void ReadValue(ConfigNode settings, string bodyName, string key, ref bool value)
        {
            settings.TryGetValue(key, ref value);
            settings.GetNode(bodyName)?.TryGetValue(key, ref value);
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
                Debug.Log("[RealSolarSystem] Calling StartUpSphere() to prevent missing PQ tiles");
                curVessel.mainBody.pqsController.StartUpSphere();
            }
        }
    }
}
