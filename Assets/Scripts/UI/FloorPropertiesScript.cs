using ErgoShop.Managers;
using ErgoShop.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    /// <summary>
    ///     UI to create and change floors
    /// </summary>
    public class FloorPropertiesScript : MonoBehaviour
    {
        // IN WALLS FORM
        public Dropdown currentFloorDD;

        // IN FLOOR FORM
        public Dropdown floorToCopyDD;
        public InputField nameField, heightField;
        public Toggle copyToggle;

        public static FloorPropertiesScript Instance { get; private set; }

        // Start is called before the first frame update
        private void Start()
        {
            Instance = this;
            currentFloorDD.onValueChanged.AddListener(v =>
            {
                ProjectManager.Instance.SetCurrentFloor(v);
                currentFloorDD.captionText.text = ProjectManager.Instance.Project.GetCurrentFloor().FloorName;
            });

            copyToggle.onValueChanged.AddListener(b =>
            {
                if (b) LoadFloorsFromProject();
            });

            floorToCopyDD.onValueChanged.AddListener(v =>
            {
                Debug.Log("Copy value changed : " + v);
                floorToCopyDD.captionText.text = ProjectManager.Instance.Project.Floors[v].FloorName;
            });
        }

        // Update is called once per frame
        private void Update()
        {
            floorToCopyDD.gameObject.SetActive(copyToggle.isOn);
        }

        /// <summary>
        ///     update dropdown ui
        /// </summary>
        /// <param name="name"></param>
        public void AddFloorToDD(string name)
        {
            currentFloorDD.options.Add(new Dropdown.OptionData
            {
                text = name
            });
        }

        /// <summary>
        ///     Load a floor according to selected one in dropdown
        /// </summary>
        /// <param name="v"></param>
        public void SetCurrentFloor(int v)
        {
            LoadFloorsFromProject();
            currentFloorDD.value = v;
        }

        /// <summary>
        ///     Get floors in project and update dropdowns
        /// </summary>
        public void LoadFloorsFromProject()
        {
            currentFloorDD.ClearOptions();
            floorToCopyDD.ClearOptions();
            foreach (var floor in ProjectManager.Instance.Project.Floors)
            {
                currentFloorDD.options.Add(new Dropdown.OptionData
                {
                    text = floor.FloorName + ""
                });
                floorToCopyDD.options.Add(new Dropdown.OptionData
                {
                    text = floor.FloorName + ""
                });
            }
        }

        /// <summary>
        ///     Create a new floor with parameters
        /// </summary>
        public void CreateFloor()
        {
            if (!ParsingFunctions.ParseFloatCommaDot(heightField.text, out var wh)) return;
            ProjectManager.Instance.AddFloor(nameField.text, copyToggle.isOn,
                floorToCopyDD.options[floorToCopyDD.value].text, wh);
        }
    }
}