using ErgoShop.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    /// <summary>
    ///     Update an input field according to slider value
    /// </summary>
    public class SliderToInput : MonoBehaviour
    {
        public Slider slider;
        public InputField input;

        public float multix;

        // Start is called before the first frame update
        private void Start()
        {
            if (multix == 0) multix = 1;
            slider.onValueChanged.AddListener(v => { input.text = v * multix + ""; });
            input.onEndEdit.AddListener(s =>
            {
                float f;
                if (ParsingFunctions.ParseFloatCommaDot(s, out f)) slider.value = f / multix;
            });
        }

        // Update is called once per frame
        private void Update()
        {
        }
    }
}