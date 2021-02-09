using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Crosstales.FB;
using Dummiesman;
using ErgoShop.POCO;
using ErgoShop.UI;
using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.Managers
{
    public class ImportManager : MonoBehaviour
    {
        public GameObject CustomFurniturePrefab;
        public GameObject CustomFurnitureButton;
        public GameObject modelsListContent;

        public List<GameObject> customFurnituresList;

        // the list with ALL FURNITURES in UI
        public GameObject furnitureDefaultListContent;

        public static ImportManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        private void Start()
        {
            customFurnituresList = new List<GameObject>();
        }

        // Update is called once per frame
        private void Update()
        {
        }

        /// <summary>
        ///     Update the list in software settings when a custom furniture is deleted or added
        /// </summary>
        public void UpdateList()
        {
            // CLEAN LISTS
            var children = new List<GameObject>();
            foreach (Transform child in modelsListContent.transform) children.Add(child.gameObject);
            children.ForEach(child => Destroy(child));


            while (customFurnituresList.Count > 0)
            {
                var go = customFurnituresList.FirstOrDefault().gameObject;
                customFurnituresList.Remove(go);
                Destroy(go);
            }


            // REFILL CUSTOM LIST IN PARAMS
            foreach (var cf in SettingsManager.Instance.SoftwareParameters.CustomFurnitures)
            {
                var go = Instantiate(CustomFurniturePrefab, modelsListContent.transform);

                go.GetComponent<Text>().text = cf.Name;
                go.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() =>
                {
                    SettingsManager.Instance.SoftwareParameters.CustomFurnitures.Remove(cf);
                    SettingsManager.Instance.SaveParameters();
                    UpdateList();
                });
            }

            // REFILL FURNITURE LIST
            foreach (var cf in SettingsManager.Instance.SoftwareParameters.CustomFurnitures)
            {
                var go = Instantiate(CustomFurnitureButton, furnitureDefaultListContent.transform);
                customFurnituresList.Add(go);
                go.transform.GetChild(0).GetComponent<Text>().text = cf.Name;
                go.GetComponent<Button>().onClick.AddListener(() =>
                {
                    FurnitureCreator.Instance.AddCustomFurniture(GetGameObjectFromCustomObject(cf), cf);
                });
            }
        }

        /// <summary>
        ///     Select an obj file in browser and waits for a name
        /// </summary>
        public void ImportNewObject()
        {
            var filePath = FileBrowser.OpenSingleFile("obj");
            if (!string.IsNullOrEmpty(filePath))
            {
                UIManager.Instance.ShowStringBox("Entrez un nom pour le modèle et appuyez sur Entrée.");
                StartCoroutine(WaitForName(filePath));
            }
        }

        /// <summary>
        ///     Loads the obj file contained in cf.Path
        /// </summary>
        /// <param name="cf">CustomFurniture we want to load</param>
        /// <returns>GameObject (without colliders !)</returns>
        public GameObject GetGameObjectFromCustomObject(CustomFurniture cf)
        {
            return new OBJLoader().Load(cf.Path);
        }

        /// <summary>
        ///     Waits for user entering a name in inputfield then copy paste the obj file in AppData
        ///     Also Saves the path and name entered in software settings
        /// </summary>
        /// <param name="filePath">the path we got in ImportNewObject() method</param>
        /// <returns>(coroutine)</returns>
        private IEnumerator WaitForName(string filePath)
        {
            while (!CustomNamePopinScript.Instance.NameEntered) yield return null;

            CustomNamePopinScript.Instance.NameEntered = false;

            var appdata = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData), "ErgoShop", "Imports");

            Directory.CreateDirectory(appdata);

            var dest = Path.Combine(appdata,
                Path.GetFileNameWithoutExtension(filePath) + DateTime.Now.Day + "_" + DateTime.Now.Hour + "_" +
                DateTime.Now.Minute + "_" + DateTime.Now.Second + ".obj");
            Debug.Log("dest = " + dest);
            File.Copy(filePath, dest);

            SettingsManager.Instance.SoftwareParameters.CustomFurnitures.Add(
                new CustomFurniture
                {
                    Path = dest,
                    Name = CustomNamePopinScript.Instance.Name
                });

            SettingsManager.Instance.SaveParameters();
            UpdateList();
        }
    }
}