/*
 * File: ArcaneCreditWallet.cs
 * Summary: Stores and manages the Arcane Credit (AC) balance.
 * Notes:   Serializable so it can be embedded and visible in the ArcaneCreditManager inspector.
 */

using System;
using UnityEngine;

namespace Programming.P5_Yellow_Economy.Scripts.ArcaneCredit
{
    [Serializable]
    public class ArcaneCreditWallet
    {
        [Tooltip("Current Arcane Credit balance (persistent via ArcaneCreditManager).")]
        [SerializeField] private int balance;

        /// <summary>
        /// Current Arcane Credit balance.
        /// </summary>
        public int Balance => balance;

        /// <summary>
        /// Fired whenever the balance changes.
        /// </summary>
        public event Action<int> OnBalanceChanged;

        /// <summary>
        /// Returns true if the wallet has at least the requested amount.
        /// </summary>
        public bool CanAfford(int amount) => amount <= balance;

        /// <summary>
        /// Sets the balance directly (clamped to 0+).
        /// Intended for loading/saving or admin/debug operations.
        /// </summary>
        public void SetBalance(int newBalance)
        {
            int clampedBalance = Mathf.Max(0, newBalance);
            if (clampedBalance == balance) return;

            balance = clampedBalance;
            OnBalanceChanged?.Invoke(balance);
        }

        /// <summary>
        /// Adds credits to the wallet.
        /// </summary>
        public void Add(int amount)
        {
            if (amount <= 0) return;
            SetBalance(balance + amount);
        }

        /// <summary>
        /// Attempts to spend credits. Returns false if insufficient funds.
        /// </summary>
        public bool TrySpend(int amount)
        {
            if (amount <= 0) return true;
            if (balance < amount) return false;

            SetBalance(balance - amount);
            return true;
        }
    }
}