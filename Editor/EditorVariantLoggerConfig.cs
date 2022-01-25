using System.Collections;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace UTJ.VariantLogger
{
    internal class EditorVariantLoggerConfig
    {

        private const string ConfigFile = "Library/com.utj.shadervariantlogger/config.txt";
        internal static bool EnableFlag { get; set; } = false;

        [System.Serializable]
        struct ConfigData
        {
            public bool flag;
        }

        [InitializeOnLoadMethod]
        public static void Init()
        {
            if (!File.Exists(ConfigFile))
            {
                EnableFlag = false;
                return;
            }
            var config = ReadConfigData();
            EnableFlag = config.flag;
        }


        internal static void SetEnable(bool flag)
        {
            EnableFlag = flag;
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
            ConfigData data = new ConfigData() { flag = EnableFlag };
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