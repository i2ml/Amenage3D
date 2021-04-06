using System.Collections.Generic;
using ErgoShop.Operations;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ErgoShop.UI
{
    /// <summary>
    ///     List of autosaves for cancel/redo system
    ///     The user can click any to go to the wanted state
    ///     Manages also the buttons to cancel and redo
    /// </summary>
    public class OperationsScrollScript : MonoBehaviour, IListScrollScript
    {
        public GameObject OperationButtonPrefab;

        private List<Button> m_buttons;

        private ElementsScrollScript m_elemScroll;

        public static OperationsScrollScript Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        private void Start()
        {
            m_elemScroll = GetComponent<ElementsScrollScript>();
            m_buttons = new List<Button>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (!m_elemScroll) m_elemScroll = GetComponent<ElementsScrollScript>();
        }

        /// <summary>
        ///     Clean and loads all states into a button list
        /// </summary>
        public void UpdateList()
        {
            // Clear old buttons
            var children = new List<GameObject>();
            foreach (Transform child in m_elemScroll.Content)
            {
                children.Add(child.gameObject);
            }
            children.ForEach(child => Destroy(child));
            m_buttons.Clear();

            // Add new ones
            var operations = OperationsBufferScript.Instance.GetOperations();
            for (var i = operations.Count - 1; i >= 0; i--)
            {
                var autosave = operations[i];
                var btn = Instantiate(OperationButtonPrefab, m_elemScroll.Content);

                btn.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = autosave.OperationName;
                btn.gameObject.name = autosave.OperationName;

                btn.GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 30f);

                // Add anonynmous method to button
                btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    GoToOperation(autosave);
                    SetCurrentColor(btn.GetComponent<Button>());
                });

                if (i == operations.Count - 1)
                {
                    SetCurrentColor(btn.GetComponent<Button>());
                }
                m_buttons.Add(btn.GetComponent<Button>());
            }
        }

        void SortAlphab()
        {
            List<string> sortList = new List<string>();
            foreach (var item in m_buttons)
            {
                sortList.Add(item.GetComponentInChildren<TextMeshProUGUI>().text);
            }
            sortList.Sort();

            Debug.Log("///////////____________SortListe______________////////////");
            for (int j = 0; j < sortList.Count - 1; j++)
            {
                Debug.Log(sortList[j]);

                for (int i = 0; i < m_buttons.Count - 1; i++)
                {
                    if (sortList[j] == m_buttons[i].gameObject.GetComponentInChildren<TextMeshProUGUI>().text)
                    {
                        if (j > 0)
                        {
                            var tmp = sortList[j - 1];
                            sortList[j - 1] = sortList[j];
                            sortList[j] = tmp;
                            //i = 0;
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     When user clicks on a button
        /// </summary>
        /// <param name="auto">autosave we want to load</param>
        public void GoToOperation(AutoSave auto)
        {
            Debug.Log("goto " + auto.OperationName);
            OperationsBufferScript.Instance.GoToOperation(auto);
        }

        /// <summary>
        ///     Set button color to white
        /// </summary>
        public void ResetColors()
        {
            foreach (Transform child in m_elemScroll.Content)
            {
                var colors = child.GetComponent<Button>().colors;
                colors.normalColor = Color.white;
                child.GetComponent<Button>().colors = colors;
            }
        }

        /// <summary>
        ///     The current state is colored
        /// </summary>
        /// <param name="opId"></param>
        public void SetCurrentColor(int opId)
        {
            opId = m_buttons.Count - 1 - opId;
            if (opId >= 0) SetCurrentColor(m_buttons[opId]);
        }

        /// <summary>
        ///     The current state is colored in cyan
        /// </summary>
        /// <param name="opId"></param>
        public void SetCurrentColor(Button b)
        {
            ResetColors();
            var colors = b.colors;
            colors.normalColor = Color.cyan;
            colors.pressedColor = Color.cyan;
            b.colors = colors;
        }
    }
}