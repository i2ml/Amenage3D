using ErgoShop.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    /// <summary>
    /// Form to create a rectangular room by setting height and width
    /// </summary>
    public class RectRoomFormScript : MonoBehaviour
    {
        public InputField heightField, widthField;

        public static RectRoomFormScript Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public float GetHeight()
        {
            if (ParsingFunctions.ParseFloatCommaDot(heightField.text, out float res))
            {
                return res;
            }
            return -1f;
        }

        public float GetWidth()
        {
            if (ParsingFunctions.ParseFloatCommaDot(widthField.text, out float res))
            {
                return res;
            }
            return -1f;
        }
    }
}