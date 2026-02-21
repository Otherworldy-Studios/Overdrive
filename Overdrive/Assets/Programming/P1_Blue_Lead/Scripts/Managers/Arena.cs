using System;
using System.Collections.Generic;
using UnityEngine;

public class Arena : MonoBehaviour
{
    [SerializeField] int arenaNum;
    [SerializeField] private string arenaName;
    [SerializeField] private bool cleared;
    [SerializeField] private List<GameObject> enemies;
    [SerializeField] private ArenaType arenaType;
    [SerializeField] private RewardsPacket rewards;
    [SerializeField] private Door door;
    public event Action<int> OnArenaCleared;
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
            //enemy.OnDeath -= UpdateEnemyCount
            enemies.Remove(deadEnemy);
        }
        if(enemies.Count == 0)
        {
            ClearArena();
        }
    }

    private void ClearArena()
    {
        cleared = true;
        OnArenaCleared?.Invoke(arenaNum);
        rewards?.GiveRewards();
        door.UnlockDoor();
    }

    private void OnDisable()
    {
        OnEnemyDeathPlaceHolder -= UpdateEnemyCount;
    }
}

public enum ArenaType 
{
    CombatArena,
    TerminalArena,
    BossArena
}