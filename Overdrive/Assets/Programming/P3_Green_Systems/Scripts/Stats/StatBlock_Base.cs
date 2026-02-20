using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Base Stat Block", menuName = "Stats/Base Stat Block")]
public class StatBlock_Base : ScriptableObject
{
    [SerializeField] private List<Stat> baseStats;
    [SerializeField] private Stat maxHealth;
    [SerializeField] private Stat currentHealth;
    [SerializeField] private Stat moveSpeedMultiplier;
    [SerializeField] private Stat damageMultiplier;
    [SerializeField] private Stat damageReduction;

    protected List<Stat> GetBaseStats()
    {
        return baseStats;
    }

    public virtual List<Stat> GetStatBlock()
    {
        return baseStats;
    }

    public static List<StatRuntime> InitializeBlock(List<Stat> stats)
    {
        List<StatRuntime> runtimeStats = new();

        foreach (Stat stat in stats)
        {
            StatRuntime runtimeStat = new StatRuntime(stat);
            
            runtimeStats.Add(runtimeStat);
        }
        
        return runtimeStats;
    }
}
