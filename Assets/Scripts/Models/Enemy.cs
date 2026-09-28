using SerializeReferenceEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Enemy
{
    [field: SerializeReference] public List<EnemyData> ListEnemyData { get; set; }
}
