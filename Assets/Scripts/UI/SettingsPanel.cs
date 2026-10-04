using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Volume settings window. The prefab lives at Resources/SettingsPanel; open it with SettingsPanel.Open().
public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TMP_Text bgmValueText;
    [SerializeField] private TMP_Text sfxValueText;
    [SerializeField] private Button closeButton;

    private static SettingsPanel openPanel;

    public static void Open()
    {
        if (openPanel != null) return;
        SettingsPanel prefab = Resources.Load<SettingsPanel>("SettingsPanel");
        if (prefab == null)
        {
            Debug.LogWarning("SettingsPanel prefab not found in Resources.");
            return;
        }
        openPanel = Instantiate(prefab);
    }

    private void Start()
    {
        bgmSlider.SetValueWithoutNotify(AudioManager.Instance.BgmVolume);
        sfxSlider.SetValueWithoutNotify(AudioManager.Instance.SfxVolume);
        UpdateLabels();

        bgmSlider.onValueChanged.AddListener(value =>
        {
            AudioManager.Instance.SetBgmVolume(value);
            UpdateLabels();
        });
        sfxSlider.onValueChanged.AddListener(value =>
        {
            AudioManager.Instance.SetSfxVolume(value);
            UpdateLabels();
        });
        closeButton.onClick.AddListener(Close);
    }

    private void UpdateLabels()
    {
        bgmValueText.text = Mathf.RoundToInt(bgmSlider.value * 100f) + "%";
        sfxValueText.text = Mathf.RoundToInt(sfxSlider.value * 100f) + "%";
    }

    public void Close()
    {
        PlayerPrefs.Save();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (openPanel == this) openPanel = null;
    }
}
