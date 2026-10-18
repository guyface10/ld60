using System.IO;
using UnityEditor;
using UnityEngine;

// On Linux, Unity only auto-refreshes when its window regains focus, and focus
// events are unreliable under Wayland/XWayland (e.g. Hyprland). This watches the
// Assets folder for script changes and triggers a refresh directly.
[InitializeOnLoad]
public static class AutoRefreshOnSave
{
    const double DebounceSeconds = 0.3;

    static FileSystemWatcher watcher;
    static volatile bool dirty;
    static double lastChangeTime;

    static AutoRefreshOnSave()
    {
        // watcher = new FileSystemWatcher(Application.dataPath)
        // {
        //     IncludeSubdirectories = true,
        //     NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
        // };
        // watcher.Changed += OnFileEvent;
        // watcher.Created += OnFileEvent;
        // watcher.Deleted += OnFileEvent;
        // watcher.Renamed += OnFileEvent;
        // watcher.EnableRaisingEvents = true;

        // EditorApplication.update += Update;
        // AssemblyReloadEvents.beforeAssemblyReload += Dispose;
        // EditorApplication.quitting += Dispose;
    }

    // Called on a background thread, so only set a flag here.
    static void OnFileEvent(object sender, FileSystemEventArgs e)
    {
        if (IsScriptFile(e.FullPath) || (e is RenamedEventArgs r && IsScriptFile(r.OldFullPath)))
            dirty = true;
    }

    static bool IsScriptFile(string path)
    {
        string ext = Path.GetExtension(path);
        return ext == ".cs" || ext == ".asmdef" || ext == ".asmref";
    }

    static void Update()
    {
        if (dirty)
        {
            // Editors often write a file in several steps; wait for them to settle.
            dirty = false;
            lastChangeTime = EditorApplication.timeSinceStartup;
            return;
        }

        if (lastChangeTime == 0 || EditorApplication.timeSinceStartup - lastChangeTime < DebounceSeconds)
            return;

        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        lastChangeTime = 0;
        AssetDatabase.Refresh();
    }

    static void Dispose()
    {
        if (watcher == null)
            return;
        watcher.EnableRaisingEvents = false;
        watcher.Dispose();
        watcher = null;
    }
}
