using System.Collections;
using UnityEngine;
using static RunwayCollisionHandler;

namespace RealSolarSystem
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class RSSRunwayFix : MonoBehaviour
    {
        internal bool hold = false;

        public bool debug = false;

        public float holdThreshold = 2700;
        public float holdThresholdSqr;

        public float originalThreshold = 0;
        public float originalThresholdSqr = 0;

        private int layerMask = 1<<15;

        private int frameSkip = 0;
        internal bool isOnRunway = false;
        internal string lastHitColliderName;

        internal bool collidersDisabled = false;
        private bool waiting = false;
        private IEnumerator waitCoro = null;
        private bool coroComplete = false;

        private Coroutine _sectionsLoadRoutine;

        public static RSSRunwayFix Instance { get; private set; } = null;

        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(Instance);
            }
            Instance = this;
        }

        public void Start()
        {
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

            GameEvents.onVesselGoOffRails.Add(OnVesselGoOffRails);
            GameEvents.onVesselGoOnRails.Add(OnVesselGoOnRails);
            GameEvents.onVesselSwitching.Add(OnVesselSwitching);
            GameEvents.onVesselSituationChange.Add(OnVesselSituationChange);
            DestructibleBuilding.OnLoaded.Add(OnSectionLoaded);
        }

        public void OnDestroy()
        {
            GameEvents.onVesselGoOffRails.Remove(OnVesselGoOffRails);
            GameEvents.onVesselGoOnRails.Remove(OnVesselGoOnRails);
            GameEvents.onVesselSwitching.Remove(OnVesselSwitching);
            GameEvents.onVesselSituationChange.Remove(OnVesselSituationChange);
            DestructibleBuilding.OnLoaded.Remove(OnSectionLoaded);
        }

        private void OnSectionLoaded(DestructibleBuilding data)
        {
            // At the end of the frame, KSP will fire this event for every destructible KSC prop.
            // We wait until the next frame so that all of the runway sections are guaranteed to be loaded.
            if (!collidersDisabled && _sectionsLoadRoutine == null)
            {
                _sectionsLoadRoutine = StartCoroutine(SectionsLoadRoutine());
            }
        }

        private IEnumerator SectionsLoadRoutine()
        {
            yield return null;

            TryDisableColliders();
            _sectionsLoadRoutine = null;
        }

        public void OnVesselGoOnRails(Vessel v)
        {
            FloatingOrigin.fetch.threshold = originalThreshold;
            FloatingOrigin.fetch.thresholdSqr = originalThresholdSqr;
            hold = false;
        }

        public void OnVesselGoOffRails(Vessel v)
        {
            originalThreshold = FloatingOrigin.fetch.threshold;
            originalThresholdSqr = FloatingOrigin.fetch.thresholdSqr;

            if (debug) PrintDebug($"original threshold={originalThreshold}");
            holdThresholdSqr = holdThreshold * holdThreshold;

            if (!collidersDisabled)
            {
                if (debug) PrintDebug("colliders not disabled yet");
                hold = false;
                return;
            }

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

            waiting = false;
        }

        private Vector3 GetDownwardVector()
        {
            Vessel v = FlightGlobals.ActiveVessel;
            return (v.CoM - v.mainBody.transform.position).normalized * -1;
        }

        public void OnVesselSituationChange(GameEvents.HostedFromToAction<Vessel, Vessel.Situations> data)
        {
            if (data.host != FlightGlobals.ActiveVessel)
            {
                return;
            }
            
            hold = data.to == Vessel.Situations.LANDED;
            if (debug) PrintDebug($"vessel: {data.host.vesselName}, situation: {data.to}, hold: {hold}");

            if (!hold && FloatingOrigin.fetch.threshold > originalThreshold && originalThreshold > 0)
            {
                if (debug) PrintDebug($"coro: {waitCoro}, complete: {coroComplete}");
                if (waitCoro != null && !coroComplete)
                {
                    if (debug) PrintDebug("stopping coro");
                    StopCoroutine(waitCoro);
                }

                waitCoro = RestoreThreshold();
                if (debug) PrintDebug($"created new coro: {waitCoro}");

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
                if (debug) PrintDebug("waiting is over");
            }

            // Check again as situation could have changed
            if (!hold && FloatingOrigin.fetch.threshold > originalThreshold && originalThreshold > 0)
            {
                if (debug) PrintDebug($"Restoring original thresholds ({FloatingOrigin.fetch.threshold} > {originalThreshold}), "+
                                      $"alt={FlightGlobals.ActiveVessel.radarAltitude}");
                FloatingOrigin.fetch.threshold = originalThreshold;
                FloatingOrigin.fetch.thresholdSqr = originalThresholdSqr;
            }
            if (debug) PrintDebug("coro finished");
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
                if (isOnRunway)
                {
                    if (debug) PrintDebug($"rwy=false; threshold={FloatingOrigin.fetch.threshold}, original threshold={originalThreshold}");
                    isOnRunway = false;
                }
                
                return;
            }
            
            FloatingOrigin.fetch.threshold = holdThreshold;
            FloatingOrigin.fetch.thresholdSqr = holdThresholdSqr;
            
            if (!isOnRunway)
            {
                if (debug) PrintDebug($"rwy=true; threshold={FloatingOrigin.fetch.threshold}, original threshold={originalThreshold}");
                isOnRunway = true;
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
            if (v == null || (v.situation != Vessel.Situations.LANDED && v.situation != Vessel.Situations.PRELAUNCH))
            {
                return false;
            }

            Vector3 down = GetDownwardVector();
            bool hit = Physics.Raycast(v.transform.position, down, out RaycastHit raycastHit, 100, layerMask);
            if (!hit)
            {
                return false;
            }
            
            lastHitColliderName = raycastHit.collider.gameObject.name;
            //if (debug) printDebug($"hit collider: {colliderName}");

            return lastHitColliderName == "runway_collider";
        }

        internal void PrintDebug(string message)
        {
            if (!debug) return;

            var trace = new System.Diagnostics.StackTrace();
            string caller = trace.GetFrame(1).GetMethod().Name;
            int line = trace.GetFrame(1).GetFileLineNumber();
            Debug.Log($"[RealSolarSystem] {caller}:{line}: {message}");
        }

        private void TryDisableColliders()
        {
            // Once disabled, the colliders will stay disabled
            if (collidersDisabled) return;

            var rwHandler = FindObjectOfType<RunwayCollisionHandler>();
            if (rwHandler == null)
            {
                if (debug) PrintDebug("rwHandler is null");
                return;
            }

            foreach (RunwaySection section in rwHandler.runwaySections)
            {
                Collider sc = section.sectionCollider;
                sc.enabled = false;
            }

            collidersDisabled = true;
            if (debug) PrintDebug("disabled runway colliders");
        }
    }
}
