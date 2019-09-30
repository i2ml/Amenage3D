using Crosstales.FB;
using ErgoShop.Cameras;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.Managers
{
    /// <summary>
    /// Manager to handle screenshot system
    /// </summary>
    public class Screenshot : MonoBehaviour
    {
        public int resWidth = 1920;
        public int resHeight = 1080;

        public string folderPath;

        public Text folderText;

        public static Screenshot Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (folderPath == "")
            {
                folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ErgoShop/Screenshots");
                Directory.CreateDirectory(Screenshot.Instance.folderPath);
            }
        }

        private void Update()
        {
            if (folderPath != folderText.text)
                folderText.text = folderPath;
        }

        /// <summary>
        /// Get complete path with name for screenshot file
        /// </summary>
        /// <returns></returns>
        public string GetScreenShotName()
        {
            return Path.Combine(folderPath, GetNameWithoutNumber() + "_" + GetNumber() + ".png");
        }

        /// <summary>
        /// Build a name for the screenshot with date
        /// </summary>
        /// <returns></returns>
        private string GetNameWithoutNumber()
        {
            return ProjectManager.Instance.Project.ProjectName + "_"
                + (GlobalManager.Instance.GetActiveCamera().name.Contains("2D") ? "2D" : "3D") + "_"
                + String.Format("{0:dd-MM-yyyy}", ProjectManager.Instance.Project.Date);
        }

        /// <summary>
        /// Increment number if same screenshot
        /// </summary>
        /// <returns></returns>
        public string GetNumber()
        {
            List<string> paths = Directory.GetFiles(folderPath).ToList();
            int number = 1;
            foreach (string path in paths)
            {
                if (!path.Contains('_')) continue;
                string nameWithoutNumber = path.Substring(0, path.LastIndexOf('_'));
                if (nameWithoutNumber == Path.Combine(folderPath, GetNameWithoutNumber()))
                {
                    number++;
                }
            }
            return number.ToString().PadLeft(3, '0');
        }

        void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                TakeScreenshot();
            }
        }

        /// <summary>
        /// Set a folder path to store screenshots and save the path in software settings
        /// </summary>
        public void SetFolderPath()
        {
            string fp = FileBrowser.OpenSingleFolder();
            if (!string.IsNullOrEmpty(fp))
            {
                folderPath = fp;
                SettingsManager.Instance.SoftwareParameters.ScreenShotFolder = folderPath;
                SettingsManager.Instance.SaveParameters();
            }
        }

        /// <summary>
        /// Take a screenshot using RenderTexture and save it
        /// </summary>
        public void TakeScreenshot(bool isForOdt = false, string odtPath="")
        {
            if (!System.IO.Directory.Exists(folderPath))
                System.IO.Directory.CreateDirectory(folderPath);
            string fileName = GetScreenShotName();

            RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
            GlobalManager.Instance.GetActiveCamera().targetTexture = rt;
            Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            GlobalManager.Instance.GetActiveCamera().Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            GlobalManager.Instance.GetActiveCamera().targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            byte[] bytes = screenShot.EncodeToPNG();

            if (isForOdt)
            {
                System.IO.File.WriteAllBytes(odtPath, bytes);
            }
            else
            {
                string filename = GetScreenShotName();
                System.IO.File.WriteAllBytes(filename, bytes);
                Debug.Log(string.Format("Took screenshot to: {0}", filename));

                //ScreenCapture.CaptureScreenshot(filename);
                UIManager.Instance.ShowScreenShotOK(filename);
            }
        }

        public IEnumerator TakeBig2DScreenshot(string odtImagesFolder, Vector3 position, string wname)
        {
            // Set camera to have whole scene 2D
            GlobalManager.Instance.Set3DMode();

            GlobalManager.Instance.cam3D.GetComponent<Camera3DMove>().SetPosition(position);
            GlobalManager.Instance.cam3D.GetComponent<Camera3DMove>().SetRotation(Vector3.right * 90f);

            yield return new WaitForSeconds(0.3f);

            GlobalManager.Instance.Set2DTopMode();

            yield return new WaitForSeconds(0.3f);

            string file = Path.Combine(odtImagesFolder, wname + ".png");

            // Take screenshot
            TakeScreenshot(true, file);
        }

        public string TakeBig3DScreenshot(string odtImagesFolder, Vector3 position, string wname)
        {
            // Set camera to have whole scene 3D
            GlobalManager.Instance.Set3DMode();
            
            GlobalManager.Instance.cam3D.GetComponent<Camera3DMove>().SetPosition(position);
            GlobalManager.Instance.cam3D.GetComponent<Camera3DMove>().SetRotation(Vector3.right*90f);

            string file = Path.Combine(odtImagesFolder, wname + ".png");
            // Take screenshot
            TakeScreenshot(true, file);
            return file;
        }

    }
}