using UnityEngine;

namespace Overdrive.Enemies
{
    /// <summary>
    /// This provides the tuning for the different tiers of enemies.
    /// Elite enemies are made using multiplier rather than hard values so they scale consistantly.
    
        [CreateAssetMenu(
        fileName = "SO_EnemyTierDatabase",
        menuName = "Overdrive/Enemies/Enemy Tier Database",
        order = 11)]
        public sealed class SO_EnemyTierDatabase : ScriptableObject
        {
        [Header("Direct Tier Stats")]
        [Tooltip("Stats for Minor tier.")]
        [SerializeField] private SO_EnemyTierStats minor;

        [Tooltip("Stats for Standard tier.")]
        [SerializeField] private SO_EnemyTierStats standard;

        [Header("Elite Multipliers")]
        [Tooltip("HP Multiplier applied to Elite")]
        [SerializeField] private float eliteHpMultiplier = 2.2f;

        [Tooltip("Elite damage multiplier")]
        [SerializeField] private float eliteDamageMultiplier = 1.6f;

        [Tooltip("Elite move speed multiplier")]
        [SerializeField] private float eliteSpeedMultiplier = 1.1f;

        /// <summary>
        //Adding some economy stuff here to stay consistent and make sure that setting it all up later isnt cringe.
        /// </summary>
        [Header("Elite Economy Stats")]
        [Tooltip("Elite drop chance guaranteed")]
        [Range(0f, 1f)]
        [SerializeField] private float eliteDropChance = 1.0f;

        [Tooltip("Elite min credits")]
        [SerializeField] private int eliteMoneyMin = 35;

        [Tooltip("Elite max credits.")]
        [SerializeField] private int eliteMoneyMax = 60;

        /// <summary>
        /// Returns Minor stats asset if assigned; otherwise null.
        /// </summary>
        public SO_EnemyTierStats Minor => minor;

        /// <summary>
        /// Returns Standard stats asset if assigned; otherwise null.
        /// </summary>
        public SO_EnemyTierStats Standard => standard;

        public float EliteHpMultiplier => eliteHpMultiplier;
        public float EliteDamageMultiplier => eliteDamageMultiplier;
        public float EliteSpeedMultiplier => eliteSpeedMultiplier;

        public float EliteDropChance => eliteDropChance;
        public int EliteMoneyMin => eliteMoneyMin;
        public int EliteMoneyMax => eliteMoneyMax;
        }
}
