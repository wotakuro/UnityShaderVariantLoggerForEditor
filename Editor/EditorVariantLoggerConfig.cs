using System.Collections;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace UTJ.VariantLogger
{
    public class EditorVariantLoggerConfig
    {
        [System.Serializable]
        struct ConfigData
        {
            public bool flag;
            public bool clearShaderCache;
            public string fileHeader;
        }
        private const string WorkingDir =  "Library/com.utj.shadervariantlogger";
        private const string ConfigFile = WorkingDir + "/config.txt";
        public static readonly string SaveDir = WorkingDir + "/logs";

        private static ConfigData currentConfig;
        public static bool EnableFlag
        {
            get
            {
                return currentConfig.flag;
            }
            set
            {
                currentConfig.flag = value;
                SaveConfigData();
            }
        }
        internal static bool ClearShaderCache {
            get
            {
                return currentConfig.clearShaderCache;
            }
            set
            {
                currentConfig.clearShaderCache = value;
                SaveConfigData();
            }
        }
        internal static string FileHeader
        {
            get
            {
                return currentConfig.fileHeader;
            }
            set
            {
                string old = currentConfig.fileHeader;
                currentConfig.fileHeader = value;
                if(old != value) { SaveConfigData(); }
            }
        }


        [InitializeOnLoadMethod]
        internal static void Init()
        {
            if (!File.Exists(ConfigFile))
            {
                EnableFlag = false;
                ClearShaderCache = true;
                FileHeader = GetDefaultHeader();
                return;
            }
            currentConfig = ReadConfigData();
            if(FileHeader == null)
            {
                FileHeader = GetDefaultHeader();
            }
        }
        private static string GetDefaultHeader()
        {
            return SystemInfo.deviceName.Replace("/", "").Replace("\\", ""); ;
        }

        



        private static ConfigData ReadConfigData()
        {
            string str = File.ReadAllText(ConfigFile);
            var data = JsonUtility.FromJson<ConfigData>(str);
            return data;
        }

        private static void SaveConfigData()
        {
            var str = JsonUtility.ToJson(currentConfig);
            string dir = Path.GetDirectoryName(ConfigFile);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(ConfigFile, str);
        }
    }

}