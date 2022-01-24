// dllmain.cpp : DLL アプリケーションのエントリ ポイントを定義します。
#include "UnityPluginHeaders/IUnityProfilerCallbacks.h"
#include <iostream>
#include <fstream>

// From PlatformDependSource
long GetThreadId();

static IUnityProfilerCallbacks* s_UnityProfilerCallbacks = NULL;

static bool s_executeFlag = false;
static int s_frameIndex = 0;
static std::string s_filename = "Variant.log";
static int warmupShaderCollectionLevel = -1;
static long warmupShaderThread = -1;

static void UNITY_INTERFACE_API OnProfilerEvent(const UnityProfilerMarkerDesc* markerDesc, UnityProfilerMarkerEventType eventType, unsigned short eventDataCount, const UnityProfilerMarkerData* eventData, void* userData)
{
    if (!s_executeFlag) { return; }

    switch (eventType)
    {
    case kUnityProfilerMarkerEventTypeBegin:
    {
        if (warmupShaderCollectionLevel >= 0 &&
            warmupShaderThread == GetThreadId() )
        {
            ++warmupShaderCollectionLevel;
        }


        if (eventDataCount == 3 &&
            strncmp(markerDesc->name, "Shader.CompileGPUProgram", 24) == 0)
        {



            std::ofstream file_out;
            file_out.open(s_filename, std::ios_base::app);
            file_out << s_frameIndex << "," <<
                // shader name
                reinterpret_cast<const char*>(eventData[0].ptr) << "," << "0.0,";

            if (warmupShaderCollectionLevel > 0) {
                file_out << "True" << ",";
            }
            else {
                file_out << "False" << ",";
            }

                // pass
            file_out << reinterpret_cast<const char*>(eventData[1].ptr) << "," <<
                // stage
                "," <<
                // keyword
                reinterpret_cast<const char*>(eventData[2].ptr) << std::endl;
            file_out.close();
        }
        else if (strncmp(markerDesc->name, "ShaderVariantCollection.WarmupShaders",37) == 0) {
            warmupShaderCollectionLevel = 0;
            warmupShaderThread = GetThreadId();
        }
        break;
    }
    case kUnityProfilerMarkerEventTypeEnd:
        if (warmupShaderCollectionLevel >= 0 &&
            warmupShaderThread == GetThreadId() ) 
        {
            --warmupShaderCollectionLevel;
        }
        break;

    }
}


extern "C" void UNITY_INTERFACE_EXPORT  _ShaderCompileWatcherForEditorSetupFile(const char* file)
{
    s_filename = file;
    std::ofstream file_out;
    file_out.open(s_filename, std::ios_base::out);
    file_out <<"frameIdx,Shader,exec(ms),isWarmupCall,pass,stage,keyword,"<<std::endl ;
    file_out.close();
}
extern "C" void UNITY_INTERFACE_EXPORT _ShaderCompileWatcherForEditorSetFrame(int idx)
{
    s_frameIndex = idx;
}
extern "C" void UNITY_INTERFACE_EXPORT _ShaderCompileWatcherForEditorSetEnable(bool enable)
{
    s_executeFlag = enable;
}
extern "C" bool UNITY_INTERFACE_EXPORT _ShaderCompileWatcherForEditorGetEnable(bool enable)
{
    return s_executeFlag;
}



static void UNITY_INTERFACE_API SetupCreateMarkerCallback(const UnityProfilerMarkerDesc* markerDesc, void* userData)
{
    s_UnityProfilerCallbacks->RegisterMarkerEventCallback(markerDesc, OnProfilerEvent, NULL);
}

static bool s_IsLoadedPlugin = false;

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces * unityInterfaces)
{
    if (!s_IsLoadedPlugin) {
        s_UnityProfilerCallbacks = unityInterfaces->Get<IUnityProfilerCallbacks>();
        s_UnityProfilerCallbacks->RegisterCreateMarkerCallback(&SetupCreateMarkerCallback, NULL);
        s_IsLoadedPlugin = true;
    }
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
    if (s_IsLoadedPlugin) {
        s_UnityProfilerCallbacks->UnregisterCreateMarkerCallback(&SetupCreateMarkerCallback, NULL);
        s_UnityProfilerCallbacks->UnregisterMarkerEventCallback(NULL, &OnProfilerEvent, NULL);
        s_IsLoadedPlugin = false;
    }
}

