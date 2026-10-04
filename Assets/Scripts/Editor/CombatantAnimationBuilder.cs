using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.U2D.Sprites;
using UnityEngine;

// Builds Attack / Hurt / Death animations for the heroes and every enemy and adds them to their existing
// Animator Controller (states + triggers named "Attack", "Hurt", "Death").
// Frames are sliced on the same grid as the idle animation and use the idle's foot position as pivot,
// so the character stays in place when it switches animation. Safe to run again after adding sprites.
public static class CombatantAnimationBuilder
{
    private const string EnemyMediaRoot = "Assets/Assets/Media/Enemy/";
    private const string HeroMediaRoot = "Assets/Assets/Media/Characther/";
    private const float FramesPerSecond = 12f;

    private enum SourceKind { Strip, GridRow, Files }

    private class Source
    {
        public SourceKind Kind;
        public string Path;      // texture for Strip / GridRow, folder for Files
        public string Prefix;    // Files: only files starting with this
        public int Row;          // GridRow: row index from the top
        public int Frames;       // Strip: frame count when the sheet's cells differ from the idle cells
    }

    private class EnemyAnimations
    {
        public string EnemyData;    // EnemyData or HeroData asset name
        public bool Hero;
        public Vector2Int GridCell; // only for enemies whose sprites are on one multi-row sheet
        public Source Attack, Hurt, Death;
    }

    private static Source Strip(string path) => new() { Kind = SourceKind.Strip, Path = EnemyMediaRoot + path };
    private static Source Row(string path, int row) => new() { Kind = SourceKind.GridRow, Path = EnemyMediaRoot + path, Row = row };
    private static Source Files(string folder, string prefix = "") => new() { Kind = SourceKind.Files, Path = EnemyMediaRoot + folder, Prefix = prefix };
    private static Source HeroStrip(string path, int frames) => new() { Kind = SourceKind.Strip, Path = HeroMediaRoot + path, Frames = frames };

    private static readonly EnemyAnimations[] Enemies =
    {
        // Heroes
        new() { EnemyData = "Knight", Hero = true, Attack = HeroStrip("Knight/New/Attack 1.png", 5), Hurt = HeroStrip("Knight/New/Hurt.png", 2), Death = HeroStrip("Knight/New/Dead.png", 6) },
        new() { EnemyData = "Wizard", Hero = true, Attack = HeroStrip("Wizard/Attack1.png", 8), Hurt = HeroStrip("Wizard/Hit.png", 4), Death = HeroStrip("Wizard/Death.png", 7) },
        // Act 1
        new() { EnemyData = "Slime Red", Attack = Strip("Act 1/Slime/Red/attack_2.png"), Hurt = Strip("Act 1/Slime/Red/hurt_1.png"), Death = Strip("Act 1/Slime/Red/death.png") },
        new() { EnemyData = "Slime Purple", Attack = Strip("Act 1/Slime/Purple/attack_2.png"), Hurt = Strip("Act 1/Slime/Purple/hurt_1.png"), Death = Strip("Act 1/Slime/Purple/death.png") },
        new() { EnemyData = "Rat", Attack = Strip("Act 1/Rat/attack_bite.png"), Hurt = Strip("Act 1/Rat/hurt.png"), Death = Strip("Act 1/Rat/rat-death.png") },
        new() { EnemyData = "Bat", Attack = Strip("Act 1/Bat/attack.png"), Hurt = Strip("Act 1/Bat/hurt.png"), Death = Strip("Act 1/Bat/death.png") },
        new() { EnemyData = "Flying Eye", Attack = Strip("Act 1/Flying Eye/Attack.png"), Hurt = Strip("Act 1/Flying Eye/Take Hit.png"), Death = Strip("Act 1/Flying Eye/Death.png") },
        new() { EnemyData = "Goblin", Attack = Strip("Act 1/Goblin/Attack.png"), Hurt = Strip("Act 1/Goblin/Take Hit.png"), Death = Strip("Act 1/Goblin/Death.png") },
        new() { EnemyData = "Mushroom", Attack = Strip("Act 1/Mushroom/Attack.png"), Hurt = Strip("Act 1/Mushroom/Take Hit.png"), Death = Strip("Act 1/Mushroom/Death.png") },
        new() { EnemyData = "Skeleton GS", Attack = Files("Act 1/Skeleton GS", "attack-B"), Hurt = Files("Act 1/Skeleton GS", "hit-"), Death = Files("Act 1/Skeleton GS", "dead-") },
        new() { EnemyData = "Skeleton Shield", Attack = Files("Act 1/Skeleton Shield", "attack-B"), Hurt = Files("Act 1/Skeleton Shield", "hit-"), Death = Files("Act 1/Skeleton Shield", "dead-") },
        new() { EnemyData = "Skeleton Spear", Attack = Files("Act 1/Skeleton Spear", "attack-A"), Hurt = Files("Act 1/Skeleton Spear", "hit-"), Death = Files("Act 1/Skeleton Spear", "dead-") },
        new() { EnemyData = "Frost Guardian", Attack = Files("Act 1/Frost Guardian/1_atk"), Hurt = Files("Act 1/Frost Guardian/take_hit"), Death = Files("Act 1/Frost Guardian/death") },
        // Act 2
        new() { EnemyData = "Dog", GridCell = new Vector2Int(64, 64), Attack = Row("Act 2/Dog/Massacre Sprite Sheet.png", 1), Hurt = Row("Act 2/Dog/Massacre Sprite Sheet.png", 2), Death = Row("Act 2/Dog/Massacre Sprite Sheet.png", 3) },
        new() { EnemyData = "Kobold", Attack = Strip("Act 2/Kobold/ATTACK.png") }, // the asset pack has no hurt / death
        new() { EnemyData = "Golem Blue", Attack = Strip("Act 2/Golem Blue/Golem_1_attack.png"), Hurt = Strip("Act 2/Golem Blue/Golem_1_hurt.png"), Death = Strip("Act 2/Golem Blue/Golem_1_die.png") },
        new() { EnemyData = "Golem Orange", Attack = Strip("Act 2/Golem Orange/Golem_1_attack.png"), Hurt = Strip("Act 2/Golem Orange/Golem_1_hurt.png"), Death = Strip("Act 2/Golem Orange/Golem_1_die.png") },
        new() { EnemyData = "Flying Demon", Attack = Strip("Act 2/Flying Demon/ATTACK.png"), Hurt = Strip("Act 2/Flying Demon/HURT.png"), Death = Strip("Act 2/Flying Demon/DEATH.png") },
        new() { EnemyData = "Evil Wizard", Attack = Strip("Act 2/Evil Wizard 3/Attack.png"), Hurt = Strip("Act 2/Evil Wizard 3/Get hit.png"), Death = Strip("Act 2/Evil Wizard 3/Death.png") },
        new() { EnemyData = "Evil Wizard Void", Attack = Strip("Act 2/Evil Wizard 2/Attack2.png"), Hurt = Strip("Act 2/Evil Wizard 2/Take hit.png"), Death = Strip("Act 2/Evil Wizard 2/Death.png") },
        new() { EnemyData = "Evil Wizard Fire", Attack = Strip("Act 2/Evil Wizard/Attack.png"), Hurt = Strip("Act 2/Evil Wizard/Take Hit.png"), Death = Strip("Act 2/Evil Wizard/Death.png") },
        new() { EnemyData = "Harbinger of Death", Attack = Files("Act 2/Harbinger of Death/Attack"), Hurt = Files("Act 2/Harbinger of Death/Hurt"), Death = Files("Act 2/Harbinger of Death/Death") },
        // Phase 1 of the Act 2 boss only buffs, so its "Attack" is the squash/move row.
        new() { EnemyData = "Demon Slime", GridCell = new Vector2Int(32, 32), Attack = Row("Act 2/Demon Slime/Slime_Orange.png", 1), Hurt = Row("Act 2/Demon Slime/Slime_Orange.png", 2), Death = Row("Act 2/Demon Slime/Slime_Orange.png", 3) },
        new() { EnemyData = "Demon", Attack = Files("Act 2/Demon Slime/03_demon_cleave"), Hurt = Files("Act 2/Demon Slime/04_demon_take_hit"), Death = Files("Act 2/Demon Slime/05_demon_death") },
    };

    [MenuItem("Tools/Animations/Build Attack, Hurt and Death Animations")]
    public static void BuildAll()
    {
        Dictionary<string, RuntimeAnimatorController> enemyControllers = AssetDatabase.FindAssets("t:EnemyData")
            .Select(guid => AssetDatabase.LoadAssetAtPath<EnemyData>(AssetDatabase.GUIDToAssetPath(guid)))
            .ToDictionary(d => d.name, d => d.AnimatorController);
        Dictionary<string, RuntimeAnimatorController> heroControllers = AssetDatabase.FindAssets("t:HeroData")
            .Select(guid => AssetDatabase.LoadAssetAtPath<HeroData>(AssetDatabase.GUIDToAssetPath(guid)))
            .ToDictionary(d => d.name, d => d.Animator);

        List<string> report = new();
        foreach (EnemyAnimations enemy in Enemies)
        {
            Dictionary<string, RuntimeAnimatorController> controllers = enemy.Hero ? heroControllers : enemyControllers;
            if (!controllers.TryGetValue(enemy.EnemyData, out RuntimeAnimatorController controller))
            {
                report.Add($"{enemy.EnemyData}: {(enemy.Hero ? "HeroData" : "EnemyData")} not found");
                continue;
            }
            report.Add(Build(enemy, controller as AnimatorController));
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[Combatant Animations]\n" + string.Join("\n", report));
    }

    private static string Build(EnemyAnimations enemy, AnimatorController controller)
    {
        if (controller == null) return $"{enemy.EnemyData}: no Animator Controller";

        AnimationClip idleClip = controller.layers[0].stateMachine.defaultState?.motion as AnimationClip;
        Sprite idleSprite = idleClip != null ? FirstSprite(idleClip) : null;
        if (idleSprite == null) return $"{enemy.EnemyData}: idle animation has no sprites";

        bool oneFramePerFile = new[] { enemy.Attack, enemy.Hurt, enemy.Death }.Any(s => s != null && s.Kind == SourceKind.Files);
        Vector2Int cell = enemy.GridCell != Vector2Int.zero ? enemy.GridCell
            : oneFramePerFile ? new Vector2Int(idleSprite.texture.width, idleSprite.texture.height)
            : IdleCellSize(idleSprite, idleClip);
        // the cell is found from the sprite centre: trimmed idle frames can start a pixel outside their cell
        Vector2 anchor = new(
            idleSprite.rect.center.x - Mathf.Floor(idleSprite.rect.center.x / cell.x) * cell.x,
            idleSprite.rect.yMin - Mathf.Floor(idleSprite.rect.center.y / cell.y) * cell.y);
        Vector2 pivot = new(anchor.x / cell.x, anchor.y / cell.y);
        float pixelsPerUnit = idleSprite.pixelsPerUnit;

        string folder = Path.GetDirectoryName(AssetDatabase.GetAssetPath(controller)).Replace('\\', '/');
        List<string> built = new();
        Dictionary<string, AnimationClip> clips = new();
        foreach ((string state, Source source) in new[] { ("Attack", enemy.Attack), ("Hurt", enemy.Hurt), ("Death", enemy.Death) })
        {
            if (source == null) continue;
            List<Sprite> frames = LoadFrames(source, cell, anchor, pixelsPerUnit, out string error);
            if (frames.Count == 0)
            {
                built.Add($"{state} FAILED ({error})");
                continue;
            }
            clips[state] = SaveClip($"{folder}/{enemy.EnemyData} {state}.anim", frames, loop: false);
            built.Add($"{state} {frames.Count}f");
        }

        AddStatesToController(controller, clips);
        return $"{enemy.EnemyData}: cell {cell.x}x{cell.y}, pivot ({pivot.x:0.###}, {pivot.y:0.###}) -> {string.Join(", ", built)}";
    }

    private static Sprite FirstSprite(AnimationClip clip)
    {
        EditorCurveBinding binding = AnimationUtility.GetObjectReferenceCurveBindings(clip)
            .FirstOrDefault(b => b.type == typeof(SpriteRenderer) && b.propertyName == "m_Sprite");
        ObjectReferenceKeyframe[] keys = AnimationUtility.GetObjectReferenceCurve(clip, binding);
        return keys != null && keys.Length > 0 ? keys[0].value as Sprite : null;
    }

    // Idle sheets are a single row: square cells when they fit, otherwise width / frame count.
    private static Vector2Int IdleCellSize(Sprite idleSprite, AnimationClip idleClip)
    {
        Texture2D texture = idleSprite.texture;
        if (texture.width % texture.height == 0) return new Vector2Int(texture.height, texture.height);

        EditorCurveBinding binding = AnimationUtility.GetObjectReferenceCurveBindings(idleClip).First();
        int frames = AnimationUtility.GetObjectReferenceCurve(idleClip, binding).Select(k => k.value).Distinct().Count();
        return new Vector2Int(texture.width / Mathf.Max(1, frames), texture.height);
    }

    private static List<Sprite> LoadFrames(Source source, Vector2Int cell, Vector2 anchor, float pixelsPerUnit, out string error)
    {
        error = null;
        if (source.Kind == SourceKind.Files)
        {
            List<string> files = Directory.GetFiles(source.Path, "*.png")
                .Select(f => f.Replace('\\', '/'))
                .Where(f => Path.GetFileName(f).StartsWith(source.Prefix))
                .OrderBy(f => int.Parse(Regex.Match(Path.GetFileNameWithoutExtension(f), @"(\d+)$").Value))
                .ToList();
            if (files.Count == 0) error = "no files";
            return files.Select(f => ImportSingle(f, anchor, cell, pixelsPerUnit)).Where(s => s != null).ToList();
        }

        if (!File.Exists(source.Path))
        {
            error = "file missing";
            return new List<Sprite>();
        }
        Texture2D readable = LoadReadable(source.Path);
        Vector2Int idleCell = cell;
        List<RectInt> rects = new();
        if (source.Kind == SourceKind.Strip)
        {
            if (source.Frames > 0)
            {
                cell = new Vector2Int(readable.width / source.Frames, readable.height);
            }
            else if (readable.height != cell.y)
            {
                // a few packs use a different height for some sheets; fall back to square cells
                cell = new Vector2Int(readable.height, readable.height);
            }
            if (readable.width % cell.x != 0)
            {
                error = $"width {readable.width} is not a multiple of {cell.x}";
                return new List<Sprite>();
            }
            for (int x = 0; x < readable.width; x += cell.x)
                rects.Add(new RectInt(x, 0, cell.x, cell.y));
        }
        else
        {
            int y = readable.height - (source.Row + 1) * cell.y;
            for (int x = 0; x + cell.x <= readable.width; x += cell.x)
                rects.Add(new RectInt(x, y, cell.x, cell.y));
        }
        rects = rects.Where(r => !IsEmpty(readable, r)).ToList();
        Object.DestroyImmediate(readable);

        string prefix = source.Kind == SourceKind.GridRow ? $"{Path.GetFileNameWithoutExtension(source.Path)}_r{source.Row}" : Path.GetFileNameWithoutExtension(source.Path);
        return SliceSheet(source.Path, rects, prefix, PivotFor(anchor, idleCell, cell), pixelsPerUnit, source.Kind == SourceKind.GridRow);
    }

    // Pivot that puts the idle foot position at the same place. When a sheet uses a different cell size,
    // keep the same distance from the bottom and from the horizontal centre of the cell.
    private static Vector2 PivotFor(Vector2 anchor, Vector2Int idleCell, Vector2Int cell)
    {
        float x = anchor.x - idleCell.x / 2f + cell.x / 2f;
        return new Vector2(x / cell.x, anchor.y / cell.y);
    }

    private static Texture2D LoadReadable(string path)
    {
        Texture2D texture = new(2, 2);
        texture.LoadImage(File.ReadAllBytes(path));
        return texture;
    }

    private static bool IsEmpty(Texture2D texture, RectInt rect)
    {
        Color32[] pixels = texture.GetPixels32();
        for (int y = rect.yMin; y < rect.yMax; y++)
            for (int x = rect.xMin; x < rect.xMax; x++)
                if (pixels[y * texture.width + x].a > 8) return false;
        return true;
    }

    private static void ApplyCommonSettings(TextureImporter importer, float pixelsPerUnit)
    {
        importer.textureType = TextureImporterType.Sprite;
        importer.filterMode = FilterMode.Point;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
    }

    private static Sprite ImportSingle(string path, Vector2 anchor, Vector2Int idleCell, float pixelsPerUnit)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        ApplyCommonSettings(importer, pixelsPerUnit);
        importer.spriteImportMode = SpriteImportMode.Single;
        TextureImporterSettings settings = new();
        importer.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Custom;
        importer.GetSourceTextureWidthAndHeight(out int width, out int height);
        settings.spritePivot = PivotFor(anchor, idleCell, new Vector2Int(width, height));
        settings.spriteMeshType = SpriteMeshType.FullRect;
        importer.SetTextureSettings(settings);
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    // Slices a sheet into the given cells. Existing sprites with the same name keep their ID, and on a
    // shared multi-row sheet sprites of the other rows are left untouched.
    private static List<Sprite> SliceSheet(string path, List<RectInt> rects, string prefix, Vector2 pivot, float pixelsPerUnit, bool keepOtherSprites)
    {
        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
        ApplyCommonSettings(importer, pixelsPerUnit);
        importer.spriteImportMode = SpriteImportMode.Multiple;
        importer.SaveAndReimport();

        SpriteDataProviderFactories factories = new();
        factories.Init();
        ISpriteEditorDataProvider provider = factories.GetSpriteEditorDataProviderFromObject(importer);
        provider.InitSpriteEditorDataProvider();

        List<SpriteRect> existing = provider.GetSpriteRects().ToList();
        List<SpriteRect> result = keepOtherSprites ? existing.Where(s => !s.name.StartsWith(prefix + "_")).ToList() : new List<SpriteRect>();
        List<string> names = new();
        for (int i = 0; i < rects.Count; i++)
        {
            string name = $"{prefix}_{i}";
            names.Add(name);
            SpriteRect old = existing.FirstOrDefault(s => s.name == name);
            result.Add(new SpriteRect
            {
                name = name,
                spriteID = old != null ? old.spriteID : GUID.Generate(),
                rect = new Rect(rects[i].x, rects[i].y, rects[i].width, rects[i].height),
                alignment = SpriteAlignment.Custom,
                pivot = pivot,
            });
        }
        provider.SetSpriteRects(result.ToArray());

        ISpriteNameFileIdDataProvider nameProvider = provider.GetDataProvider<ISpriteNameFileIdDataProvider>();
        nameProvider?.SetNameFileIdPairs(result.Select(r => new SpriteNameFileIdPair(r.name, r.spriteID)).ToList());

        provider.Apply();
        importer.SaveAndReimport();

        Dictionary<string, Sprite> sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(s => s.name);
        return names.Where(sprites.ContainsKey).Select(n => sprites[n]).ToList();
    }

    private static AnimationClip SaveClip(string path, List<Sprite> frames, bool loop)
    {
        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, path);
        }
        clip.ClearCurves();
        clip.frameRate = FramesPerSecond;

        EditorCurveBinding binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        List<ObjectReferenceKeyframe> keys = frames
            .Select((sprite, i) => new ObjectReferenceKeyframe { time = i / FramesPerSecond, value = sprite })
            .ToList();
        // hold the last frame for a full frame so the clip length matches the frame count
        keys.Add(new ObjectReferenceKeyframe { time = frames.Count / FramesPerSecond, value = frames[frames.Count - 1] });
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys.ToArray());

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    // Any State --trigger--> Attack / Hurt --end--> Idle, Any State --Death--> Death (stays on last frame).
    private static void AddStatesToController(AnimatorController controller, Dictionary<string, AnimationClip> clips)
    {
        AnimatorStateMachine machine = controller.layers[0].stateMachine;
        AnimatorState idle = machine.defaultState;

        foreach (string stateName in new[] { "Attack", "Hurt", "Death" })
        {
            foreach (AnimatorStateTransition transition in machine.anyStateTransitions.Where(t => t.destinationState != null && t.destinationState.name == stateName).ToList())
                machine.RemoveAnyStateTransition(transition);
            foreach (ChildAnimatorState child in machine.states.Where(s => s.state.name == stateName).ToList())
                machine.RemoveState(child.state);
            foreach (AnimatorControllerParameter parameter in controller.parameters.Where(p => p.name == stateName).ToList())
                controller.RemoveParameter(parameter);

            if (!clips.TryGetValue(stateName, out AnimationClip clip)) continue;

            controller.AddParameter(stateName, AnimatorControllerParameterType.Trigger);
            AnimatorState state = machine.AddState(stateName);
            state.motion = clip;

            AnimatorStateTransition enter = machine.AddAnyStateTransition(state);
            enter.AddCondition(AnimatorConditionMode.If, 0, stateName);
            enter.hasExitTime = false;
            enter.duration = 0f;
            enter.canTransitionToSelf = stateName != "Death";

            if (stateName == "Death") continue;
            AnimatorStateTransition back = state.AddTransition(idle);
            back.hasExitTime = true;
            back.exitTime = 1f;
            back.duration = 0f;
        }
        EditorUtility.SetDirty(controller);
    }
}
