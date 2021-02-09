using UnityEngine;

namespace ErgoShop.UI
{
    /// <summary>
    ///     UI Properties
    ///     Currently just a Hide method is common
    /// </summary>
    public abstract class PropertiesBehaviour : MonoBehaviour
    {
        internal void Update()
        {
            if (PropertiesFormManager.Instance.HideAllBool) Hide();
        }

        public abstract void Hide();
    }
}