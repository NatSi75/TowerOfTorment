using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Applies the responsive setup (canvas scaling, camera fitting, background cover and the
// Responsive WebGL template) to every scene in Build Settings. Safe to run more than once,
// e.g. after adding a new scene.
public static class ResponsiveSetupTool
{
    private const string WebGLTemplate = "PROJECT:Responsive";
    private static readonly Vector2 ReferenceResolution = new(1920f, 1080f);

    [MenuItem("Tools/Responsive/Setup All Build Scenes")]
    public static void SetupAllBuildScenes()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string previousScene = SceneManager.GetActiveScene().path;
        List<string> report = new();

        foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
        {
            if (!buildScene.enabled) continue;
            Scene scene = EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
            report.Add(SetupScene(scene));
            EditorSceneManager.SaveScene(scene);
        }

        SetupPlayerSettings();
        report.Add("WebGL template: " + PlayerSettings.WebGL.template + ", orientation: landscape only");

        if (!string.IsNullOrEmpty(previousScene))
        {
            EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);
        }

        Debug.Log("[Responsive] Setup finished\n" + string.Join("\n", report));
    }

    public static void SetupPlayerSettings()
    {
        PlayerSettings.WebGL.template = WebGLTemplate;

        // Landscape only (used by native mobile builds; the web template handles browsers).
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        AssetDatabase.SaveAssets();
    }

    public static string SetupScene(Scene scene)
    {
        int canvases = 0, cameras = 0, backgrounds = 0;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (CanvasScaler scaler in root.GetComponentsInChildren<CanvasScaler>(true))
            {
                Canvas canvas = scaler.GetComponent<Canvas>();
                if (canvas == null || !canvas.isRootCanvas || canvas.renderMode == RenderMode.WorldSpace) continue;

                Undo.RecordObject(scaler, "Responsive Canvas");
                if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = ReferenceResolution;
                }
                scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                EditorUtility.SetDirty(scaler);
                canvases++;
            }

            foreach (Camera cam in root.GetComponentsInChildren<Camera>(true))
            {
                if (!cam.orthographic || cam.targetTexture != null) continue;

                ResponsiveCamera responsiveCamera = cam.GetComponent<ResponsiveCamera>();
                if (responsiveCamera == null)
                {
                    responsiveCamera = Undo.AddComponent<ResponsiveCamera>(cam.gameObject);
                    responsiveCamera.SetDesignOrthographicSize(cam.orthographicSize);
                    EditorUtility.SetDirty(responsiveCamera);
                }
                cameras++;
            }

            foreach (SpriteRenderer spriteRenderer in root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (spriteRenderer.name.Trim().ToLowerInvariant() != "background") continue;
                if (spriteRenderer.GetComponentInParent<Canvas>(true) != null) continue;

                if (spriteRenderer.GetComponent<ResponsiveBackground>() == null)
                {
                    Undo.AddComponent<ResponsiveBackground>(spriteRenderer.gameObject);
                }
                backgrounds++;
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        return $"{scene.name}: {canvases} canvas, {cameras} camera, {backgrounds} background";
    }
}
