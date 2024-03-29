﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UTJ.VariantLogger
{
    internal class SceneCoverageAnalyzer
    {


        internal struct LoggedSceneInfo
        {
            public string scenePath;
            public int loadFrame;
            public int activeFrame;
        }
        private struct SceneState
        {
            public string scenePath;
            public bool isLoad;
            public bool isActive;
            public int lastActiveFrame;
            public int lastLoadFrame;
            public SceneState(string path,bool load,int frame=0)
            {
                scenePath = path;
                isLoad = load;
                isActive = false;
                lastActiveFrame = -1;
                lastLoadFrame = frame;
            }
        }


        private Dictionary<string, LoggedSceneInfo> loggerInfo;
        private Dictionary<string, SceneState> currentState;

        internal Dictionary<string,LoggedSceneInfo> Result
        {
            get { return loggerInfo; }
        }

        internal int GetActiveFrame(string path)
        {
            LoggedSceneInfo info;
            if (loggerInfo.TryGetValue(path, out info))
            {
                return info.activeFrame;
            }
            return 0;
        }
        internal int GetLoadFrame(string path)
        {
            LoggedSceneInfo info;
            if (loggerInfo.TryGetValue(path, out info))
            {
                return info.loadFrame;
            }
            return 0;
        }


        internal void Execute()
        {
            var dir = EditorVariantLoggerConfig.LogSaveDir.Replace("/logs", "/scenelogs");
            var files = System.IO.Directory.GetFiles(dir);

            foreach (var file in files)
            {
                try
                {
                    ExecuteLog(file);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(file + "\n" + e.ToString());
                }
            }
        }


        private void ExecuteLog(string file)
        {
            if(this.loggerInfo == null)
            {
                this.loggerInfo = new Dictionary<string, LoggedSceneInfo>();
            }
            if(this.currentState == null) {
                this.currentState = new Dictionary<string, SceneState>();
            }
            this.currentState.Clear();
            var lines = File.ReadAllLines(file);
            int lineCount = lines.Length;
            for( int i = 1; i < lineCount; ++i)
            {
                var columns = lines[i].Split(',');
                if (columns.Length < 2) { continue; }
                int frame;
                if( !int.TryParse(columns[0],out frame)) { continue; }
                switch (columns[1])
                {
                    case "start":
                        ExecStartLine(columns, frame);
                        break;
                    case "active":
                        ExecActiveLine(columns, frame);
                        break;
                    case "changeActive":
                        ExecChangeActiveLine(columns, frame);
                        break;
                    case "loadScene":
                        ExecLoadSceneAddLine(columns, frame);
                        break;
                    case "loadSceneAdd":
                        ExecLoadSceneAddLine(columns, frame);
                        break;
                    case "unload":
                        ExecUnloadSceneLine(columns, frame);
                        break;
                    case "end":
                        ExecEndLine(columns, frame);
                        break;
                }
            }
        }

        private void ExecStartLine(string[] columns,int frame)
        {
            for(int i = 2; i < columns.Length; ++i)
            {
                string path = columns[i];
                var state = new SceneState(path, true, frame);
                this.currentState[path] = state;
            }
        }

        private void ExecActiveLine(string[] columns, int frame)
        {
            SceneState state;
            string path = columns[2];
            if( !this.currentState.TryGetValue(path,out state))
            {
                state = new SceneState(path, false, frame);
            }
            state.isActive = true;
            state.lastActiveFrame = frame;

            currentState[path] = state;
        }

        private void ExecChangeActiveLine(string[] columns, int frame)
        {
            // CurrentScene
            SceneState state;
            string path = columns[2];
            if (!string.IsNullOrEmpty(path) &&
                this.currentState.TryGetValue(path, out state))
            {
                if (state.isActive && state.lastActiveFrame >=0)
                {
                    this.AppendLoggedSceneInfo(path, 0, frame - state.lastActiveFrame );
                }
                state.isActive = false;
                state.lastActiveFrame = -1;
                this.currentState[path] = state;
            }

            // nextScene
            SceneState nextState;
            string nextPath = columns[3];
            if (!string.IsNullOrEmpty(nextPath))
            {
                if(!this.currentState.TryGetValue(nextPath, out nextState))
                {
                    nextState = new SceneState(nextPath, false, frame);
                }
                nextState.isActive = true;
                nextState.lastActiveFrame = frame;
                this.currentState[nextPath] = nextState;
            }

        }

        private void ExecLoadSceneLine(string[] columns, int frame)
        {
            SceneState state;
            string path = columns[2];
            if (!this.currentState.TryGetValue(path, out state))
            {
                state = new SceneState(path, true, frame);
            }
            else
            {
                state.isLoad = true;
                state.lastLoadFrame = frame;
            }
            currentState[path] = state;
        }

        private void ExecLoadSceneAddLine(string[] columns, int frame)
        {
            SceneState state;
            string path = columns[2];
            if (!this.currentState.TryGetValue(path, out state))
            {
                state = new SceneState(path, true, frame);
            }
            else
            {
                state.isLoad = true;
                state.lastLoadFrame = frame;
            }
            currentState[path] = state;
        }

        private void ExecUnloadSceneLine(string[] columns, int frame)
        {

            SceneState state;
            string path = columns[2];
            if (this.currentState.TryGetValue(path, out state))
            {
                int loadFrame = 0;
                int activeFrame = 0;
                if(state.lastLoadFrame >= 0)
                {
                    loadFrame = frame - state.lastLoadFrame;
                }
                if (state.lastActiveFrame >= 0)
                {
                    activeFrame = frame - state.lastActiveFrame;
                }
                AppendLoggedSceneInfo(path, loadFrame, activeFrame);

                state.isLoad = false;
                state.lastLoadFrame = -1;
                state.isActive = false;
                state.lastActiveFrame = -1;
            }
            else
            {
                state = new SceneState(path, false, -1);

            }
            this.currentState[path] = state;
        }
        private void ExecEndLine(string[] columns, int frame)
        {
            var keys = new List<string>(this.currentState.Keys);
            foreach( var key in keys)
            {
                var state = this.currentState[key];
                if (state.isActive && state.lastActiveFrame >= 0)
                {
                    AppendLoggedSceneInfo(key, 0,frame - state.lastActiveFrame);
                }
                if (state.isLoad && state.lastLoadFrame >= 0)
                {
                    AppendLoggedSceneInfo(key, frame - state.lastLoadFrame, 0);
                }
                state.isActive = state.isLoad = false;
                state.lastActiveFrame = state.lastLoadFrame = -1;

                this.currentState[key] = state;
            }
        }

        private void AppendLoggedSceneInfo(string path,int loaded,int active)
        {
            if (string.IsNullOrEmpty(path)) { return; }
            LoggedSceneInfo info;
            if( loggerInfo.TryGetValue(path,out info) ){
                info.loadFrame += loaded;
                info.activeFrame += active;
            }
            else
            {
                info = new LoggedSceneInfo();
                info.scenePath = path;
                info.loadFrame = loaded;
                info.activeFrame = active;
            }
            loggerInfo[path] = info;
        }
    }
}