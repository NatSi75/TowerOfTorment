using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Makes the game's TextMeshPro texts easier to read without breaking layouts:
// - one font for the whole game (Kurale), with the material that belongs to it
// - texts may grow up to GrowFactor, but only as far as their own box allows (auto size,
//   minimum = current size), so nothing overflows or becomes smaller than before
// - light texts get a soft dark shadow so they stay readable over busy backgrounds
// Texts that already use auto size are left alone, so running it again changes nothing.
public static class ReadableTextTool
{
    private const string FontPath = "Assets/Assets/UI/Artsystack - Fantasy RPG GUI/ResourcesData/Font/Kurale-Regular SDF.asset";
    private const string ReadableMaterialPath = "Assets/Settings/Text/Kurale-Regular SDF Readable.mat";
    private const float GrowFactor = 1.35f;
    private const float LightTextLuminance = 0.6f;

    [MenuItem("Tools/Responsive/Improve Text Readability")]
    public static void ImproveAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (font == null)
        {
            Debug.LogError("[Readable Text] Font not found: " + FontPath);
            return;
        }
        Material readableMaterial = GetOrCreateReadableMaterial(font);
        List<string> report = new();

        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
            int changed = ImproveTexts(prefabRoot.GetComponentsInChildren<TMP_Text>(true), font, readableMaterial);
            if (changed > 0)
            {
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
                report.Add($"{path}: {changed}");
            }
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        string previousScene = SceneManager.GetActiveScene().path;
        foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
        {
            if (!buildScene.enabled) continue;
            Scene scene = EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
            List<TMP_Text> texts = new();
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                texts.AddRange(root.GetComponentsInChildren<TMP_Text>(true));
            }
            int changed = ImproveTexts(texts, font, readableMaterial);
            if (changed > 0)
            {
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                report.Add($"{scene.name}: {changed}");
            }
        }
        if (!string.IsNullOrEmpty(previousScene))
        {
            EditorSceneManager.OpenScene(previousScene, OpenSceneMode.Single);
        }

        Debug.Log("[Readable Text] Updated texts\n" + string.Join("\n", report));
    }

    private static int ImproveTexts(IEnumerable<TMP_Text> texts, TMP_FontAsset font, Material readableMaterial)
    {
        int changed = 0;
        foreach (TMP_Text text in texts)
        {
            if (text.enableAutoSizing) continue;

            Undo.RecordObject(text, "Readable Text");
            text.font = font;
            text.fontSharedMaterial = IsLight(text.color) ? readableMaterial : font.material;

            float size = text.fontSize;
            text.enableAutoSizing = true;
            text.fontSizeMin = size;
            text.fontSizeMax = Mathf.Round(size * GrowFactor);
            text.fontSize = size;

            EditorUtility.SetDirty(text);
            if (PrefabUtility.IsPartOfPrefabInstance(text))
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(text);
            }
            changed++;
        }
        return changed;
    }

    private static bool IsLight(Color color)
    {
        return 0.2126f * color.r + 0.7152f * color.g + 0.0722f * color.b >= LightTextLuminance;
    }

    private static Material GetOrCreateReadableMaterial(TMP_FontAsset font)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(ReadableMaterialPath);
        if (material != null) return material;

        material = new Material(font.material) { name = "Kurale-Regular SDF Readable" };
        material.SetFloat(ShaderUtilities.ID_FaceDilate, 0.08f);
        material.EnableKeyword(ShaderUtilities.Keyword_Underlay);
        material.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0f, 0f, 0f, 0.8f));
        material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0.35f);
        material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.35f);
        material.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0.25f);
        material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.35f);

        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(ReadableMaterialPath));
        AssetDatabase.CreateAsset(material, ReadableMaterialPath);
        AssetDatabase.SaveAssets();
        return material;
    }
}
