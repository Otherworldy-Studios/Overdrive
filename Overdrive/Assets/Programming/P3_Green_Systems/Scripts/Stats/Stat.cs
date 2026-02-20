using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Stat", menuName = "Stats/Stat")]
public class Stat : ScriptableObject
{
    [SerializeField] private string statName = "New Stat";

    [SerializeField] private float baseValue = 1.0f;

    [SerializeField] private float hardCap = 10.0f;

    private List<StatModifier> modifiers;

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
        }

        return finalValue;
    }
}
