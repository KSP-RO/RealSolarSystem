using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace RealSolarSystem
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    class RSSInstallationCheck : MonoBehaviour
    {
        public void Start()
        {
            try
            {
                CheckTexturesInstalled();
                CheckCompatibleKopInstalled();
            }
            catch (Exception ex)
            {
                Debug.Log("[RealSolarSystem] RSSInstallationCheck.Start() caught an exception: " + ex);
            }
            finally
            {
                Destroy(this);
            }
        }

        private void CheckTexturesInstalled()
        {
            string szTextureFolderPath = $"{KSPUtil.ApplicationRootPath}GameData{Path.AltDirectorySeparatorChar}RSS-Textures";

            const string szUserMessage = "The texture pack for RealSolarSystem is missing from your installation\n\nDownload it from the official GitHub KSP-RO repository.";

            if (!Directory.Exists(szTextureFolderPath))
            {
                Debug.Log("[RealSolarSystem] No texture pack detected!");

                PopupDialog.SpawnPopupDialog
                (
                    new Vector2(0.0f, 0.0f),
                    new Vector2(0.0f, 0.0f),
                    new MultiOptionDialog
                    (
                        "RSSInstallCheck",
                        szUserMessage,
                        "Missing RSS Texture Pack",
                        HighLogic.UISkin,
                        new Rect(0.25f, 0.75f, 320f, 80f),
                        new DialogGUIFlexibleSpace(),
                        new DialogGUIButton
                        (
                            "Download",
                            delegate
                            {
                                OpenRSSTexturesDownloadPage();
                            },
                            140.0f,
                            30.0f,
                            true
                        )
                    ),
                    false,
                    HighLogic.UISkin,
                    true,
                    string.Empty
                );
            }
            else
            {
                var fi = new FileInfo(Path.Combine(szTextureFolderPath, "EarthColor.dds"));
                if (fi.Exists)
                {
                    Debug.Log($"[RealSolarSystem] EarthColor.dds size: {(fi.Length / 1024d / 1024d):F1}MB");
                }
            }
        }

        private void CheckCompatibleKopInstalled()
        {
            string kopVer = Kopernicus.Constants.Version.VersionNumber;    // Is in format "Release-160"
            string sRelNum = kopVer.Split('-').Last();
            bool success = int.TryParse(sRelNum, out int relNum);

            const int minKopReleaseVer = 161;

            if (!success || relNum < minKopReleaseVer)
            {
                Debug.Log("[RealSolarSystem] Invalid Kopernicus version detected: " + kopVer);

                string errorMsg = $"The currently installed version of Kopernicus ({kopVer}) is missing features required for RSS to function properly.\n\nPlease install Kopernicus release {minKopReleaseVer} or newer.";

                PopupDialog.SpawnPopupDialog
                (
                    new Vector2(0.0f, 0.0f),
                    new Vector2(0.0f, 0.0f),
                    new MultiOptionDialog
                    (
                        "RSSKopInstallCheck",
                        errorMsg,
                        "Incompatible Kopernicus version found",
                        HighLogic.UISkin,
                        new Rect(0.35f, 0.65f, 320f, 80f),
                        new DialogGUIFlexibleSpace(),
                        new DialogGUIButton
                        (
                            "Understood",
                            delegate { },
                            140.0f,
                            30.0f,
                            true
                        )
                    ),
                    false,
                    HighLogic.UISkin,
                    true,
                    string.Empty
                );
            }
        }

        private void OpenRSSTexturesDownloadPage()
        {
            Application.OpenURL("https://github.com/KSP-RO/RSS-Textures/releases/latest");
        }
    }
}
