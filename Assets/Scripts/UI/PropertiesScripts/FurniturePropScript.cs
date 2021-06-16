using System.Collections.Generic;
using ErgoShop.Managers;
using ErgoShop.Operations;
using ErgoShop.POCO;
using ErgoShop.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    /// <summary>
    ///     UI Properties for selected element
    /// </summary>
    public class FurniturePropScript : PropertiesBehaviour
    {
        // Bindings for furniture properties    
        public GameObject furniturePropertiesForm;
        public InputField furnitureXField, furnitureYField, furnitureZField;
        public InputField furnitureNameInput;
        public Slider furnitureRotationSlider;
        public Toggle moveToggle;

        private bool needsUpdate;

        public static FurniturePropScript Instance { get; private set; }

        // Start is called before the first frame update
        private void Start()
        {
            Instance = this;
        }

        // Update is called once per frame
        new private void Update()
        {
            if (CheckPropertiesBindings(SelectedObjectManager.Instance.currentFurnitureData))
                UIManager.Instance.instructionsText.text =
                    "F pour centrer la caméra sur le meuble. Suppr pour supprimer.";
            base.Update();
        }

        public void UpdateFurnitureProperties()
        {
            needsUpdate = true;
        }

        private bool AreFurnituresSame(List<Furniture> furnituresData)
        {
            var model = furnituresData[0];
            var same = furnituresData.TrueForAll(f => f.Name == model.Name && f.Size == model.Size);
            return furnituresData.Count == 1 || same;
        }

        private bool CheckPropertiesBindings(List<Furniture> furnituresData)
        {
            furniturePropertiesForm.SetActive(furnituresData.Count > 0);
            if (furniturePropertiesForm.activeInHierarchy)
            {
                var model = furnituresData[0];
                var sameName = furnituresData.TrueForAll(f => f.Name == model.Name);
                var sameX = furnituresData.TrueForAll(f => f.Size.x == model.Size.x);
                var sameY = furnituresData.TrueForAll(f => f.Size.y == model.Size.y);
                var sameZ = furnituresData.TrueForAll(f => f.Size.z == model.Size.z);

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

                    //moveToggle.isOn = furnituresData[0].IsLocked;
                }

                needsUpdate = false;
            }

            return furniturePropertiesForm.activeInHierarchy;
        }

        public void SetFurnitureX(string x)
        {
            float res = 0;
            var ok = ParsingFunctions.ParseFloatCommaDot(x, out res);
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
            var ok = ParsingFunctions.ParseFloatCommaDot(y, out res);
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
            var ok = ParsingFunctions.ParseFloatCommaDot(z, out res);
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
                SetFurnitureRotation(f.Rotation + add);
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

            var rot2D = Vector3.zero;
            var rot3D = Vector3.zero;

            foreach (var f in SelectedObjectManager.Instance.currentFurnitureData)
            {
                var d3 = f.associated3DObject.transform.localEulerAngles;
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
            foreach (var f in SelectedObjectManager.Instance.currentFurnitureData) f.Name = name;
            SelectedObjectManager.Instance.UpdateFurnitureName();
            UpdateFurnitureProperties();

            //to do..
            //REfresh Hyrachies Scene

        }

        public void SetFurnitureRotation(string r)
        {
            float res = 0;
            if (ParsingFunctions.ParseFloatCommaDot(r, out res)) SetFurnitureRotation(res);
        }

        public override void Hide()
        {
            furniturePropertiesForm.SetActive(false);
        }
    }
}