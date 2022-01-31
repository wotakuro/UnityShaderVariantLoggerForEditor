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



        public override void OnEnable()
        {
            var treePath = "Packages/com.utj.shadervariantlogger/Extentions/SceneCoverage/Editor/UXML/SceneLogAnalyze.uxml";
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(treePath);
            this.rootVisualElement.Add(tree.CloneTree());

            var resultView = this.rootVisualElement.Q<VisualElement>("ResultView");
            resultView.visible = false;

            var analyzeButton = this.rootVisualElement.Q<Button>("AnalyzeBtn");
            analyzeButton.clicked += OnClickAnalyze;
        }

        private void OnClickAnalyze()
        {
            var resultView = this.rootVisualElement.Q<VisualElement>("ResultView");
            resultView.visible = true;

            var anlyzeBtn = this.rootVisualElement.Q<Button>("AnalyzeBtn");
            analyzer = new SceneCoverageAnalyzer();
            analyzer.Execute();
            anlyzeBtn.visible = false;

            var res = analyzer.Result;
            foreach( var val in res)
            {
                Debug.Log(val.Value.scenePath + ":" + val.Value.loadFrame + ":" + val.Value.activeFrame );
            }

        }

        public override bool enabled => true;


    }
}
