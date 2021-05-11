using System;
using System.Collections.Generic;
using System.IO;
using ErgoShop.POCO;
using Newtonsoft.Json;
using UnityEngine;

namespace ErgoShop.Managers
{
    /// <summary>
    ///     Manager to handle sotftware settings
    ///     Settings are splitted into two categories :
    ///     -> Project Settings, stored in Project file
    ///     -> Software Settings, stored in appdata in another json file
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        private string m_folder = "";
        private string m_jsonPath = "";

        /// <summary>
        ///     Software settings
        /// </summary>
        public ErgoShopParameters SoftwareParameters { get; set; }

        public bool LoadOK { get; set; }

        public static SettingsManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        private void Start()
        {
            m_folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ErgoShop");
            m_jsonPath = Path.Combine(m_folder, "ergoshop_parameters.json");

            if (!LoadParameters())
            {
                SoftwareParameters = new ErgoShopParameters
                {
                    ShowGrid = true
                };
                SaveParameters();
                LoadOK = false;
            }
            else
            {
                LoadOK = true;
            }

            // Custom Furnitures
            if (SoftwareParameters.CustomFurnitures == null)
                SoftwareParameters.CustomFurnitures = new List<CustomFurniture>();
            ImportManager.Instance.UpdateList();
        }

        // Update is called once per frame
        private void Update()
        {

        }

        /// <summary>
        ///     Load parameters from json file. If none is found, create a new one
        /// </summary>
        /// <returns>true if parameters found</returns>
        public bool LoadParameters()
        {
            
            JsonSerializerSettings settings = new JsonSerializerSettings();
            //settings.Converters.Add(new ColorConverter());
            //settings.Converters.Add(new QuaternionConverter());
            // %APPDATA%/ErgoShop
            if (File.Exists(m_jsonPath))
            {
                SoftwareParameters = JsonConvert.DeserializeObject<ErgoShopParameters>(File.ReadAllText(m_jsonPath), settings);

                //gm = FindObjectOfType<GlobalManager>();
                if (string.IsNullOrEmpty(SoftwareParameters.ScreenShotFolder))
                {
                    // In screenshot script
                }
                else
                {
                    Screenshot.Instance.folderPath = Instance.SoftwareParameters.ScreenShotFolder;
                }

                ThemeManager.Instance.ChangeTheme(SoftwareParameters.Theme);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Save parameters into json file
        /// </summary>
        public void SaveParameters()
        {
            //ThemeSave
            SoftwareParameters.Theme = ThemeManager.Instance.Theme;

            JsonSerializerSettings settings = new JsonSerializerSettings();
            //settings.Converters.Add(new ColorConverter());
            //settings.Converters.Add(new QuaternionConverter());
            String jsonString = JsonConvert.SerializeObject(SoftwareParameters, settings);
            File.WriteAllText(m_jsonPath, jsonString);
        }
    }
}