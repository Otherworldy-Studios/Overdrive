using UnityEngine;

[CreateAssetMenu(fileName = "Base Stat Block", menuName = "Stats/Base Stat Block")]
public class StatBlock_Base : ScriptableObject
{
    [SerializeField] private Stat maxHealth;
    [SerializeField] private Stat currentHealth;
    [SerializeField] private Stat moveSpeedMultiplier;
    [SerializeField] private Stat damageMultiplier;
    [SerializeField] private Stat damageReduction;
}
