using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualTargetSystem : Singleton<ManualTargetSystem>
{
    [SerializeField] private ArrowView arrowView;
    [SerializeField] private LayerMask targetLayerMask;

    private EnemyView highlighted;
    private SpriteRenderer highlight;

    public void StartTargeting(Vector3 startPosition)
    {
        arrowView.gameObject.SetActive(true);
        arrowView.SetupArrow(startPosition);
    }

    public EnemyView EndTargeting(Vector3 endPosition)
    {
        arrowView.gameObject.SetActive(false);
        Highlight(null);
        return EnemyAt(endPosition);
    }

    // The enemy under the arrow glows, so the player sees which one the card will hit.
    private void Update()
    {
        if (!arrowView.gameObject.activeSelf) return;
        Highlight(EnemyAt(MouseUtil.GetMousePositionInWorldSpace()));
    }

    private EnemyView EnemyAt(Vector3 position)
    {
        Collider2D hitCollider = Physics2D.OverlapPoint(position, targetLayerMask);
        if (hitCollider != null && hitCollider.TryGetComponent(out EnemyView enemyView))
        {
            return enemyView;
        }
        return null;
    }

    private void Highlight(EnemyView enemy)
    {
        if (enemy == highlighted) return;
        if (highlight != null) Destroy(highlight.gameObject);
        highlight = null;
        highlighted = enemy;
        if (enemy == null || enemy.spriteRenderer == null) return;

        SpriteRenderer body = enemy.spriteRenderer;
        highlight = GlowEffect.CreateBehind(body.transform, 1.1f, body.sortingOrder - 1, true);
        if (highlight == null) return;
        highlight.sortingLayerID = body.sortingLayerID;
        highlight.color = GlowEffect.Target;
    }
}
