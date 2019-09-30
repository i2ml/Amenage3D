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
    /// UI Properties for selected element
    /// </summary>
    public class FurniturePropScript : PropertiesBehaviour
    {
        // Bindings for furniture properties    
        public GameObject furniturePropertiesForm;
        public InputField furnitureXField, furnitureYField, furnitureZField;
        public InputField furnitureNameInput;
        public Slider furnitureRotationSlider;
        public Toggle moveToggle;

        public static FurniturePropScript Instance { get; private set; }

        private bool needsUpdate;

        // Start is called before the first frame update
        void Start()
        {
            Instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            if (CheckPropertiesBindings(SelectedObjectManager.Instance.currentFurnitureData))
            {
                UIManager.Instance.instructionsText.text = "F pour centrer la caméra sur le meuble. Suppr pour supprimer.";
            }
            base.Update();
        }

        public void UpdateFurnitureProperties()
        {
            needsUpdate = true;
        }

        private bool AreFurnituresSame(List<Furniture> furnituresData)
        {
            Furniture model = furnituresData[0];
            bool same = furnituresData.TrueForAll(f => f.Name == model.Name && f.Size == model.Size);
            return (furnituresData.Count == 1) || same;
        }

        private bool CheckPropertiesBindings(List<Furniture> furnituresData)
        {
            furniturePropertiesForm.SetActive(furnituresData.Count > 0);
            if (furniturePropertiesForm.activeInHierarchy)
            {
                Furniture model = furnituresData[0];
                bool sameName = furnituresData.TrueForAll(f => f.Name == model.Name);
                bool sameX = furnituresData.TrueForAll(f => f.Size.x == model.Size.x);
                bool sameY = furnituresData.TrueForAll(f => f.Size.y == model.Size.y);
                bool sameZ = furnituresData.TrueForAll(f => f.Size.z == model.Size.z);

                furnitureNameInput.gameObject.SetActive(true);
                // FURNITURE PROPERTIES
                if (needsUpdate)
                {
                    furnitureNameInput.text = sameName ? furnituresData[0].Name : "";
                    furnitureXField.text = sameX ? Mathf.Floor(furnituresData[0].Size.x * 100f) + "" : "";
                    furnitureYField.text = sameY ? Mathf.Floor(furnituresData[0].Size.y * 100f) + "" : "";
                    furnitureZField.text = sameZ ? Mathf.Floor(furnituresData[0].Size.z * 100f) + "" : "";

                    if (furnituresData.Count == 1)
                    {
                        if (furnituresData[0].Rotation < 0) furnituresData[0].Rotation += 360f;
                        furnitureRotationSlider.value = furnituresData[0].Rotation / 5f;
                    }
                    moveToggle.isOn = furnituresData[0].IsLocked;
                }
                needsUpdate = false;
            }
            return furniturePropertiesForm.activeInHierarchy;
        }

        public void SetFurnitureX(string x)
        {
            float res = 0;
            bool ok = ParsingFunctions.ParseFloatCommaDot(x, out res);
            if (!ok) return;
            res /= 100f;
            foreach (var f in SelectedObjectManager.Instance.currentFurnitureData)
            {
                f.Size =
                new Vector3(
                res,
                f.Size.y,
                f.Size.z);
                SelectedObjectManager.Instance.UpdateFurnitureSize();
            }

            UpdateFurnitureProperties();
        }

        public void SetFurnitureY(string y)
        {
            float res = 0;
            bool ok = ParsingFunctions.ParseFloatCommaDot(y, out res);
            if (!ok) return;
            res /= 100f;
            foreach (var f in SelectedObjectManager.Instance.currentFurnitureData)
            {
                f.Size =
                new Vector3(
                f.Size.x,
                res,
                f.Size.z);
                SelectedObjectManager.Instance.UpdateFurnitureSize();
            }
            UpdateFurnitureProperties();
        }

        public void SetFurnitureZ(string z)
        {
            float res = 0;
            bool ok = ParsingFunctions.ParseFloatCommaDot(z, out res);
            if (!ok) return;
            res /= 100f;
            foreach (var f in SelectedObjectManager.Instance.currentFurnitureData)
            {
                f.Size =
                new Vector3(
                f.Size.x,
                f.Size.y,
                res);
                SelectedObjectManager.Instance.UpdateFurnitureSize();
            }
            UpdateFurnitureProperties();
        }

        public void AddRotationToFurniture(float add)
        {
            foreach (var f in SelectedObjectManager.Instance.currentFurnitureData)
            {
                SetFurnitureRotation(f.Rotation + add);
            }
            UpdateFurnitureProperties();
        }

        public void SetFurnitureRotationSlider(float rotation)
        {
            rotation *= 5;
            SetFurnitureRotation(rotation);
        }

        public void SetFurnitureRotation(float rotation)
        {
            rotation = rotation % 360;
            Vector3 rot2D = Vector3.zero;
            Vector3 rot3D = Vector3.zero;

            foreach (var f in SelectedObjectManager.Instance.currentFurnitureData)
            {

                Vector3 d3 = f.associated3DObject.transform.localEulerAngles;
                if (f.IsOnWall)
                {
                    rot3D = new Vector3(0, d3.y, rotation);
                }
                else
                {
                    rot3D = Vector3.up * rotation;
                    rot2D = Vector3.forward * rotation * -1;
                    f.associated2DObject.transform.localEulerAngles = rot2D;
                }
                f.associated3DObject.transform.localEulerAngles = rot3D;
                f.Rotation = rotation;
                f.EulerAngles = rot3D;
            }
            OperationsBufferScript.Instance.AddAutoSave("Rotation meuble(s)");
        }

        public void SetFurnitureName(string name)
        {
            foreach (var f in SelectedObjectManager.Instance.currentFurnitureData)
            {
                f.Name = name;
            }
            SelectedObjectManager.Instance.UpdateFurnitureName();
            UpdateFurnitureProperties();
        }

        public void SetFurnitureRotation(string r)
        {
            float res = 0;
            if (ParsingFunctions.ParseFloatCommaDot(r, out res))
            {
                SetFurnitureRotation(res);
            }
        }

        public override void Hide()
        {
            furniturePropertiesForm.SetActive(false);
        }
    }
}