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
    public class CharacterPropertiesScript : PropertiesBehaviour
    {
        public GameObject characterPropertiesPanel;

        public InputField characterXField, characterYField, characterZField;
        public Slider characterRotationSlider;
        public Toggle moveToggle;

        public Dropdown typeDD;
        public Toggle spreadArmsToggle;


        private bool needsUpdate = true;

        public static CharacterPropertiesScript Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        private void Start()
        {
        }

        // Update is called once per frame
        new void Update()
        {
            if (CheckPropertiesBindings(SelectedObjectManager.Instance.currentCharacters))
                UIManager.Instance.instructionsText.text =
                    "F pour centrer la caméra sur le personnage. Suppr pour supprimer.";

            base.Update();
        }

        public void UpdateCharacterProperties()
        {
            needsUpdate = true;
        }

        private bool CheckPropertiesBindings(List<CharacterElement> chs)
        {
            characterPropertiesPanel.SetActive(chs.Count > 0);
            if (characterPropertiesPanel.activeInHierarchy)
            {
                var model = chs[0];
                var sameX = chs.TrueForAll(f => f.Size.x == model.Size.x);
                var sameY = chs.TrueForAll(f => f.Size.y == model.Size.y);
                var sameZ = chs.TrueForAll(f => f.Size.z == model.Size.z);
                var sameSA = chs.TrueForAll(f => f.SpreadArms == model.SpreadArms);
                var sameDD = chs.TrueForAll(f => f.Type == model.Type);

                // FURNITURE PROPERTIES
                if (needsUpdate)
                {
                    characterXField.text = sameX ? Mathf.Floor(chs[0].Size.x * 100f) + "" : "";
                    characterYField.text = sameY ? Mathf.Floor(chs[0].Size.y * 100f) + "" : "";
                    characterZField.text = sameZ ? Mathf.Floor(chs[0].Size.z * 100f) + "" : "";

                    typeDD.value = sameDD ? (int)chs[0].Type : -1;
                    spreadArmsToggle.isOn = sameSA ? chs[0].SpreadArms : false;

                    if (chs[0].Rotation < 0) chs[0].Rotation += 360f;
                    characterRotationSlider.value = chs[0].Rotation / 5f;

                    moveToggle.isOn = chs[0].IsLocked;
                }

                spreadArmsToggle.gameObject.SetActive(chs[0].Type != CharacterType.WheelChairEmpty &&
                                                      chs[0].Type != CharacterType.OnWheelChair);

                needsUpdate = false;
            }

            return characterPropertiesPanel.activeInHierarchy;
        }

        public override void Hide()
        {
            characterPropertiesPanel.SetActive(false);
        }

        public void SetCharacterX(string x)
        {
            float res = 0;
            var ok = ParsingFunctions.ParseFloatCommaDot(x, out res);
            if (!ok) return;
            res /= 100f;
            foreach (var f in SelectedObjectManager.Instance.currentCharacters)
            {
                f.Size =
                    new Vector3(
                        res,
                        f.Size.y,
                        f.Size.z);
                SelectedObjectManager.Instance.UpdateCharacterSize();
            }

            UpdateCharacterProperties();
        }

        public void SetCharacterY(string y)
        {
            float res = 0;
            var ok = ParsingFunctions.ParseFloatCommaDot(y, out res);
            if (!ok) return;
            res /= 100f;
            foreach (var f in SelectedObjectManager.Instance.currentCharacters)
            {
                f.Size =
                    new Vector3(
                        f.Size.x,
                        res,
                        f.Size.z);
                SelectedObjectManager.Instance.UpdateCharacterSize();
            }

            UpdateCharacterProperties();
        }

        public void SetCharacterZ(string z)
        {
            float res = 0;
            var ok = ParsingFunctions.ParseFloatCommaDot(z, out res);
            if (!ok) return;
            res /= 100f;
            foreach (var f in SelectedObjectManager.Instance.currentCharacters)
            {
                f.Size =
                    new Vector3(
                        f.Size.x,
                        f.Size.y,
                        res);
                SelectedObjectManager.Instance.UpdateCharacterSize();
            }

            UpdateCharacterProperties();
        }

        public void AddRotationToCharacter(float add)
        {
            foreach (var f in SelectedObjectManager.Instance.currentCharacters) SetCharacterRotation(f.Rotation + add);
            UpdateCharacterProperties();
        }

        public void SetCharacterRotationSlider(float rotation)
        {
            rotation *= 5;
            SetCharacterRotation(rotation);
        }

        public void SetCharacterRotation(float rotation)
        {
            rotation = rotation % 360;
            var rot2D = Vector3.zero;
            var rot3D = Vector3.zero;

            foreach (var f in SelectedObjectManager.Instance.currentCharacters)
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

            OperationsBufferScript.Instance.AddAutoSave("Rotation personnage");
        }

        public void SetCharacterRotation(string r)
        {
            float res = 0;
            if (ParsingFunctions.ParseFloatCommaDot(r, out res)) SetCharacterRotation(res);
        }

        public void SetSpreadArms(bool spread)
        {
            foreach (var ch in SelectedObjectManager.Instance.currentCharacters)
            {
                ch.SpreadArms = spread;
                ch.RebuildSceneData();
            }
        }

        public void SetCharacterType(int v)
        {
            foreach (var ch in SelectedObjectManager.Instance.currentCharacters)
            {
                ch.Type = (CharacterType)v;
                ch.RebuildSceneData();
                if (ch.Type == CharacterType.OnWheelChair || ch.Type == CharacterType.WheelChairEmpty)
                {
                    SetCharacterX("80");
                    SetCharacterY("110");
                    SetCharacterZ("130");
                }
                else
                {
                    SetCharacterX("176");
                    SetCharacterY("177");
                    SetCharacterZ("32");
                }
            }
        }
    }
}