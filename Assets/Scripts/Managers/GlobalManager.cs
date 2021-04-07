using ErgoShop.Cameras;
using ErgoShop.Interactable;
using ErgoShop.UI;
using ErgoShop.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
///     To know if we are currently in 2D or 3D view
/// </summary>
public enum ViewMode
{
    Top,
    ThreeD
}

namespace ErgoShop.Managers
{
    /// <summary>
    ///     Main class used for software runtime
    /// </summary>
    public class GlobalManager : MonoBehaviour
    {
        /// <summary>
        ///     Software version. Used for export txt/odt
        /// </summary>
        public const float VERSION = 1.0f;

        /// <summary>
        ///     Instance
        /// </summary>
        public static GlobalManager Instance;
        //private WallArrowsScript was;
        //private WallOpeningArrowsScript waso;

        /// <summary>
        ///     Cam 2D
        /// </summary>
        public GameObject cam2DTop;

        /// <summary>
        ///     Cam 3D
        /// </summary>
        public GameObject cam3D;

        /// <summary>
        ///     Grid object to show or hide (2D)
        /// </summary>
        public GameObject grid;

        /// <summary>
        ///     The unused camera is rendered in a texture
        /// </summary>
        public RenderTexture previewTexture1;

        /// <summary>
        ///     Texture for cursor
        /// </summary>
        public Texture2D handTexture;

        /// <summary>
        ///     Texture for cursor
        /// </summary>
        public Texture2D arrowsTexture;

        /// <summary>
        ///     Texture for cursor
        /// </summary>
        public Texture2D plusTexture;

        ///<summary>
        ///Ref FurnitureScript
        /// </summary>
        public WallsCreator Sc_WallsCreator;

        /// <summary>
        ///     Current view
        /// </summary>
        private ViewMode m_mode;

        /// <summary>
        ///     Event system used for UI / Scene mouse management
        /// </summary>
        public EventSystem eventSystem { get; private set; }

        private void Awake()
        {
            Instance = this;

        }

        // Start is called before the first frame update
        private void Start()
        {
            eventSystem = FindObjectOfType<EventSystem>();

            Sc_WallsCreator = FindObjectOfType<WallsCreator>();
            if (Sc_WallsCreator == null)
            {
                Debug.LogWarning("GlobalManager 'WallsCreator' Script is null");
            }

            Set2DTopMode();

            FloorPropertiesScript.Instance.LoadFloorsFromProject();
        }

        // Update is called once per frame
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1) && InputFunctions.IsMouseOutsideUI()) SwitchViewMode();

            grid.SetActive(SettingsManager.Instance.SoftwareParameters.ShowGrid);

            var go = InputFunctions.GetHoveredObject2D(cam2DTop.GetComponent<Camera>());
            var isElementArrow = false;
            if (go && go.tag.Contains("Arrow")) isElementArrow = true;

            // CURSOR
            UpdateCursor(isElementArrow);

            // Camera render texture

            if (m_mode == ViewMode.Top)
            {
                //if (!cam3D.GetComponent<Camera>().orthographic)
                //{
                //    cam3D.GetComponent<Camera3DMove>().SetTopView();
                //}
                cam3D.transform.localEulerAngles = Vector3.right * 90f;
                cam3D.GetComponent<Camera3DMove>().SetPosition(VectorFunctions.Switch2D3D(
                    cam2DTop.transform.position + Vector3.right * 2.5f, cam2DTop.transform.position.z * -1f));
            }
            else if (m_mode == ViewMode.ThreeD)
            {
                if (cam3D.GetComponent<Camera>().orthographic) cam3D.GetComponent<Camera3DMove>().SetNormalView();
                cam2DTop.GetComponent<Camera2DMove>().SetPosition(cam3D.transform.position + Vector3.right * 2.5f);
            }

            // UI Bindings
            //UI.faceButton.interactable = som.currentWallsData.Count == 1;
        }

        /// <summary>
        ///     Update room height PARAMETERS (project -> software settings)
        /// </summary>
        /// <param name="heightText"></param>
        public void UpdateRoomHeight(string heightText)
        {
            WallsCreator.Instance.wallHeight = int.Parse(heightText);
        }

        /// <summary>
        ///     Change cursor according to context
        /// </summary>
        /// <param name="isElementArrow">Force changing to hand</param>
        public void UpdateCursor(bool isElementArrow)
        {
            if (SelectedObjectManager.Instance.IsOccupied() || ElementArrowsScript.Instance.isMoving)
                Cursor.SetCursor(arrowsTexture, new Vector2(40, 0), CursorMode.Auto);
            else if (WallsCreator.Instance.IsCreating() || isElementArrow || HelpersCreator.Instance.IsOccupied())
                Cursor.SetCursor(handTexture, new Vector2(40, 0), CursorMode.Auto);
            else if (MeasureManager.Instance.IsMesuring)
                Cursor.SetCursor(plusTexture, new Vector2(16, 16), CursorMode.Auto);
            else
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        /// <summary>
        ///     Returns active Camera component according to view
        /// </summary>
        /// <returns></returns>
        public Camera GetActiveCamera()
        {
            switch (m_mode)
            {
                case ViewMode.Top:
                    return cam2DTop.GetComponent<Camera>();
                case ViewMode.ThreeD:
                    return cam3D.GetComponent<Camera>();
            }

            return null;
        }

        /// <summary>
        ///     Are we in 3D ?
        /// </summary>
        /// <returns>true if 3D</returns>
        public bool Is3D()
        {
            return m_mode == ViewMode.ThreeD;
        }

        /// <summary>
        ///     Check all managers to see if camera must be locked or not
        /// </summary>
        /// <param name="cam"></param>
        /// <returns>true if camera can move</returns>
        public bool CanCameraMove(Camera2DMove cam)
        {
            return !SelectedObjectManager.Instance.IsOccupied()
                   && !WallsCreator.Instance.IsCreating()
                   && InputFunctions.IsMouseOutsideUI()
                   && cam.GetComponent<Camera>().targetTexture == null
                   && !WallArrowsScript.Instance.isMoving
                   && !ElementArrowsScript.Instance.isMoving
                   && !HelpersCreator.Instance.IsOccupied()
                   && !ProjectManager.Instance.IsOccupied();
            //&& !WallOpeningArrowsScript.Instance.isMoving;
        }

        /// <summary>
        ///     Check all managers to see if camera must be locked or not
        /// </summary>
        /// <param name="cam"></param>
        /// <returns>true if camera can move</returns>
        public bool CanCameraMove(Camera3DMove cam)
        {
            return !SelectedObjectManager.Instance.IsOccupied()
                   && !WallsCreator.Instance.IsCreating()
                   && InputFunctions.IsMouseOutsideUI()
                   && cam.GetComponent<Camera>().targetTexture == null
                   && !WallArrowsScript.Instance.isMoving
                   && !ElementArrowsScript.Instance.isMoving
                   && !HelpersCreator.Instance.IsOccupied()
                   && !ProjectManager.Instance.IsOccupied()
                   && InputFunctions.IsMouseOutsideUI();
            //&& !WallOpeningArrowsScript.Instance.isMoving;
        }

        /// <summary>
        ///     Current view
        /// </summary>
        /// <returns>2d/3d</returns>
        public ViewMode GetCurrentMode()
        {
            return m_mode;
        }

        public void QuitApp()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #region view modes

        /// <summary>
        ///     Switch between 2D and 3D
        /// </summary>
        public void SwitchViewMode()
        {
            // Top -> Face -> ThreeD -> Top (etc)
            switch (m_mode)
            {
                case ViewMode.Top:
                    Set3DMode();
                    break;
                case ViewMode.ThreeD:
                    Set2DTopMode();
                    break;
            }
        }

        /// <summary>
        ///     Switch to 3D view
        /// </summary>
        public void Set3DMode()
        {
            m_mode = ViewMode.ThreeD;

            cam3D.GetComponent<Camera3DMove>().SetNormalView();

            cam2DTop.GetComponent<Camera>().enabled = true;
            cam2DTop.GetComponent<Camera>().forceIntoRenderTexture = true;
            cam2DTop.GetComponent<Camera>().targetTexture = previewTexture1;

            cam3D.GetComponent<Camera>().forceIntoRenderTexture = false;
            cam3D.GetComponent<Camera>().targetTexture = null;
        }

        /// <summary>
        ///     Switch to 2D view
        /// </summary>
        public void Set2DTopMode()
        {
            m_mode = ViewMode.Top;
            cam2DTop.GetComponent<Camera>().enabled = true;
            cam3D.GetComponent<Camera>().forceIntoRenderTexture = true;
            cam3D.GetComponent<Camera>().targetTexture = previewTexture1;

            cam2DTop.GetComponent<Camera>().forceIntoRenderTexture = false;
            cam2DTop.GetComponent<Camera>().targetTexture = null;
            cam2DTop.GetComponent<Camera2DMove>().SetPosition(cam3D.transform.position - Vector3.right * 2.5f);
        }

        #endregion
    }
}