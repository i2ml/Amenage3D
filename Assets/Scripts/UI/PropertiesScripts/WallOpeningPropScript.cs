using ErgoShop.Managers;
using ErgoShop.POCO;
using ErgoShop.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    /// <summary>
    /// UI Properties for wall openings
    /// </summary>
    public class WallOpeningPropScript : PropertiesBehaviour
    {
        // Bindings for wall opening properties
        public GameObject woPropertiesForm;
        public Text woNameText;
        public InputField woXField, woYField;
        public InputField woWindowHeightField;
        public Toggle woIsDoubleToggle, woIsFlippedToggle, woIsLeftToggle;
        public static WallOpeningPropScript Instance { get; private set; }

        private bool m_needsUpdate;

        // Start is called before the first frame update
        void Start()
        {
            Instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            foreach (var wo in SelectedObjectManager.Instance.currentWallOpenings)
            {
                if (CheckPropertiesBindings(wo))
                {
                    UIManager.Instance.instructionsText.text = "F pour centrer la caméra sur l'ouverture. Suppr pour supprimer.";
                }
            }
            base.Update();
        }

        public void UpdateWallOpeningProperties()
        {
            m_needsUpdate = true;
        }

        private bool CheckPropertiesBindings(WallOpening wo)
        {
            woPropertiesForm.SetActive(wo != null);
            if (woPropertiesForm.activeInHierarchy)
            {
                if (m_needsUpdate)
                {
                    woWindowHeightField.transform.parent.gameObject.SetActive(wo.IsWindow);
                    woIsDoubleToggle.isOn = wo.IsDouble;
                    woIsDoubleToggle.transform.parent.gameObject.SetActive(wo.IsWindow);

                    woIsFlippedToggle.isOn = wo.IsPull;
                    woIsLeftToggle.isOn = wo.IsLeft;
                    woIsLeftToggle.transform.parent.gameObject.SetActive(!wo.IsDouble);

                    if (wo.IsWindow)
                    {
                        string dble = wo.IsDouble ? " Double" : " Simple";
                        woNameText.text = "Fenetre" + dble;
                        woWindowHeightField.text = Mathf.Floor(wo.WindowHeight * 100f) + "";
                    }
                    else
                    {
                        woNameText.text = "Porte";
                    }
                    woXField.text = Mathf.Floor(wo.Size.x * 100f) + "";
                    woYField.text = Mathf.Floor(wo.Size.y * 100f) + "";
                }
            }
            m_needsUpdate = !woPropertiesForm.activeInHierarchy;

            return woPropertiesForm.activeInHierarchy;
        }


        public void SetWOX(string x)
        {
            float res = 0;
            bool ok = ParsingFunctions.ParseFloatCommaDot(x, out res);
            if (!ok) return;
            res /= 100f;
            foreach (var wo in SelectedObjectManager.Instance.currentWallOpenings)
            {
                wo.Size = new Vector3(res, wo.Size.y, wo.Size.z);
            }
            WallsCreator.Instance.AdjustAllWalls();
            UpdateWallOpeningProperties();
        }


        public void SetWOY(string y)
        {
            float res = 0;
            bool ok = ParsingFunctions.ParseFloatCommaDot(y, out res);
            if (!ok) return;
            res /= 100f;
            foreach (var wo in SelectedObjectManager.Instance.currentWallOpenings)
            {
                wo.Size = new Vector3(wo.Size.x, res, wo.Size.z);
            }
            WallsCreator.Instance.AdjustAllWalls();
            UpdateWallOpeningProperties();
        }

        public void SetIsDouble(bool v)
        {
            foreach (var wo in SelectedObjectManager.Instance.currentWallOpenings)
            {
                wo.IsDouble = v;
                WallsCreator.Instance.InstantiateWallOpening(wo);
            }
            WallsCreator.Instance.AdjustAllWalls();
            UpdateWallOpeningProperties();
        }

        public void SetIsPull(bool v)
        {
            foreach (var wo in SelectedObjectManager.Instance.currentWallOpenings)
            {
                wo.IsPull = v;
            }
            WallsCreator.Instance.AdjustAllWalls();
            UpdateWallOpeningProperties();
        }

        public void SetIsLeft(bool v)
        {
            foreach (var wo in SelectedObjectManager.Instance.currentWallOpenings)
            {
                wo.IsLeft = v;
            }
            WallsCreator.Instance.AdjustAllWalls();
            UpdateWallOpeningProperties();
        }

        public void SetWindowHeight(string h)
        {
            float res = 0;
            bool ok = ParsingFunctions.ParseFloatCommaDot(h, out res);
            if (!ok) return;
            res /= 100f;
            foreach (var wo in SelectedObjectManager.Instance.currentWallOpenings)
            {
                wo.WindowHeight = res;
            }
            WallsCreator.Instance.AdjustAllWalls();
            Instance.UpdateWallOpeningProperties();
        }

        public override void Hide()
        {
            woPropertiesForm.SetActive(false);
        }
    }
}