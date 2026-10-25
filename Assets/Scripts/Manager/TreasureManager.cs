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
    [SerializeField] private List<RelicChoiceUI> relicSlots;

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
        if (GameDataManager.Instance == null || GameDataManager.Instance.AllRelicPool.Count == 0)
        {
            SceneManager.LoadScene("Map");
            return;
        }

        List<RelicData> availableRelics = new List<RelicData>();
        foreach (var relic in GameDataManager.Instance.AllRelicPool)
        {
            if (!GameDataManager.Instance.RelicDatas.Contains(relic))
            {
                availableRelics.Add(relic);
            }
        }

        List<RelicData> chosenRelics = new ();
        int countToPick = Mathf.Min(3, availableRelics.Count);

        for (int i = 0; i < countToPick; i++)
        {
            int randomIndex = Random.Range(0, availableRelics.Count);
            chosenRelics.Add(availableRelics[randomIndex]);
            availableRelics.RemoveAt(randomIndex);
        }

        rewardPanelObject.SetActive(true);

        for (int i = 0; i < relicSlots.Count; i++)
        {
            if (i < chosenRelics.Count)
            {
                relicSlots[i].gameObject.SetActive(true);
                relicSlots[i].Setup(chosenRelics[i]);

                int index = i;
                relicSlots[i].SelectButton.onClick.RemoveAllListeners();
                relicSlots[i].SelectButton.onClick.AddListener(() => OnRelicSelected(chosenRelics[index]));
            }
            else
            {
                relicSlots[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnRelicSelected(RelicData selectedRelic)
    {
        AudioManager.PlaySfx(Sfx.RewardPickup);
        if (GameDataManager.Instance != null)
        {
            GameDataManager.Instance.AddRelic(selectedRelic);
        }

        rewardPanelObject.SetActive(false);
        SceneManager.LoadScene("Map");
    }
}
