using System;
using System.Collections;
using UnityEngine;

namespace RealSolarSystem
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class RSSRunwayFix : MonoBehaviour
    {
        private bool hold = false;

        public bool debug = false;

        public float holdThreshold = 2700;
        public float holdThresholdSqr;

        public float originalThreshold = 0;
        public float originalThresholdSqr = 0;

        private int layerMask = 1<<15;

        private int frameSkip = 0;
        private bool rwy = false;

        private bool waiting = false;
        private IEnumerator waitCoro = null;
        private bool coroComplete = false;

        private Vector3 down;

        private string[] collidersToFix =
        {
            "End09", "Section4", "Section3", "Section2", "Section1", "End27"
        };
        
        public void Start()
        {
            PrintDebug("Start");

            foreach (ConfigNode n in GameDatabase.Instance.GetConfigNodes("RSSRUNWAYFIX"))
            {
                if (bool.TryParse(n.GetValue("debug"), out bool bTemp))
                {
                    debug = bTemp;
                }
                if (float.TryParse(n.GetValue("holdThreshold"), out float fTemp))
                {
                    holdThreshold = fTemp;
                }
            }

            GameEvents.onVesselGoOffRails.Add (OnVesselGoOffRails);
            GameEvents.onVesselGoOnRails.Add (OnVesselGoOnRails);
            GameEvents.onVesselSwitching.Add (OnVesselSwitching);
            GameEvents.onVesselSituationChange.Add (OnVesselSituationChange);

            //GameEvents.onFloatingOriginShift.Add(onFloatingOriginShift);
        }

        public void OnDestroy()
        {
            PrintDebug("OnDestroy");
            GameEvents.onVesselGoOffRails.Remove (OnVesselGoOffRails);
            GameEvents.onVesselGoOnRails.Remove (OnVesselGoOnRails);
            GameEvents.onVesselSwitching.Remove (OnVesselSwitching);
            GameEvents.onVesselSituationChange.Remove(OnVesselSituationChange);

            //GameEvents.onFloatingOriginShift.Remove(onFloatingOriginShift);
        }

        public void OnFloatingOriginShift(Vector3d v0, Vector3d v1)
        {
            if (!hold)
            {
                return;
            }
            if (debug) PrintDebug($"RSSRWF: v0: {v0}, v1: {v1}, threshold: {FloatingOrigin.fetch.threshold}");
        }

        public void OnVesselGoOnRails(Vessel v)
        {
            FloatingOrigin.fetch.threshold = originalThreshold;
            FloatingOrigin.fetch.thresholdSqr = originalThresholdSqr;
            hold = false;
        }

        public void OnVesselGoOffRails(Vessel v)
        {
            PrintDebug("started");

            originalThreshold = FloatingOrigin.fetch.threshold;
            originalThresholdSqr = FloatingOrigin.fetch.thresholdSqr;

            if (debug) PrintDebug($"original threshold={originalThreshold}");
            holdThresholdSqr = holdThreshold * holdThreshold;

            GameObject end09 = GameObject.Find(collidersToFix[0]);
            if (end09 == null)
            {
                PrintDebug("no end09 found");
                hold = false;
                return;
            }

            //combine(end09.transform.parent.gameObject);
            DisableColliders();
            GetDownwardVector();
            hold = true;
            waiting = false;
        }

        public void OnVesselSwitching(Vessel from, Vessel to)
        {
            if (to == null || to.situation != Vessel.Situations.LANDED)
            {
                // FIXME: Do we need PRELAUNCH here?
                return;
            }

            GetDownwardVector();
            waiting = false;
        }

        private void GetDownwardVector()
        {
            Vessel v = FlightGlobals.ActiveVessel;
            down = (v.CoM - v.mainBody.transform.position).normalized * -1;
        }

        public void OnVesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> data)
        {
            if (data.host != FlightGlobals.ActiveVessel)
            {
                return;
            }
            
            hold = data.to == Vessel.Situations.LANDED;
            PrintDebug($"vessel: {data.host.vesselName}, situation: {data.to}, hold: {hold}");

            if (!hold && FloatingOrigin.fetch.threshold > originalThreshold && originalThreshold > 0)
            {
                PrintDebug($"coro: {waitCoro}, complete: {coroComplete}");
                if (waitCoro != null && !coroComplete)
                {
                    PrintDebug("stopping coro");
                    StopCoroutine(waitCoro);
                }

                waitCoro = RestoreThreshold();
                PrintDebug($"created new coro: {waitCoro}");

                coroComplete = false;
                StartCoroutine(waitCoro);
            }
        }

        private IEnumerator RestoreThreshold()
        {
            if (debug) PrintDebug($"in coro; hold={hold}, waiting={waiting}, alt={FlightGlobals.ActiveVessel.radarAltitude}");
            while (!hold && !waiting &&  FlightGlobals.ActiveVessel.radarAltitude < 10)
            {
                if (debug) PrintDebug($"radar alt: {FlightGlobals.ActiveVessel.radarAltitude}, waiting 5 sec");
                waiting = true;
                yield return new WaitForSeconds(5);
                waiting = false;
                PrintDebug("waiting is over");
            }

            // Check again as situation could have changed
            if (!hold && FloatingOrigin.fetch.threshold > originalThreshold && originalThreshold > 0) {
                if (debug) PrintDebug($"Restoring original thresholds ({FloatingOrigin.fetch.threshold} > {originalThreshold}), "+
                                      $"alt={FlightGlobals.ActiveVessel.radarAltitude}");
                FloatingOrigin.fetch.threshold = originalThreshold;
                FloatingOrigin.fetch.thresholdSqr = originalThresholdSqr;
            }
            PrintDebug("coro finished");
            coroComplete = true;
        }

        public void FixedUpdate()
        {
            frameSkip++;
            if (frameSkip < 25)
            {
                return;
            }
            frameSkip = 0;
            
            if (!CheckRunway())
            {
                if (rwy)
                {
                    if (debug) PrintDebug($"rwy=false; threshold={FloatingOrigin.fetch.threshold}, original threshold={originalThreshold}");
                    rwy = false;
                }
                
                return;
            }
            
            FloatingOrigin.fetch.threshold = holdThreshold;
            FloatingOrigin.fetch.thresholdSqr = holdThresholdSqr;
            
            if (!rwy)
            {
                if (debug) PrintDebug($"rwy=true; threshold={FloatingOrigin.fetch.threshold}, original threshold={originalThreshold}");
                rwy = true;
            }

            FloatingOrigin.SetSafeToEngage(false);
        }
        
        private bool CheckRunway()
        {
            if (!hold)
            {
                return false;
            }
            
            Vessel v = FlightGlobals.ActiveVessel;
            if (v.situation != Vessel.Situations.LANDED && v.situation != Vessel.Situations.PRELAUNCH)
            {
                return false;
            }
            
            GetDownwardVector();

            bool hit = Physics.Raycast(v.transform.position, down, out RaycastHit raycastHit, 100, layerMask);
            if (!hit)
            {
                return false;
            }
            
            string colliderName = raycastHit.collider.gameObject.name;
            //if (debug) printDebug($"hit collider: {colliderName}");
            if (colliderName != "runway_collider")
            {
                return false;
            }

            return true;
        }

        internal void PrintDebug(string message)
        {

            if (!debug) return;

            var trace = new System.Diagnostics.StackTrace();
            string caller = trace.GetFrame(1).GetMethod().Name;
            int line = trace.GetFrame(1).GetFileLineNumber();
            Debug.Log($"[RealSolarSystem] {caller}:{line}: {message}");
        }

        private void DisableColliders()
        {
            foreach (string c in collidersToFix)
            {
                bool notFound = true;
                foreach (GameObject o in Resources.FindObjectsOfTypeAll<GameObject>())
                {
                    if (o.name != c)
                        continue;

                    notFound = false;
                    if (!o.activeInHierarchy)
                    {
                        if (debug) PrintDebug($"{o.name} is not active, skipping");
                        continue;
                    }

                    MeshCollider cl = o.GetComponentInChildren<MeshCollider>();
                    if (cl == null)
                    {
                        if (debug) PrintDebug($" No mesh collider in {c}");
                        continue;
                    }
                    if (debug) PrintDebug($" disabling {cl.name}");
                    cl.enabled = false;
                }
                if (notFound)
                {
                    if (debug) PrintDebug($" Object {c} not found, skipping");
                    continue;
                }
            }
            }
    }
}
