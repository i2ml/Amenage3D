using System;
using System.Collections;
using System.IO;
using System.Linq;
using Crosstales.FB;
using ErgoShop.Cameras;
using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.Managers
{
    /// <summary>
    ///     Manager to handle screenshot system
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
                folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "ErgoShop/Screenshots");
                Directory.CreateDirectory(Instance.folderPath);
            }
        }

        private void Update()
        {
            if (folderPath != folderText.text)
                folderText.text = folderPath;
        }

        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F8)) TakeScreenshot();
        }

        /// <summary>
        ///     Get complete path with name for screenshot file
        /// </summary>
        /// <returns></returns>
        public string GetScreenShotName()
        {
            return Path.Combine(folderPath, GetNameWithoutNumber() + "_" + GetNumber() + ".png");
        }

        /// <summary>
        ///     Build a name for the screenshot with date
        /// </summary>
        /// <returns></returns>
        private string GetNameWithoutNumber()
        {
            return ProjectManager.Instance.Project.ProjectName + "_"
                                                               + (GlobalManager.Instance.GetActiveCamera().name
                                                                   .Contains("2D")
                                                                   ? "2D"
                                                                   : "3D") + "_"
                                                               + string.Format("{0:dd-MM-yyyy}",
                                                                   ProjectManager.Instance.Project.Date);
        }

        /// <summary>
        ///     Increment number if same screenshot
        /// </summary>
        /// <returns></returns>
        public string GetNumber()
        {
            var paths = Directory.GetFiles(folderPath).ToList();
            var number = 1;
            foreach (var path in paths)
            {
                if (!path.Contains('_')) continue;
                var nameWithoutNumber = path.Substring(0, path.LastIndexOf('_'));
                if (nameWithoutNumber == Path.Combine(folderPath, GetNameWithoutNumber())) number++;
            }

            return number.ToString().PadLeft(3, '0');
        }

        /// <summary>
        ///     Set a folder path to store screenshots and save the path in software settings
        /// </summary>
        public void SetFolderPath()
        {
            var fp = FileBrowser.OpenSingleFolder();
            if (!string.IsNullOrEmpty(fp))
            {
                folderPath = fp;
                SettingsManager.Instance.SoftwareParameters.ScreenShotFolder = folderPath;
                SettingsManager.Instance.SaveParameters();
            }
        }

        /// <summary>
        ///     Take a screenshot using RenderTexture and save it
        /// </summary>
        public void TakeScreenshot(bool isForOdt = false, string odtPath = "")
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            var fileName = GetScreenShotName();

            var rt = new RenderTexture(resWidth, resHeight, 24);
            GlobalManager.Instance.GetActiveCamera().targetTexture = rt;
            var screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
            GlobalManager.Instance.GetActiveCamera().Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
            GlobalManager.Instance.GetActiveCamera().targetTexture = null;
            RenderTexture.active = null; // JC: added to avoid errors
            Destroy(rt);
            var bytes = screenShot.EncodeToPNG();

            if (isForOdt)
            {
                File.WriteAllBytes(odtPath, bytes);
            }
            else
            {
                var filename = GetScreenShotName();
                File.WriteAllBytes(filename, bytes);
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

            var file = Path.Combine(odtImagesFolder, wname + ".png");

            // Take screenshot
            TakeScreenshot(true, file);
        }

        public string TakeBig3DScreenshot(string odtImagesFolder, Vector3 position, string wname)
        {
            // Set camera to have whole scene 3D
            GlobalManager.Instance.Set3DMode();

            GlobalManager.Instance.cam3D.GetComponent<Camera3DMove>().SetPosition(position);
            GlobalManager.Instance.cam3D.GetComponent<Camera3DMove>().SetRotation(Vector3.right * 90f);

            var file = Path.Combine(odtImagesFolder, wname + ".png");
            // Take screenshot
            TakeScreenshot(true, file);
            return file;
        }
    }
}