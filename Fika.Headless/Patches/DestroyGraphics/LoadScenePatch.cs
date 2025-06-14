using BSG.Unity.Wires;
using EFT.Interactive;
using HarmonyLib;
using Fika.Core.Patching;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fika.Headless.Patches.DestroyGraphics
{
    public class LoadScenePatch : FikaPatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(SceneManager), nameof(SceneManager.LoadSceneAsync), [typeof(string), typeof(LoadSceneMode)]);
        }

        [PatchPostfix]
        static void Postfix(string sceneName, AsyncOperation __result)
        {
            GameObject tempGameObject = new("SceneModificationHandler");
            SceneModificationHandler handler = tempGameObject.AddComponent<SceneModificationHandler>();

            handler.StartCoroutine(handler.WaitForSceneLoad(sceneName, __result));
        }
    }

    public class SceneModificationHandler : MonoBehaviour
    {
        public IEnumerator WaitForSceneLoad(string sceneName, AsyncOperation operation)
        {
            // Wait for the scene to finish loading
            while (!operation.isDone)
            {
                yield return null;
            }

            FikaHeadlessPlugin.FikaHeadlessLogger.LogDebug($"Scene {sceneName} is fully loaded.");

            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            if (loadedScene.IsValid())
            {
                ModifyLoadedScene(loadedScene);
            }

            Destroy(gameObject);
        }

        private void ModifyLoadedScene(Scene scene)
        {
            foreach (GameObject RootGameObjects in scene.GetRootGameObjects())
            {
                //Logger.LogInfo($"Inspecting root object: {rootObj.name}");
                DestroyRenderers(RootGameObjects);
            }
        }

        private static readonly Type[] _protectedComponents = [
            typeof(WindowBreaker),
            typeof(AmbientLight),
            typeof(AreaLight),
            typeof(HotObject),
            typeof(RoadSplineGenerator),
            typeof(RoadSolidMarkGenerator),
            typeof(RoadMarksGenerator),
            typeof(WireGenerator), // These can't be removed, MeshRenderers keeps these alive at the moment
			typeof(WireSplineGenerator)
        ];

        private void DestroyRenderers(GameObject prefab)
        {
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            ParticleSystem[] particles = prefab.GetComponentsInChildren<ParticleSystem>(true);

            foreach (ParticleSystem particle in particles)
            {
                if (particle.gameObject.name.ToLower().Contains("door"))
                {
                    continue;
                }

                particle.Stop();
                Destroy(particle);
            }

            foreach (Renderer renderer in renderers)
            {
                // Check for protected components we absolutely cannot unload, these would break the game someway or another.
                bool hasProtectedRenderer = false;
                foreach (Type componentType in _protectedComponents)
                {
                    if (renderer.gameObject.GetComponent(componentType) != null || renderer.gameObject.name.ToLower().Contains("door") || renderer.gameObject.name.ToLower().Contains("glass"))
                    {
                        hasProtectedRenderer = true;
                        break;
                    }
                }

                // Unload materials and textures.
                foreach (Material material in renderer.sharedMaterials)
                {
                    if (material != null)
                    {
                        if (material.name.ToLower().Contains("glass"))
                        {
                            continue;
                        }

                        if (material.HasProperty("_MainTex"))
                        {
                            material.mainTexture = null;
                        }

                        Destroy(material);
                    }
                }

                if (hasProtectedRenderer)
                {
                    continue;
                }

                // Destroy the renderer itself, note: This should not remove any colliders and such.
#if DEBUG
                FikaHeadlessPlugin.FikaHeadlessLogger.LogDebug($"Removing Renderer: {renderer.gameObject.name}");
#endif
                Destroy(renderer);
            }
        }
    }
}