using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.Rendering;

namespace UTJ.VariantLogger
{

    internal class CreateVariantCollectionAssetFromLog : VariantLoggerWindow.UIMenuItem
    {
        private const string MenuName = "Tools/UTJ/ShaderVariantLogger";

        private ScrollView logListView;
        private Toggle deleteFlagToggle;

        private ObjectField targetObjectField;
        private Button addExecButton;
        private Button openDirButton;

        private Toggle includeAssetsToggle;
        private Toggle includePackagesToggle;
        private Toggle includeBuiltInToggle;
        private Toggle includeBuiltInExtraToggle;
        private Toggle includeOthersToggle;
        

        public override string toolbar => "Create Asset";

        public override int order => 1;

        public override void OnEnable()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.utj.shadervariantlogger/Editor/UXML/VariantCollectionCreateUI.uxml");

            this.rootVisualElement.Add(tree.CloneTree());

            this.logListView = this.rootVisualElement.Q<ScrollView>("LogList"); 
            this.deleteFlagToggle = this.rootVisualElement.Q<Toggle>("DeleteFlag");

            
            this.targetObjectField = this.rootVisualElement.Q<ObjectField>("TargetAsset");
            this.addExecButton = this.rootVisualElement.Q<Button>("AddExec");
            this.openDirButton = this.rootVisualElement.Q<Button>("OpenDir");

            // Detail 
            includeAssetsToggle = this.rootVisualElement.Q<Toggle>("IncludeAssets"); ;
            includePackagesToggle = this.rootVisualElement.Q<Toggle>("IncludePackages"); ;
            includeBuiltInToggle = this.rootVisualElement.Q<Toggle>("IncludeUnityBuiltIn"); ;
            includeBuiltInExtraToggle = this.rootVisualElement.Q<Toggle>("IncludeUnityBuiltinExtra"); ;
            includeOthersToggle = this.rootVisualElement.Q<Toggle>("IncludeOthers"); ;

            // setup UI
            openDirButton.clicked += OnClickOpenDirectory;
            addExecButton.clicked += OnClickAddExecute;
            this.targetObjectField.objectType = typeof(ShaderVariantCollection);
            // set DetailDefault
            includeAssetsToggle.value = true;
            includePackagesToggle.value = true;
            includeBuiltInToggle.value = false;
            includeBuiltInExtraToggle.value = false;
            includeOthersToggle.value = false;

            SetupLogListView();

        }

        private void SetupLogListView()
        {
            var files = GeneralSettingsUI.GetFiles();
            this.logListView.Clear();
            foreach (var file in files)
            {
                var fileonly = Path.GetFileName(file);
                this.logListView.Add(new Label(fileonly));
            }
        }

        private void OnClickAddExecute()
        {
            var deleteFlag = this.deleteFlagToggle.value;
            var targetAsset = this.targetObjectField.value as ShaderVariantCollection;
            if (!targetAsset)
            {
                var file = EditorUtility.SaveFilePanelInProject("Create ShaderVariant", "ShaderVariantCollection", "shadervariants", "please set create shadervariants");
                if (string.IsNullOrEmpty(file))
                {
                    return;
                }
                targetAsset = new ShaderVariantCollection();
                ExecuteToShaderVariantAsset(targetAsset, deleteFlag);
                AssetDatabase.CreateAsset(targetAsset, file);
                EditorUtility.DisplayDialog("Complete", "create shader variant collection.", "ok");
            }
            else
            {
                ExecuteToShaderVariantAsset(targetAsset, deleteFlag);
                EditorUtility.SetDirty(targetAsset);
                AssetDatabase.SaveAssets();
                EditorUtility.DisplayDialog("Complete", "Add shader variants to collection.", "ok");
            }
        }
        private void ExecuteToShaderVariantAsset(ShaderVariantCollection targetAsset,bool deleteFlag) { 
            var files = GeneralSettingsUI.GetFiles();
            int length = files.Count;
            int idx = 0;
            foreach( var file in files)
            {
                EditorUtility.DisplayProgressBar("Adding Variat collection", file,(float)idx/ (float)length);
                AppendVariantFromLog(targetAsset, file);
                ++idx;
            }
            EditorUtility.ClearProgressBar();

            if (deleteFlag)
            {
                DeleteLogs(files);
            }

            // after
            this.SetupLogListView();            
        }

        private void DeleteLogs(List<string> files)
        {
            var res = EditorUtility.DisplayDialog("DeleteFile", "Delete log files?", "OK", "Cancel");
            if (!res) { return; }
            foreach(var file in files)
            {
                File.Delete(file);
            }
        }

        private void OnClickOpenDirectory()
        {
            if (!Directory.Exists(EditorVariantLoggerConfig.LogSaveDir))
            {
                Directory.CreateDirectory(EditorVariantLoggerConfig.LogSaveDir);
            }
            EditorUtility.RevealInFinder(EditorVariantLoggerConfig.LogSaveDir);
        }

        private void OnChangeEnableLogger(ChangeEvent<bool> val)
        {
            EditorVariantLoggerConfig.EnableFlag = val.newValue;
        }

        private void OnChangeClearShaderCache(ChangeEvent<bool> val)
        {
            EditorVariantLoggerConfig.ClearShaderCache = val.newValue;
        }
        


        public void AppendVariantFromLog(ShaderVariantCollection collection ,
            string logFile)
        {
            var lines = File.ReadAllLines(logFile);
            int length = lines.Length;
            ShaderVariantCollection.ShaderVariant variant;
            for (int i = 1; i < length; ++i)
            {
                bool res = GetVariantFromLogLine(lines[i],out variant);
                if (res && !collection.Contains(variant))
                {
                    collection.Add(variant);
                }
            }
        }

        private bool GetVariantFromLogLine(string line,out ShaderVariantCollection.ShaderVariant variant)
        {
            variant = new ShaderVariantCollection.ShaderVariant();

            var datas = line.Split(',');
            if(datas.Length < 7)
            {
                return false;
            }
            var stage = datas[5];
            var shaderName = datas[1];
            var pass = datas[4];
            var keywords = datas[6];

            // The keywords of "Shader.CompileGPUProgram" is different from "Shader.CreateGPUProgram"
            // have to something todo.....
            if(stage == "EditorCompile")
            {
                return false;
            }

            Shader shader = Shader.Find(shaderName);
            if(shader == null) { return false; }
            string shaderPath = AssetDatabase.GetAssetPath(shader).ToLower();
            if (shaderPath.StartsWith("assets/") )
            {
                if (!this.includeAssetsToggle.value)
                {
                    return false;
                }
            }
            else if (shaderPath.StartsWith("packages/") )
            {
                if (!this.includePackagesToggle.value)
                {
                    return false;
                }
            }
            else if (shaderPath == "resources/unity_builtin" )
            {
                if (!this.includeBuiltInToggle.value)
                {
                    return false;
                }
            }
            else if (shaderPath == "resources/unity_builtin_extra")
            {
                if (!this.includeBuiltInExtraToggle.value)
                {
                    return false;
                }
            }
            else
            {
                Debug.Log("other pass shader found " + shader.name + "::" + shaderPath);
                if (!this.includeOthersToggle.value)
                {
                    return false;
                }
            }

            variant.shader = shader;
            variant.keywords = GetKeywordArray(keywords);
            var lightMode = ShaderPassLightModeConverter.GetLightModeByPasssName(shader, pass);
            variant.passType = GetPassType(lightMode);

            return true;
        }

        private string[] GetKeywordArray(string keywords)
        {

            string[] keywordArray;
            if (string.IsNullOrEmpty(keywords) || keywords== "<no keywords>")
            {
                keywordArray = new string[] { "" };
            }
            else
            {
                keywordArray = keywords.Split(' ');
            }
            return keywordArray;
        }

        private static PassType GetPassType(string str)
        {
            if(str == null)
            {
                return PassType.Normal;
            }
            str = str.ToUpper();
            switch (str)
            {
                case "":
                case "ALWAYS":
                    return PassType.Normal;
                case "VERTEX":
                    return PassType.Vertex;
                case "VERTEXLM":
                    return PassType.VertexLM;
                case "VERTEXLMRGBM":
                    return PassType.ForwardBase;
                case "FORWARDADD":
                    return PassType.ForwardAdd;
                case "PREPASSBASE":
                    return PassType.LightPrePassBase;
                case "PREPASSFINAL":
                    return PassType.LightPrePassFinal;
                case "SHADOWCASTER":
                    return PassType.ShadowCaster;
                case "DEFERRED":
                    return PassType.Deferred;
                case "META":
                    return PassType.Meta;
                case "MOTIONVECTORS":
                    return PassType.MotionVectors;
                case "SRPDEFAULTUNLIT":
                    return PassType.ScriptableRenderPipelineDefaultUnlit;
            }
            //                PassType.ScriptableRenderPipelineDefaultUnlit
            return PassType.ScriptableRenderPipeline;
        }
        
    }
}
