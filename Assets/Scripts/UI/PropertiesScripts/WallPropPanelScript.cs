using ErgoShop.Managers;
using ErgoShop.Operations;
using ErgoShop.POCO;
using ErgoShop.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    /// <summary>
    /// UI Properties for wall and rooms
    /// </summary>
    public class WallPropPanelScript : PropertiesBehaviour
    {
        // Bindings for wall properties
        public GameObject wallPropertiesForm;
        //public GameObject wallColorsGameObject;
        public GameObject roomOptionGameObject;
        public GameObject splitWallButton;
        public GameObject wallDimensionsGameObject;
        public Text heightText;
        public InputField thicknessField;
        public InputField lengthField;

        public CUIColorPicker colorPicker;
        public Image imageToggle;

        private bool needsUpdate;

        public static WallPropPanelScript Instance { get; private set; }

        // Start is called before the first frame update
        void Start()
        {
            Instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            if (SelectedObjectManager.Instance.currentRoomData == null)
            {
                if (CheckPropertiesBindings(SelectedObjectManager.Instance.currentWallsData))
                {
                    UIManager.Instance.instructionsText.text = "F pour centrer la caméra sur le(s) mur(s). Suppr pour supprimer.";
                }
            }
            else
            {
                wallPropertiesForm.SetActive(false);
            }
            base.Update();
        }

        public void UpdateWallProperties()
        {
            needsUpdate = true;
        }

        //private void ResetWallRoomPropertiesForm()
        //{
        //    for (int i = 0; i < wallColorsGameObject.transform.childCount; i++)
        //    {
        //        wallColorsGameObject.transform.GetChild(i).GetComponent<UnityEngine.UI.Outline>().enabled = false;
        //    }
        //}

        public void SetWallThickness(string thick)
        {
            float res = 0;
            bool ok = ParsingFunctions.ParseFloatCommaDot(thick, out res);
            if (ok)
            {
                foreach (var w in SelectedObjectManager.Instance.currentWallsData)
                {
                    w.Thickness = res / 100f;
                }
                UpdateWallProperties();

                WallsCreator.Instance.AdjustAllWalls();
            }
        }

        public void SetWallLength(string length)
        {
            float res = 0;
            bool ok = ParsingFunctions.ParseFloatCommaDot(length, out res);
            if (ok)
            {
                WallsCreator.Instance.UpdateWallLength(SelectedObjectManager.Instance.currentWallsData[0], res / 100f, true);
                SelectedObjectManager.Instance.StartCoroutine(
                    SelectedObjectManager.Instance.SWLRoutine(
                        SelectedObjectManager.Instance.currentWallsData[0], res / 100f, true));
            }
        }

        // return true if properties form is shown
        private bool CheckPropertiesBindings(List<Wall> wallsData)
        {
            wallPropertiesForm.SetActive(wallsData.Count > 0);

            if (wallPropertiesForm.activeInHierarchy)
            {
                //ResetWallRoomPropertiesForm();
                // WALL PROPERTIES
                if (wallPropertiesForm.activeInHierarchy)
                {
                    float thick = wallsData[0].Thickness;
                    Color col = wallsData[0].Color;
                    wallDimensionsGameObject.SetActive((wallsData.Count == 1));
                    if (needsUpdate)
                    {
                        heightText.text = wallsData[0].Height * 100f + " cm";
                        lengthField.text = wallsData[0].InsideLength * 100f + "";
                        thicknessField.text = wallsData[0].Thickness * 100f + "";

                        colorPicker.Color = new Color(wallsData[0].Color.r, wallsData[0].Color.g, wallsData[0].Color.b);
                        imageToggle.color = colorPicker.Color;
                        OperationsBufferScript.Instance.AddAutoSave("Mise à jour mur");
                    }

                    imageToggle.color = new Color(colorPicker.Color.r, colorPicker.Color.g, colorPicker.Color.b);
                    // TO DO TODO Several walls
                    if (wallsData[0].Color != imageToggle.color)
                    {
                        wallsData[0].Color = imageToggle.color;
                        WallsCreator.Instance.AdjustAllWalls();
                    }

                    //for (int i = 0; i < wallColorsGameObject.transform.childCount; i++)
                    //{
                    //    if (wallColorsGameObject.transform.GetChild(i).GetComponent<Button>().colors.normalColor == col)
                    //    {
                    //        wallColorsGameObject.transform.GetChild(i).GetComponent<UnityEngine.UI.Outline>().enabled = true;
                    //    }
                    //}

                    roomOptionGameObject.SetActive(wallsData.Count > 2);
                    splitWallButton.SetActive(wallsData.Count == 1);
                }
            }
            else
            {
                colorPicker.gameObject.SetActive(false);
            }
            needsUpdate = !wallPropertiesForm.activeInHierarchy;
            return wallPropertiesForm.activeInHierarchy;
        }

        public void ShowWallToggle()
        {
            colorPicker.Color = imageToggle.color;
        }


        public override void Hide()
        {
            wallPropertiesForm.SetActive(false);
        }
    }
}