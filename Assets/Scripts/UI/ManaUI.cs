using DG.Tweening;
using TMPro;
using UnityEngine;

public class ManaUI : MonoBehaviour
{
    [SerializeField] private TMP_Text mana;
    [SerializeField] private Color notEnoughColor = new(1f, 0.35f, 0.3f);

    private Color? defaultColor;

    public void UpdateManaText(int currentMana, int maxMana)
    {
        mana.text = currentMana + "/" + maxMana;
        transform.DOKill(true);
        transform.DOPunchScale(Vector3.one * 0.12f, 0.25f, 6, 0.5f);
    }

    public void ShowNotEnough()
    {
        defaultColor ??= mana.color;
        transform.DOKill(true);
        mana.DOKill();
        mana.color = notEnoughColor;
        transform.DOShakePosition(0.4f, new Vector3(12f, 0f, 0f), 20, 0f);
        mana.DOColor(defaultColor.Value, 0.6f).SetDelay(0.3f);
    }
}
