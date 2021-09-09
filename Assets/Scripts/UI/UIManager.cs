using System.Collections;
using ErgoShop.UI;
using ErgoShop.Utils;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEditor;

namespace ErgoShop.Managers
{
    /// <summary>
    ///     Main UI Manager
    ///     Used to show / hide the several parts of software UI
    ///     Also shows help into screen bottom
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        //Manage
        private WallsCreator Sc_WallsCreator;

        // Forms
        public GameObject projectForm, wallsForm, floorsForm, furnitureForm, othersForm, rectRoomForm, selectedOption, handicapForm;

        public GameObject stairsForm;

        public GameObject mergeRoomsPopin, customNamePopin , ShowPassageMessageObject;

        public Button focusFaceButton;

        // Up left buttons
        //public GameObject projectButton, wallsButton, furnitureButton, othersButton;

        // modes and views buttons
        //public Button textureButton, planButton, , faceButton, ;
        public Button topButton, threeDButton;

        public TextMeshProUGUI instructionsText;

        public Text customNamePopinInstructions;

        public Text screenShotMessage;

        // Cotation text
        public InputField cotationField;

        private ColorBlock m_normalColorsForModes, m_activeColorsForModes;

        public static UIManager Instance { get; private set; }

        public Color copieColeurPicker = new Color();

        // Start is called before the first frame update
        private void Start()
        {
            m_normalColorsForModes = threeDButton.colors;
            m_activeColorsForModes = m_normalColorsForModes;
            m_activeColorsForModes.normalColor = m_activeColorsForModes.pressedColor;
            ResetTopForms();
            instructionsText.text = "";
            Instance = this;
            ShowProjectForm();
        }

        // Update is called once per frame
        private void Update()
        {
            CheckTogglesColors();
            SetInstructionsText();
            stairsForm.SetActive(true);
            focusFaceButton.interactable = GlobalManager.Instance.Is3D();


            //message
            if (TimerShowMessage > 0)
            {
                TimerShowMessage -= Time.deltaTime;
                ShowPassageMessageObject.SetActive(true);
            }
            else
            {
                ShowPassageMessageObject.SetActive(false);
            }
        }

        /// <summary>
        ///     Updates instructions when no selection context
        /// </summary>
        private void SetInstructionsText()
        {
            if (HelpersCreator.Instance.IsOccupied())
            {
            }
            else if (SelectedObjectManager.Instance.HasNoSelection())
            {
                if (InputFunctions.IsMouseOutsideUI())
                {
                    if (GlobalManager.Instance.GetActiveCamera().gameObject == GlobalManager.Instance.cam2DTop)
                    {
                        Instance.instructionsText.text = "Déplacez la caméra avec le clic droit. F1 pour changer de vue vers la 3D sur mac fin + F1";
                    }
                    else if (GlobalManager.Instance.GetActiveCamera().gameObject == GlobalManager.Instance.cam3D)
                    {
                        Instance.instructionsText.text = "Déplacez la caméra avec les fleche et avec le clic molette tournez votre vue. F1 pour changer de vue vers la 2D, sur mac fin + F1";
                    }
                    else
                    {
                        Instance.instructionsText.text = "Déplacez la caméra avec le clic molette ou avec les fleches du clavier. Tournez la caméra avec le clic droit. F1 pour changer de vue, sur mac fin + F1.";
                    }
                    //Debug.Log(GlobalManager.Instance.GetActiveCamera());
                }
            }
        }

        /// <summary>
        ///     Updates view buttons colors
        /// </summary>
        private void CheckTogglesColors()
        {
            //textureButton.colors = m_normalColorsForModes;
            //planButton.colors = m_normalColorsForModes;
            topButton.colors = m_normalColorsForModes;
            //faceButton.colors = m_normalColorsForModes;
            threeDButton.colors = m_normalColorsForModes;
            switch (GlobalManager.Instance.GetCurrentMode())
            {
                case ViewMode.ThreeD:
                    threeDButton.colors = m_activeColorsForModes;
                    break;
                case ViewMode.Top:
                    topButton.colors = m_activeColorsForModes;
                    break;
            }

            //if (gm.GetCurrentTMode() == TextureMode.Plan) planButton.colors = m_activeColorsForModes;
            //else textureButton.colors = m_activeColorsForModes;
        }

        #region public methods

        /// <summary>
        ///     Hide forms
        /// </summary>
        public void ResetTopForms(bool ShowFurnitureForme = false)
        {
            projectForm.SetActive(false);

            for (var i = 0; i < selectedOption.transform.childCount; i++)
            {
                selectedOption.transform.GetChild(i).gameObject.SetActive(false);
            }

            wallsForm.SetActive(false);
            floorsForm.SetActive(false);
            rectRoomForm.SetActive(false);
            othersForm.SetActive(false);
            handicapForm.SetActive(false);

            if (!ShowFurnitureForme)
            {
                furnitureForm.SetActive(false);
            }
        }

        //uniquement pour le menu de creation de mur
        public void ResetTopFormsForCreationWall()
        {
            projectForm.SetActive(false);

            for (var i = 0; i < selectedOption.transform.childCount; i++)
            {
                selectedOption.transform.GetChild(i).gameObject.SetActive(false);
            }

            if (Sc_WallsCreator == null)
            {
                Sc_WallsCreator = FindObjectOfType<WallsCreator>();
                if (Sc_WallsCreator == null)
                {
                    Debug.LogError("UIManager 'Sc_WallsCreator' is null");
                }
            }


            Sc_WallsCreator.CancelRoom();

            wallsForm.SetActive(false);
            floorsForm.SetActive(false);
            rectRoomForm.SetActive(false);
            othersForm.SetActive(false);
            furnitureForm.SetActive(false);
        }

        public void ShowProjectForm()
        {
            ResetTopForms();
            projectForm.SetActive(true);
            selectedOption.transform.GetChild(0).gameObject.SetActive(true);
            SelectedObjectManager.Instance.ResetSelection();
        }

        public void ShowFloorsForm()
        {
            ResetTopForms();
            floorsForm.SetActive(true);
            selectedOption.transform.GetChild(1).gameObject.SetActive(true);
            SelectedObjectManager.Instance.ResetSelection();
        }

        public void ShowOthersForm()
        {
            ResetTopForms();
            othersForm.SetActive(true);
            selectedOption.transform.GetChild(3).gameObject.SetActive(true);
            SelectedObjectManager.Instance.ResetSelection();
        }

        float TimerShowMessage = 0;
        public void ShowPassageMessage(float _time)
        {
            TimerShowMessage = _time;
        }

        public void ShowWallsForm()
        {
            ResetTopForms();
            wallsForm.SetActive(true);
            selectedOption.transform.GetChild(1).gameObject.SetActive(true);
            SelectedObjectManager.Instance.ResetSelection();
        }

        public void ShowHandicapForm()
        {
            ResetTopForms();
            handicapForm.SetActive(true);
            //selectedOption.transform.GetChild(1).gameObject.SetActive(true);
            SelectedObjectManager.Instance.ResetSelection();
        }

        string filenamePath;
        public void ShowScreenShotOK(string filename)
        {
            filenamePath = filename;
            screenShotMessage.text = "Capture d'écran effectuée et enregistrée dans \n" + filename;
            StartCoroutine(ShowHideScreenShotMessage(5));
        }

        public void showScreenShootInExplorer()
        {
            //EditorUtility.RevealInFinder(filenamePath);
        }

        public void ShowMergeRoomsMessage()
        {
            mergeRoomsPopin.SetActive(true);
        }

        public void ShowStringBox(string message)
        {
            customNamePopin.SetActive(true);
            customNamePopinInstructions.text = message;
        }

        public void ShowSaveOK()
        {
            screenShotMessage.text = "Fichier sauvegardé.";
            StartCoroutine(ShowHideScreenShotMessage(1));
        }

        public void ShowCustomMessage(string message, float time = 1)
        {
            screenShotMessage.text = message;
            StartCoroutine(ShowHideScreenShotMessage(time));
        }

        private IEnumerator ShowHideScreenShotMessage(float time)
        {
            screenShotMessage.gameObject.SetActive(true);
            yield return new WaitForSeconds(time);
            screenShotMessage.gameObject.SetActive(false);
        }

        public void ShowFurnitureForm()
        {
            ResetTopForms();
            furnitureForm.SetActive(true);
            selectedOption.transform.GetChild(2).gameObject.SetActive(true);
            SelectedObjectManager.Instance.ResetSelection();
            FloorPropertiesScript.Instance.LoadFloorsFromProject();
        }

        public void OpenLinkWeb(string _link)
        {
            Application.OpenURL(_link);
        }

        #endregion
    }
}