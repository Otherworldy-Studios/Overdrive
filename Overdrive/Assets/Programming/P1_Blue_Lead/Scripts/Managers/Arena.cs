using System;
using System.Collections.Generic;
using UnityEngine;

public class Arena : MonoBehaviour
{
    [SerializeField] int arenaNum;
    [SerializeField] private int arenaName;
    [SerializeField] private bool cleared;
    [SerializeField] private List<GameObject> enemies;
    public event Action<int> ArenaCleared;
    private event Action<GameObject> OnEnemyDeathPlaceHolder;

    private void OnEnable()
    {
        foreach (GameObject enemy in enemies)
        {
            OnEnemyDeathPlaceHolder += UpdateEnemyCount;
        }
    }

    private void UpdateEnemyCount(GameObject deadEnemy)
    {
        if(enemies.Contains(deadEnemy))
        {
            enemies.Remove(deadEnemy);
        }
        if(enemies.Count == 0)
        {
            ArenaCleared?.Invoke(arenaNum);
        }
    }
}
