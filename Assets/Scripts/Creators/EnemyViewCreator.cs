using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyViewCreator : Singleton<EnemyViewCreator>
{
    [SerializeField] private EnemyView enemyViewPrefab;
    public EnemyView CreateEnemyView(EnemyData enemyData, Vector3 position, Quaternion rotation)
    {
        EnemyView enemyView = Instantiate(enemyViewPrefab, position, rotation);
        enemyView.spriteRenderer.transform.localScale = new Vector3(enemyData.scaleX, enemyData.scaleY, 1f);
        enemyView.Setup(enemyData);
        return enemyView;
    }
}
