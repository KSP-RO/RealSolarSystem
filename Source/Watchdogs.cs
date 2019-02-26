using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using KSP;
using System.IO;

namespace RealSolarSystem
{
    // Checks to make sure useLegacyAtmosphere didn't get munged with
    // Could become a general place to prevent RSS changes from being reverted when our back is turned.
    [KSPAddon(KSPAddon.Startup.EveryScene, false)]
    public class RSSWatchDog : MonoBehaviour
    {
        ConfigNode RSSSettings = null;
        double delayCounter = 0;
        const double initialDelay = 1; // 1 second wait before cam fixing

        bool watchdogRun = false;
        protected bool isCompatible = true;
        public void Start()
        {
            if (!CompatibilityChecker.IsCompatible())
            {
                isCompatible = false;
                return;
            }
            if (!(HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.SPACECENTER))
                return;
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("REALSOLARSYSTEM"))
                RSSSettings = node;

            if (RSSSettings != null)
            {
                RSSSettings.TryGetValue("dumpOrbits", ref dumpOrbits);
            }

            UpdateAtmospheres();
            GameEvents.onVesselSOIChanged.Add(OnVesselSOIChanged);
        }
        public void OnDestroy()
        {
            if (isCompatible)
            {
                GameEvents.onVesselSOIChanged.Remove(OnVesselSOIChanged);
            }
        }

        public void Update()
        {
            if (!isCompatible)
                return;
            if (!(HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.SPACECENTER))
                return;

            if (watchdogRun)
                return;
            delayCounter += TimeWarp.fixedDeltaTime;

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
                if (farClip > 0)
                    cam.farClipPlane = farClip;
                if (nearClip > 0)
                    cam.nearClipPlane = nearClip;
            }
        }

        double counter = 0;
        bool dumpOrbits = false;
        public void FixedUpdate()
        {
            if (!isCompatible)
                return;
            if (!(HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.SPACECENTER))
                return;

            if (!dumpOrbits)
                return;
            counter += TimeWarp.fixedDeltaTime;
            if (counter < 3600)
                return;
            counter = 0;
            if (FlightGlobals.Bodies == null)
            {
                print("**RSS OBTDUMP*** - null body list!");
                return;
            }
            print("**RSS OBTDUMP***");
            int time = (int)Planetarium.GetUniversalTime();
            print("At time " + time + ", " + KSPUtil.PrintDate(time, true, true));
            for (int i = 0; i < FlightGlobals.Bodies.Count; i++)
            {
                CelestialBody body = FlightGlobals.Bodies[i];
                if (body == null || body.orbitDriver == null)
                    continue;
                if (body.orbitDriver.orbit == null)
                    continue;
                Orbit o = body.orbitDriver.orbit;
                print("********* BODY **********");
                print("name = " + body.name + "(" + i + ")");
                Type oType = o.GetType();
                foreach (FieldInfo f in oType.GetFields())
                {
                    if (f == null || f.GetValue(o) == null)
                        continue;
                    print(f.Name + " = " + f.GetValue(o));
                }
            }
        }

        public void OnVesselSOIChanged(GameEvents.HostedFromToAction<Vessel, CelestialBody> evt)
        {

        }
        public void UpdateAtmospheres()
        {
            if (RSSSettings != null)
            {
                AtmosphereFromGround[] AFGs = (AtmosphereFromGround[])Resources.FindObjectsOfTypeAll(typeof(AtmosphereFromGround));
                foreach (ConfigNode node in RSSSettings.nodes)
                {
                    foreach (CelestialBody body in FlightGlobals.Bodies)
                    {
                        if (body.name.Equals(node.name))
                        {
                            print("*RSS* checking useLegacyAtmosphere for " + body.GetName());
                            if (node.HasValue("useLegacyAtmosphere"))
                            {
                                bool UseLegacyAtmosphere = true;
                                bool.TryParse(node.GetValue("useLegacyAtmosphere"), out UseLegacyAtmosphere);
                                //print("*RSSWatchDog* " + body.GetName() + ".useLegacyAtmosphere = " + body.useLegacyAtmosphere.ToString());
                                if (UseLegacyAtmosphere != body.useLegacyAtmosphere)
                                {
                                    print("*RSSWatchDog* resetting useLegacyAtmosphere to " + UseLegacyAtmosphere.ToString());
                                    body.useLegacyAtmosphere = UseLegacyAtmosphere;
                                }
                            }
                            if (node.HasNode("AtmosphereFromGround"))
                            {
                                foreach (AtmosphereFromGround ag in AFGs)
                                {
                                    if (ag != null && ag.planet != null)
                                    {
                                        if (ag.planet.name.Equals(node.name))
                                        {
                                            RealSolarSystem.UpdateAFG(body, ag, node.GetNode("AtmosphereFromGround"));
                                            print("*RSSWatchDog* reapplying AtmosphereFromGround settings for " + body.name);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}