using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RealSolarSystem
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class RealSolarSystemEditor : MonoBehaviour
    {
        static Rect windowPosition = new Rect(64, 64, 320, 640);
        static GUIStyle windowStyle = null;

        private bool _GUIOpen = false;
        private double _dtCounter = 0;

        private Vector2 _scrollPos;

        private List<CameraWrapper> _cams = null;

        private string _sMinDist = null;
        private string _sMinDiv = null;
        private string _sMaxDiv = null;

        private string _sDepthOffset = null;
        private string _sOceanRadiusOffset = null;
        private string _sCoastOrder = null;

        private string _sOceanFactor = null;
        private string _sCoastLessThan = null;
        private string _sCoastFactor = null;
        private string _sEnhanceOrder = null;

        private string _sMinHeightOffset;
        private string _sMaxHeightOffset;
        private string _sSlopeScale;
        private string _sRssDefineOrder;

        private string _sHeightStart;
        private string _sHeightEnd;
        private string _sDeformity;
        private string _sFrequency;
        private string _sOctaves;
        private string _sPersistance;
        private string _sHeightNoiseOrder;

        private List<PQSMod> _modList;
        private PQSMod_VertexDefineCoastLine _pModDefine = null;
        private PQSMod_QuadEnhanceCoast _pModEnhance = null;
        private PQSMod_VertexDefineCoastSmooth _pModRssDefine = null;
        private PQSMod_VertexHeightNoiseVertHeight _pModHeightNoise = null;
        private PQSMod_VertexHeightMap _pModRssHMap = null;

        public void Update()
        {
            if (_dtCounter < 5)
            {
                _dtCounter += TimeWarp.fixedDeltaTime;
                return;
            }

            if (_cams == null)
            {
                _cams = new List<CameraWrapper>();

                Camera[] cameras = Camera.allCameras;

                foreach (Camera cam in cameras)
                {
                    try
                    {
                        var thisCam = new CameraWrapper
                        {
                            camName = cam.name,

                            depth = cam.depth.ToString()
                        };

                        thisCam.farClipPlane += cam.farClipPlane.ToString();
                        thisCam.nearClipPlane += cam.nearClipPlane.ToString();

                        _cams.Add(thisCam);
                    }
                    catch (Exception exceptionStack)
                    {
                        Debug.Log($"[RealSolarSystem] Exception getting camera {cam.name}\n{exceptionStack}");
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.G) && Input.GetKey(KeyCode.LeftAlt))
            {
                _GUIOpen = !_GUIOpen;
            }

            if (_GUIOpen && Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftAlt))
            {
                FlightGlobals.currentMainBody?.pqsController?.StartUpSphere();
            }
        }

        public void OnGUI()
        {
            if (_GUIOpen)
            {
                if (HighLogic.LoadedSceneIsFlight && FlightGlobals.ActiveVessel != null)
                    windowPosition = GUILayout.Window(69105, windowPosition, ShowGUI, "RealSolarSystem Parameters", windowStyle);
            }
        }

        public void Start()
        {
            windowStyle = new GUIStyle(HighLogic.Skin.window);
            windowStyle.stretchHeight = true;
        }

        private void ShowGUI(int windowID)
        {
            GUILayout.BeginVertical();

            _scrollPos = GUILayout.BeginScrollView(_scrollPos);

            GUILayout.Label("RSSRunwayFix");

            GUILayout.BeginHorizontal();
            GUILayout.Label("collidersDisabled: ");
            GUILayout.Label(RSSRunwayFix.Instance.collidersDisabled.ToString(), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("isOnRunway: ");
            GUILayout.Label(RSSRunwayFix.Instance.isOnRunway.ToString(), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("hold: ");
            GUILayout.Label(RSSRunwayFix.Instance.hold.ToString(), GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("lastHitCollider: ");
            GUILayout.Label(RSSRunwayFix.Instance.lastHitColliderName, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();

            if (_cams != null)
            {
                GUILayout.Label("--------------");
                GUILayout.BeginHorizontal();
                GUILayout.Label("CAMERA EDITOR");
                GUILayout.EndHorizontal();

                foreach (CameraWrapper cam in _cams)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Camera: " + cam.camName);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Depth");
                    cam.depth = GUILayout.TextField(cam.depth, 10);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Far Clip");
                    cam.farClipPlane = GUILayout.TextField(cam.farClipPlane, 10);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Near Clip");
                    cam.nearClipPlane = GUILayout.TextField(cam.nearClipPlane, 10);
                    GUILayout.EndHorizontal();

                    if (GUILayout.Button("Apply to " + cam.camName))
                    {
                        cam.Apply();
                    }
                }
            }

            PQS pqs = FlightGlobals.currentMainBody.pqsController;
            if (_modList == null)
            {
                _modList = new List<PQSMod>();
                GetChildMods(pqs.gameObject, _modList);
                _pModDefine = (PQSMod_VertexDefineCoastLine)_modList.FirstOrDefault(t => t is PQSMod_VertexDefineCoastLine);
                _pModEnhance = (PQSMod_QuadEnhanceCoast)_modList.FirstOrDefault(t => t is PQSMod_QuadEnhanceCoast);
                _pModRssDefine = (PQSMod_VertexDefineCoastSmooth)_modList.FirstOrDefault(t => t is PQSMod_VertexDefineCoastSmooth);
                _pModHeightNoise = (PQSMod_VertexHeightNoiseVertHeight)_modList.FirstOrDefault(t => t is PQSMod_VertexHeightNoiseVertHeight);
                _pModRssHMap = (PQSMod_VertexHeightMap)_modList.FirstOrDefault(t => t is PQSMod_VertexHeightMap);
            }

            PQSCache.PQSSpherePreset preset = PQSCache.PresetList?.GetPreset(pqs.gameObject.name);
            if (preset != null)
            {
                if (_sMinDist == null)
                {
                    _sMinDist = preset.minDistance.ToString();
                    _sMinDiv = preset.minSubdivision.ToString();
                    _sMaxDiv = preset.maxSubdivision.ToString();

                    if (_pModDefine != null)
                    {
                        _sDepthOffset = _pModDefine.depthOffset.ToString();
                        _sOceanRadiusOffset = _pModDefine.oceanRadiusOffset.ToString();
                        _sCoastOrder = _pModDefine.order.ToString();
                    }

                    if (_pModEnhance != null)
                    {
                        _sOceanFactor = _pModEnhance.oceanFactor.ToString();
                        _sCoastLessThan = _pModEnhance.coastLessThan.ToString();
                        _sCoastFactor = _pModEnhance.coastFactor.ToString();
                        _sEnhanceOrder = _pModEnhance.order.ToString();
                    }

                    if (_pModRssDefine != null)
                    {
                        _sMinHeightOffset = _pModRssDefine.minHeightOffset.ToString();
                        _sMaxHeightOffset = _pModRssDefine.maxHeightOffset.ToString();
                        _sSlopeScale = _pModRssDefine.slopeScale.ToString();
                        _sRssDefineOrder = _pModRssDefine.order.ToString();
                    }

                    if (_pModHeightNoise != null)
                    {
                        _sHeightStart = _pModHeightNoise.heightStart.ToString();
                        _sHeightEnd = _pModHeightNoise.heightEnd.ToString();
                        _sDeformity = _pModHeightNoise.deformity.ToString();
                        _sFrequency = _pModHeightNoise.frequency.ToString();
                        _sOctaves = _pModHeightNoise.octaves.ToString();
                        _sPersistance = _pModHeightNoise.persistance.ToString();
                        _sHeightNoiseOrder = _pModHeightNoise.order.ToString();
                    }
                }

                GUILayout.Label("Preset " + preset.name);

                GUILayout.BeginHorizontal();
                GUILayout.Label("minDistance: ");
                GUILayout.EndHorizontal();
                _sMinDist = GUILayout.TextField(_sMinDist);
                if (double.TryParse(_sMinDist, out double minDist))
                {
                    preset.minDistance = minDist;
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("minSubdivision: ");
                GUILayout.EndHorizontal();
                _sMinDiv = GUILayout.TextField(_sMinDiv);
                if (int.TryParse(_sMinDiv, out int minDiv))
                {
                    preset.minSubdivision = minDiv;
                }

                GUILayout.BeginHorizontal();
                GUILayout.Label("maxSubdivision: ");
                GUILayout.EndHorizontal();
                _sMaxDiv = GUILayout.TextField(_sMaxDiv);
                if (int.TryParse(_sMaxDiv, out int maxDiv))
                {
                    preset.maxSubdivision = maxDiv;
                }

                GUILayout.Label("-----------------");
                foreach (PQSMod pqsmod in _modList)
                {
                    GUILayout.BeginHorizontal();
                    pqsmod.modEnabled = GUILayout.Toggle(pqsmod.modEnabled, pqsmod.GetType().Name);
                    GUILayout.EndHorizontal();
                }

                if (_pModDefine != null)
                {
                    GUILayout.Label("-----------------");
                    GUILayout.Label("VertexDefineCoastLine");

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Coastline depthOffset: ");
                    GUILayout.EndHorizontal();
                    _sDepthOffset = GUILayout.TextField(_sDepthOffset);
                    if (double.TryParse(_sDepthOffset, out double depthOffset))
                    {
                        _pModDefine.depthOffset = depthOffset;
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Coastline oceanRadiusOffset: ");
                    GUILayout.EndHorizontal();
                    _sOceanRadiusOffset = GUILayout.TextField(_sOceanRadiusOffset);
                    if (double.TryParse(_sOceanRadiusOffset, out double oceanRadiusOffset))
                    {
                        _pModDefine.oceanRadiusOffset = oceanRadiusOffset;
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Coastline order: ");
                    GUILayout.EndHorizontal();
                    _sCoastOrder = GUILayout.TextField(_sCoastOrder);
                    if (int.TryParse(_sCoastOrder, out int coastOrder))
                    {
                        _pModDefine.order = coastOrder;
                    }
                }

                if (_pModEnhance!= null)
                {
                    GUILayout.Label("-----------------");
                    GUILayout.Label("QuadEnhanceCoast");

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Enhance oceanFactor: ");
                    GUILayout.EndHorizontal();
                    _sOceanFactor = GUILayout.TextField(_sOceanFactor);
                    if (double.TryParse(_sOceanFactor, out double oceanfactor))
                    {
                        _pModEnhance.oceanFactor = oceanfactor;
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Enhance coastFactor: ");
                    GUILayout.EndHorizontal();
                    _sCoastFactor = GUILayout.TextField(_sCoastFactor);
                    if (double.TryParse(_sCoastFactor, out double coastFactor))
                    {
                        _pModEnhance.coastFactor = coastFactor;
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Enhance coastLessThan: ");
                    GUILayout.EndHorizontal();
                    _sCoastLessThan = GUILayout.TextField(_sCoastLessThan);
                    if (double.TryParse(_sCoastLessThan, out double lt))
                    {
                        _pModEnhance.coastLessThan = lt;
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Enhance order: ");
                    GUILayout.EndHorizontal();
                    _sEnhanceOrder = GUILayout.TextField(_sEnhanceOrder);
                    if (int.TryParse(_sEnhanceOrder, out int order))
                    {
                        _pModEnhance.order = order;
                    }
                }

                if (_pModRssDefine != null)
                {
                    GUILayout.Label("-----------------");
                    GUILayout.Label("VertexDefineCoastSmooth");

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("minHeightOffset: ");
                    GUILayout.EndHorizontal();
                    _sMinHeightOffset = GUILayout.TextField(_sMinHeightOffset);
                    if (double.TryParse(_sMinHeightOffset, out double minHeightOffset))
                    {
                        _pModRssDefine.minHeightOffset = minHeightOffset;
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("maxHeightOffset: ");
                    GUILayout.EndHorizontal();
                    _sMaxHeightOffset = GUILayout.TextField(_sMaxHeightOffset);
                    if (double.TryParse(_sMaxHeightOffset, out double maxHeightOffset))
                    {
                        _pModRssDefine.maxHeightOffset = maxHeightOffset;
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("slopeScale: ");
                    GUILayout.EndHorizontal();
                    _sSlopeScale = GUILayout.TextField(_sSlopeScale);
                    if (double.TryParse(_sSlopeScale, out double val))
                    {
                        _pModRssDefine.slopeScale = val;
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Mod order: ");
                    GUILayout.EndHorizontal();
                    _sRssDefineOrder = GUILayout.TextField(_sRssDefineOrder);
                    if (int.TryParse(_sRssDefineOrder, out int order))
                    {
                        _pModRssDefine.order = order;
                    }
                }

                if (_pModHeightNoise != null)
                {
                    GUILayout.Label("-----------------");
                    GUILayout.Label("VertexHeightNoiseVertHeight");

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("heightStart: ");
                    GUILayout.EndHorizontal();
                    _sHeightStart = GUILayout.TextField(_sHeightStart);
                    if (double.TryParse(_sHeightStart, out double val))
                    {
                        _pModHeightNoise.heightStart = (float)val;
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("maxHeightOffset: ");
                    GUILayout.EndHorizontal();
                    _sHeightEnd = GUILayout.TextField(_sHeightEnd);
                    if (double.TryParse(_sHeightEnd, out val))
                    {
                        _pModHeightNoise.heightEnd = (float)val;
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("deformity: ");
                    GUILayout.EndHorizontal();
                    _sDeformity = GUILayout.TextField(_sDeformity);
                    if (double.TryParse(_sDeformity, out val))
                    {
                        _pModHeightNoise.deformity = (float)val;
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("frequency: ");
                    GUILayout.EndHorizontal();
                    _sFrequency = GUILayout.TextField(_sFrequency);
                    if (double.TryParse(_sFrequency, out val))
                    {
                        _pModHeightNoise.frequency = (float)val;
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("octaves: ");
                    GUILayout.EndHorizontal();
                    _sOctaves = GUILayout.TextField(_sOctaves);
                    if (int.TryParse(_sOctaves, out int val2))
                    {
                        _pModHeightNoise.octaves = val2;
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("persistance: ");
                    GUILayout.EndHorizontal();
                    _sPersistance = GUILayout.TextField(_sPersistance);
                    if (double.TryParse(_sPersistance, out val))
                    {
                        _pModHeightNoise.persistance = (float)val;
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Mod order: ");
                    GUILayout.EndHorizontal();
                    _sHeightNoiseOrder = GUILayout.TextField(_sHeightNoiseOrder);
                    if (int.TryParse(_sHeightNoiseOrder, out int order))
                    {
                        _pModHeightNoise.order = order;
                    }
                }
            }

            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUI.DragWindow();
        }

        private void GetChildMods(GameObject obj, List<PQSMod> mods)
        {
            IEnumerator enumerator = obj.transform.GetEnumerator();
            try
            {
                while (enumerator.MoveNext())
                {
                    Transform current = (Transform)enumerator.Current;
                    if (!(current == transform) && !(current.GetComponent<PQS>() != null))
                    {
                        PQSMod[] components = current.GetComponents<PQSMod>();
                        if (components != null)
                        {
                            mods.AddRange(components);
                            GetChildMods(current.gameObject, mods);
                        }
                    }
                }
            }
            finally
            {
                (enumerator as IDisposable)?.Dispose();
            }
        }
    }
}
