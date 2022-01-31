using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace UTJ.VariantLogger
{

    internal class EditorShaderVariantLogger
    {

        [InitializeOnLoadMethod]
        public static void Init()
        {
            EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
        }

        private static void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
        {
            if (obj == PlayModeStateChange.ExitingEditMode)
            {
                Execute();
            }
        }

        public static void Execute()
        {
            if (!EditorVariantLoggerConfig.EnableFlag) {
                ShaderVariantLoggerInterface.SetEnable(false);
                return;
            }
            if (!EditorSettings.asyncShaderCompilation)
            {
                EditorUtility.DisplayDialog("Change EditorSettings", "[EditorSettings]Asynchronous Shader Compilation Disable->Enable", "OK");
                EditorSettings.asyncShaderCompilation = true;
            }
            ReloadShaders();
            SetupLogger();
        }

        public static void SetupLogger()
        {
                
            ShaderVariantLoggerInterface.SetEnable(true);
            ShaderVariantLoggerInterface.SetFrame(0);
            

            if (!Directory.Exists(EditorVariantLoggerConfig.SaveDir))
            {
                Directory.CreateDirectory(EditorVariantLoggerConfig.SaveDir);
            }
            var currentTime = System.DateTime.Now;
            ShaderVariantLoggerInterface.SetupFile(EditorVariantLoggerConfig.SaveDir + "/" +
                EditorVariantLoggerConfig .FileHeader + currentTime.ToString("_yyyyMMdd_HHmmss") + ".log");
        }

        public static void ReloadShaders() {
            var backupCompilation = ShaderUtil.allowAsyncCompilation;
            ShaderUtil.allowAsyncCompilation = true;
            if (EditorVariantLoggerConfig.ClearShaderCache)
            {
                System.IO.Directory.Delete("Library/ShaderCache", true);
            }

            var method = typeof(ShaderUtil).GetMethod("ReloadAllShaders", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
            method.Invoke(null,null);
        }

    }
}