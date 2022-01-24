#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UTJ.VariantLogger
{
    public class ShaderVariantLoggerBehaviour : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Init()
        {
            if (ShaderVariantLoggerInterface.GetEnable())
            {
                var gmo = new GameObject("ShaderVariantLogger");
                gmo.AddComponent<ShaderVariantLoggerBehaviour>();
                DontDestroyOnLoad(gmo);
            }
        }
        void Update()
        {
            ShaderVariantLoggerInterface.SetFrame(Time.frameCount);
        }
        private void OnDestroy()
        {
            ShaderVariantLoggerInterface.SetEnable(false);
        }
    }
}
#endif