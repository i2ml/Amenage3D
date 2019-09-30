using ErgoShop.Managers;
using ErgoShop.POCO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    /// <summary>
    /// List of all furnitures in scene
    /// User can click on it to select and view it
    /// </summary>
    public class FurnitureListScroll : MonoBehaviour, IListScrollScript
    {
        public GameObject OperationButtonPrefab;

        public static FurnitureListScroll Instance { get; private set; }

        private ElementsScrollScript m_elemScroll;

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            m_elemScroll = GetComponent<ElementsScrollScript>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_elemScroll)
            {
                m_elemScroll = GetComponent<ElementsScrollScript>();
            }
        }

        /// <summary>
        /// Clean and update list
        /// Generates buttons, each one associated to a furniture
        /// </summary>
        public void UpdateList()
        {
            var children = new List<GameObject>();
            foreach (Transform child in m_elemScroll.Content) children.Add(child.gameObject);
            children.ForEach(child => Destroy(child));

            foreach (Furniture furniture in FurnitureCreator.Instance.GetFurnitures())
            {
                var btn = Instantiate(OperationButtonPrefab, m_elemScroll.Content);
                btn.transform.GetChild(0).GetComponent<Text>().text = furniture.Name;
                btn.GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 30f);
                // anonymous method to select the furniture
                btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    SelectedObjectManager.Instance.Select(furniture, false);
                    SelectedObjectManager.Instance.FocusOnSelection();
                });
            }
        }
    }
}