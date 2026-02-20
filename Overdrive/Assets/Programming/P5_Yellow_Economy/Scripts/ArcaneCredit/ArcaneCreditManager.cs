/*
 * File: ArcaneCreditManager.cs
 * Summary: Centralized manager for Arcane Credits (AC). Owns the persistent wallet,
 *          provides a single API for credit transactions, and logs balance changes.
 * Notes:   Persists across scenes (DontDestroyOnLoad) and saves balance using PlayerPrefs.
 */

using UnityEngine;

namespace Programming.P5_Yellow_Economy.Scripts.ArcaneCredit
{
    [DisallowMultipleComponent]
    public class ArcaneCreditManager : MonoBehaviour
    {
        /// <summary>
        /// Primary singleton instance (preferred over abbreviations).
        /// </summary>
        public static ArcaneCreditManager Instance { get; private set; }

        /// <summary>
        /// Backward-compatible alias (so existing code using ".I" keeps working).
        /// </summary>
        public static ArcaneCreditManager I => Instance;

        private const string SaveKey = "P5_Yellow_AC_Balance";

        [Header("Arcane Credit")]
        [Tooltip("Persistent wallet holding the player's Arcane Credit balance.")]
        [SerializeField] private ArcaneCreditWallet wallet = new ArcaneCreditWallet();

        [Header("Debug")]
        [Tooltip("When enabled, logs transactions and balance updates to the Console.")]
        [SerializeField] private bool logTransactions = true;

        /// <summary>
        /// Current persistent Arcane Credit balance.
        /// </summary>
        public int Balance => wallet.Balance;

        private void Awake()
        {
            InitializeSingleton();

            wallet.OnBalanceChanged += HandleBalanceChanged;

            LoadPersistentBalance();

            if (logTransactions)
            {
                Debug.Log($"[ArcaneCredit] Manager ready. Balance: {wallet.Balance}");
            }
        }

        private void OnDestroy()
        {
            // Only clean up if this object is the active singleton instance.
            if (Instance != this) return;

            wallet.OnBalanceChanged -= HandleBalanceChanged;
            Instance = null;
        }

        /// <summary>
        /// Adds Arcane Credits through the centralized manager API.
        /// </summary>
        /// <param name="amount">Amount to add (must be > 0).</param>
        public void AddCredits(int amount)
        {
            if (amount <= 0) return;

            wallet.Add(amount);

            if (logTransactions)
            {
                Debug.Log($"[ArcaneCredit] +{amount}");
            }
        }

        /// <summary>
        /// Attempts to spend Arcane Credits through the centralized manager API.
        /// </summary>
        /// <param name="amount">Amount to spend (must be > 0).</param>
        /// <returns>True if the spend succeeded; otherwise false.</returns>
        public bool TrySpendCredits(int amount)
        {
            if (amount <= 0) return true;

            bool success = wallet.TrySpend(amount);

            if (logTransactions)
            {
                Debug.Log(success
                    ? $"[ArcaneCredit] -{amount}"
                    : $"[ArcaneCredit] Spend FAILED (-{amount})");
            }

            return success;
        }

        [ContextMenu("AC/Reset Balance To 0")]
        private void ResetBalanceToZero()
        {
            wallet.SetBalance(0);
        }

        private void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void HandleBalanceChanged(int newBalance)
        {
            SavePersistentBalance(newBalance);

            if (logTransactions)
            {
                Debug.Log($"[ArcaneCredit] Balance now: {newBalance}");
            }
        }

        private void LoadPersistentBalance()
        {
            int savedBalance = PlayerPrefs.GetInt(SaveKey, 0);
            wallet.SetBalance(savedBalance);
        }

        private void SavePersistentBalance(int balanceValue)
        {
            PlayerPrefs.SetInt(SaveKey, balanceValue);
            PlayerPrefs.Save();
        }
    }
}