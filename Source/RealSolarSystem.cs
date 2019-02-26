using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using KSP;
using System.IO;


namespace RealSolarSystem
{
    public class RSSLoadInfo
    {
        public double epoch = 0;
        public bool useEpoch = false;
        public bool doWrap = false;
        public bool compressNormals = false;
        public bool spheresOnly = false;
        public bool orbitCalc = true;
        public bool defaultAtmoScale = true;
        public float SSAtmoScale = 1.0f;

        public MeshFilter joolMesh = null;

        public ConfigNode node;

        public RSSLoadInfo(ConfigNode RSSnode)
        {
            useEpoch = RSSnode.TryGetValue("Epoch", ref epoch);
            RSSnode.TryGetValue("wrap", ref doWrap);
            RSSnode.TryGetValue("compressNormals", ref compressNormals);
            RSSnode.TryGetValue("spheresOnly", ref spheresOnly);
            RSSnode.TryGetValue("orbitCalc", ref orbitCalc);
            RSSnode.TryGetValue("defaultAtmoScale", ref defaultAtmoScale);
            RSSnode.TryGetValue("SSAtmoScale", ref SSAtmoScale);

            node = new ConfigNode();
            RSSnode.CopyTo(node);
            // get spherical scaledspace mesh
            if (ScaledSpace.Instance != null)
            {
                //print("*RSS* Printing ScaledSpace Transforms");
                foreach (Transform t in ScaledSpace.Instance.scaledSpaceTransforms)
                {
                    /*print("***** TRANSFROM: " + t.name);
                    Utils.PrintTransformUp(t);
                    Utils.PrintTransformRecursive(t);*/
                    if (t.name.Equals("Jool"))
                        joolMesh = (MeshFilter)t.GetComponent(typeof(MeshFilter));
                }
                //print("*RSS* InverseScaleFactor = " + ScaledSpace.InverseScaleFactor);
            }
        }
    }
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RealSolarSystem : MonoBehaviour
    {
        public static bool doneRSS = false;
        public static bool workingRSS = false;

        public bool showGUI = false;
        private string guiMajor = "";
        private string guiMinor = "";
        private string guiExtra = "";
        private Rect screenRect;
        private int GuiIdx = -1;
        private GUISkin skins = HighLogic.Skin;

        public void OnGUI()
        {
            if (showGUI)
            {
                if (GuiIdx < 0)
                {
                    GuiIdx = "RealSolarSystem".GetHashCode();
                    screenRect = new Rect(100, 200, 300, 220);
                }
                screenRect = GUILayout.Window(GuiIdx, screenRect, RSSGUI, "RealSolarSystem Status", skins.window);
            }

        }

        public void RSSGUI(int GuiIdx)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label(guiMajor, skins.label);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(guiMinor, skins.label);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label(guiExtra, skins.label);
            GUILayout.EndHorizontal();
            if (doneRSS)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("OK", skins.button))
                    showGUI = false;
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        Texture GetRamp(string bodyName)
        {
            foreach (Transform t in ScaledSpace.Instance.scaledSpaceTransforms)
                if (t.name.Equals(bodyName))
                    return t.gameObject.renderer.material.GetTexture("_rimColorRamp");
            return null;
        }

        public static void UpdateAFG(CelestialBody body, AtmosphereFromGround afg, ConfigNode modNode = null)
        {
            if (modNode != null)
            {
                float ftmp;
                // the default
                afg.outerRadius = (float)body.Radius * 1.025f * ScaledSpace.InverseScaleFactor;
                if (modNode.TryGetValue("outerRadius", ref afg.outerRadius))
                {
                    afg.outerRadius *= ScaledSpace.InverseScaleFactor;
                }
                else if (modNode.HasValue("outerRadiusAtmo"))
                {
                    afg.outerRadius = ((float)body.Radius + body.maxAtmosphereAltitude) * ScaledSpace.InverseScaleFactor;
                }
                else if (modNode.TryGetValue("outerRadiusMult", ref afg.outerRadius))
                {
                    afg.outerRadius *= (float)body.Radius * ScaledSpace.InverseScaleFactor;
                }

                // the default
                afg.innerRadius = afg.outerRadius * 0.975f;
                if (modNode.TryGetValue("innerRadius", ref afg.innerRadius))
                {
                    afg.innerRadius *= ScaledSpace.InverseScaleFactor;
                }
                else if (modNode.TryGetValue("innerRadiusMult", ref afg.innerRadius))
                {
                    afg.innerRadius *= afg.outerRadius;
                }

                modNode.TryGetValue("doScale", ref afg.doScale);
                if (modNode.HasValue("transformScale"))
                {
                    if (float.TryParse(modNode.GetValue("transformScale"), out ftmp) && afg.transform != null)
                        afg.transform.localScale = new Vector3(ftmp, ftmp, ftmp);
                }
                else if (modNode.HasValue("transformAtmo"))
                {
                    afg.transform.localScale = Vector3.one * ((float)(body.Radius + body.maxAtmosphereAltitude) / (float)body.Radius);
                }


                if (modNode.HasValue("invWaveLength"))
                {
                    //will be recomputed by SQUAD anyway so no point.
                    // so instead, compute waveLength from it
                    try
                    {
                        Vector4 col = KSPUtil.ParseVector4(modNode.GetValue("invWaveLength"));
                        afg.invWaveLength = new Color(col.x, col.y, col.z, col.w);
                        afg.waveLength = new Color((float)Math.Pow(1 / col.x, 0.25), (float)Math.Pow(1 / col.y, 0.25), (float)Math.Pow(1 / col.z, 0.25), 1f);
                    }
                    catch (Exception e)
                    {
                        print("*RSS* Error parsing as color4: original text: " + modNode.GetValue("invWaveLength") + " --- exception " + e.Message);
                    }
                }
                if (modNode.HasValue("waveLength"))
                {
                    try
                    {
                        Vector4 col = KSPUtil.ParseVector4(modNode.GetValue("waveLength"));
                        afg.waveLength = new Color(col.x, col.y, col.z, col.w);
                    }
                    catch (Exception e)
                    {
                        print("*RSS* Error parsing as color4: original text: " + modNode.GetValue("waveLength") + " --- exception " + e.Message);
                    }
                }
                modNode.TryGetValue("Kr", ref afg.Kr);
                modNode.TryGetValue("Km", ref afg.Km);
                modNode.TryGetValue("ESun", ref afg.ESun);
                modNode.TryGetValue("g", ref afg.g);
                modNode.TryGetValue("samples", ref afg.samples);
            }
            else
            {
                // the defaults
                afg.outerRadius = (float)body.Radius * 1.025f * ScaledSpace.InverseScaleFactor;
                afg.innerRadius = afg.outerRadius * 0.975f;
            }
            afg.KrESun = afg.Kr * afg.ESun;
            afg.KmESun = afg.Km * afg.ESun;
            afg.Kr4PI = afg.Kr * 4f * (float)Math.PI;
            afg.Km4PI = afg.Km * 4f * (float)Math.PI;
            afg.g2 = afg.g * afg.g;
            afg.outerRadius2 = afg.outerRadius * afg.outerRadius;
            afg.innerRadius2 = afg.innerRadius * afg.innerRadius;
            afg.scale = 1f / (afg.outerRadius - afg.innerRadius);
            afg.scaleDepth = -0.25f;
            afg.scaleOverScaleDepth = afg.scale / afg.scaleDepth;
            try
            {
                MethodInfo setMaterial = afg.GetType().GetMethod("SetMaterial", BindingFlags.NonPublic | BindingFlags.Instance);
                setMaterial.Invoke(afg, new object[] { true });
            }
            catch (Exception e)
            {
                print("*RSS* *ERROR* setting AtmosphereFromGround " + afg.name + " for body " + body.name + ": " + e);
            }
        }

        public static Vector3 LLAtoECEF(double lat, double lon, double alt, double radius)
        {
            const double degreesToRadians = Math.PI / 180.0;
            lat = (lat - 90) * degreesToRadians;
            lon *= degreesToRadians;
            double x, y, z;
            double n = radius; // for now, it's still a sphere, so just the radius
            x = (n + alt) * -1.0 * Math.Sin(lat) * Math.Cos(lon);
            y = (n + alt) * Math.Cos(lat); // for now, it's still a sphere, so no eccentricity
            z = (n + alt) * -1.0 * Math.Sin(lat) * Math.Sin(lon);
            return new Vector3((float)x, (float)y, (float)z);
        }
        public void GeeASLToOthers(CelestialBody body)
        {
            double rsq = body.Radius * body.Radius;
            body.gMagnitudeAtCenter = body.GeeASL * 9.81 * rsq;
            body.gravParameter = body.gMagnitudeAtCenter;
            body.Mass = body.gravParameter * (1 / 6.674E-11);
        }

        // converts mass to Gee ASL using a body's radius.
        public void MassToOthers(CelestialBody body)
        {
            double rsq = body.Radius * body.Radius;
            body.GeeASL = body.Mass * (6.674E-11 / 9.81) / rsq;
            body.gMagnitudeAtCenter = body.GeeASL * 9.81 * rsq;
            body.gravParameter = body.gMagnitudeAtCenter;
        }

        public static void GravParamToOthers(CelestialBody body)
        {
            double rsq = body.Radius * body.Radius;
            body.Mass = body.gravParameter * (1 / 6.674E-11);
            body.GeeASL = body.gravParameter / 9.81 / rsq;
            body.gMagnitudeAtCenter = body.gravParameter;
        }


        public static bool done = false;

        public static RSSLoadInfo loadInfo = null;

        // Constants
        public const double DEG2RAD = Math.PI / 180.0;

        private IEnumerator<YieldInstruction> LoadCB(ConfigNode node, CelestialBody body)
        {
            bool updateMass = false;
            //OnGui();
            #region CBChanges
            print("Fixing CB " + node.name + " of radius " + body.Radius);
            guiMinor = "CelestialBody";
            //OnGui();
            double origRadius = body.Radius;
            double origAtmo = body.maxAtmosphereAltitude;

            #region CBMassRadius

            node.TryGetValue("bodyName", ref body.bodyName);
            node.TryGetValue("bodyDescription", ref body.bodyDescription);
            if (node.TryGetValue("Radius", ref body.Radius))
                updateMass = true;
            print("Radius ratio: " + body.Radius / origRadius);

            if (node.TryGetValue("Mass", ref body.Mass))
            {
                MassToOthers(body);
                
            }
            if (node.TryGetValue("GeeASL", ref body.GeeASL))
            {
                GeeASLToOthers(body);
                updateMass = false;
            }
            if (node.TryGetValue("gravParameter", ref body.gravParameter))
            {
                GravParamToOthers(body);
                updateMass = false;
            }
            #endregion

            #region CBAtmosphereTemperature
            node.TryGetValue("atmosphericAmbientColor", ref body.atmosphericAmbientColor);
            node.TryGetValue("atmosphere", ref body.atmosphere);
            node.TryGetValue("atmosphereScaleHeight", ref body.atmosphereScaleHeight);
            node.TryGetValue("atmosphereMultiplier", ref body.atmosphereMultiplier);
            node.TryGetValue("maxAtmosphereAltitude", ref body.maxAtmosphereAltitude);
            node.TryGetValue("staticPressureASL", ref body.staticPressureASL);
            node.TryGetValue("useLegacyAtmosphere", ref body.useLegacyAtmosphere);
            if (!body.useLegacyAtmosphere)
            {
                ConfigNode PCnode = node.GetNode("pressureCurve");
                if (PCnode != null)
                {
                    body.altitudeMultiplier = 1f;
                    body.pressureMultiplier = 1f;
                    AnimationCurve pressureCurve = Utils.LoadAnimationCurve(PCnode);
                    if (pressureCurve != null)
                        body.pressureCurve = pressureCurve;
                    else
                    {
                        body.useLegacyAtmosphere = true;
                        Debug.LogWarning("Unable to load pressureCurve data for " + body.name + ": Using legacy atmosphere");
                    }
                    print("*RSS* finished with " + body.GetName() + ".pressureCurve (" + body.pressureCurve.keys.Length.ToString() + " keys)");
                }
                else
                {
                    print("*RSS* useLegacyAtmosphere = False but pressureCurve not found!");
                }
            }
            if (node.HasNode("temperatureCurve"))
            {
                ConfigNode TCnode = node.GetNode("temperatureCurve");
                if (TCnode != null)
                {
                    AnimationCurve temperatureCurve = Utils.LoadAnimationCurve(TCnode);
                    if (temperatureCurve != null)
                    {
						body.temperatureCurve = temperatureCurve;
						// Following two lines corrects situations where planets without atmosphere's have these two fields zeroed out
						// Maybe think about making these configurable in the planet's config node? yes? no? meh?
						body.atmoshpereTemperatureMultiplier = 1f;
						body.altitudeMultiplier = 1f;
                        print("*RSS* found and loaded temperatureCurve data for " + body.name);
                    }
                }
            }
            #endregion

            #region CBRotation
            node.TryGetValue("rotationPeriod", ref body.rotationPeriod);
            node.TryGetValue("tidallyLocked", ref body.tidallyLocked);
            node.TryGetValue("initialRotation", ref body.initialRotation);
            node.TryGetValue("inverseRotation", ref body.inverseRotation);
            #endregion

            if (updateMass)
                GeeASLToOthers(body);

            /*if (node.HasValue("axialTilt"))
            {
                if (!body.inverseRotation && double.TryParse(node.GetValue("axialTilt"), out dtmp))
                {
                    CBRotationFixer.CBRotations.Add(body.name, new CBRotation(body.name, dtmp, body.rotationPeriod, body.initialRotation));
                    body.rotationPeriod = 0;
                }
            }*/
            yield return null;

            #region CBOrbit
            ConfigNode onode = node.GetNode("Orbit");
            if (body.orbitDriver != null && body.orbit != null && onode != null)
            {
                if (loadInfo.useEpoch)
                    body.orbit.epoch = loadInfo.epoch;

                onode.TryGetValue("semiMajorAxis", ref body.orbit.semiMajorAxis);
                onode.TryGetValue("eccentricity", ref body.orbit.eccentricity);
                onode.TryGetValue("meanAnomalyAtEpoch", ref body.orbit.meanAnomalyAtEpoch);
                if (onode.TryGetValue("meanAnomalyAtEpochD", ref body.orbit.meanAnomalyAtEpoch))
                    body.orbit.meanAnomalyAtEpoch *= DEG2RAD;
                onode.TryGetValue("inclination", ref body.orbit.inclination);
                onode.TryGetValue("period", ref body.orbit.period);
                onode.TryGetValue("LAN", ref body.orbit.LAN);
                onode.TryGetValue("argumentOfPeriapsis", ref body.orbit.argumentOfPeriapsis);
                if (onode.HasValue("orbitColor"))
                {
                    try
                    {
                        Vector4 col = KSPUtil.ParseVector4(onode.GetValue("orbitColor"));
                        Color c = new Color(col.x, col.y, col.z, col.w);
                        body.GetOrbitDriver().orbitColor = c;
                    }
                    catch (Exception e)
                    {
                        print("*RSS* Error parsing as color4: original text: " + onode.GetValue("orbitColor") + " --- exception " + e.Message);
                    }
                }
                string bodyname = "";
                if (onode.TryGetValue("referenceBody", ref bodyname))
                {
                    if (body.orbit.referenceBody == null || !body.orbit.referenceBody.Equals(bodyname))
                    {
                        foreach (CelestialBody b in FlightGlobals.Bodies)
                        {
                            if (b.name.Equals(bodyname))
                            {
                                if (body.orbit.referenceBody)
                                {
                                    body.orbit.referenceBody.orbitingBodies.Remove(body);
                                }
                                b.orbitingBodies.Add(body);
                                body.orbit.referenceBody = b;
                                break;
                            }
                        }
                    }
                }
            }
            yield return null;
            // SOI and HillSphere done at end
            body.CBUpdate();
            #endregion
            #endregion

            #region SSPQSFade
            // Scaled space fader
            float SSFMult = 1.0f;
            float SSFStart = -1, SSFEnd = -1;
            node.TryGetValue("SSFStart", ref SSFStart);
            node.TryGetValue("SSFEnd", ref SSFEnd);
            node.TryGetValue("SSFMult", ref SSFMult);

            foreach (ScaledSpaceFader ssf in Resources.FindObjectsOfTypeAll(typeof(ScaledSpaceFader)))
            {
                if (ssf.celestialBody != null)
                {
                    if (ssf.celestialBody.name.Equals(node.name))
                    {
                        if (SSFStart >= 0)
                            ssf.fadeStart = SSFStart;
                        else
                            ssf.fadeStart *= SSFMult;

                        if (SSFEnd >= 0)
                            ssf.fadeEnd = SSFEnd;
                        else
                            ssf.fadeEnd *= SSFMult;
                    }
                }
            }
            // The CBT that fades out the PQS
            // Should probably do this as just another PQSMod, actually.
            foreach (PQSMod_CelestialBodyTransform c in Resources.FindObjectsOfTypeAll(typeof(PQSMod_CelestialBodyTransform)))
            {
                try
                {
                    if (c.body != null)
                    {
                        if (c.body.name.Equals(node.name))
                        {
                            print("Found CBT for " + node.name);
                            node.TryGetValue("PQSdeactivateAltitude", ref c.deactivateAltitude);
                            if (c.planetFade != null)
                            {
                                node.TryGetValue("PQSfadeStart", ref c.planetFade.fadeStart);
                                node.TryGetValue("PQSfadeEnd", ref c.planetFade.fadeEnd);
                                if (c.secondaryFades != null)
                                {
                                    foreach (PQSMod_CelestialBodyTransform.AltitudeFade af in c.secondaryFades)
                                    {
                                        node.TryGetValue("PQSSecfadeStart", ref af.fadeStart);
                                        node.TryGetValue("PQSSecfadeEnd", ref af.fadeEnd);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    print("CBT fix for " + node.name + " failed: " + e.Message);
                }
            }
            print("Did CBT for " + node.name);
            yield return null;
            #endregion

            #region Science
            // Science
            if (node.HasNode("CelestialBodyScienceParams"))
            {
                guiMinor = "Science";
                //OnGui();
                ConfigNode spNode = node.GetNode("CelestialBodyScienceParams");
                if (body.scienceValues != null)
                {
                    foreach (ConfigNode.Value val in spNode.values)
                    {
                        // meh, for now hard-code it. Saves worry of GIGO.
                        /*if(body.scienceValues.GetType().GetField(val.name) != null)
                            if(float.TryParse(val.value, out ftmp))
                                body.scienceValues.GetType().GetField(val.name).SetValue(*/
                        spNode.TryGetValue("LandedDataValue", ref body.scienceValues.LandedDataValue);
                        spNode.TryGetValue("SplashedDataValue", ref body.scienceValues.SplashedDataValue);
                        spNode.TryGetValue("FlyingLowDataValue", ref body.scienceValues.FlyingLowDataValue);
                        spNode.TryGetValue("FlyingHighDataValue", ref body.scienceValues.FlyingHighDataValue);
                        spNode.TryGetValue("InSpaceLowDataValue", ref body.scienceValues.InSpaceLowDataValue);
                        spNode.TryGetValue("InSpaceHighDataValue", ref body.scienceValues.InSpaceHighDataValue);
                        spNode.TryGetValue("RecoveryValue", ref body.scienceValues.RecoveryValue);
                        spNode.TryGetValue("flyingAltitudeThreshold", ref body.scienceValues.flyingAltitudeThreshold);
                        spNode.TryGetValue("spaceAltitudeThreshold", ref body.scienceValues.spaceAltitudeThreshold);
                    }
                }
                guiMinor = "";
                //OnGui();
            }
            yield return null;
            #endregion
        }
        private IEnumerator<YieldInstruction> LoadFinishOrbits()
        {
            if (loadInfo.orbitCalc)
            {
                // do final update for all SoIs and hillSpheres and periods
                guiMajor = "Final orbit pass";
                //OnGui();
                print("*RSS* Doing final orbit pass");
                foreach (CelestialBody body in FlightGlobals.fetch.bodies)
                {
                    guiMinor = body.name;
                    guiExtra = "Orbital params";
                    yield return null;
                    // this used to be in a try, seems unnecessary
                    if (body.orbitDriver != null)
                    {
                        if (body.referenceBody != null)
                        {
                            body.hillSphere = body.orbit.semiMajorAxis * (1.0 - body.orbit.eccentricity) * Math.Pow(body.Mass / body.orbit.referenceBody.Mass, 1 / 3);
                            body.sphereOfInfluence = body.orbit.semiMajorAxis * Math.Pow(body.Mass / body.orbit.referenceBody.Mass, 0.4);
                            if (body.sphereOfInfluence < body.Radius * 1.5 || body.sphereOfInfluence < body.Radius + 20000.0)
                                body.sphereOfInfluence = Math.Max(body.Radius * 1.5, body.Radius + 20000.0); // sanity check

                            body.orbit.period = 2 * Math.PI * Math.Sqrt(Math.Pow(body.orbit.semiMajorAxis, 2) / 6.674E-11 * body.orbit.semiMajorAxis / (body.Mass + body.referenceBody.Mass));
                            if (body.orbit.eccentricity <= 1.0)
                            {
                                body.orbit.meanAnomaly = body.orbit.meanAnomalyAtEpoch;
                                body.orbit.orbitPercent = body.orbit.meanAnomalyAtEpoch / (Math.PI * 2);
                                body.orbit.ObTAtEpoch = body.orbit.orbitPercent * body.orbit.period;
                            }
                            else
                            {
                                // ignores this body's own mass for this one...
                                body.orbit.meanAnomaly = body.orbit.meanAnomalyAtEpoch;
                                body.orbit.ObT = Math.Pow(Math.Pow(Math.Abs(body.orbit.semiMajorAxis), 3.0) / body.orbit.referenceBody.gravParameter, 0.5) * body.orbit.meanAnomaly;
                                body.orbit.ObTAtEpoch = body.orbit.ObT;
                            }
                            print("Computing params for " + body.name + ". SoI = " + body.sphereOfInfluence);
                        }
                        else
                        {
                            body.sphereOfInfluence = double.PositiveInfinity;
                            body.hillSphere = double.PositiveInfinity;
                        }
                        // doesn't seem needed - body.orbitDriver.QueuedUpdate = true;
                    }
                    yield return null;
                    guiExtra = "CBUpdate";
                    yield return null;
                    try
                    {
                        body.CBUpdate();
                    }
                    catch (Exception e)
                    {
                        print("CBUpdate for " + body.name + " failed: " + e.Message);
                    }
                    yield return null;
                }
            }
        }
        private IEnumerator<YieldInstruction> LoadPQS(ConfigNode node, CelestialBody body, double origRadius)
        {
            #region PQS
            double dtmp;
            float ftmp;
            int itmp;
            bool btmp;

            guiMinor = "PQS";
            //OnGui();
            // the Planet Quadtree Sphere
            List<string> PQSs = new List<string>();
            bool custom = false;
            if (node.HasNode("PQS"))
            {
                foreach (ConfigNode n in node.GetNode("PQS").nodes)
                    PQSs.Add(n.name);
                custom = true;
            }
            else
            {
                if (body.Radius != origRadius)
                {
                    PQSs.Add(node.name);
                    PQSs.Add(node.name + "Ocean");
                }
            }
            foreach (string pName in PQSs)
            {
                print("Finding PQS " + pName);
                foreach (PQS p in Resources.FindObjectsOfTypeAll(typeof(PQS)))
                {
                    if (p.name.Equals(pName))
                    {
                        if (body.pqsController != p)
                            if (body.pqsController != p.parentSphere)
                                continue;
                        guiMinor = "PQS " + p.name;
                        //OnGui();
                        p.radius = body.Radius;
                        print("Editing PQS " + pName + ", default set radius = " + p.radius);
                        if (custom) // YES, THIS IS SILLY
                        // I SHOULD JUST WRITE A REAL C# EXTENSIBLE LOADER
                        // Oh well. Hacks are quicker.
                        {
                            ConfigNode pqsNode = node.GetNode("PQS").GetNode(pName);

                            // PQS members
                            if (pqsNode.HasValue("radius"))
                            {
                                if (double.TryParse(pqsNode.GetValue("radius"), out dtmp))
                                {
                                    p.radius = dtmp;
                                    print("Editing PQS " + pName + ", config set radius = " + p.radius);
                                }
                            }
                            if (pqsNode.HasValue("maxLevel"))
                            {
                                if (int.TryParse(pqsNode.GetValue("maxLevel"), out itmp))
                                {
                                    p.maxLevel = itmp;
                                    try
                                    {
                                        PQSCache.PresetList.GetPreset(p.name).maxSubdivision = itmp;
                                    }
                                    catch (Exception e)
                                    {
                                        print("*RSS* ERROR: Applying change to preset for " + p.name + ", exception: " + e.Message);
                                    }
                                }
                            }
                            if (pqsNode.HasValue("maxQuadLenghtsPerFrame"))
                            {
                                if (float.TryParse(pqsNode.GetValue("maxQuadLenghtsPerFrame"), out ftmp))
                                {
                                    p.maxQuadLenghtsPerFrame = ftmp;
                                }
                            }
                            if (pqsNode.HasValue("visRadAltitudeMax"))
                            {
                                if (double.TryParse(pqsNode.GetValue("visRadAltitudeMax"), out dtmp))
                                {
                                    p.visRadAltitudeMax = dtmp;
                                }
                            }
                            if (pqsNode.HasValue("visRadAltitudeValue"))
                            {
                                if (double.TryParse(pqsNode.GetValue("visRadAltitudeValue"), out dtmp))
                                {
                                    p.visRadAltitudeValue = dtmp;
                                }
                            }
                            if (pqsNode.HasValue("visRadSeaLevelValue"))
                            {
                                if (double.TryParse(pqsNode.GetValue("visRadSeaLevelValue"), out dtmp))
                                {
                                    p.visRadSeaLevelValue = dtmp;
                                }
                            }

                            yield return null;
                            // PQSMods
                            var mods = p.transform.GetComponentsInChildren(typeof(PQSMod), true);
                            foreach (var m in mods)
                            {
                                print("Processing " + m.GetType().Name);
                                guiExtra = m.GetType().Name;
                                yield return null;
                                //OnGui();
                                foreach (ConfigNode modNode in pqsNode.nodes)
                                {
                                    if (modNode.name.Equals("PQSMod_VertexSimplexHeightAbsolute") && m.GetType().ToString().Equals(modNode.name))
                                    {
                                        PQSMod_VertexSimplexHeightAbsolute mod = m as PQSMod_VertexSimplexHeightAbsolute;
                                        if (modNode.HasValue("deformity"))
                                        {
                                            if (double.TryParse(modNode.GetValue("deformity"), out dtmp))
                                                mod.deformity = dtmp;
                                        }
                                        if (modNode.HasValue("persistence"))
                                        {
                                            if (double.TryParse(modNode.GetValue("persistence"), out dtmp))
                                                mod.persistence = dtmp;
                                        }
                                        if (modNode.HasValue("frequency"))
                                        {
                                            if (double.TryParse(modNode.GetValue("frequency"), out dtmp))
                                                mod.frequency = dtmp;
                                        }
                                        yield return null;
                                        mod.OnSetup();
                                    }
                                    if (modNode.name.Equals("PQSMod_VertexHeightNoiseVertHeightCurve2") && m.GetType().ToString().Equals(modNode.name))
                                    {
                                        PQSMod_VertexHeightNoiseVertHeightCurve2 mod = m as PQSMod_VertexHeightNoiseVertHeightCurve2;
                                        if (modNode.HasValue("deformity"))
                                        {
                                            if (float.TryParse(modNode.GetValue("deformity"), out ftmp))
                                                mod.deformity = ftmp;
                                        }
                                        if (modNode.HasValue("ridgedAddFrequency"))
                                        {
                                            if (float.TryParse(modNode.GetValue("ridgedAddFrequency"), out ftmp))
                                                mod.ridgedAddFrequency = ftmp;
                                        }
                                        if (modNode.HasValue("ridgedSubFrequency"))
                                        {
                                            if (float.TryParse(modNode.GetValue("ridgedSubFrequency"), out ftmp))
                                                mod.ridgedSubFrequency = ftmp;
                                        }
                                        if (modNode.HasValue("ridgedAddOctaves"))
                                        {
                                            if (int.TryParse(modNode.GetValue("ridgedAddOctaves"), out itmp))
                                                mod.ridgedAddOctaves = itmp;
                                        }
                                        if (modNode.HasValue("simplexHeightStart"))
                                        {
                                            if (float.TryParse(modNode.GetValue("simplexHeightStart"), out ftmp))
                                                mod.simplexHeightStart = ftmp;
                                        }
                                        if (modNode.HasValue("simplexHeightEnd"))
                                        {
                                            if (float.TryParse(modNode.GetValue("simplexHeightEnd"), out ftmp))
                                                mod.simplexHeightEnd = ftmp;
                                        }
                                        yield return null;
                                        mod.OnSetup();
                                    }
                                    if (modNode.name.Equals("PQSMod_VertexRidgedAltitudeCurve") && m.GetType().ToString().Equals(modNode.name))
                                    {
                                        PQSMod_VertexRidgedAltitudeCurve mod = m as PQSMod_VertexRidgedAltitudeCurve;
                                        if (modNode.HasValue("deformity"))
                                        {
                                            if (float.TryParse(modNode.GetValue("deformity"), out ftmp))
                                                mod.deformity = ftmp;
                                        }
                                        if (modNode.HasValue("ridgedAddFrequency"))
                                        {
                                            if (float.TryParse(modNode.GetValue("ridgedAddFrequency"), out ftmp))
                                                mod.ridgedAddFrequency = ftmp;
                                        }
                                        if (modNode.HasValue("ridgedAddOctaves"))
                                        {
                                            if (int.TryParse(modNode.GetValue("ridgedAddOctaves"), out itmp))
                                                mod.ridgedAddOctaves = itmp;
                                        }
                                        if (modNode.HasValue("simplexHeightStart"))
                                        {
                                            if (float.TryParse(modNode.GetValue("simplexHeightStart"), out ftmp))
                                                mod.simplexHeightStart = ftmp;
                                        }
                                        if (modNode.HasValue("simplexHeightEnd"))
                                        {
                                            if (float.TryParse(modNode.GetValue("simplexHeightEnd"), out ftmp))
                                                mod.simplexHeightEnd = ftmp;
                                        }
                                        yield return null;
                                        mod.OnSetup();
                                    }
                                    if (modNode.name.Equals("PQSMod_VertexPlanet") && m.GetType().ToString().Equals(modNode.name))
                                    {
                                        PQSMod_VertexPlanet mod = m as PQSMod_VertexPlanet;
                                        if (modNode.HasValue("seed"))
                                        {
                                            if (int.TryParse(modNode.GetValue("seed"), out itmp))
                                                mod.seed = itmp;
                                        }
                                        if (modNode.HasValue("deformity"))
                                        {
                                            if (double.TryParse(modNode.GetValue("deformity"), out dtmp))
                                                mod.deformity = dtmp;
                                        }
                                        if (modNode.HasValue("colorDeformity"))
                                        {
                                            if (double.TryParse(modNode.GetValue("colorDeformity"), out dtmp))
                                                mod.colorDeformity = dtmp;
                                        }
                                        if (modNode.HasValue("oceanLevel"))
                                        {
                                            if (double.TryParse(modNode.GetValue("oceanLevel"), out dtmp))
                                                mod.oceanLevel = dtmp;
                                        }
                                        if (modNode.HasValue("oceanStep"))
                                        {
                                            if (double.TryParse(modNode.GetValue("oceanStep"), out dtmp))
                                                mod.oceanStep = dtmp;
                                        }
                                        if (modNode.HasValue("oceanDepth"))
                                        {
                                            if (double.TryParse(modNode.GetValue("oceanDepth"), out dtmp))
                                                mod.oceanDepth = dtmp;
                                        }
                                        if (modNode.HasValue("oceanSnap"))
                                        {
                                            if (bool.TryParse(modNode.GetValue("oceanSnap"), out btmp))
                                                mod.oceanSnap = btmp;
                                        }
                                        if (modNode.HasValue("terrainSmoothing"))
                                        {
                                            if (double.TryParse(modNode.GetValue("terrainSmoothing"), out dtmp))
                                                mod.terrainSmoothing = dtmp;
                                        }
                                        if (modNode.HasValue("terrainShapeStart"))
                                        {
                                            if (double.TryParse(modNode.GetValue("terrainShapeStart"), out dtmp))
                                                mod.terrainShapeStart = dtmp;
                                        }
                                        if (modNode.HasValue("terrainShapeEnd"))
                                        {
                                            if (double.TryParse(modNode.GetValue("terrainShapeEnd"), out dtmp))
                                                mod.terrainShapeEnd = dtmp;
                                        }
                                        if (modNode.HasValue("terrainRidgesMin"))
                                        {
                                            if (double.TryParse(modNode.GetValue("terrainRidgesMin"), out dtmp))
                                                mod.terrainRidgesMin = dtmp;
                                        }
                                        if (modNode.HasValue("terrainRidgesMax"))
                                        {
                                            if (double.TryParse(modNode.GetValue("terrainRidgesMax"), out dtmp))
                                                mod.terrainRidgesMax = dtmp;
                                        }
                                        if (modNode.HasValue("buildHeightColors"))
                                        {
                                            if (bool.TryParse(modNode.GetValue("buildHeightColors"), out btmp))
                                                mod.buildHeightColors = btmp;
                                        }
                                        if (modNode.HasValue("terrainRidgeBalance"))
                                        {
                                            if (double.TryParse(modNode.GetValue("terrainRidgeBalance"), out dtmp))
                                                mod.terrainRidgeBalance = dtmp;
                                        }
                                        if (modNode.HasValue("order"))
                                        {
                                            if (int.TryParse(modNode.GetValue("order"), out itmp))
                                                mod.order = itmp;
                                        }
                                        // Also supports Landclasses. Maybe come back and do these later...
                                        yield return null;
                                        mod.OnSetup();
                                    }
                                    ///
                                    if (modNode.name.Equals("PQSMod_VertexHeightMap") && m.GetType().ToString().Equals(modNode.name))
                                    {
                                        PQSMod_VertexHeightMap mod = m as PQSMod_VertexHeightMap;
                                        if (modNode.HasValue("heightMapDeformity"))
                                        {
                                            if (double.TryParse(modNode.GetValue("heightMapDeformity"), out dtmp))
                                                mod.heightMapDeformity = dtmp;
                                        }
                                        if (modNode.HasValue("heightMapOffset"))
                                        {
                                            if (double.TryParse(modNode.GetValue("heightMapOffset"), out dtmp))
                                                mod.heightMapOffset = dtmp;
                                            print("*RSS* Set offset " + mod.heightMapOffset);
                                        }
                                        if (modNode.HasValue("heightMap"))
                                        {
                                            Texture2D map = null;
                                            bool localLoad = Utils.LoadTexture(modNode.GetValue("heightMap"), ref map, false, false, false);
                                            if((object)map != null)
                                            {
                                                yield return null;
                                                mod.heightMap.CreateMap(MapSO.MapDepth.Greyscale, map);
                                                if (localLoad)
                                                {
                                                    DestroyImmediate(map);
                                                    map = null;
                                                }
                                                yield return null;
                                            }
                                            else
                                                print("*RSS* *ERROR* texture does not exist! " + modNode.GetValue("heightMap"));
                                        }
                                        if (modNode.HasValue("order"))
                                        {
                                            if (int.TryParse(modNode.GetValue("order"), out itmp))
                                                mod.order = itmp;
                                        }
                                        yield return null;
                                        mod.OnSetup();
                                    }
                                    if (modNode.name.Equals("PQSMod_AltitudeAlpha") && m.GetType().ToString().Equals(modNode.name))
                                    {
                                        PQSMod_AltitudeAlpha mod = m as PQSMod_AltitudeAlpha;
                                        if (modNode.HasValue("atmosphereDepth"))
                                        {
                                            if (double.TryParse(modNode.GetValue("atmosphereDepth"), out dtmp))
                                                mod.atmosphereDepth = dtmp;
                                        }
                                        yield return null;
                                        mod.OnSetup();
                                    }
                                    if (modNode.name.Equals("PQSMod_AerialPerspectiveMaterial") && m.GetType().ToString().Equals(modNode.name))
                                    {
                                        PQSMod_AerialPerspectiveMaterial mod = m as PQSMod_AerialPerspectiveMaterial;
                                        if (modNode.HasValue("heightMapDeformity"))
                                        {
                                            if (float.TryParse(modNode.GetValue("heightMapDeformity"), out ftmp))
                                                mod.atmosphereDepth = ftmp;
                                        }
                                        yield return null;
                                        mod.OnSetup();
                                    }
                                    if (modNode.name.Equals("PQSMod_VertexSimplexHeight") && m.GetType().ToString().Equals(modNode.name))
                                    {
                                        PQSMod_VertexSimplexHeight mod = m as PQSMod_VertexSimplexHeight;
                                        if (modNode.HasValue("deformity"))
                                        {
                                            if (double.TryParse(modNode.GetValue("deformity"), out dtmp))
                                                mod.deformity = dtmp;
                                        }
                                        if (modNode.HasValue("persistence"))
                                        {
                                            if (double.TryParse(modNode.GetValue("persistence"), out dtmp))
                                                mod.persistence = dtmp;
                                        }
                                        if (modNode.HasValue("frequency"))
                                        {
                                            if (double.TryParse(modNode.GetValue("frequency"), out dtmp))
                                                mod.frequency = dtmp;
                                        }
                                        if (modNode.HasValue("octaves"))
                                        {
                                            if (double.TryParse(modNode.GetValue("octaves"), out dtmp))
                                                mod.octaves = dtmp;
                                        }
                                        yield return null;
                                        mod.OnSetup();
                                    }
                                    if (modNode.name.Equals("PQSMod_VertexHeightNoiseVertHeight") && m.GetType().ToString().Equals(modNode.name))
                                    {
                                        PQSMod_VertexHeightNoiseVertHeight mod = m as PQSMod_VertexHeightNoiseVertHeight;
                                        if (modNode.HasValue("deformity"))
                                        {
                                            if (float.TryParse(modNode.GetValue("deformity"), out ftmp))
                                                mod.deformity = ftmp;
                                        }
                                        if (modNode.HasValue("frequency"))
                                        {
                                            if (float.TryParse(modNode.GetValue("frequency"), out ftmp))
                                                mod.frequency = ftmp;
                                        }
                                        if (modNode.HasValue("octaves"))
                                        {
                                            if (int.TryParse(modNode.GetValue("octaves"), out itmp))
                                                mod.octaves = itmp;
                                        }
                                        yield return null;
                                        mod.OnSetup();
                                    }
                                    if (modNode.name.Equals("PQSMod_VoronoiCraters") && m.GetType().ToString().Equals(modNode.name))
                                    {
                                        PQSMod_VoronoiCraters mod = m as PQSMod_VoronoiCraters;
                                        if (modNode.HasValue("KEYvoronoiSeed"))
                                            if (int.Parse(modNode.GetValue("KEYvoronoiSeed")) != mod.voronoiSeed)
                                                continue;

                                        if (modNode.HasValue("deformation"))
                                        {
                                            if (double.TryParse(modNode.GetValue("deformation"), out dtmp))
                                                mod.deformation = dtmp;
                                        }
                                        if (modNode.HasValue("voronoiFrequency"))
                                        {
                                            if (double.TryParse(modNode.GetValue("voronoiFrequency"), out dtmp))
                                                mod.voronoiFrequency = dtmp;
                                        }
                                        yield return null;
                                        mod.OnSetup();
                                    }
                                    if (modNode.name.Equals("PQSMod_VertexHeightNoise") && m.GetType().ToString().Equals(modNode.name))
                                    {
                                        PQSMod_VertexHeightNoise mod = m as PQSMod_VertexHeightNoise;
                                        if (modNode.HasValue("deformity"))
                                        {
                                            if (float.TryParse(modNode.GetValue("deformity"), out ftmp))
                                                mod.deformity = ftmp;
                                        }
                                        if (modNode.HasValue("frequency"))
                                        {
                                            if (float.TryParse(modNode.GetValue("frequency"), out ftmp))
                                                mod.frequency = ftmp;
                                        }
                                        yield return null;
                                        mod.OnSetup();
                                    }
                                    if (modNode.name.Equals("PQSLandControl") && m.GetType().ToString().Equals(modNode.name))
                                    {
                                        PQSLandControl mod = m as PQSLandControl;
                                        if (modNode.HasValue("useHeightMap"))
                                        {
                                            if (bool.TryParse(modNode.GetValue("useHeightMap"), out btmp))
                                                mod.useHeightMap = btmp;

                                            if (mod.useHeightMap && modNode.HasValue("heightMap"))
                                            {
                                                Texture2D map = null;
                                                bool localLoad = Utils.LoadTexture(modNode.GetValue("heightMap"), ref map, false, false, false);
                                                if ((object)map != null)
                                                {
                                                    yield return null;
                                                    mod.heightMap.CreateMap(MapSO.MapDepth.Greyscale, map);
                                                    if (localLoad)
                                                    {
                                                        DestroyImmediate(map);
                                                        map = null;
                                                    }
                                                    yield return null;
                                                }
                                                else
                                                {
                                                    print("*RSS* *ERROR* texture does not exist! " + modNode.GetValue("heightMap"));
                                                    mod.useHeightMap = false;
                                                }
                                            }
                                            else
                                                mod.useHeightMap = false; // If there was no heightMap given
                                        }


                                        if (modNode.HasValue("vHeightMax"))
                                        {
                                            if (float.TryParse(modNode.GetValue("vHeightMax"), out ftmp))
                                                mod.vHeightMax = ftmp;
                                        }
                                        if (modNode.HasValue("altitudeBlend"))
                                        {
                                            if (float.TryParse(modNode.GetValue("altitudeBlend"), out ftmp))
                                                mod.altitudeBlend = ftmp;
                                        }
                                        if (modNode.HasValue("altitudeFrequency"))
                                        {
                                            if (float.TryParse(modNode.GetValue("altitudeFrequency"), out ftmp))
                                                mod.altitudeFrequency = ftmp;
                                        }
                                        if (modNode.HasValue("altitudePersistance"))
                                        {
                                            if (float.TryParse(modNode.GetValue("altitudePersistance"), out ftmp))
                                                mod.altitudePersistance = ftmp;
                                        }
                                        if (modNode.HasValue("latitudeBlend"))
                                        {
                                            if (float.TryParse(modNode.GetValue("latitudeBlend"), out ftmp))
                                                mod.latitudeBlend = ftmp;
                                        }
                                        if (modNode.HasValue("latitudeFrequency"))
                                        {
                                            if (float.TryParse(modNode.GetValue("latitudeFrequency"), out ftmp))
                                                mod.latitudeFrequency = ftmp;
                                        }
                                        if (modNode.HasValue("latitudePersistance"))
                                        {
                                            if (float.TryParse(modNode.GetValue("latitudePersistance"), out ftmp))
                                                mod.latitudePersistance = ftmp;
                                        }
                                        if (modNode.HasValue("longitudeBlend"))
                                        {
                                            if (float.TryParse(modNode.GetValue("longitudeBlend"), out ftmp))
                                                mod.longitudeBlend = ftmp;
                                        }
                                        if (modNode.HasValue("longitudeFrequency"))
                                        {
                                            if (float.TryParse(modNode.GetValue("longitudeFrequency"), out ftmp))
                                                mod.longitudeFrequency = ftmp;
                                        }
                                        if (modNode.HasValue("longitudePersistance"))
                                        {
                                            if (float.TryParse(modNode.GetValue("longitudePersistance"), out ftmp))
                                                mod.longitudePersistance = ftmp;
                                        }
                                        foreach (ConfigNode lcNode in modNode.GetNodes("LandClass"))
                                        {
                                            bool found = false;
                                            string lcName = lcNode.GetValue("landClassName");
                                            foreach (PQSLandControl.LandClass lc in mod.landClasses)
                                            {
                                                if (lc.landClassName.Equals(lcName))
                                                {
                                                    found = true;
                                                    if (lcNode.HasValue("color"))
                                                    {
                                                        try
                                                        {
                                                            Vector4 col = KSPUtil.ParseVector4(lcNode.GetValue("color"));
                                                            lc.color = new Color(col.x, col.y, col.z, col.w);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            print("*RSS* Error parsing as color4: original text: " + lcNode.GetValue("color") + " --- exception " + e.Message);
                                                        }
                                                    }
                                                    if (lcNode.HasValue("noiseColor"))
                                                    {
                                                        try
                                                        {
                                                            Vector4 col = KSPUtil.ParseVector4(lcNode.GetValue("noiseColor"));
                                                            lc.noiseColor = new Color(col.x, col.y, col.z, col.w);
                                                        }
                                                        catch (Exception e)
                                                        {
                                                            print("*RSS* Error parsing as color4: original text: " + lcNode.GetValue("noiseColor") + " --- exception " + e.Message);
                                                        }
                                                    }

                                                    // ranges
                                                    if (lcNode.HasNode("altitudeRange"))
                                                    {
                                                        ConfigNode range = lcNode.GetNode("altitudeRange");
                                                        if (range.HasValue("startStart"))
                                                            if (double.TryParse(range.GetValue("startStart"), out dtmp))
                                                                lc.altitudeRange.startStart = dtmp;
                                                        if (range.HasValue("startEnd"))
                                                            if (double.TryParse(range.GetValue("startEnd"), out dtmp))
                                                                lc.altitudeRange.startEnd = dtmp;
                                                        if (range.HasValue("endStart"))
                                                            if (double.TryParse(range.GetValue("endStart"), out dtmp))
                                                                lc.altitudeRange.endStart = dtmp;
                                                        if (range.HasValue("endEnd"))
                                                            if (double.TryParse(range.GetValue("endEnd"), out dtmp))
                                                                lc.altitudeRange.endEnd = dtmp;
                                                    }
                                                    if (lcNode.HasNode("latitudeRange"))
                                                    {
                                                        ConfigNode range = lcNode.GetNode("latitudeRange");
                                                        if (range.HasValue("startStart"))
                                                            if (double.TryParse(range.GetValue("startStart"), out dtmp))
                                                                lc.latitudeRange.startStart = dtmp;
                                                        if (range.HasValue("startEnd"))
                                                            if (double.TryParse(range.GetValue("startEnd"), out dtmp))
                                                                lc.latitudeRange.startEnd = dtmp;
                                                        if (range.HasValue("endStart"))
                                                            if (double.TryParse(range.GetValue("endStart"), out dtmp))
                                                                lc.latitudeRange.endStart = dtmp;
                                                        if (range.HasValue("endEnd"))
                                                            if (double.TryParse(range.GetValue("endEnd"), out dtmp))
                                                                lc.latitudeRange.endEnd = dtmp;
                                                    }
                                                    if (lcNode.HasNode("longitudeRange"))
                                                    {
                                                        ConfigNode range = lcNode.GetNode("longitudeRange");
                                                        if (range.HasValue("startStart"))
                                                            if (double.TryParse(range.GetValue("startStart"), out dtmp))
                                                                lc.longitudeRange.startStart = dtmp;
                                                        if (range.HasValue("startEnd"))
                                                            if (double.TryParse(range.GetValue("startEnd"), out dtmp))
                                                                lc.longitudeRange.startEnd = dtmp;
                                                        if (range.HasValue("endStart"))
                                                            if (double.TryParse(range.GetValue("endStart"), out dtmp))
                                                                lc.longitudeRange.endStart = dtmp;
                                                        if (range.HasValue("endEnd"))
                                                            if (double.TryParse(range.GetValue("endEnd"), out dtmp))
                                                                lc.longitudeRange.endEnd = dtmp;
                                                    }
                                                    if (lcNode.HasValue("latitudeDouble"))
                                                    {
                                                        if (bool.TryParse(lcNode.GetValue("latitudeDouble"), out btmp))
                                                            lc.latitudeDouble = btmp;
                                                    }
                                                    if (lcNode.HasValue("minimumRealHeight"))
                                                        if (double.TryParse(lcNode.GetValue("minimumRealHeight"), out dtmp))
                                                            lc.minimumRealHeight = dtmp;
                                                    if (lcNode.HasValue("alterRealHeight"))
                                                        if (double.TryParse(lcNode.GetValue("alterRealHeight"), out dtmp))
                                                            lc.alterRealHeight = dtmp;
                                                    if (lcNode.HasValue("alterApparentHeight"))
                                                        if (float.TryParse(lcNode.GetValue("alterApparentHeight"), out ftmp))
                                                            lc.alterApparentHeight = ftmp;


                                                    break; // don't need to find any more
                                                }
                                            }
                                            if (!found)
                                                print("*RSS* LandClass " + lcName + " not found in PQSLandControl for PQS " + p.name + " of CB " + body.name);
                                        }
                                        yield return null;
                                        mod.OnSetup();
                                    }

                                    // City
                                    if (modNode.name.Equals("PQSCity") && m.GetType().ToString().Equals(modNode.name))
                                    {
                                        PQSCity mod = m as PQSCity;
                                        if (modNode.HasValue("KEYname"))
                                            if (!(mod.name.Equals(modNode.GetValue("KEYname"))))
                                                continue;

                                        if (modNode.HasValue("repositionRadial"))
                                        {
                                            try
                                            {
                                                mod.repositionRadial = KSPUtil.ParseVector3(modNode.GetValue("repositionRadial"));
                                            }
                                            catch (Exception e)
                                            {
                                                print("*RSS* Error parsing as vec3: original text: " + modNode.GetValue("repositionRadial") + " --- exception " + e.Message);
                                            }
                                        }
                                        if (modNode.HasValue("latitude") && modNode.HasValue("longitude"))
                                        {
                                            double lat, lon;
                                            double.TryParse(modNode.GetValue("latitude"), out lat);
                                            double.TryParse(modNode.GetValue("longitude"), out lon);

                                            mod.repositionRadial = LLAtoECEF(lat, lon, 0, body.Radius);
                                        }
                                        if (modNode.HasValue("reorientInitialUp"))
                                        {
                                            try
                                            {
                                                mod.reorientInitialUp = KSPUtil.ParseVector3(modNode.GetValue("reorientInitialUp"));
                                            }
                                            catch (Exception e)
                                            {
                                                print("*RSS* Error parsing as vec3: original text: " + modNode.GetValue("reorientInitialUp") + " --- exception " + e.Message);
                                            }
                                        }
                                        if (modNode.HasValue("repositionToSphere"))
                                        {
                                            if (bool.TryParse(modNode.GetValue("repositionToSphere"), out btmp))
                                                mod.repositionToSphere = btmp;
                                        }
                                        if (modNode.HasValue("repositionToSphereSurface"))
                                        {
                                            if (bool.TryParse(modNode.GetValue("repositionToSphereSurface"), out btmp))
                                                mod.repositionToSphereSurface = btmp;
                                        }
                                        if (modNode.HasValue("repositionToSphereSurfaceAddHeight"))
                                        {
                                            if (bool.TryParse(modNode.GetValue("repositionToSphereSurfaceAddHeight"), out btmp))
                                                mod.repositionToSphereSurfaceAddHeight = btmp;
                                        }
                                        if (modNode.HasValue("reorientToSphere"))
                                        {
                                            if (bool.TryParse(modNode.GetValue("reorientToSphere"), out btmp))
                                                mod.reorientToSphere = btmp;
                                        }
                                        if (modNode.HasValue("repositionRadiusOffset"))
                                        {
                                            if (double.TryParse(modNode.GetValue("repositionRadiusOffset"), out dtmp))
                                                mod.repositionRadiusOffset = dtmp;
                                        }
                                        if (modNode.HasValue("lodvisibleRangeMult"))
                                        {
                                            if (double.TryParse(modNode.GetValue("lodvisibleRangeMult"), out dtmp))
                                                foreach (PQSCity.LODRange l in mod.lod)
                                                    l.visibleRange *= (float)dtmp;
                                        }

                                        if (modNode.HasValue("reorientFinalAngle"))
                                        {
                                            if (float.TryParse(modNode.GetValue("reorientFinalAngle"), out ftmp))
                                                mod.reorientFinalAngle = ftmp;
                                        }
                                        yield return null;
                                        mod.OnSetup();
                                    }
                                    // KSC Flat area
                                    if (modNode.name.Equals("PQSMod_MapDecalTangent") && m.GetType().ToString().Equals(modNode.name))
                                    {
                                        // thanks to asmi for this!
                                        PQSMod_MapDecalTangent mod = m as PQSMod_MapDecalTangent;
                                        if (modNode.HasValue("position"))
                                        {
                                            try
                                            {
                                                mod.position = KSPUtil.ParseVector3(modNode.GetValue("position"));
                                            }
                                            catch (Exception e)
                                            {
                                                print("*RSS* Error parsing as vec3: original text: " + modNode.GetValue("position") + " --- exception " + e.Message);
                                            }
                                        }
                                        if (modNode.HasValue("radius"))
                                        {
                                            if (double.TryParse(modNode.GetValue("radius"), out dtmp))
                                                mod.radius = dtmp;
                                        }
                                        if (modNode.HasValue("heightMapDeformity"))
                                        {
                                            if (double.TryParse(modNode.GetValue("heightMapDeformity"), out dtmp))
                                                mod.heightMapDeformity = dtmp;
                                        }
                                        if (modNode.HasValue("absoluteOffset"))
                                        {
                                            if (double.TryParse(modNode.GetValue("absoluteOffset"), out dtmp))
                                                mod.absoluteOffset = dtmp;
                                        }

                                        if (modNode.HasValue("absolute"))
                                        {
                                            if (bool.TryParse(modNode.GetValue("absolute"), out btmp))
                                                mod.absolute = btmp;
                                        }
                                        if (modNode.HasValue("rescaleToRadius"))
                                        {
                                            mod.position *= (float)(body.Radius / origRadius);
                                            mod.radius *= (body.Radius / origRadius);
                                        }
                                        if (modNode.HasValue("latitude") && modNode.HasValue("longitude"))
                                        {
                                            double lat, lon;
                                            double.TryParse(modNode.GetValue("latitude"), out lat);
                                            double.TryParse(modNode.GetValue("longitude"), out lon);

                                            mod.position = LLAtoECEF(lat, lon, 0, body.Radius);
                                        }
                                        yield return null;
                                        mod.OnSetup();
                                    }
                                    if (modNode.name.Equals("PQSMod_VertexColorMapBlend") && m.GetType().ToString().Equals(modNode.name))
                                    {
                                        PQSMod_VertexColorMapBlend mod = m as PQSMod_VertexColorMapBlend;
                                        if (modNode.HasValue("blend"))
                                            if (float.TryParse(modNode.GetValue("blend"), out ftmp))
                                                mod.blend = ftmp;

                                        if (modNode.HasValue("order"))
                                            if (int.TryParse(modNode.GetValue("order"), out itmp))
                                                mod.order = itmp;
                                        if (modNode.HasValue("enabled"))
                                            if (bool.TryParse(modNode.GetValue("enabled"), out btmp))
                                                mod.enabled = mod.modEnabled = btmp;

                                        if (modNode.HasValue("vertexColorMap"))
                                        {
                                            // for now don't destroy old map, use GC.
                                            Texture2D map = null;
                                            bool localLoad = Utils.LoadTexture(modNode.GetValue("vertexColorMap"), ref map, false, false, false);
                                            if ((object)map != null)
                                            {
                                                map.LoadImage(System.IO.File.ReadAllBytes(KSPUtil.ApplicationRootPath + modNode.GetValue("vertexColorMap")));
                                                yield return null;
                                                //DestroyImmediate(mod.vertexColorMap);
                                                //mod.vertexColorMap = ScriptableObject.CreateInstance<MapSO>();
                                                mod.vertexColorMap.CreateMap(MapSO.MapDepth.RGB, map);
                                                if (localLoad)
                                                {
                                                    DestroyImmediate(map);
                                                    map = null;
                                                }
                                            }
                                            yield return null;
                                        }
                                        else
                                            print("*RSS* *ERROR* texture does not exist! " + modNode.GetValue("vertexColorMap"));
                                        yield return null;
                                        mod.OnSetup();
                                    }
                                    if (modNode.name.Equals("PQSMod_VertexColorSolid") && m.GetType().ToString().Equals(modNode.name))
                                    {
                                        PQSMod_VertexColorSolid mod = m as PQSMod_VertexColorSolid;
                                        if (modNode.HasValue("blend"))
                                            if (float.TryParse(modNode.GetValue("blend"), out ftmp))
                                                mod.blend = ftmp;

                                        if (modNode.HasValue("order"))
                                            if (int.TryParse(modNode.GetValue("order"), out itmp))
                                                mod.order = itmp;

                                        if (modNode.HasValue("color"))
                                        {
                                            try
                                            {
                                                Vector4 col = KSPUtil.ParseVector4(modNode.GetValue("color"));
                                                Color c = new Color(col.x, col.y, col.z, col.w);
                                                mod.color = c;
                                            }
                                            catch (Exception e)
                                            {
                                                print("*RSS* Error parsing as vec4: original text: " + modNode.GetValue("color") + " --- exception " + e.Message);
                                            }
                                        }
                                        yield return null;
                                        mod.OnSetup();
                                    }
                                }
                            }
                            if (pqsNode.HasNode("Add"))
                            {
                                foreach (ConfigNode modNode in pqsNode.GetNode("Add").nodes)
                                {
                                    print("Adding " + modNode.name);
                                    guiExtra = "Add " + modNode.name;
                                    yield return null;
                                    //OnGui();
                                    if (modNode.name.Equals("PQSMod_VertexColorMapBlend"))
                                    {
                                        Texture2D map = null;
                                        bool localLoad = Utils.LoadTexture(modNode.GetValue("vertexColorMap"), ref map, false, false, false);
                                        if((object)map != null)
                                        {
                                            GameObject tempObj = new GameObject();

                                            PQSMod_VertexColorMapBlend colorMap = (PQSMod_VertexColorMapBlend)tempObj.AddComponent(typeof(PQSMod_VertexColorMapBlend));

                                            tempObj.transform.parent = p.gameObject.transform;
                                            colorMap.sphere = p;

                                            yield return null;
                                            colorMap.vertexColorMap = ScriptableObject.CreateInstance<MapSO>();
                                            colorMap.vertexColorMap.CreateMap(MapSO.MapDepth.RGB, map);
                                            yield return null;
                                            colorMap.blend = 1.0f;
                                            if (modNode.HasValue("blend"))
                                                if (float.TryParse(modNode.GetValue("blend"), out ftmp))
                                                    colorMap.blend = ftmp;

                                            colorMap.order = 9999993;
                                            if (modNode.HasValue("order"))
                                                if (int.TryParse(modNode.GetValue("order"), out itmp))
                                                    colorMap.order = itmp;

                                            colorMap.modEnabled = true;

                                            if (localLoad)
                                            {
                                                DestroyImmediate(map);
                                                map = null;
                                            }
                                            yield return null;
                                            colorMap.OnSetup();
                                        }
                                        else
                                            print("*RSS* *ERROR* texture does not exist! " + modNode.GetValue("vertexColorMap"));

                                    }
                                    if (modNode.name.Equals("PQSMod_VertexSimplexNoiseColor"))
                                    {
                                        GameObject tempObj = new GameObject();
                                        PQSMod_VertexSimplexNoiseColor vertColor = (PQSMod_VertexSimplexNoiseColor)tempObj.AddComponent(typeof(PQSMod_VertexSimplexNoiseColor));

                                        tempObj.transform.parent = p.gameObject.transform;
                                        vertColor.sphere = p;

                                        vertColor.blend = 1.0f;
                                        modNode.TryGetValue("blend", ref vertColor.blend);

                                        vertColor.order = 9999994;
                                        modNode.TryGetValue("order", ref vertColor.order);
                                        modNode.TryGetValue("octaves", ref vertColor.octaves);
                                        modNode.TryGetValue("persistence", ref vertColor.persistence);
                                        modNode.TryGetValue("frequency", ref vertColor.frequency);
                                        modNode.TryGetValue("colorStart", ref vertColor.colorStart);
                                        modNode.TryGetValue("colorEnd", ref vertColor.colorEnd);
                                        modNode.TryGetValue("frequency", ref vertColor.seed);

                                        vertColor.modEnabled = true;
                                        yield return null;
                                        vertColor.OnSetup();
                                    }
                                    if (modNode.name.Equals("PQSMod_VertexColorSolid"))
                                    {
                                        GameObject tempObj = new GameObject();


                                        PQSMod_VertexColorSolid vertColor = (PQSMod_VertexColorSolid)tempObj.AddComponent(typeof(PQSMod_VertexColorSolid));

                                        tempObj.transform.parent = p.gameObject.transform;
                                        vertColor.sphere = p;



                                        vertColor.blend = 1.0f;
                                        if (modNode.HasValue("blend"))
                                            if (float.TryParse(modNode.GetValue("blend"), out ftmp))
                                                vertColor.blend = ftmp;

                                        vertColor.order = 9999992;
                                        if (modNode.HasValue("order"))
                                            if (int.TryParse(modNode.GetValue("order"), out itmp))
                                                vertColor.order = itmp;

                                        if (modNode.HasValue("color"))
                                        {
                                            try
                                            {
                                                Vector4 col = KSPUtil.ParseVector4(modNode.GetValue("color"));
                                                Color c = new Color(col.x, col.y, col.z, col.w);
                                                vertColor.color = c;
                                            }
                                            catch (Exception e)
                                            {
                                                print("*RSS* Error parsing as vec4: original text: " + modNode.GetValue("color") + " --- exception " + e.Message);
                                            }
                                        }
                                        vertColor.modEnabled = true;
                                        yield return null;
                                        vertColor.OnSetup();
                                    }
                                    if (modNode.name.Equals("PQSMod_VertexHeightMap"))
                                    {
                                        Texture2D map = null;
                                        bool localLoad = Utils.LoadTexture(modNode.GetValue("heightMap"), ref map, false, false, false);
                                        if ((object)map != null)
                                        {
                                            GameObject tempObj = new GameObject();


                                            PQSMod_VertexHeightMap heightMap = (PQSMod_VertexHeightMap)tempObj.AddComponent(typeof(PQSMod_VertexHeightMap));
                                            tempObj.transform.parent = p.gameObject.transform;
                                            heightMap.sphere = p;

                                            yield return null;
                                            heightMap.heightMap = ScriptableObject.CreateInstance<MapSO>();
                                            heightMap.heightMap.CreateMap(MapSO.MapDepth.Greyscale, map);
                                            DestroyImmediate(map);
                                            map = null;
                                            yield return null;
                                            heightMap.heightMapOffset = 0.0f;
                                            modNode.TryGetValue("heightMapOffset", ref heightMap.heightMapOffset);

                                            heightMap.heightMapDeformity = 100.0f;
                                            modNode.TryGetValue("heightMapDeformity", ref heightMap.heightMapDeformity);

                                            heightMap.scaleDeformityByRadius = false;
                                            modNode.TryGetValue("scaleDeformityByRadius", ref heightMap.scaleDeformityByRadius);

                                            heightMap.order = 10;
                                            modNode.TryGetValue("order", ref heightMap.order);
                                            heightMap.scaleDeformityByRadius = false;

                                            heightMap.modEnabled = true;
                                            yield return null;
                                            heightMap.OnSetup();
                                        }
                                        else
                                            print("*RSS* *ERROR* texture does not exist! " + modNode.GetValue("vertexColorMap"));

                                    }
                                    yield return null;
                                }
                            }
                            if (pqsNode.HasNode("Disable"))
                            {
                                foreach (ConfigNode modNode in pqsNode.GetNode("Disable").nodes)
                                {
                                    string mName = modNode.name;
                                    print("Disabling " + mName);
                                    guiExtra = "Disable " + mName;
                                    yield return null;
                                    //OnGui();
                                    if (mName.Equals("PQSLandControl"))
                                    {
                                        List<PQSLandControl> modList = p.transform.GetComponentsInChildren<PQSLandControl>(true).ToList();
                                        /*while(modList.Count > 0)
                                        {
                                            PQSLandControl m = modList[0];
                                            if (m.heightMap != null)
                                                DestroyImmediate(m.heightMap);
                                            m.modEnabled = false;
                                            m.enabled = false;
                                            modList.RemoveAt(0);
                                            DestroyImmediate(m);
                                        }*/
                                        foreach (PQSLandControl m in modList)
                                        {
                                            m.modEnabled = false;
                                            m.enabled = false;
                                            m.OnSetup();
                                        }
                                    }
                                    else
                                    {
                                        int idx = 0;
                                        bool doAll = false;
                                        if (mName.Contains(","))
                                        {
                                            string[] splt = mName.Split(',');
                                            mName = splt[0];
                                            if (splt[1][0].Equals('*'))
                                                doAll = true;
                                            else
                                                int.TryParse(splt[1], out idx);
                                        }
                                        print("Generic disable: " + mName + " with idx: " + idx + "; doAll: " + doAll);
                                        int cur = 0;
                                        foreach (var m in mods)
                                        {
                                            if (modNode.name.Equals(m.GetType().Name))
                                            {
                                                if (cur == idx || doAll)
                                                {
                                                    m.GetType().GetField("modEnabled").SetValue(m, false);
                                                    print("Found and disabled " + m.GetType().Name);
                                                }
                                                else
                                                    cur++;
                                            }
                                        }
                                    }
                                }
                            }
                            yield return null;
                        }
                        print("Rebuilding sphere " + p.name);
                        guiExtra = "Rebuilding " + p.name;
                        yield return null;
                        try
                        {
                            //OnGui();
                            //p.ResetSphere();
                            p.RebuildSphere();
                        }
                        catch (Exception e)
                        {
                            print("Rebuild sphere for " + node.name + " failed: " + e.Message);
                        }
                    }
                }
            }
            guiExtra = "";
            //OnGui();
            #endregion
        }

        public static void Print(string msg, params object[] args)
        {
            MonoBehaviour.print(string.Format(msg, args));
        }

        private IEnumerator<YieldInstruction> LoadScaledSpace(ConfigNode node, CelestialBody body, double origRadius)
        {
            #region ScaledSpace
            // Scaled space
            Transform scaledSpaceTransform = null;
            Transform atmo = null;
            guiMinor = "Scaled Space";
            //OnGui();
            if (ScaledSpace.Instance != null)
            {
                float SSTScale = 1.0f;
                node.TryGetValue("SSTScale", ref SSTScale);
                foreach (Transform t in ScaledSpace.Instance.scaledSpaceTransforms)
                {
                    if (t.name.Equals(node.name))
                    {
                        print("*RSS* Found scaledspace transform for " + t.name + ", scale " + t.localScale.x);
                        scaledSpaceTransform = t;
                        // replace
                        int replaceColor = 0;
                        string path = "";
                        if (node.HasValue("SSColor"))
                        {
                            replaceColor = 1;
                            path = node.GetValue("SSColor");
                        }
                        if (node.HasValue("SSColor32"))
                        {
                            replaceColor = 2;
                            path = node.GetValue("SSColor32");
                        }
                        if (replaceColor > 0)
                        {
                            guiExtra = "Color map";
                            Texture2D map = null;
                            bool local = Utils.LoadTexture(path, ref map, true, true, true);
                            if ((object)map != null)
                            {
                                Texture oldColor = t.gameObject.renderer.material.GetTexture("_MainTex");
                                if ((object)oldColor != null)
                                {
                                    foreach (Material m in Resources.FindObjectsOfTypeAll(typeof(Material)))
                                    {
                                        if (m.GetTexture("_MainTex") == oldColor)
                                            m.SetTexture("_MainTex", map);
                                    }
                                    DestroyImmediate(oldColor);
                                    oldColor = null;
                                    yield return null;
                                }
                            }
                        }
                        yield return null;
                        guiExtra = "";
                        if (node.HasValue("SSBump"))
                        {
                            guiExtra = "Normal Map";
                            path = node.GetValue("SSBump");
                            Texture2D map = null;
                            bool local = Utils.LoadTexture(path, ref map, loadInfo.compressNormals, true, true);
                            yield return null;
                            if ((object)map != null)
                            {
                                Texture oldBump = t.gameObject.renderer.material.GetTexture("_BumpMap");
                                if (oldBump != null)
                                {
                                    foreach (Material m in Resources.FindObjectsOfTypeAll(typeof(Material)))
                                    {
                                        if (m.GetTexture("_BumpMap") == oldBump)
                                            m.SetTexture("_BumpMap", map);
                                    }
                                    t.gameObject.renderer.material.SetTexture("_BumpMap", map); // in case one wasn't set.
                                    DestroyImmediate(oldBump);
                                    oldBump = null;
                                    yield return null;
                                }
                            }
                            yield return null;
                            guiExtra = "";
                            //OnGui();
                        }
                        /*if (t.gameObject.renderer.material.GetTexture("_rimColorRamp") != null)
                        {
                            try
                            {
                                System.IO.File.WriteAllBytes(KSPUtil.ApplicationRootPath + body.name + "Ramp.png", ((Texture2D)t.gameObject.renderer.material.GetTexture("_rimColorRamp")).EncodeToPNG());
                            }
                            catch (Exception e)
                            {
                                print("*RSS* Failed to get/write ramp for " + body.name + ", exception: " + e.Message);
                            }
                        }*/
                        guiExtra = "Ramp and Specularity";
                        yield return null;
                        if (node.HasValue("SSRampRef"))
                        {
                            //if (t.gameObject.renderer.material.GetTexture("_rimColorRamp") != null)
                            // for now try setting anyway.
                            Texture map = null;
                            map = GetRamp(node.GetValue("SSRampRef"));
                            if (map != null)
                                t.gameObject.renderer.material.SetTexture("_rimColorRamp", map);
                            else
                                print("*RSS* *ERROR* texture does not exist! " + node.GetValue("SSRamp"));
                        }
                        if (node.HasValue("SSRamp"))
                        {
                            Texture2D map = null;
                            bool localLoad = Utils.LoadTexture(node.GetValue("SSRamp"), ref map, true, true, true);
                            if ((object)map != null)
                            {
                                t.gameObject.renderer.material.SetTexture("_rimColorRamp", map);
                            }
                            else
                                print("*RSS* *ERROR* texture does not exist! " + node.GetValue("SSRamp"));
                        }
                        yield return null;
                        if (node.HasValue("SSSpec"))
                        {
                            try
                            {
                                Vector4 col = KSPUtil.ParseVector4(node.GetValue("SSSpec"));
                                Color c = new Color(col.x, col.y, col.z, col.w);
                                t.gameObject.renderer.material.SetColor("_SpecColor", c);
                            }
                            catch (Exception e)
                            {
                                print("*RSS* Error reading SSSpec as color4: original text: " + node.GetValue("SSSpec") + " --- exception " + e.Message);
                            }
                        }
                        yield return null;

                        // Fix mesh
                        guiExtra = "Wrapping mesh";
                        yield return null;
                        bool rescale = true;
                        bool doWrapHere = loadInfo.doWrap;
                        node.TryGetValue("wrap", ref doWrapHere);
                        bool sphereVal = loadInfo.spheresOnly;
                        bool sphereHere = node.TryGetValue("useSphericalSSM", ref sphereVal);
                        float origLocalScale = t.localScale.x; // assume uniform scale
                        if (body.pqsController != null && doWrapHere)
                        {
                            MeshFilter m = (MeshFilter)t.GetComponent(typeof(MeshFilter));
                            if (m == null || m.mesh == null)
                            {
                                print("*RSS* Failure getting SSM for " + body.pqsController.name + ": mesh is null");
                            }
                            else
                            {
                                char sep = System.IO.Path.DirectorySeparatorChar;
                                string filePath = KSPUtil.ApplicationRootPath + "GameData" + sep + "RealSolarSystem" + sep + "Plugins"
                                            + sep + "PluginData" + sep + t.name;

                                filePath += ".obj";


                                bool wrap = true;
                                if (File.Exists(filePath))
                                {
                                    try
                                    {
                                        print("*RSS* loading cached ScaledSpace mesh " + m.name);
                                        ProfileTimer.Push("LoadSSM_" + body.name);
                                        Mesh tMesh = new Mesh();
                                        Utils.CopyMesh(loadInfo.joolMesh.mesh, tMesh);

                                        ObjLib.UpdateMeshFromFile(tMesh, filePath);
                                        m.mesh = tMesh;
                                        m.mesh.RecalculateBounds();
                                        print("*RSS* Loaded " + filePath + " and wrapped.");
                                        ProfileTimer.Pop("LoadSSM_" + body.name);
                                        wrap = false;
                                    }
                                    catch (Exception ex)
                                    {
                                        Print("*RSS* Failed to load cached SSM: {0}", ex);
                                    }
                                    yield return null;
                                }
                                if (wrap)
                                {
                                    try
                                    {
                                        ProfileTimer.Push("Wrap time for " + body.name);
                                        Mesh tMesh = new Mesh();
                                        Utils.CopyMesh(loadInfo.joolMesh.mesh, tMesh);
                                        if (sphereVal)
                                        {
                                            float scaleFactor = (float)(origRadius / (1000 * 6000 * (double)origLocalScale)); // scale mesh such that it will end up right.
                                            // (need to scale it such that in the end localScale will = origLocalScale * radius/origRadius)
                                            print("*RSS* using Jool scaledspace mesh (spherical) for body " + body.pqsController.name + ". Vertex Scale " + scaleFactor);
                                            Utils.ScaleVerts(tMesh, scaleFactor);
                                        }
                                        else
                                        {
                                            print("*RSS* wrapping ScaledSpace mesh " + m.name + " to PQS " +
                                                  body.pqsController.name);
                                            float scaleFactor = (float)(origRadius / (1000 * 6000 * (double)origLocalScale));
                                            // scale mesh such that it will end up right.
                                            // (need to scale it such that in the end localScale will = origLocalScale * radius/origRadius)
                                            Utils.MatchVerts(tMesh, body.pqsController, body.ocean ? body.Radius : 0.0,
                                                scaleFactor);
                                            //ProfileTimer.Push("Recalc Normals");
                                            tMesh.RecalculateNormals();

                                            //ProfileTimer.Pop("Recalc Normals");
                                            ObjLib.UpdateTangents(tMesh);
                                            //print("*RSS* wrapped.");
                                        }
                                        m.mesh = tMesh;
                                        try
                                        {
                                            ObjLib.MeshToFile(m, filePath);
                                        }
                                        catch (Exception e)
                                        {
                                            print("*RSS* Exception saving wrapped mesh " + filePath + ": " + e.Message);
                                        }
                                        //print("*RSS*: Done wrapping and exporting. Setting scale");

                                        tMesh.RecalculateBounds();
                                        // do normal rescaling below.
                                        ProfileTimer.Pop("Wrap time for " + body.name);
                                    }
                                    catch (Exception e)
                                    {
                                        print("*RSS* Exception wrapping: " + e.Message);
                                    }
                                }
                                yield return null;
                                //OnGui();
                            }
                        }
                        atmo = t.FindChild("Atmosphere");
                        //OnGui();
                        if (rescale)
                        {
                            float scaleFactor = (float)((double)origLocalScale * body.Radius / origRadius * SSTScale);
                            t.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                        }
                        else
                        {
                            // rescale only atmo
                            if (atmo != null)
                            {
                                guiExtra = "Atmosphere";
                                yield return null;
                                print("*RSS* found atmo transform for " + node.name);
                                float scaleFactor = loadInfo.SSAtmoScale; // default to global default
                                if (!node.TryGetValue("SSAtmoScale", ref scaleFactor)) // if no override multiplier
                                {
                                    if (loadInfo.defaultAtmoScale) // use stock KSP multiplier
                                        scaleFactor *= 1.025f;
                                    else // or use atmosphere height-dependent multiplier
                                        scaleFactor *= (float)((body.Radius + body.maxAtmosphereAltitude) / body.Radius);
                                }
                                scaleFactor *= origLocalScale / t.localScale.x * (float)(body.Radius / origRadius); // since our parent transform changed, we no longer are the same scale as the planet.
                                atmo.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
                                print("*RSS* final scale of atmo for " + body.name + " in scaledspace: " + atmo.localScale.x);
                            }
                        }
                        print("*RSS* final scale of " + body.name + " in scaledspace: " + t.localScale.x);
                        yield return null;
                        guiExtra = "";
                    }
                }
            }
            #region AtmosphereFromGround
            guiExtra = "AtmosphereFromGround";
            yield return null;
            foreach (AtmosphereFromGround ag in Resources.FindObjectsOfTypeAll(typeof(AtmosphereFromGround)))
            {
                //print("*RSS* Found AG " + ag.name + " " + (ag.tag == null ? "" : ag.tag) + ". Planet " + (ag.planet == null ? "NULL" : ag.planet.name));
                if (ag != null && ag.planet != null)
                {
                    // generalized version of Starwaster's code. Thanks Starwaster!
                    if (ag.planet.name.Equals(node.name))
                    {
                        print("Found atmo for " + node.name + ": " + ag.name + ", has localScale " + ag.transform.localScale.x);
                        UpdateAFG(body, ag, node.GetNode("AtmosphereFromGround"));
                        print("Atmo updated");
                    }
                }
            }
            yield return null;
            guiExtra = "";
            #endregion
            #endregion
        }
        private IEnumerator<YieldInstruction> LoadExport(ConfigNode node, CelestialBody body)
        {
            #region Export
            double dtmp;
            int itmp;
            bool btmp;
            // texture rebuild
            if (node.HasNode("Export"))
            {
                guiMinor = "Exporting maps";
                guiExtra = "";
                yield return null;
                //OnGui();
                int res = 2048;
                bool ocean = false;
                Color oceanColor;
                double maxHeight, oceanHeight;
                PQS bodyPQS = null;
                foreach (PQS p in Resources.FindObjectsOfTypeAll(typeof(PQS)))
                    if (p.name.Equals(body.name))
                    {
                        bodyPQS = p;
                        break;
                    }
                if (bodyPQS != null)
                {
                    maxHeight = bodyPQS.radiusDelta * 0.5;
                    oceanHeight = 0;
                    ocean = body.ocean;
                    oceanColor = new Color(0.1255f, 0.22353f, 0.35683f);
                    ConfigNode exportNode = node.GetNode("Export");
                    if (exportNode.HasValue("resolution"))
                    {
                        if (int.TryParse(exportNode.GetValue("resolution"), out itmp))
                            res = itmp;
                    }
                    if (exportNode.HasValue("maxHeight"))
                    {
                        if (double.TryParse(exportNode.GetValue("maxHeight"), out dtmp))
                            maxHeight = dtmp;
                    }
                    if (exportNode.HasValue("ocean"))
                    {
                        if (bool.TryParse(exportNode.GetValue("ocean"), out btmp))
                            ocean = btmp;
                    }
                    if (exportNode.HasValue("oceanHeight"))
                    {
                        if (double.TryParse(exportNode.GetValue("oceanHeight"), out dtmp))
                            oceanHeight = dtmp;
                    }
                    if (exportNode.HasValue("oceanColor"))
                    {
                        try
                        {
                            ocean = true;
                            Vector4 col = KSPUtil.ParseVector4(exportNode.GetValue("oceanColor"));
                            oceanColor = new Color(col.x, col.y, col.z, col.w);
                        }
                        catch (Exception e)
                        {
                            print("*RSS* Error parsing as col3: original text: " + exportNode.GetValue("oceanColor") + " --- exception " + e.Message);
                        }
                    }
                    Texture2D[] outputMaps = bodyPQS.CreateMaps(res, maxHeight, ocean, oceanHeight, oceanColor);
                    yield return null;
                    System.IO.File.WriteAllBytes(KSPUtil.ApplicationRootPath + body.name + "1.png", outputMaps[0].EncodeToPNG());
                    yield return null;
                    System.IO.File.WriteAllBytes(KSPUtil.ApplicationRootPath + body.name + "2.png", outputMaps[1].EncodeToPNG());
                    yield return null;
                }
                guiMinor = "";
                //OnGui();
            }
            #endregion
        }
        private IEnumerator<YieldInstruction> LoadRSS()
        {
            workingRSS = true;

            // First, run GC.
            // GCDISABLE
            //ProfileTimer.Push("RSS_FirstGC");
            Resources.UnloadUnusedAssets();
            MusicLogic.fetch.audio1.Stop();
            /*initialMemory = GC.GetTotalMemory(true);
            print("*RSS*: Total memory in use before load: " + initialMemory);*/
            //ProfileTimer.Pop("RSS_FirstGC");
            InputLockManager.SetControlLock(ControlTypes.MAIN_MENU, "RSSLoad");
            print("*RSS* Modifying bodies");
            //OnGui();
            int nodeCount = loadInfo.node.nodes.Count - (loadInfo.node.HasNode("LaunchSites") ? 1 : 0);
            for (int i = 0; i < loadInfo.node.nodes.Count; i++)
            {
                ConfigNode node = loadInfo.node.nodes[i];
                foreach (CelestialBody body in FlightGlobals.fetch.bodies) //Resources.FindObjectsOfTypeAll(typeof(CelestialBody))) //in FlightGlobals.fetch.bodies)
                {
                    if (body.name.Equals(node.name))
                    {
                        guiMajor = "Editing Body: " + node.name + "   (" + (i + 1) + "/" + nodeCount + ")";
                        double origRadius = body.Radius;
                        var retval = LoadCB(node, body);
                        while (retval.MoveNext()) yield return retval.Current;
                        retval = LoadPQS(node, body, origRadius);
                        while (retval.MoveNext()) yield return retval.Current;
                        yield return null;
                        retval = LoadScaledSpace(node, body, origRadius);
                        while (retval.MoveNext()) yield return retval.Current;
                        retval = LoadExport(node, body);
                        while (retval.MoveNext()) yield return retval.Current;
                    }
                }
            }
            yield return null;
            var retval2 = LoadFinishOrbits();
            while (retval2.MoveNext()) yield return retval2.Current;
            yield return null;
            Resources.UnloadUnusedAssets();
            yield return null;
            /*finalMemory = GC.GetTotalMemory(true);
            print("*RSS*: Total Heap memory in use after load: " + finalMemory);
            print("*RSS* Done loading!");
            guiMajor = "Done! Initial Heap Memory: " + ((double)initialMemory/1024/1024).ToString("F2") + "MB";
            guiMinor = "Final Heap Memory: " + ((double)finalMemory/1024/1024).ToString("F2") + "MB";
            guiExtra = "Increase (may go down): " + ((double)(finalMemory - initialMemory)/1024/1024).ToString("F2") + "MB";*/
            guiMajor = "Done!";
            guiMinor = guiExtra = "";
            doneRSS = true;
            workingRSS = false;
            InputLockManager.RemoveControlLock("RSSLoad");
            MusicLogic.fetch.audio1.Play();
        }
        public void Start()
        {
            enabled = true;
            if (doneRSS || !CompatibilityChecker.IsCompatible())
                return;

            if ((object)(KSCLoader.instance) == null)
                KSCLoader.instance = new KSCLoader(); // just in case the other hasn't run first

            ConfigNode RSSSettings = null;
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes("REALSOLARSYSTEM"))
                RSSSettings = node;

            if (RSSSettings == null)
                throw new UnityException("*RSS* REALSOLARSYSTEM node not found!");


            loadInfo = new RSSLoadInfo(RSSSettings);
            /*print("*RSS* Printing CBTs");
            foreach (PQSMod_CelestialBodyTransform c in Resources.FindObjectsOfTypeAll(typeof(PQSMod_CelestialBodyTransform)))
                Utils.DumpCBT(c);*/
            showGUI = true;
            MusicLogic.fetch.audio1.Stop();
            StartCoroutine(LoadRSS());
        }

    }

}
