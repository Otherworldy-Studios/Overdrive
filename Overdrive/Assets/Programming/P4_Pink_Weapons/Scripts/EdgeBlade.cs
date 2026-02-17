using UnityEngine;

public class EdgeBlade : MonoBehaviour
{
    void Start()
    {
        WeaponBase EdgeBlade = new WeaponBase();
        EdgeBlade.IsMelee = true;
        EdgeBlade.PrimaryDamage = 18;
        EdgeBlade.SecondaryDamage = 45;
        EdgeBlade.SecondaryCooldown = 3;
        EdgeBlade.MaxCombo = 4;
    }
}
