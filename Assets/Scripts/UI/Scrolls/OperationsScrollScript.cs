using System.Collections.Generic;
using ErgoShop.Operations;
using UnityEngine;
using UnityEngine.UI;

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
            foreach (Transform child in m_elemScroll.Content) children.Add(child.gameObject);
            children.ForEach(child => Destroy(child));
            m_buttons.Clear();

            // Add new ones
            var operations = OperationsBufferScript.Instance.GetOperations();
            for (var i = operations.Count - 1; i >= 0; i--)
            {
                var autosave = operations[i];
                var btn = Instantiate(OperationButtonPrefab, m_elemScroll.Content);
                btn.transform.GetChild(0).GetComponent<Text>().text = autosave.OperationName;
                btn.GetComponent<RectTransform>().sizeDelta = new Vector2(300f, 30f);
                // Add anonynmous method to button
                btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    GoToOperation(autosave);
                    SetCurrentColor(btn.GetComponent<Button>());
                });
                if (i == operations.Count - 1) SetCurrentColor(btn.GetComponent<Button>());
                m_buttons.Add(btn.GetComponent<Button>());
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