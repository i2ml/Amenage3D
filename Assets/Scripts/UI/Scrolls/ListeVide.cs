using System.Collections.Generic;
using ErgoShop.Managers;
using UnityEngine;
using ErgoShop.POCO;
using ErgoShop.UI;
using TMPro;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    public class ListeVide : MonoBehaviour
    {
        public GameObject txt;

        private FurnitureListScroll furnitureListScroll;

        // Start is called before the first frame update
        void Start()
        {
            furnitureListScroll = gameObject.GetComponent<FurnitureListScroll>();
            if (furnitureListScroll == null)
            {
                Debug.LogError(" FurnitureListScroll is null in ListeVide");
            }

            txt.SetActive(true);
        }

        // Update is called once per frame
        void Update()
        {
            if (furnitureListScroll.isEmpty())
            {
                txt.SetActive(true);
            }
            else
            {
                txt.SetActive(false);
            }
        }

        /// <summary>
        /// for find Furniture in the list 
        /// </summary>
        public void FindInBrowser(TMP_InputField _field)
        {
            List<GameObject> tmp_listScroll = furnitureListScroll.getContent();

            if (!furnitureListScroll.isEmpty() || tmp_listScroll.Count != 0)
            {

                // <Sort>
                foreach (GameObject item in tmp_listScroll)
                {
                    string tmpStr = item.GetComponentInChildren<TextMeshProUGUI>().text.ToLower().Trim();
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
        }
    }
}
