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
    internal class CreateVariantCollectionAssetFromLog : EditorWindow
    {
        private const string MenuName = "Tools/UTJ/ShaderVariantLogger/CreateAssetFromLog";

        private ScrollView logListView;
        private Toggle deleteFlagToggle;
        private Toggle enagleLoggerToggle;
        private ObjectField targetObjectField;
        private Button addExecButton;
        private Button openDirButton;

        [MenuItem(MenuName)]
        public static void Create()
        {
            EditorWindow.GetWindow<CreateVariantCollectionAssetFromLog>();
        }
        private void OnEnable()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.utj.shadervariantlogger/Editor/UXML/VariantCollectionCreateUI.uxml");

            this.rootVisualElement.Add(tree.CloneTree());

            this.logListView = this.rootVisualElement.Q<ScrollView>("LogList"); 
            this.deleteFlagToggle = this.rootVisualElement.Q<Toggle>("DeleteFlag");
            this.enagleLoggerToggle = this.rootVisualElement.Q<Toggle>("LoggerEnable");
            this.targetObjectField = this.rootVisualElement.Q<ObjectField>("TargetAsset");
            this.addExecButton = this.rootVisualElement.Q<Button>("AddExec");
            this.openDirButton = this.rootVisualElement.Q<Button>("OpenDir");
            // setup UI
            enagleLoggerToggle.value = EditorVariantLoggerMenu.EnableFlag;
            enagleLoggerToggle.RegisterValueChangedCallback(OnChangeEnableLogger);
            openDirButton.clicked += OnClickOpenDirectory;
            addExecButton.clicked += OnClickAddExecute;
            this.targetObjectField.objectType = typeof(ShaderVariantCollection);
            SetupLogListView();
        }

        private void SetupLogListView()
        {
            var files = GetFiles();
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
            var files = this.GetFiles();
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
            EditorUtility.RevealInFinder(EditorShaderVariantLogger.SaveDir);
        }

        private void OnChangeEnableLogger(ChangeEvent<bool> val)
        {
            EditorVariantLoggerMenu.SetEnable(val.newValue);
        }

        private List<string> GetFiles()
        {
            var files = Directory.GetFiles(EditorShaderVariantLogger.SaveDir);
            var list = new List<string>(files.Length);
            foreach (var file in files)
            {
                list.Add(file);
            }
            list.Sort();
            return list;

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
            var shaderName = datas[1];
            var pass = datas[4];
            var keywords = datas[6];

            Shader shader = Shader.Find(shaderName);
            if(shader == null) { return false; }
            string shaderPath = AssetDatabase.GetAssetPath(shader);
            if (!shaderPath.StartsWith("Assets/")||
                !shaderPath.StartsWith("Packages/"))
            {
                return false;
            }

            variant.shader = shader;
            variant.keywords = GetKeywordArray(keywords);
            variant.passType = GetPassType(pass);

            return false;
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
