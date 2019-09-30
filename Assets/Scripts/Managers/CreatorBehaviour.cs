using ErgoShop.POCO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoShop.Managers
{
    /// <summary>
    /// MonoBehaviour with methods for all creators
    /// Mostly for Elements and MovableElements
    /// </summary>
    public abstract class CreatorBehaviour : MonoBehaviour
    {
        /// <summary>
        /// Destroy every element (gameobjects and data)
        /// </summary>
        public abstract void DestroyEverything();

        /// <summary>
        /// Paste a copied element by getting a copy and instantiate it, and rebuilding gameobjects
        /// </summary>
        /// <param name="m_copiedElement">Copied element</param>
        /// <returns>The new element, identical to the copied one</returns>
        public abstract Element CopyPaste(Element elem);
    }
}