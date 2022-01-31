
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
        public struct ResultInfo
        {
            public SceneAsset sceneAsset;
            public string path;

            public ResultInfo(string filePath)
            {
                path = filePath;
                sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
            }
            public override int GetHashCode()
            {
                return path.GetHashCode();
            }
            public override bool Equals(object obj)
            {
                if(obj is ResultInfo)
                {
                    var target = (ResultInfo)obj;
                    return target.path == this.path;
                }
                return false;
            }
        }
        private List<ResultInfo> currentSceneAsset;
        public List<ResultInfo> Result
        {
            get { return currentSceneAsset; }
        }

        public BuildTargetSceneCollector()
        {
            currentSceneAsset = new List<ResultInfo>();
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

        private void AddBuildScene(List<ResultInfo> scenes)
        {
            var buildScenes = EditorBuildSettings.scenes;
            foreach (var buildScene in buildScenes)
            {
                if (!buildScene.enabled) { continue; }
                scenes.Add(new ResultInfo(buildScene.path));
            }
        }

        private void AddAssetBundleLabelScene(List<ResultInfo> scenes, List<string> scenePath)
        {
            foreach (var path in scenePath)
            {
                var importer = AssetImporter.GetAtPath(path);
                if (importer == null || string.IsNullOrEmpty(importer.assetBundleName))
                {
                    continue;
                }
                var resultInfo = new ResultInfo(path);
                if (!scenes.Contains(resultInfo))
                {
                    scenes.Add(resultInfo);
                }

            }
        }

        private void AddAddressableLabelScenes(List<ResultInfo> scenes, List<string> scenePath)
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
                var resultInfo = new ResultInfo(path);
                if (!scenes.Contains(resultInfo))
                {
                    scenes.Add(resultInfo);
                }
            }
#endif
        }

    }
}
