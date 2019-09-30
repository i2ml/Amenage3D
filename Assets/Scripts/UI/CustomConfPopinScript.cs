using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoShop.UI
{
    /// <summary>
    /// Can be generic confirmation popin to show a choice between yes and no to an user
    /// </summary>
    public class CustomConfPopinScript : MonoBehaviour
    {
        public bool MadeChoice = false;
        public bool IsYes = false;

        public static CustomConfPopinScript Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void SayYes()
        {
            MadeChoice = true;
            IsYes = true;
        }

        public void SayNo()
        {
            MadeChoice = true;
            IsYes = false;
        }
    }
}