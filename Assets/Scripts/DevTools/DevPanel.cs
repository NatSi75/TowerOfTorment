// ============================================================================================
//  DEVELOPER DEBUG PANEL (for demos only)
//
//  Self-contained: it creates itself when the game starts and no other script or scene refers to it.
//  To remove it after the demo, delete the folder  Assets/Scripts/DevTools  (this file + its .meta).
//  To keep the file but turn the panel off, set Enabled to false.
//
//  Open / close: the small "DEV" button at the top of the screen, or F1 / the ` key.
// ============================================================================================
using UnityEngine;
using UnityEngine.SceneManagement;

public class DevPanel : MonoBehaviour
{
    private const bool Enabled = true;

    private static DevPanel instance;

    private bool open;
    private bool oneHitEnemies;
    private Rect windowRect = new(20f, 110f, 430f, 10f);
    private Vector2 scroll;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (!Enabled || instance != null) return;
        GameObject go = new("DevPanel");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<DevPanel>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.BackQuote)) open = !open;

        // one hit: every enemy is kept at 1 HP, so any damage kills it
        if (oneHitEnemies && InBattle() && EnemySystem.Instance != null && EnemySystem.Instance.Enemies != null)
        {
            foreach (EnemyView enemy in EnemySystem.Instance.Enemies)
            {
                if (enemy == null || enemy.CurrentHealth <= 1) continue;
                enemy.CurrentHealth = 1;
                enemy.UpdateHealthText();
            }
        }
    }

    // ---------------------------------------------------------------- GUI

    private void OnGUI()
    {
        // design for 1080p and scale with the screen, so it's usable on any resolution
        float scale = Screen.height / 1080f;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));
        float width = Screen.width / scale;

        GUI.depth = -1000;
        if (GUI.Button(new Rect(width / 2f - 45f, 4f, 90f, 34f), open ? "DEV ▲" : "DEV ▼")) open = !open;
        if (!open) return;

        windowRect.height = 0f; // let GUILayout size it
        windowRect = GUILayout.Window(918273, windowRect, DrawWindow, "Developer Panel (demo only)");
    }

    private void DrawWindow(int id)
    {
        GUILayout.BeginVertical();
        GameDataManager data = GameDataManager.Instance;
        if (data == null)
        {
            GUILayout.Label("Start a run (pick a character) to use the panel.");
            GUILayout.EndVertical();
            GUI.DragWindow();
            return;
        }

        scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(600f));

        // ---- status
        GUILayout.Label($"Scene: {SceneManager.GetActiveScene().name}   Act: {data.currentAct}");
        GUILayout.Label($"HP: {CurrentHp():0}/{MaxHp()}   Torment: {CurrentTorment()}/{MaxTorment()}   Gold: {data.gold}");

        // ---- battle
        Header("Battle");
        if (InBattle())
        {
            oneHitEnemies = GUILayout.Toggle(oneHitEnemies, " One hit enemies (enemies stay at 1 HP)");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Kill all enemies (win)")) KillAllEnemies();
            if (GUILayout.Button("+3 Mana")) GainMana(3);
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.Label("(only in a battle)");
        }

        // ---- hero
        Header("Hero");
        Row("HP", ("-10", () => ChangeHp(-10)), ("+10", () => ChangeHp(10)), ("Full", () => ChangeHp(99999)));
        Row("Torment", ("-5", () => ChangeTorment(-5)), ("+5", () => ChangeTorment(5)), ("0", () => ChangeTorment(-99999)), ("Max", () => ChangeTorment(99999)));
        Row("Gold", ("-100", () => ChangeGold(-100)), ("+100", () => ChangeGold(100)), ("+500", () => ChangeGold(500)));

        // ---- skip area
        Header("Skip area");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Leave room → Map")) GoTo("Map");
        if (GUILayout.Button("Go to Act 2")) GoToAct2();
        GUILayout.EndHorizontal();
        GUILayout.Label("Jump to a room (comes back to the same map spot):");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Minor")) StartBattle(0);
        if (GUILayout.Button("Elite")) StartBattle(1);
        if (GUILayout.Button("Boss")) StartBattle(2);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Shop")) GoTo("Shop");
        if (GUILayout.Button("Rest")) GoTo("RestSite");
        if (GUILayout.Button("Treasure")) GoTo("Treasure");
        if (GUILayout.Button("Pillar")) GoTo("Pillar of Despair");
        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();
        GUILayout.Label("F1 / ` to toggle.  Delete Assets/Scripts/DevTools to remove.");
        GUILayout.EndVertical();
        GUI.DragWindow();
    }

    private static void Header(string title)
    {
        GUILayout.Space(6f);
        GUILayout.Label("— " + title + " —");
    }

    private static void Row(string label, params (string text, System.Action action)[] buttons)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, GUILayout.Width(70f));
        foreach (var (text, action) in buttons)
        {
            if (GUILayout.Button(text)) action();
        }
        GUILayout.EndHorizontal();
    }

    // ---------------------------------------------------------------- state helpers
    // In a battle the hero keeps its own HP / torment (written back to GameDataManager when the battle is won);
    // outside a battle GameDataManager holds them.

    private static bool InBattle()
    {
        return SceneManager.GetActiveScene().name == "Battle" && Hero() != null;
    }

    private static HeroView Hero()
    {
        if (HeroSystem.Instance == null) return null;
        HeroView hero = HeroSystem.Instance.HeroView;
        return hero != null && hero.gameObject.activeInHierarchy ? hero : null;
    }

    private static float CurrentHp() => InBattle() ? Hero().CurrentHealth : GameDataManager.Instance.CurrentHeroHP;
    private static int MaxHp() => InBattle() ? Hero().MaxHealth : GameDataManager.Instance.MaxHeroHP;
    private static int MaxTorment() => GameDataManager.Instance.heroData != null ? GameDataManager.Instance.heroData.MaxTorment : 0;
    private static int CurrentTorment() => InBattle() ? Mathf.Min(Hero().savedTorment, Hero().savedMaxTorment) : GameDataManager.Instance.torment;

    private static void ChangeHp(int amount)
    {
        if (InBattle())
        {
            HeroView hero = Hero();
            hero.CurrentHealth = Mathf.Clamp(hero.CurrentHealth + amount, 1, hero.MaxHealth);
            hero.UpdateHealthText();
        }
        else
        {
            GameDataManager data = GameDataManager.Instance;
            data.CurrentHeroHP = Mathf.Clamp(data.CurrentHeroHP + amount, 1, data.MaxHeroHP);
        }
    }

    private static void ChangeTorment(int amount)
    {
        if (InBattle())
        {
            HeroView hero = Hero();
            int current = Mathf.Min(hero.savedTorment, hero.savedMaxTorment);
            int target = Mathf.Clamp(current + amount, 0, hero.savedMaxTorment);
            hero.savedTorment = current;
            hero.UpdateTormentText(target - current);
            hero.RefreshTormentStatus(true); // the Torment status follows right away
        }
        else
        {
            GameDataManager.Instance.AddTorment(amount);
        }
    }

    private static void ChangeGold(int amount)
    {
        GameDataManager data = GameDataManager.Instance;
        data.gold = Mathf.Max(0, data.gold + amount);
    }

    private static void GainMana(int amount)
    {
        if (ManaSystem.Instance != null) ManaSystem.Instance.GainMana(amount);
    }

    private static void KillAllEnemies()
    {
        if (ActionSystem.Instance == null || ActionSystem.Instance.IsPerforming) return;
        if (EnemySystem.Instance == null || EnemySystem.Instance.Enemies == null || EnemySystem.Instance.Enemies.Count == 0) return;
        // through the normal damage flow, so the victory and reward screens run as usual
        DealDamageGA kill = new(99999, new(EnemySystem.Instance.Enemies), null) { IsRaw = true, IgnoresArmor = true };
        ActionSystem.Instance.Perform(kill);
    }

    // ---------------------------------------------------------------- navigation

    // Keeps what happened in the current battle (HP / torment) before leaving it.
    private static void SaveBattleState()
    {
        if (!InBattle()) return;
        HeroView hero = Hero();
        GameDataManager.Instance.CurrentHeroHP = hero.CurrentHealth;
        GameDataManager.Instance.torment = Mathf.Min(hero.savedTorment, hero.savedMaxTorment);
    }

    private static void GoTo(string sceneName)
    {
        SaveBattleState();
        Time.timeScale = 1f;
        TopBarManager.isViewOnlyMode = false;
        SceneManager.LoadScene(sceneName);
    }

    private static void StartBattle(int enemyType)
    {
        GameDataManager data = GameDataManager.Instance;
        switch (enemyType)
        {
            case 1: data.SetEliteEnemy(); break;
            case 2: data.SetBossEnemy(); break;
            default: data.SetSequentialMinorEnemy(); break;
        }
        GoTo("Battle");
    }

    private static void GoToAct2()
    {
        // MapManager builds the Act 2 map when it has no saved map and the last fight was a boss
        GameDataManager data = GameDataManager.Instance;
        data.currentAct = 2;
        data.indexEnemy = 2;
        PlayerPrefs.DeleteKey("Map");
        GoTo("Map");
    }
}
