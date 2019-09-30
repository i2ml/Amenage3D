using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoShop.UI
{
    /// <summary>
    /// UI Properties
    /// Currently just a Hide method is common
    /// </summary>
    public abstract class PropertiesBehaviour : MonoBehaviour
    {
        public abstract void Hide();

        internal void Update()
        {
            if (PropertiesFormManager.Instance.HideAllBool)
            {
                Hide();
            }
        }
    }
}