using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManualTargetSystem : Singleton<ManualTargetSystem>
{
    [SerializeField] private ArrowView arrowView;
    [SerializeField] private LayerMask targetLayerMask;

    public void StartTargeting(Vector3 startPosition)
    {
        arrowView.gameObject.SetActive(true);
        arrowView.SetupArrow(startPosition);
    }

    public EnemyView EndTargeting(Vector3 endPosition)
    {
        arrowView.gameObject.SetActive(false);
        Collider2D hitCollider = Physics2D.OverlapPoint(endPosition, targetLayerMask);
        if (hitCollider != null && hitCollider.TryGetComponent(out EnemyView enemyView))
        {
            return enemyView;
        } 
            return null;
    }
}
