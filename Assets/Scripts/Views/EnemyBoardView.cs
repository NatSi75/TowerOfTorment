using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBoardView : MonoBehaviour
{
    [SerializeField] private List<Transform> slots;
    public List<EnemyView> EnemyViews { get; private set; } = new();
    private float strength = 0f;
    public void AddEnemy(EnemyData enemyData)
    {
        if (EnemyViews.Count >= slots.Count)
        {
            return;
        }
        Transform slot = slots[EnemyViews.Count];
        EnemyView enemyView = EnemyViewCreator.Instance.CreateEnemyView(enemyData, slot.position, slot.rotation);
        enemyView.transform.parent = slot;
        EnemyViews.Add(enemyView);
    }

    public IEnumerator RemoveEnemy(EnemyView enemyView)
    {
        if (GameDataManager.Instance != null)
        {
            if (GameDataManager.Instance.currentAct == 2 && GameDataManager.Instance.indexEnemy == 2)
            {
                SetStacksStrength(enemyView.GetStatusEffectStacks(StatusEffectType.STRENGTH));
            }
        }
        EnemyViews.Remove(enemyView);
        enemyView.PrepareForDeath();
        float deathLength = enemyView.PlayAnimation("Death");
        if (deathLength > 0f)
        {
            yield return new WaitForSeconds(deathLength);
            Tween fade = enemyView.spriteRenderer.DOFade(0f, 0.3f);
            yield return fade.WaitForCompletion();
        }
        else
        {
            Tween tween = enemyView.transform.DOScale(Vector3.zero, 0.25f);
            yield return tween.WaitForCompletion();
        }
        Destroy(enemyView.gameObject);
    }
    public void SetStacksStrength(float strengthStacks)
    {
        strength = strengthStacks;
    }

    public float GetStacksStrength()
    {
        return strength;
    }
}
