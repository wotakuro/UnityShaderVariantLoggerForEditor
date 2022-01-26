#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Text;

namespace UTJ.VariantLogger
{
    public class SceneLogger : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            if (ShaderVariantLoggerInterface.GetEnable())
            {
                var gmo = new GameObject("SceneLogger", typeof(SceneLogger));
                Object.DontDestroyOnLoad(gmo);
                gmo.hideFlags = HideFlags.HideInHierarchy;
            }

        }
        private int frameCount;
        private string logFile;
        private StringBuilder stringBuilder;

        private void Awake()
        {
            stringBuilder = new StringBuilder(256);
            logFile = ShaderVariantLoggerInterface.GetCurrentFile().Replace("/logs/","/scenelogs/");
            Debug.Log(logFile);
            var dir = logFile.Substring(0, logFile.LastIndexOf('/'));
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            stringBuilder.Clear();
            stringBuilder.Append("frame,event,currentScene,nextScene\n");

            stringBuilder.Append(Time.frameCount).Append(",start,");
            for (int i = 0; i < SceneManager.sceneCount; ++i)
            {
                stringBuilder.Append(SceneManager.GetSceneAt(i).path);
            }
            stringBuilder.Append(",");
            stringBuilder.Append(Time.frameCount).Append(",active,").Append(SceneManager.GetActiveScene().path).Append("\n");
            File.WriteAllText(logFile, stringBuilder.ToString());

            SceneManager.sceneLoaded += OnSceneLoad;
            SceneManager.sceneUnloaded += OnSceneUnload;
            SceneManager.activeSceneChanged += OnActiveSceneChange;
        }

        private void Update()
        {
            this.frameCount = Time.frameCount;
        }


        private void OnDestroy()
        {
            stringBuilder.Clear();
            stringBuilder.Append(frameCount).Append(",end,").Append(SceneManager.GetActiveScene().path).Append("\n");
            File.AppendAllText(logFile, stringBuilder.ToString());

            SceneManager.sceneLoaded -= OnSceneLoad;
            SceneManager.sceneUnloaded -= OnSceneUnload;
            SceneManager.activeSceneChanged -= OnActiveSceneChange;
        }

        void OnActiveSceneChange(Scene current, Scene next)
        {
            stringBuilder.Clear();
            stringBuilder.Append(Time.frameCount).Append(",changeActive,").Append(current.path).Append(",").Append(next.path).Append("\n");
            File.AppendAllText(logFile, stringBuilder.ToString());
        }

        void OnSceneLoad(Scene scene,LoadSceneMode mode)
        {
            stringBuilder.Clear();
            stringBuilder.Append(Time.frameCount);
            switch (mode )
            {
                case LoadSceneMode.Single:
                    stringBuilder.Append(",loadScene,");
                    break;
                case LoadSceneMode.Additive:
                    stringBuilder.Append(",loadSceneAdd,");
                    break;
                default:
                    stringBuilder.Append(",,");
                    break;
            }
            stringBuilder.Append(scene.path).Append("\n");
            File.AppendAllText(logFile, stringBuilder.ToString());
        }

        void OnSceneUnload(Scene scene)
        {
            stringBuilder.Clear();
            stringBuilder.Append(Time.frameCount).Append(",unload,").Append(scene.path).Append("\n");
            File.AppendAllText(logFile, stringBuilder.ToString());


        }

    }
}
#endif
