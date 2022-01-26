#if UNITY_EDITOR
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

        [DllImport(DllName)]
        private extern static bool _ShaderCompileWatcherForEditorGetEnable();

        [DllImport(DllName)]
        private extern static System.IntPtr _ShaderCompileWatcherForEditorGetCurrentFile();

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

        public static bool GetEnable()
        {
            return _ShaderCompileWatcherForEditorGetEnable();
        }

        public static string GetCurrentFile()
        {
            var ptr = _ShaderCompileWatcherForEditorGetCurrentFile();
            return Marshal.PtrToStringAnsi(ptr);
        }
    }
}

#endif