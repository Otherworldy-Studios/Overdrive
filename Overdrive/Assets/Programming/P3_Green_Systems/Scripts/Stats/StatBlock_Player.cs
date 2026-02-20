using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player Stat Block", menuName = "Stats/Player Stat Block")]
public class StatBlock_Player : StatBlock_Base
{
    [SerializeField] private List<Stat> playerStats;
    [SerializeField] private Stat critChance;
    [SerializeField] private Stat critDamage;
    [SerializeField] private Stat dashCharges;
    [SerializeField] private Stat dashCooldown;
    [SerializeField] private Stat reloadSpeedMultiplier;
    [SerializeField] private Stat ultDamageMultiplier;
    [SerializeField] private Stat ultChargeRate;

    public override List<Stat> GetStatBlock()
    {
        foreach (Stat stat in GetBaseStats())
        {
            playerStats.Add(stat);
        }
        
        return playerStats;
    }
}
