using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace ErgoShop.UI
{
    /// <summary>
    ///     Filter furnitures by type in the Add Furniture window.
    ///     Seeks gameobject names starting with two underscores
    ///     Those are the categories (assise, cuisine...)
    ///     This script sets those categories to be toggling and showing / hiding furnitures in them
    /// </summary>
    public class FurnitureListContentScript : MonoBehaviour
    {
        private bool hasBeenEnabled;
        RectTransform rect;
        short Nb_ActifChild = 0;

        private void Start()
        {
            rect = GetComponent<RectTransform>();
        }

        private void Update()
        {
            Nb_ActifChild = getNbActiveInHierarchy();
            rect.sizeDelta = new Vector2(0, 35 * Nb_ActifChild);
        }

        /// <summary>
        /// Compte le nombre d'enfants actif
        /// </summary>
        private short getNbActiveInHierarchy()
        {
            short nObjects = 0;
            for (short i = 0; i < transform.childCount; i++)
            {
                Transform t_Child = transform.GetChild(i);

                if (t_Child.gameObject.activeInHierarchy)
                {
                    nObjects++;
                }
            }
            return nObjects;
        }

        /// <summary>
        /// for find Furniture in the list 
        /// </summary>
        public void FindInBrowser(TMP_InputField _field)
        {
            // <get child>
            List<GameObject> listObjTmp = new List<GameObject>();
            for (int i = 0; i < transform.childCount; i++)
            {
                listObjTmp.Add(transform.GetChild(i).gameObject);
            }
            // <\get child>

            // <Sort>
            foreach (GameObject item in listObjTmp)
            {
                string tmpStr = item.GetComponentInChildren<Text>().text.ToLower().Trim();
                string fieldToLower = _field.text.ToLower().Trim();

                if (tmpStr != null)
                {
                    if (tmpStr.StartsWith(fieldToLower))
                    {
                        item.gameObject.SetActive(true);
                    }
                    else
                    {
                        item.gameObject.SetActive(false);
                    }
                }
            }
            // <\Sort>
        }

        private void OnEnable()
        {
            if (!hasBeenEnabled)
            {
                Toggle currentToggle = null;
                for (var i = 0; i < transform.childCount; i++)
                {
                    var go = transform.GetChild(i).gameObject;
                    // Categorie
                    if (go.name.StartsWith("__"))
                    {
                        currentToggle = go.AddComponent<Toggle>();
                    }
                    else
                    {
                        currentToggle.onValueChanged.AddListener(b => { go.SetActive(b); });
                        go.SetActive(false);
                    }
                }

                hasBeenEnabled = true;
            }
        }
    }
}