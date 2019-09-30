using Crosstales.FB;
using ErgoShop.Managers;
using ErgoShop.POCO;
using ErgoShop.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    /// <summary>
    /// UI For software settings
    /// </summary>
    public class SettingsPopinScript : MonoBehaviour
    {
        // PROJECT
        public InputField thicknessField, wallsHeightField;

        // SOFTWARE
        public Slider cameraSlider;
        public Dropdown tagWalltagAllDD;
        public Toggle showGridToggle;

        public static SettingsPopinScript Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void UpdateUI()
        {
            thicknessField.text = Mathf.Floor(ProjectManager.Instance.Project.WallThickness * 100f) + "";
            wallsHeightField.text = Mathf.Floor(ProjectManager.Instance.Project.WallHeight * 100f) + "";

            cameraSlider.value = SettingsManager.Instance.SoftwareParameters.CameraSpeed;
            tagWalltagAllDD.value = SettingsManager.Instance.SoftwareParameters.TagOnlyWall ? 1 : 0;

            showGridToggle.isOn = SettingsManager.Instance.SoftwareParameters.ShowGrid;
        }

        /// <summary>
        /// Get all data in UI and store it to json
        /// </summary>
        public void SaveParameters()
        {
            // Software
            SettingsManager.Instance.SoftwareParameters.CameraSpeed = cameraSlider.value;
            SettingsManager.Instance.SoftwareParameters.ShowGrid = showGridToggle.isOn;
            SettingsManager.Instance.SoftwareParameters.ScreenShotFolder = Screenshot.Instance.folderPath;
            SettingsManager.Instance.SoftwareParameters.TagOnlyWall = tagWalltagAllDD.value == 1;

            // Project
            ParsingFunctions.ParseFloatCommaDot(thicknessField.text, out float tf);
            ParsingFunctions.ParseFloatCommaDot(wallsHeightField.text, out float wf);
            ProjectManager.Instance.Project.WallHeight = WallsCreator.Instance.wallHeight = wf / 100f;
            ProjectManager.Instance.Project.WallThickness = WallsCreator.Instance.wallThickness = tf / 100f;

            // Auto Save Software parameters
            SettingsManager.Instance.SaveParameters();
        }

        private void OnEnable()
        {
            if (Time.time > 1)
            {
                if (SettingsManager.Instance.LoadOK)
                {
                    UpdateUI();
                }
            }
        }
    }
}