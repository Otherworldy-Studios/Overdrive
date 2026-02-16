using UnityEngine;

public enum ModifierType
{
    Additive,
    Multiplicative
}

[CreateAssetMenu(fileName = "Stat Modifier", menuName = "Stats/Stat Modifier")]
public class StatModifier : ScriptableObject
{
    [SerializeField] private ModifierType modifierType = ModifierType.Additive;

    [SerializeField] private float value;

    public ModifierType GetModifierType()
    {
        return modifierType;
    }

    public float GetModifierValue()
    {
        return value;
    }
}
