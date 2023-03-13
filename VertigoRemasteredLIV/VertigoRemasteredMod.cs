using LIV.SDK.Unity;
using MelonLoader;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace VertigoRemasteredLIV {

    public class VertigoRemasteredLIVMod : MelonMod {
        public static Action OnPlayerReady;

        private GameObject livObject;
        private Camera spawnedCamera;
        private static LIV.SDK.Unity.LIV livInstance;

        public override void OnInitializeMelon() {
            base.OnInitializeMelon();

            SetUpLiv();
            OnPlayerReady += TrySetupLiv;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;

            SystemLibrary.LoadLibrary($@"{MelonUtils.BaseDirectory}\Mods\LIVAssets\LIV_Bridge.dll");
        }

        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1) {
            TrySetupLiv();
        }

        public override void OnUpdate() {
            base.OnUpdate();

            if (Input.GetKeyDown(KeyCode.F3)) {
                MelonLogger.Msg(">>> F3: TrySetupLiv");
                TrySetupLiv();
            }

            UpdateFollowSpawnedCamera();
        }

        public void TrySetupLiv() {
            /*
             * Vertigo Remastered
             * 2019.4.16f1
             *
             * Camera names:
			 *   Camera (Head)
			*/

            Camera[] arrCam = GameObject.FindObjectsOfType<Camera>().ToArray();
            //MelonLogger.Msg(">>> Camera count: " + arrCam.Length);
            foreach (Camera cam in arrCam) {
                if (cam.name.Contains("LIV ")) {
                    continue;
                } else if (cam.name.Contains("Camera (Head)")) {
                    SetUpLiv(cam);
                    break;
                } // else MelonLogger.Msg(cam.name);
            }
        }

        private void UpdateFollowSpawnedCamera() {
            var livRender = GetLivRender();
            if (livRender == null || spawnedCamera == null) return;

            // When spawned objects get removed in Boneworks, they might not be destroyed and just be disabled.
            if (!spawnedCamera.gameObject.activeInHierarchy) {
                spawnedCamera = null;
                return;
            }

            var cameraTransform = spawnedCamera.transform;
            livRender.SetPose(cameraTransform.position, cameraTransform.rotation, spawnedCamera.fieldOfView);
        }

        private static void SetUpLiv() {
            AssetManager assetManager = new AssetManager($@"{MelonUtils.BaseDirectory}\Mods\LIVAssets\");
            var livAssetBundle = assetManager.LoadBundle("liv-shaders");
            SDKShaders.LoadFromAssetBundle(livAssetBundle);
        }

        private static Camera GetLivCamera() {
            try {
                return !livInstance ? null : livInstance.HMDCamera;
            } catch (Exception) {
                livInstance = null;
            }
            return null;
        }

        private static SDKRender GetLivRender() {
            try {
                return !livInstance ? null : livInstance.render;
            } catch (Exception) {
                livInstance = null;
            }
            return null;
        }

        private void SetUpLiv(Camera camera) {
            if (!camera) {
                MelonLogger.Msg("No camera provided, aborting LIV setup.");
                return;
            }

            var livCamera = GetLivCamera();
            if (livCamera == camera) {
                MelonLogger.Msg("LIV already set up with this camera, aborting LIV setup.");
                return;
            }

            MelonLogger.Msg($"Setting up LIV with camera: {camera.name}...");
            if (livObject) {
                Object.Destroy(livObject);
            }

            //var cameraParent = camera.transform.parent;
            var cameraParent = GameObject.Find("Playspace").transform;
            var cameraPrefab = new GameObject("LivCameraPrefab");
            cameraPrefab.SetActive(false);
            var cameraFromPrefab = cameraPrefab.AddComponent<Camera>();
            cameraFromPrefab.allowHDR = false;
            cameraPrefab.transform.SetParent(cameraParent, false);

            livObject = new GameObject("LIV");
            livObject.SetActive(false);

            livInstance = livObject.AddComponent<LIV.SDK.Unity.LIV>();
            livInstance.HMDCamera = camera;
            livInstance.MRCameraPrefab = cameraFromPrefab;
            livInstance.stage = cameraParent;
            livInstance.fixPostEffectsAlpha = true; //Vertigo Remastered, doesn't seem to impact anything.
            livInstance.spectatorLayerMask = ~0;
            livInstance.spectatorLayerMask &= ~(1 << (int)GameLayer.IgnoreRaycast); //Some lighting
            livInstance.spectatorLayerMask &= ~(1 << (int)GameLayer.PlayerBody);
            livInstance.spectatorLayerMask &= ~(1 << (int)GameLayer.VRRenderingOnly);

            //Current there is A LOT of lighting/alpha issues

            livObject.SetActive(true);

            GameObject sonjaSD = GameObject.Find("skin_default");
            foreach(Transform part in sonjaSD.transform) {
                if (part.gameObject.layer == (int)GameLayer.Default) {
                    // Don't want to change PlayerBody otherwise you'll see character's nose
                    part.gameObject.layer = (int)GameLayer.VRRenderingOnly;
                }
            }
        }
    }
}