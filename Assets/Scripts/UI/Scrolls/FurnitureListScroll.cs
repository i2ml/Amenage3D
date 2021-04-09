using System.Collections.Generic;
using ErgoShop.Managers;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ErgoShop.POCO;

namespace ErgoShop.UI
{
    /// <summary>
    ///     List of all furnitures in scene
    ///     User can click on it to select and view it
    /// </summary>
    public class FurnitureListScroll : MonoBehaviour, IListScrollScript
    {
        public GameObject OperationButtonPrefab;

        public bool sortList = false;

        private ElementsScrollScript m_elemScroll;

        public static FurnitureListScroll Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        private void Start()
        {
            m_elemScroll = GetComponent<ElementsScrollScript>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (!m_elemScroll) m_elemScroll = GetComponent<ElementsScrollScript>();
        }

        /// <summary>
        ///     Clean and update list
        ///     Generates buttons, each one associated to a furniture
        /// </summary>
        public void UpdateList()
        {
            /////////////////////////////////// SORT
            var children = new List<GameObject>();
            foreach (Transform child in m_elemScroll.Content)
            {
                children.Add(child.gameObject);
            }

            children.ForEach(child => Destroy(child));

            //foreach (var item in children)
            //{
            //    Destroy();
            //}

            List<Furniture> allFurnitures = FurnitureCreator.Instance.GetFurnitures();



            if (sortList)
            {
                bool finish = false;
                while (!finish)
                {
                    //Aucun Ellement a trier
                    finish = true;

                    for (int i = 0; i < allFurnitures.Count - 1; i++)
                    {
                        if (allFurnitures[i].Name.CompareTo(allFurnitures[i + 1].Name) > 0)
                        {
                            finish = false; // quelque chose a trier 

                            Furniture furniture_tmp = allFurnitures[i + 1];
                            allFurnitures[i + 1] = allFurnitures[i];
                            allFurnitures[i] = furniture_tmp;
                        }
                    }
                }
            }

            ///////////////////////////////////

            foreach (var furniture in allFurnitures)
            {
                GameObject btn = Instantiate(OperationButtonPrefab, m_elemScroll.Content);

                btn.name = furniture.Name;
                btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = furniture.Name;
                btn.GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 30f);

                // anonymous method to select the furniture
                btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    SelectedObjectManager.Instance.Select(furniture, false);
                    SelectedObjectManager.Instance.FocusOnSelection();
                });
            }
        }

        /// <summary>
        /// Return True if List is EMPTY
        /// </summary>
        /// <returns></returns>
        public bool isEmpty()
        {
            int result = 0;
            foreach (RectTransform child in m_elemScroll.Content)
            {
                result++;
            }
            return result > 0 ? false : true;
        }
    }
}