using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoShop.UI
{
    /// <summary>
    /// Ask name for an imported model obj
    /// </summary>
    public class CustomNamePopinScript : MonoBehaviour
    {
        public bool NameEntered = false;
        public string Name;

        public static CustomNamePopinScript Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void EnterName(string name)
        {
            NameEntered = true;
            Name = name;
            this.gameObject.SetActive(false);
        }

    }
}