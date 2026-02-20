using System.Collections.Generic;
using UnityEngine;

public class StatRuntime : MonoBehaviour
{
    private float currentValue;
    
    private float hardCap;
    
    private Stat statSO;
    
    private List<StatModifier> modifiers;

    public StatRuntime(Stat statSO)
    {
        this.statSO = statSO;

        currentValue = statSO.getBaseValue();
        hardCap = statSO.getHardCap();
    }
    
    private void OnEnable()
    {
        modifiers = new List<StatModifier>();
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

    public float GetStatValue()
    {
        float finalValue = currentValue;

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
        }

        currentValue = finalValue;

        return finalValue;
    }
}
