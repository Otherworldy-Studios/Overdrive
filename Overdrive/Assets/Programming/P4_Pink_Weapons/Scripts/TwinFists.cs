using UnityEngine;

public class TwinFists : MonoBehaviour
{
    void Start()
    {
        WeaponBase TwinFists = new WeaponBase();
        TwinFists.IsMelee = true;
        TwinFists.PrimaryDamage = 11;
        TwinFists.PrimaryCooldown = .2f;
        TwinFists.SecondaryDamage = 50;
        TwinFists.SecondaryCooldown = 3;
        TwinFists.MaxCombo = -1;
    }
}
