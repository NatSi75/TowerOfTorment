using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DamageSystem : MonoBehaviour
{
    [SerializeField] private GameObject defeatWindow;
    [SerializeField] private GameObject damageVFX;
    void OnEnable()
    {
        ActionSystem.AttachPerformer<DealDamageGA>(DealDamagePerformer);
    }
    void OnDisable()
    {
        ActionSystem.DetachPerformer<DealDamageGA>();
    }
    private IEnumerator DealDamagePerformer(DealDamageGA dealDamageGA)
    {
        foreach (var target in dealDamageGA.Targets)
        {
            float finalDamage = dealDamageGA.Amount;
            if (target == null || target.gameObject == null) continue;
            var caster = dealDamageGA.Caster;

            // For hero
            if (caster != null && caster == HeroSystem.Instance.HeroView)
            {
                float strengthStacks = caster.GetStatusEffectStacks(StatusEffectType.STRENGTH);
                finalDamage += strengthStacks;
                float weakStacks = caster.GetStatusEffectStacks(StatusEffectType.WEAK);
                if (weakStacks > 0)
                {
                    finalDamage *= 0.75f;
                    finalDamage = Mathf.RoundToInt(finalDamage);
                }
                float vulnerableStacks = target.GetStatusEffectStacks(StatusEffectType.VULNERABLE);
                if (vulnerableStacks > 0)
                {
                    finalDamage *= 1.5f;
                    finalDamage = Mathf.RoundToInt(finalDamage);
                }
                float bleedStacks = target.GetStatusEffectStacks(StatusEffectType.BLEED);
                if (bleedStacks > 0)
                {
                    finalDamage += bleedStacks;
                    finalDamage = Mathf.RoundToInt(finalDamage);
                }
            }

            target.Damage(finalDamage);

            if (target != null)
            {
                Instantiate(damageVFX, target.transform.position, Quaternion.identity);
            }

            yield return new WaitForSeconds(0.15f);

            if (target != null && target.CurrentHealth <= 0)
            {
                if (target is EnemyView enemyView)
                {
                    KillEnemyGA killEnemyGA = new(enemyView);
                    ActionSystem.Instance.AddReaction(killEnemyGA);
                }
                else
                {
                    defeatWindow.SetActive(true);
                    yield return new WaitForSeconds(3f);
                    SceneManager.LoadScene("Main Menu");
                    Destroy(GameDataManager.Instance.gameObject);
                    // Do some game over logic here
                    // Open Game Over Scene
                    // Death
                }
            }
        }
    }
   
}
