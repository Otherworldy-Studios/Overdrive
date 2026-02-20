using System.Collections.Generic;
using UnityEngine;

public class StatBlockRuntime : MonoBehaviour
{
    [SerializeField] private StatBlock statBlock;
    
    private Dictionary<string, StatRuntime> stats;

    // For Debug Purposes
    [SerializeField] private StatModifier testModifier;
    
    private void Start()
    {
        Initialize();
        
        // For Debug Purposes
        Debug.Log("Current Health: " + GetStatValue("current_health"));

        StatRuntime currentHealthStat = GetStat("current_health");
        
        currentHealthStat.AddModifier(testModifier);
        
        Debug.Log("Current Health: " + GetStatValue("current_health"));
    }

    public void Initialize()
    {
        List<StatRuntime> runtimeStats = StatBlock.InitializeBlock(statBlock.GetStatBlock());

        stats = new Dictionary<string, StatRuntime>();

        foreach (StatRuntime stat in runtimeStats)
        {
            stats.Add(stat.GetStatID(), stat);
        }
    }

    public StatRuntime GetStat(string statID)
    {
        if (stats.TryGetValue(statID, out StatRuntime stat))
        {
            return stat;
        }
        
        Debug.LogWarning("Stat with ID: " + statID + " not found.");

        return null;
    }

    public float GetStatValue(string statID)
    {
        float maxHealth = 0;

        if (statID == "current_health")
        {
            maxHealth = GetStatValue("max_health");
        }
        
        StatRuntime stat = GetStat(statID);
        
        if (stat != null)
        {
            return stat.GetStatValue(maxHealth);
        }

        return 0;
    }
}
