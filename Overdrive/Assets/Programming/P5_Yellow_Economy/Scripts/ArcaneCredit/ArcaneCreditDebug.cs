/*
 * File: ArcaneCreditDebug.cs
 * Summary: Simple debug helper for testing Arcane Credit transactions during Play Mode.
 * Usage:   Press the configured keys to add/spend credits and verify logs + inspector updates.
 */

using UnityEngine;

namespace Programming.P5_Yellow_Economy.Scripts.ArcaneCredit
{
    [DisallowMultipleComponent]
    public class ArcaneCreditDebug : MonoBehaviour
    {
        [Header("Debug Inputs")]
        [Tooltip("Key to add credits.")]
        [SerializeField] private KeyCode addKey = KeyCode.P;

        [Tooltip("Key to spend credits.")]
        [SerializeField] private KeyCode spendKey = KeyCode.O;

        [Header("Debug Amounts")]
        [Tooltip("Amount of credits to add when the add key is pressed.")]
        [SerializeField] private int addAmount = 45;

        [Tooltip("Amount of credits to spend when the spend key is pressed.")]
        [SerializeField] private int spendAmount = 10;

        private void Update()
        {
            ArcaneCreditManager manager = ArcaneCreditManager.Instance; // preferred accessor
            if (manager == null) return;

            if (Input.GetKeyDown(addKey))
            {
                Debug.Log($"[AC Debug] {addKey} pressed -> +{addAmount}");
                manager.AddCredits(addAmount);
            }

            if (Input.GetKeyDown(spendKey))
            {
                Debug.Log($"[AC Debug] {spendKey} pressed -> -{spendAmount}");
                manager.TrySpendCredits(spendAmount);
            }
        }
    }
}