using System;
using System.Collections.Generic;
using UnityEngine;

public class StatRuntime
{
    public event Action<float, float> OnValueChanged;
    
    private List<StatModifier> modifiers;

    private string id;

    private string name;
    
    private float baseValue;
    
    private float hardCap;

    private float cachedValue;
    
    public StatRuntime(Stat statSO)
    {
        modifiers = new List<StatModifier>();
        
        id = statSO.getStatID();
        name = statSO.getStatName();
        baseValue = statSO.getBaseValue();
        hardCap = statSO.getHardCap();

        cachedValue = baseValue;
    }

    public string GetStatID()
    {
        return id;
    }

    public string GetStatName()
    {
        return name;
    }

    public float Recalculate(float maxHealth)
    {
        float oldValue = cachedValue;
        float newValue = baseValue;

        cachedValue = CalculateStatValue(newValue, maxHealth);

        if (!Mathf.Approximately(oldValue, cachedValue))
        {
            OnValueChanged?.Invoke(oldValue, cachedValue);
        }
        
        return cachedValue;
    }
    
    public void AddModifier(StatModifier modifier, float maxHealth)
    {
        modifiers.Add(modifier);

        Recalculate(maxHealth);
    }

    public void RemoveModifier(StatModifier modifier)
    {
        modifiers.Remove(modifier);
    }

    public void ClearModifiers()
    {
        modifiers.Clear();
    }
    
    private float CalculateStatValue(float newValue, float maxHealth)
    {
        float finalValue = newValue;

        foreach (StatModifier modifier in modifiers)
        {
            switch (modifier.GetModifierType())
            {
                case ModifierType.Additive:
                    finalValue += modifier.GetModifierValue();
                    break;
                case ModifierType.Multiplicative:
                    finalValue *= 1.0f + modifier.GetModifierValue();
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
}
