using UnityEngine;

[CreateAssetMenu(fileName = "Player Stat Block", menuName = "Stats/Player Stat Block")]
public class StatBlock_Player : StatBlock_Base
{
    [SerializeField] private Stat critChance;
    [SerializeField] private Stat critDamage;
    [SerializeField] private Stat dashCharges;
    [SerializeField] private Stat dashCooldown;
    [SerializeField] private Stat reloadSpeedMultiplier;
    [SerializeField] private Stat ultDamageMultiplier;
    [SerializeField] private Stat ultChargeRate;
}
