using System.Collections.Generic;
using ErgoShop.Managers;
using UnityEngine;
using ErgoShop.POCO;
using ErgoShop.UI;


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
            if(furnitureListScroll == null)
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
    }
}
