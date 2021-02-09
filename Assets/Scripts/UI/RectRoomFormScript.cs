using ErgoShop.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    /// <summary>
    ///     Form to create a rectangular room by setting height and width
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
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
        }

        public float GetHeight()
        {
            if (ParsingFunctions.ParseFloatCommaDot(heightField.text, out var res)) return res;
            return -1f;
        }

        public float GetWidth()
        {
            if (ParsingFunctions.ParseFloatCommaDot(widthField.text, out var res)) return res;
            return -1f;
        }
    }
}