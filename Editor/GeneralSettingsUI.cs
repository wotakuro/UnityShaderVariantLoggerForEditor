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

    internal class GeneralSettingsUI : VariantLoggerWindow.UIMenuItem
    {
        private ScrollView logListView;
        private Toggle enagleLoggerToggle;
        private Toggle clearShaderCacheToggle;
        
        private Button openDirButton;
        
        private TextField fileHeaderTextField;

        public override string toolbar => "General";

        public override int order => 0;

        public override void OnEnable()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.utj.shadervariantlogger/Editor/UXML/GeneralSettingsUI.uxml");

            this.rootVisualElement.Add(tree.CloneTree());

            this.logListView = this.rootVisualElement.Q<ScrollView>("LogList"); 
            this.enagleLoggerToggle = this.rootVisualElement.Q<Toggle>("LoggerEnable");
            this.clearShaderCacheToggle = this.rootVisualElement.Q<Toggle>("ClearShaderCache");

            
            this.openDirButton = this.rootVisualElement.Q<Button>("OpenDir");
            

            // setup UI
            enagleLoggerToggle.value = EditorVariantLoggerConfig.EnableFlag;
            clearShaderCacheToggle.value = EditorVariantLoggerConfig.ClearShaderCache;
            enagleLoggerToggle.RegisterValueChangedCallback(OnChangeEnableLogger);
            clearShaderCacheToggle.RegisterValueChangedCallback(OnChangeClearShaderCache);
            openDirButton.clicked += OnClickOpenDirectory;
            // set fileHeader
            fileHeaderTextField = this.rootVisualElement.Q<TextField>("FileHeader");
            fileHeaderTextField.value = EditorVariantLoggerConfig.FileHeader;
            fileHeaderTextField.RegisterCallback<FocusOutEvent>((evt) =>
            {
                EditorVariantLoggerConfig.FileHeader = fileHeaderTextField.value;
            });

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

        internal static List<string> GetFiles()
        {
            if (!Directory.Exists(EditorVariantLoggerConfig.LogSaveDir))
            {
                return new List<string>();
            }
            var files = Directory.GetFiles(EditorVariantLoggerConfig.LogSaveDir);
            var list = new List<string>(files.Length);
            foreach (var file in files)
            {
                list.Add(file);
            }
            list.Sort();
            return list;
        }
        
        
    }
}
