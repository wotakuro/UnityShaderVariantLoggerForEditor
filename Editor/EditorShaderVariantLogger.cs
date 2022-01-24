using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

namespace UTJ.VariantLogger
{

    internal class EditorShaderVariantLogger
    {
        private const string SaveDir = "Library/com.utj.shadervariantlogger/logs";

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
            if (!EditorVariantLoggerMenu.EnableFlag) {
                ShaderVariantLoggerInterface.SetEnable(false);
                return;
            }
            if (!EditorSettings.asyncShaderCompilation)
            {
                EditorUtility.DisplayDialog("Editor設定変更", "EditorSettingsのAsynchronous Shader Compilationを有効にします", "OK");
                EditorSettings.asyncShaderCompilation = true;
            }
            RemoveShaders();
            SetupLogger();
        }

        private static void SetupLogger()
        {
            ShaderVariantLoggerInterface.SetEnable(true);
            ShaderVariantLoggerInterface.SetFrame(0);

            if (!Directory.Exists(SaveDir))
            {
                Directory.CreateDirectory(SaveDir);
            }
            var currentTime = System.DateTime.Now;
            ShaderVariantLoggerInterface.SetupFile( SaveDir + currentTime.ToString("/yyyyMMdd_HHmmss.text"));
        }

        public static void RemoveShaders() {
            var backupCompilation = ShaderUtil.allowAsyncCompilation;
            ShaderUtil.allowAsyncCompilation = true;
            System.IO.Directory.Delete("Library/ShaderCache", true);

            var method = typeof(ShaderUtil).GetMethod("ReloadAllShaders", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
            method.Invoke(null,null);
        }

    }
}