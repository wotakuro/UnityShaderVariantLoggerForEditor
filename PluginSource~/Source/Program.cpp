// dllmain.cpp : DLL アプリケーションのエントリ ポイントを定義します。
#include "UnityPluginHeaders/IUnityProfilerCallbacks.h"
#include <iostream>
#include <fstream>

static IUnityProfilerCallbacks* s_UnityProfilerCallbacks = NULL;

static bool s_executeFlag = true;
static int s_frameIndex = 0;
static std::string s_filename = "Variant.log";
static int warmupShaderCollectionLevel = -1;

static void UNITY_INTERFACE_API OnProfilerEvent(const UnityProfilerMarkerDesc* markerDesc, UnityProfilerMarkerEventType eventType, unsigned short eventDataCount, const UnityProfilerMarkerData* eventData, void* userData)
{
    if (!s_executeFlag) { return; }
    switch (eventType)
    {
    case kUnityProfilerMarkerEventTypeBegin:
    {
        if (warmupShaderCollectionLevel >= 0) {
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
        }
        break;
    }
    case kUnityProfilerMarkerEventTypeEnd:
        --warmupShaderCollectionLevel;
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

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces * unityInterfaces)
{

    s_UnityProfilerCallbacks = unityInterfaces->Get<IUnityProfilerCallbacks>();
    s_UnityProfilerCallbacks->RegisterCreateMarkerCallback(&SetupCreateMarkerCallback, NULL);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnload()
{
    s_UnityProfilerCallbacks->UnregisterCreateMarkerCallback(&SetupCreateMarkerCallback, NULL);
    s_UnityProfilerCallbacks->UnregisterMarkerEventCallback(NULL, &OnProfilerEvent, NULL);
}

