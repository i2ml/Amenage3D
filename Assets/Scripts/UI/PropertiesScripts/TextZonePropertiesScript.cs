using ErgoShop.Managers;
using ErgoShop.POCO;
using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    /// <summary>
    ///     Text Zone UI for selected zone
    /// </summary>
    public class TextZonePropertiesScript : PropertiesBehaviour
    {
        public GameObject textPropertiesPanel;
        public InputField textField;
        public Slider fontSizeSlider;
        public CUIColorPicker colorPicker;
        public CUIColorPicker colorPickerBG;
        public Image imageToggle;
        public Image imageToggleBG;

        public Toggle moveToggle;

        private bool needsUpdate;

        public static TextZonePropertiesScript Instance { get; set; }

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
            foreach (var tz in SelectedObjectManager.Instance.currentHelperElements)
                if (CheckPropertiesBindings(tz as TextZoneElement))
                    UIManager.Instance.instructionsText.text = "Propriétes du texte à gauche de l'écran";
            base.Update();
        }

        public void UpdateTextZoneProperties()
        {
            needsUpdate = true;
        }

        private bool CheckPropertiesBindings(TextZoneElement tz)
        {
            textPropertiesPanel.SetActive(tz != null);
            if (textPropertiesPanel.activeInHierarchy)
            {
                // FURNITURE PROPERTIES
                if (needsUpdate)
                {
                    textField.text = tz.Text;

                    if (moveToggle != null) { moveToggle.isOn = tz.IsLocked; }

                    colorPicker.Color = new Color(tz.TextColor.r, tz.TextColor.g, tz.TextColor.b);

                    if (colorPickerBG != null)
                    {
                        colorPickerBG.Color = new Color(tz.BackgroundColor.r, tz.BackgroundColor.g, tz.BackgroundColor.b);
                        imageToggleBG.color = new Color(colorPickerBG.Color.r, colorPickerBG.Color.g, colorPickerBG.Color.b, 0.5f);
                        imageToggleBG.color = colorPickerBG.Color;
                        tz.BackgroundColor = imageToggleBG.color;
                    }

                    imageToggle.color = colorPicker.Color;
                    needsUpdate = false;
                }

                imageToggle.color = new Color(colorPicker.Color.r, colorPicker.Color.g, colorPicker.Color.b);

                tz.TextColor = imageToggle.color;

                tz.associated2DObject.GetComponent<TextZoneScript>().tm.color = tz.TextColor;
                tz.associated2DObject.GetComponent<TextZoneScript>().bg.color = tz.BackgroundColor;
                tz.associated2DObject.GetComponent<TextZoneScript>().textSize = tz.TextSize;

                tz.associated3DObject.GetComponent<TextZoneScript>().tm.color = tz.TextColor;
                tz.associated3DObject.GetComponent<TextZoneScript>().bg.color = tz.BackgroundColor;
                tz.associated3DObject.GetComponent<TextZoneScript>().textSize = tz.TextSize;
            }

            return textPropertiesPanel.activeInHierarchy;
        }

        public void SetText(string s)
        {
            foreach (var tz in SelectedObjectManager.Instance.currentHelperElements)
                if (tz is TextZoneElement)
                {
                    (tz as TextZoneElement).Text = s;
                    (tz as TextZoneElement).associated2DObject.GetComponent<TextZoneScript>().tm.text = s;
                    (tz as TextZoneElement).associated3DObject.GetComponent<TextZoneScript>().tm.text = s;
                }
        }

        public void SetFontSize(float v)
        {
            foreach (var tz in SelectedObjectManager.Instance.currentHelperElements)
                if (tz is TextZoneElement)
                {
                    (tz as TextZoneElement).TextSize = v;
                    (tz as TextZoneElement).associated2DObject.GetComponent<TextZoneScript>().textSize = v;
                    (tz as TextZoneElement).associated3DObject.GetComponent<TextZoneScript>().textSize = v;
                }
        }

        public override void Hide()
        {
            textPropertiesPanel.SetActive(false);
        }
    }
}