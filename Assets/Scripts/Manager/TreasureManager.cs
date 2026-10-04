using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TreasureManager : MonoBehaviour
{
    [Header("Chest Settings")]
    [SerializeField] private Animator chestAnimator;
    [SerializeField] private string openAnimationTrigger = "Open";

    [Header("UI Reward Settings")]
    [SerializeField] private GameObject rewardPanelObject;
    [SerializeField] private List<PerkChoiceUI> perkSlots;

    private bool isOpened = false;

    private void Awake()
    {
        if (rewardPanelObject != null) rewardPanelObject.SetActive(false);
    }

    public void OnChestClicked()
    {
        if (isOpened) return;
        isOpened = true;
        AudioManager.PlaySfx(Sfx.ChestOpen);

        StartCoroutine(OpenChestRoutine());
    }

    private IEnumerator OpenChestRoutine()
    {
        if (chestAnimator != null)
        {
            chestAnimator.SetTrigger(openAnimationTrigger);
        }

        yield return new WaitForSeconds(1.0f);

        ShowRelicRewards();
    }

    private void ShowRelicRewards()
    {
        if (GameDataManager.Instance == null || GameDataManager.Instance.AllPerkPool.Count == 0)
        {
            SceneManager.LoadScene("Map");
            return;
        }

        List<PerkData> availableRelics = new List<PerkData>();
        foreach (var perk in GameDataManager.Instance.AllPerkPool)
        {
            if (!GameDataManager.Instance.PerkDatas.Contains(perk))
            {
                availableRelics.Add(perk);
            }
        }

        List<PerkData> chosenRelics = new ();
        int countToPick = Mathf.Min(3, availableRelics.Count);

        for (int i = 0; i < countToPick; i++)
        {
            int randomIndex = Random.Range(0, availableRelics.Count);
            chosenRelics.Add(availableRelics[randomIndex]);
            availableRelics.RemoveAt(randomIndex);
        }

        rewardPanelObject.SetActive(true);

        for (int i = 0; i < perkSlots.Count; i++)
        {
            if (i < chosenRelics.Count)
            {
                perkSlots[i].gameObject.SetActive(true);
                perkSlots[i].Setup(chosenRelics[i]);

                int index = i;
                perkSlots[i].SelectButton.onClick.RemoveAllListeners();
                perkSlots[i].SelectButton.onClick.AddListener(() => OnRelicSelected(chosenRelics[index]));
            }
            else
            {
                perkSlots[i].gameObject.SetActive(false);
            }
        }
    }

    // 3. Dipanggil saat Pemain Memilih Salah Satu Relic
    private void OnRelicSelected(PerkData selectedRelic)
    {
        AudioManager.PlaySfx(Sfx.RewardPickup);
        if (GameDataManager.Instance != null)
        {
            if (selectedRelic.Name == "Green Elixir")
            {
                GameDataManager.Instance.IncreaseMaxHP(7);
            }
            if (selectedRelic.Name == "Purple Elixir")
            {
                GameDataManager.Instance.IncreaseMaxHP(10);
            }
            if (selectedRelic.Name == "Crimson Elixir")
            {
                GameDataManager.Instance.IncreaseMaxHP(14);
            }
            if (selectedRelic.Name == "Fallen King�s Crown")
            {
                GameDataManager.Instance.gold += 300;
            }
            GameDataManager.Instance.AddPerk(selectedRelic);
        }

        rewardPanelObject.SetActive(false);
        SceneManager.LoadScene("Map");
    }
}
