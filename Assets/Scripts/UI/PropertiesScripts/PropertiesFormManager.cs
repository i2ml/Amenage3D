using System.Collections.Generic;
using System.Linq;
using ErgoShop.Managers;
using ErgoShop.POCO;
using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    /// <summary>
    ///     UI Properties for selected element
    /// </summary>
    public class PropertiesFormManager : MonoBehaviour
    {
        // Properties
        public GameObject propertiesForm;

        // Bindings for element properties
        public Toggle visibilityToggle;
        public Toggle groupsToggle;

        private bool m_hideProps;

        public bool HideAllBool { get; set; }

        public static PropertiesFormManager Instance { get; set; }

        public List<PropertiesBehaviour> AllProperties { get; private set; }

        // Start is called before the first frame update
        private void Start()
        {
            Instance = this;
            AllProperties = FindObjectsOfType<PropertiesBehaviour>().ToList();
            foreach (var prop in AllProperties) prop.Hide();
        }

        // Update is called once per frame
        private void Update()
        {
            // Never show empty properties
            if (SelectedObjectManager.Instance.HasNoSelection() || m_hideProps ||
                SelectedObjectManager.Instance.currentSelectedElements.Any(elem => elem is Stairs))
            {
                propertiesForm.SetActive(false);
            }
            else
            {
                propertiesForm.SetActive(true);
                groupsToggle.gameObject.SetActive(SelectedObjectManager.Instance.currentElementGroups.Count > 0
                                                  || SelectedObjectManager.Instance.currentSelectedElements.Count > 1);
                groupsToggle.isOn = SelectedObjectManager.Instance.currentElementGroups.Count > 0;
                var visbOn = false;
                var type = SelectedObjectManager.Instance.currentSelectedElements.First();
                HideAllBool = false;
                foreach (var e in SelectedObjectManager.Instance.currentSelectedElements)
                {
                    if (e.GetType() != type.GetType()) HideAllBool = true;
                    if (!e.associated3DObject) break;
                    if (!e.associated3DObject.activeInHierarchy) visbOn = true;
                }

                visibilityToggle.isOn = visbOn;

                UIManager.Instance.ResetTopForms();
            }
        }

        public void SetProperties(bool v)
        {
            m_hideProps = v;
        }

        public void CloseProperties()
        {
            SelectedObjectManager.Instance.ResetSelection();
        }

        public void HideAllProperties()
        {
            foreach (var p in AllProperties) p.Hide();
        }
    }
}