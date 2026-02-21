using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityStats : MonoBehaviour
{
    public event Action<GameObject> OnDeath;
    
    [SerializeField] private StatBlock statBlock;
    
    private Dictionary<string, StatRuntime> stats;

    private void Start()
    {
        Initialize();
        
        StatRuntime currentHealth = GetStat("current_health");
        
        if (currentHealth != null)
        {
            currentHealth.OnValueChanged += OnHealthChanged;
        }
    }

    private void OnDestroy()
    {
        StatRuntime currentHealth = GetStat("current_health");
        
        if (currentHealth != null)
        {
            currentHealth.OnValueChanged -= OnHealthChanged;
        }
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
            return stat.Recalculate(maxHealth);
        }

        return 0;
    }

    public void AddModifierToStat(string statID, StatModifier modifier)
    {
        float maxHealth = 0;

        if (statID == "current_health")
        {
            maxHealth = GetStatValue("max_health");
        }
        
        StatRuntime stat = GetStat(statID);

        stat?.AddModifier(modifier, maxHealth);
    }

    private void OnHealthChanged(float oldValue, float newValue)
    {
        if (oldValue > 0 && newValue <= 0)
        {
            OnDeath?.Invoke(gameObject);
        }
    }
}
