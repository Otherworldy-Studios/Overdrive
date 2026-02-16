// Copyright (c) 2024 Synty Studios Limited. All rights reserved.
//
// Use of this software is subject to the terms and conditions of the End User Licence Agreement (EULA) 
// of the store at which you purchased this asset. 
//
// Synty assets are available at:
// https://www.syntystore.com
// https://assetstore.unity.com/publishers/5217
// https://www.fab.com/sellers/Synty%20Studios
//
// Sample scripts are included only as examples and are not intended as production-ready.

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Synty.Interface.SciFiMenus.Samples
{
    /// <summary>
    ///     Allows the selection of a single option from an array of possible options
    /// </summary>
    public class SampleNavInputHelper : MonoBehaviour
    {
        public Toggle previous;
        public Toggle next;

        Toggle selectionToggle;
        bool selectPrevious = false;
        bool selectNext = false;

        public void Awake()
        {
            selectionToggle = GetComponent<Toggle>();
        }

        public void Update()
        {
            if (selectionToggle != null && selectionToggle.isOn)
            {
                if (UnifiedInput.WasPressedThisFrame(UnifiedKey.Q))
                {
                    selectPrevious = true;
                }
                else if (UnifiedInput.WasPressedThisFrame(UnifiedKey.E))
                {
                    selectNext = true;
                }
            }
        }

        public void LateUpdate()
        {
            if (selectPrevious)
            {
                SelectPrevious();
            }
            else if (selectNext)
            {
                SelectNext();
            }

            selectPrevious = false;
            selectNext = false;
        }

        private void SelectPrevious()
        {
            if (previous != null )
            {
                previous.isOn = true;
            } 
        }

        private void SelectNext()
        {
            if (next != null )
            {
                next.isOn = true;
            } 
        }
    }
}