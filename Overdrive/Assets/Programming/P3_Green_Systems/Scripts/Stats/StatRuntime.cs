using System.Collections.Generic;
using UnityEngine;

public class StatRuntime
{
    private string id;

    private string name;
    
    private float baseValue;
    
    private float hardCap;
    
    private Stat statSO;
    
    private List<StatModifier> modifiers;

    public StatRuntime(Stat statSO, float customHardCap = 0)
    {
        modifiers = new List<StatModifier>();
        
        this.statSO = statSO;

        id = statSO.getStatID();
        name = statSO.getStatName();
        baseValue = statSO.getBaseValue();
        hardCap = statSO.getHardCap();
    }

    public string GetStatID()
    {
        return id;
    }

    public float GetStatValue(float maxHealth)
    {
        float finalValue = baseValue;

        foreach (StatModifier modifier in modifiers)
        {
            switch (modifier.GetModifierType())
            {
                case ModifierType.Additive:
                    finalValue += modifier.GetModifierValue();
                    break;
                case ModifierType.Multiplicative:
                    finalValue *= (1.0f + modifier.GetModifierValue());
                    break;
            }
        }

        if (hardCap > 0)
        {
            finalValue = Mathf.Min(finalValue, hardCap);
        } else if (id == "current_health")
        {
            finalValue = Mathf.Min(finalValue, maxHealth);
        }

        return finalValue;
    }
    
    public void AddModifier(StatModifier modifier)
    {
        modifiers.Add(modifier);
    }

    public void RemoveModifier(StatModifier modifier)
    {
        modifiers.Remove(modifier);
    }

    public void ClearModifiers()
    {
        modifiers.Clear();
    }
}
