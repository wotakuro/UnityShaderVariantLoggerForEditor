
using UnityEngine;
using UnityEditor;

using System.Collections.Generic;

#if VARIANT_LOGGER_COVERAGE_ADDRESSABLE
using UnityEditor.AddressableAssets;
#endif

namespace UTJ.VariantLogger
{
    public class BuildTargetSceneCollector
    {
        private List<SceneAsset> currentSceneAsset;
        public List<SceneAsset> Result
        {
            get { return currentSceneAsset; }
        }

        public BuildTargetSceneCollector()
        {
            currentSceneAsset = new List<SceneAsset>();
            AddBuildScene(currentSceneAsset);
            var allPath = this.GetAllScenePath();
            this.AddAssetBundleLabelScene(currentSceneAsset, allPath);
            this.AddAddressableLabelScenes(currentSceneAsset, allPath);
        }

        private List<string> GetAllScenePath()
        {
            List<string> paths = new List<string>();
            var guids = AssetDatabase.FindAssets("t:scene");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                paths.Add(path);
            }
            return paths;
        }

        private void AddBuildScene(List<SceneAsset> scenes)
        {
            var buildScenes = EditorBuildSettings.scenes;
            foreach (var buildScene in buildScenes)
            {
                if (!buildScene.enabled) { continue; }
                scenes.Add(AssetDatabase.LoadAssetAtPath<SceneAsset>(buildScene.path));
            }
        }

        private void AddAssetBundleLabelScene(List<SceneAsset> scenes, List<string> scenePath)
        {
            foreach (var path in scenePath)
            {
                var importer = AssetImporter.GetAtPath(path);
                if (importer == null || string.IsNullOrEmpty(importer.assetBundleName))
                {
                    continue;
                }
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                if (!scenes.Contains(sceneAsset))
                {
                    scenes.Add(sceneAsset);
                }

            }
        }

        private void AddAddressableLabelScenes(List<SceneAsset> scenes, List<string> scenePath)
        {
#if VARIANT_LOGGER_COVERAGE_ADDRESSABLE
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            foreach (var path in scenePath)
            {
                var importer = AssetImporter.GetAtPath(path);
                if (importer == null)
                {
                    continue;
                }
                var entry = settings.FindAssetEntry(AssetDatabase.AssetPathToGUID(path));
                if (entry == null) { continue; }
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                if (!scenes.Contains(sceneAsset))
                {
                    scenes.Add(sceneAsset);
                }
            }
#endif
        }

    }
}
