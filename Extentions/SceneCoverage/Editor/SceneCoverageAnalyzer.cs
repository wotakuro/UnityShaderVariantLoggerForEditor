using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UTJ.VariantLogger
{
    internal class SceneCoverageAnalyzer : MonoBehaviour
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

        public void Execute()
        {
            var dir = EditorVariantLoggerConfig.SaveDir.Replace("/logs", "/scenelogs");
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
            SceneState currentState;
            string currentPath = columns[2];
            if (!string.IsNullOrEmpty(currentPath) &&
                this.currentState.TryGetValue(currentPath, out currentState))
            {
                if (currentState.isActive && currentState.lastActiveFrame >=0)
                {
                    this.AppendLoggedSceneInfo(currentPath, 0, currentState.lastActiveFrame - frame);
                }
                currentState.isActive = false;
                currentState.lastActiveFrame = -1;
            }
            // CurrentScene
            SceneState nextState;
            string nextPath = columns[2];
            if (!string.IsNullOrEmpty(nextPath))
            {
                if(!this.currentState.TryGetValue(nextPath, out nextState))
                {
                    nextState = new SceneState(nextPath, false, frame);
                }
                nextState.isActive = true;
                nextState.lastActiveFrame = frame;
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
            if (!this.currentState.TryGetValue(path, out state))
            {
                state = new SceneState(path, false, -1);
            }
            else
            {
                if(state.lastLoadFrame >= 0)
                {
                    AppendLoggedSceneInfo(path, frame - state.lastLoadFrame, 0);
                }
                state.isLoad = false;
                state.lastLoadFrame = -1;
            }
        }
        private void ExecEndLine(string[] columns, int frame)
        {
            var keys = this.currentState.Keys;
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
            LoggedSceneInfo info;
            if( loggerInfo.TryGetValue(path,out info) ){
                info.loadFrame += loaded;
                info.activeFrame = active;
            }
            else
            {
                info = new LoggedSceneInfo();
                info.scenePath = path;
                info.loadFrame = loaded;
                info.loadFrame = active;
            }
            loggerInfo[path] = info;
        }
    }
}