using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UTJ.VariantLogger
{
    public class ShaderVariantLoggerInterface
    {
        const string DllName = "ShaderVariantLogger";
        [DllImport(DllName)]
        private extern static void _ShaderCompileWatcherForEditorSetupFile(string file);

        [DllImport(DllName)]
        private extern static void _ShaderCompileWatcherForEditorSetFrame(int idx);

        [DllImport(DllName)]
        private extern static void _ShaderCompileWatcherForEditorSetEnable(bool enable);


        public static void SetupFile(string file)
        {
            _ShaderCompileWatcherForEditorSetupFile(file);
        }
        public static void SetFrame(int index)
        {
            _ShaderCompileWatcherForEditorSetFrame(index);
        }
        public static void SetEnable(bool flag)
        {
            _ShaderCompileWatcherForEditorSetEnable(flag);
        }
    }
}