// Editor-only helper for taking thesis screenshots of the Game view.
// Menu: Tools > Skripsi > Capture Game View  /  Toggle Dev Panel
// Screenshots go to <project>/Screenshots. Delete this file when done.
#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ThesisScreenshot
{
    [MenuItem("Tools/Skripsi/Capture Game View")]
    private static void Capture()
    {
        if (!Application.isPlaying) { Debug.LogWarning("Masuk Play Mode dulu."); return; }
        string dir = Path.Combine(Directory.GetCurrentDirectory(), "Screenshots");
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, "shot_" + DateTime.Now.ToString("HHmmss_fff") + ".png");
        ScreenCapture.CaptureScreenshot(path, 1);
        Debug.Log("Screenshot: " + path);
    }

    [MenuItem("Tools/Skripsi/Toggle Dev Panel")]
    private static void ToggleDevPanel()
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name == "DevPanel" && go.scene.IsValid())
            {
                go.SetActive(!go.activeSelf);
                Debug.Log("DevPanel active: " + go.activeSelf);
                return;
            }
        }
        Debug.Log("DevPanel tidak ditemukan.");
    }
}
#endif
