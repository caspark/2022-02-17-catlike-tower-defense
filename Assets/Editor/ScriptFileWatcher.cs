using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// A quick and dirty script file watcher for Unity, lightly adapted & cleaned up from
/// https://gist.github.com/Elringus/7c0fcf0fdcaa3d0ffa4a5408209d8f10 and
/// https://forum.unity.com/threads/editor-compile-in-background.627952/
/// </summary>
namespace ScriptFileWatching {
    public class ScriptsConfiguration {
        public string WatchedDirectory { get; set; }

        public string FilePattern { get; set; } = "*.cs";
    }

    /// <summary>
    /// Uses file system watcher to track changes to `.cs` files in the project directory.
    /// </summary>
    public static class ScriptFileWatcher {
        /// <summary>
        /// Invoked when a <see cref="Script"/> asset is created or modified; returns modified script asset path.
        /// </summary>
        public static event Action<string> OnModified;

        private static ConcurrentQueue<string> modifiedScriptPaths = new ConcurrentQueue<string>();
        private static ConcurrentStack<FileSystemWatcher> runningWatchers = new ConcurrentStack<FileSystemWatcher>();

        private static ScriptsConfiguration Config { set; get; }

        /// <summary>
        /// Set to true to enable debug logging.
        /// </summary>
        public static bool Verbose = false;

        [InitializeOnLoadMethod]
        public static void Initialize() {
            if (Verbose) {
                Debug.Log("Initializing script file watcher...");
            }

            StopWatching();
            Config = new ScriptsConfiguration();
            Config.WatchedDirectory = Application.dataPath;

            if (Verbose) {
                Debug.Log("Watching scripts in: " + Config.WatchedDirectory);
            }
            StartWatching();
        }

        private static void StartWatching() {
            EditorApplication.update += Update;
            foreach (var path in FindDirectoriesWithScripts()) {
                WatchDirectory(path);
            }
        }

        private static void StopWatching() {
            EditorApplication.update -= Update;
            foreach (var watcher in runningWatchers) {
                watcher?.Dispose();
            }
            runningWatchers.Clear();
        }

        private static void Update() {
            if (modifiedScriptPaths.Count == 0) {
                return;
            }
            if (!modifiedScriptPaths.TryDequeue(out var fullPath)) {
                return;
            }
            if (!File.Exists(fullPath)) {
                return;
            }
            var basePath = Application.dataPath;
            var projectRelativePath = "Assets" + fullPath.Substring(basePath.Length);
            if (Verbose) {
                Debug.Log($"Detected change to {fullPath} so will now import asset at ${projectRelativePath}");
            }

            AssetDatabase.ImportAsset(projectRelativePath);
            OnModified?.Invoke(projectRelativePath);

            // Required to rebuild script when editor is not in focus, because script view
            // delays rebuild, but delayed call is not invoked while editor is not in focus.
            if (!InternalEditorUtility.isApplicationActive)
                EditorApplication.delayCall?.Invoke();
        }

        private static IReadOnlyCollection<string> FindDirectoriesWithScripts() {
            var result = new List<string>();
            var dataPath = Config.WatchedDirectory;
            if (ContainsScripts(dataPath)) result.Add(dataPath);
            foreach (var path in Directory.GetDirectories(dataPath, "*", SearchOption.AllDirectories))
                if (ContainsScripts(path))
                    result.Add(path);
            return result;

            bool ContainsScripts(string path) => Directory.GetFiles(path, Config.FilePattern, SearchOption.TopDirectoryOnly).Length > 0;
        }

        private static void WatchDirectory(string path) {
            Task.Run(AddWatcher).ContinueWith(DisposeWatcher, TaskScheduler.FromCurrentSynchronizationContext());

            FileSystemWatcher AddWatcher() {
                var watcher = CreateWatcher(path);
                runningWatchers.Push(watcher);
                return watcher;
            }
        }

        private static FileSystemWatcher CreateWatcher(string path) {
            var watcher = new FileSystemWatcher();
            watcher.Path = path;
            watcher.IncludeSubdirectories = false;
            watcher.NotifyFilter = NotifyFilters.LastWrite;
            watcher.Filter = Config.FilePattern;
            watcher.Changed += (_, e) => modifiedScriptPaths.Enqueue(e.FullPath);
            watcher.EnableRaisingEvents = true;
            return watcher;
        }

        private static void DisposeWatcher(Task<FileSystemWatcher> startTask) {
            try {
                var watcher = startTask.Result;
                AppDomain.CurrentDomain.DomainUnload += (EventHandler)((_, __) => { watcher.Dispose(); });
            }
            catch (Exception e) {
                Debug.LogError($"Failed to stop script file watcher: {e.Message}");
            }
        }
    }
}
