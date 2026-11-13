using System;
using UnityEngine;

namespace RealSolarSystem
{
    public class CameraWrapper
    {
        public string depth = string.Empty;
        public string farClipPlane = string.Empty;
        public string nearClipPlane = string.Empty;
        public string camName = string.Empty;

        public void Apply()
        {
            Camera[] cameras = Camera.allCameras;

            try
            {
                bool notFound = true;

                foreach (Camera cam in cameras)
                {
                    if (camName.Equals(cam.name))
                    {
                        if (float.TryParse(depth, out float ftmp))
                            cam.depth = ftmp;

                        if (float.TryParse(farClipPlane, out ftmp))
                        {
                            cam.farClipPlane = ftmp;
                            RSSWatchDog.Instance?.SetBaseFarClip(cam, ftmp);
                        }

                        if (float.TryParse(nearClipPlane, out ftmp))
                            cam.nearClipPlane = ftmp;

                        depth = cam.depth.ToString();
                        nearClipPlane = cam.nearClipPlane.ToString();
                        farClipPlane = cam.farClipPlane.ToString();

                        notFound = false;
                    }
                }

                if (notFound)
                {
                    Debug.Log($"[RealSolarSystem] Could not find camera {camName} when applying settings!");
                }
            }
            catch (Exception exceptionStack)
            {
                Debug.Log($"[RealSolarSystem] Error applying to camera {camName}: exception {exceptionStack.Message}");
            }
        }
    }
}
