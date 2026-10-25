using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private string mainMenuScene = "Main Menu";

    private static PauseMenu openMenu;

    public static bool IsPaused => openMenu != null;

    public static void Open()
    {
        if (openMenu != null) return;
        PauseMenu prefab = Resources.Load<PauseMenu>("PauseMenu");
        if (prefab == null)
        {
            Debug.LogWarning("PauseMenu prefab not found in Resources.");
            return;
        }
        openMenu = Instantiate(prefab);
        Time.timeScale = 0f;
    }

    private void Start()
    {
        resumeButton.onClick.AddListener(Resume);
        settingsButton.onClick.AddListener(SettingsPanel.Open);
        mainMenuButton.onClick.AddListener(GoToMainMenu);
    }

    private int openedFrame;

    private void Awake()
    {
        openedFrame = Time.frameCount;
    }

    private void Update()
    {
        if (Time.frameCount > openedFrame && Input.GetKeyDown(KeyCode.Escape) && !SettingsPanel.IsOpen) Resume();
    }

    public void Resume()
    {
        Destroy(gameObject);
    }

    private void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
        if (GameDataManager.Instance != null) Destroy(GameDataManager.Instance.gameObject);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (openMenu != this) return;
        openMenu = null;
        Time.timeScale = 1f;
    }
}
