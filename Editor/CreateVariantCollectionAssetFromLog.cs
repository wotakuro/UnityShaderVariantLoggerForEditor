using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.UIElements;
using UnityEngine.Rendering;

namespace UTJ.VariantLogger
{
    internal class CreateVariantCollectionAssetFromLog : EditorWindow
    {
        private const string MenuName = "Tools/UTJ/ShaderVariantLogger/CreateAssetFromLog";

        [MenuItem(MenuName)]
        public static void Create()
        {
            EditorWindow.GetWindow<CreateVariantCollectionAssetFromLog>();
        }
        private void OnEnable()
        {
        }

        private List<string> GetFiles()
        {
            var files = Directory.GetFiles(EditorShaderVariantLogger.SaveDir);
            var list = new List<string>(files);
            foreach (var file in files)
            {
                list.Add(file);
            }
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
            var shaderName = datas[1];
            var pass = datas[4];
            var keyword = datas[6];

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
