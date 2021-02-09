using UnityEngine;

namespace ErgoShop.UI
{
    /// <summary>
    ///     Can be generic confirmation popin to show a choice between yes and no to an user
    /// </summary>
    public class CustomConfPopinScript : MonoBehaviour
    {
        public bool MadeChoice;
        public bool IsYes;

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