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

        private SceneCoverageAnalyzer analyzer;
        private BuildTargetSceneCollector targetSceneCollector;

        private ScrollView resultArea;
        private Toggle showNoActiveOnlyToggle;
        private Toggle showNoLoadOnlyToggle;


        public override void OnEnable()
        {
            var treePath = "Packages/com.utj.shadervariantlogger/Extentions/SceneCoverage/Editor/UXML/SceneLogAnalyze.uxml";
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(treePath);
            this.rootVisualElement.Add(tree.CloneTree());


            var analyzeButton = this.rootVisualElement.Q<Button>("AnalyzeBtn");
            analyzeButton.clicked += OnClickAnalyze;


            this.resultArea = this.rootVisualElement.Q<ScrollView>();
            this.showNoActiveOnlyToggle = this.rootVisualElement.Q<Toggle>("ShowNoActiveOnly");
            this.showNoLoadOnlyToggle = this.rootVisualElement.Q<Toggle>("ShowNoLoadOnly");

            this.showNoActiveOnlyToggle.RegisterValueChangedCallback((val) => { SetResult(); });
            this.showNoLoadOnlyToggle.RegisterValueChangedCallback((val) => { SetResult(); });

            // todo wip
            var resultView = this.rootVisualElement.Q<VisualElement>("ResultView");
            resultView.visible = false;
        }

        private void OnClickAnalyze()
        {
            var resultView = this.rootVisualElement.Q<VisualElement>("ResultView");
            resultView.visible = true;

            var anlyzeBtn = this.rootVisualElement.Q<Button>("AnalyzeBtn");
            analyzer = new SceneCoverageAnalyzer();
            analyzer.Execute();
            anlyzeBtn.visible = false;

            this.targetSceneCollector = new BuildTargetSceneCollector();
            this.SetResult();
        }


        private void SetResult() {
            bool isNoActiveOnly = showNoActiveOnlyToggle.value;
            bool isNoLoadOnly = showNoLoadOnlyToggle.value;

            this.resultArea.Clear();
            foreach (var scene in targetSceneCollector.Result)
            {
                if (isNoLoadOnly && analyzer.GetLoadFrame(scene.path) > 0) { continue; }
                if (isNoActiveOnly && analyzer.GetActiveFrame(scene.path) > 0) { continue; }

                VisualElement row = new VisualElement();
                var objField = new ObjectField();
                objField.value = scene.sceneAsset;
                row.Add(objField);

                Label activeLabel = new Label(analyzer.GetActiveFrame(scene.path).ToString());
                row.Add(activeLabel);
                Label loadLabel = new Label(analyzer.GetLoadFrame(scene.path).ToString());
                row.Add(loadLabel);

                row.style.flexDirection = FlexDirection.Row;
                row.style.justifyContent = Justify.SpaceBetween;
                resultArea.Add(row);
            }
            

        }

        public override bool enabled => true;


    }
}
