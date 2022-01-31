using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#if VARIANT_LOGGER_COVERAGE_ADDRESSABLE
using UnityEditor.AddressableAssets;
#endif

namespace UTJ.VariantLogger
{
    public class SceneCorvarageEditorUI : VariantLoggerWindow.UIMenuItem
    {

        public override string toolbar => "Scenes";

        public override int order => 1;


        private List<SceneAsset> currentSceneAsset;

        public override void OnEnable()
        {
            var treePath = "Packages/com.utj.shadervariantlogger/Extentions/SceneCoverage/Editor/UXML/SceneLogAnalyze.uxml";
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(treePath);
            this.rootVisualElement.Add(tree.CloneTree());

            var resultView = this.rootVisualElement.Q<VisualElement>("ResultView");
            resultView.visible = false;

            currentSceneAsset = new List<SceneAsset>();
            AddBuildScene(currentSceneAsset);

            var allPath = this.GetAllScenePath();
            this.AddAssetBundleLabelScene(currentSceneAsset, allPath);
            this.AddAddressableLabelScenes(currentSceneAsset, allPath);

            foreach (var scene in currentSceneAsset)
            {
                var field = new ObjectField();
                field.SetValueWithoutNotify(scene);
                field.SetEnabled(false);
                this.rootVisualElement.Add(field);
            }
        }

        public override bool enabled => true;


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
                if(entry == null) { continue; }
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
