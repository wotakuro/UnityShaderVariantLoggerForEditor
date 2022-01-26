using System.Collections;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace UTJ.VariantLogger
{
    public class EditorVariantLoggerConfig
    {
        private const string WorkingDir =  "Library/com.utj.shadervariantlogger";
        private const string ConfigFile = WorkingDir + "/config.txt";
        internal static readonly string SaveDir = WorkingDir + "/logs";

        public static bool EnableFlag { get; private set; } = false;
        internal static bool ClearShaderCache { get; private set; } = false;

        [System.Serializable]
        struct ConfigData
        {
            public bool flag;
            public bool clearShaderCache;
        }

        [InitializeOnLoadMethod]
        internal static void Init()
        {
            if (!File.Exists(ConfigFile))
            {
                EnableFlag = false;
                ClearShaderCache = true;
                return;
            }
            var config = ReadConfigData();
            EnableFlag = config.flag;
            ClearShaderCache = config.clearShaderCache;
        }


        internal static void SetEnable(bool flag)
        {
            EnableFlag = flag;
            SaveConfigData();
        }
        internal static void SetClearShaderCache(bool flag)
        {
            ClearShaderCache = flag;
            SaveConfigData();
        }



        private static ConfigData ReadConfigData()
        {
            string str = File.ReadAllText(ConfigFile);
            var data = JsonUtility.FromJson<ConfigData>(str);
            return data;
        }

        private static void SaveConfigData()
        {
            ConfigData data = new ConfigData() { 
                flag = EnableFlag,
                clearShaderCache = ClearShaderCache 
            };
            var str = JsonUtility.ToJson(data);
            string dir = Path.GetDirectoryName(ConfigFile);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(ConfigFile, str);
        }
    }

}