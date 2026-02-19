using UnityEngine;

namespace Overdrive.Enemies
{
    /// <summary>
    /// This is a data container for the stat values of each tier.
    /// Specifying all of the different tiers and their values here so that balancing is more modular and easier later on.
    /// Plz use this to balance in the assets section rather than touching the code and potentially breaking stuff.
    /// </summary>
    [CreateAssetMenu(
        fileName = "SO_EnemyTierStats",
        menuName = "Overdrive/Enemy Stats/Enemy Tier Stats",
        order = 10)]

    public sealed class SO_EnemyTierStats : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("This represents the tier of the enemy.")]
        [SerializeField] private EnemyTier tier = EnemyTier.Minor;

        [Header("Core HP")]
        [Tooltip("This is the Max HP value for this tier of enemy.")]
        [SerializeField] private float maxHP = 25f;

        [Tooltip("This is the minimum damage value for this tier of enemy.")]
        [SerializeField] private int damageMin = 10;

        [Tooltip("This is the maximum damage value for this tier of enemy.")]
        [SerializeField] private int damageMax = 12;

        [Tooltip("NavMeshAgent's movement speed value for this tier of enemy.")]
        [SerializeField] private float moveSpeed = 4.0f;

        /// <summary>
        //Prep so it isnt cringe having to come back and edit and add a bunch of stuff to this once I start working on the economy stuff for the enemies.
        /// </summary>
        [Header("Economy")]
        [Tooltip("Chances are 0 to 1 in terms of dropping rewards")]
        [Range(0f, 1f)]
        [SerializeField] private float dropChance = 0.15f;

        [Tooltip("Minimum number of credits dropped by this tier of enemy.")]
        [SerializeField] private int moneyMin = 5;

        [Tooltip("Maximum number of credits dropped by this tier of enemy.")]
        [SerializeField] private int moneyMax = 10;

        public EnemyTier Tier => tier;
        public float MaxHP => maxHP;
        public int DamageMin => damageMin;
        public int DamageMax => damageMax;
        public float MoveSpeed => moveSpeed;
        public float DropChance => dropChance;
        public int MoneyMin => moneyMin;
        public int MoneyMax => moneyMax;
        
    }

}
