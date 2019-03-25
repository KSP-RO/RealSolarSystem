using System;
using System.IO;
using UnityEngine;

namespace RealSolarSystem
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]

    class RSSInstallationCheck : MonoBehaviour
    {
        void Start ()
        {
            try
            {
                string szTextureFolderPath = "GameData" + Path.AltDirectorySeparatorChar + "RSS-Textures";

                const string szUserMessage = "The texture pack for RealSolarSystem is missing from your installation\n\nDownload it from the official GitHub KSP-RO repository.";

                if (!Directory.Exists (szTextureFolderPath))
                {
                    Debug.Log ("[RealSolarSystem]: No texture pack detected!");

                    PopupDialog.SpawnPopupDialog
                    (
                        new Vector2 (0.0f, 0.0f),
                        new Vector2 (0.0f, 0.0f),
                        new MultiOptionDialog
                        (
                            "RSSInstallCheck",
                            szUserMessage,
                            "Missing RSS Texture Pack",
                            HighLogic.UISkin,
                            new Rect (0.25f, 0.75f, 320f, 80f),
                            new DialogGUIFlexibleSpace (),
                            new DialogGUIButton
                            (
                                "Download",
                                delegate
                                {
                                    OnOpenPage ();
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
            }
            catch (Exception exceptionStack)
            {
                Debug.Log ("[RealSolarSystem]: RSSInstallationCheck.Start() caught an exception: " + exceptionStack);
            }
            finally
            {
                Destroy (this);
            }
        }

        void OnOpenPage ()
        {
            Application.OpenURL ("https://github.com/KSP-RO/RSS-Textures/releases/latest");
        }
    }
}
