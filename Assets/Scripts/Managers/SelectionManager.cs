using ErgoShop.Interactable;
using ErgoShop.UI;
using ErgoShop.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoShop.Managers
{
    /// <summary>
    /// Class to draw a rectangle to select several elements
    /// Is used by SelectedObjectManagers
    /// </summary>
    public class SelectionManager : MonoBehaviour
    {
        /// <summary>
        /// Texture used to show what we are selecting
        /// </summary>
        static Texture2D _whiteTexture;

        public static SelectionManager Instance { get; private set; }

        public bool IsSelecting = false;
        private Vector3 firstPosition;
        private bool firstClickOk;

        private void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            // If we press the left mouse button, save mouse location and begin selection
            if (InputFunctions.IsClickingOutsideUI())
            {
                firstPosition = Input.mousePosition;
                firstClickOk = true;
            }

            if (Input.GetMouseButton(0) && firstClickOk && Vector3.Distance(firstPosition, Input.mousePosition) > 5f)
            {
                IsSelecting = true;
            }

            // If we let go of the left mouse button, end selection
            if (Input.GetMouseButtonUp(0) || ElementArrowsScript.Instance.isMoving || WallArrowsScript.Instance.isMoving || SelectedObjectManager.Instance.IsOccupied() || WallsCreator.Instance.IsCreating())
            {
                firstClickOk = false;
            }
        }

        /// <summary>
        /// Draw function
        /// </summary>
        void OnGUI()
        {
            if (IsSelecting)
            {
                // Create a rect from both mouse positions
                var rect = GetScreenRect(firstPosition, Input.mousePosition);
                DrawScreenRect(rect, new Color(0.8f, 0.8f, 0.95f, 0.25f));
                DrawScreenRectBorder(rect, 2, new Color(0.8f, 0.8f, 0.95f));
            }
        }

        /// <summary>
        /// Check if a position is between our selection rectangle bounds
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public bool IsWithinSelectionBounds(Vector3 position)
        {
            if (!IsSelecting)
                return false;

            Camera camera = GlobalManager.Instance.GetActiveCamera();

            if (camera == GlobalManager.Instance.cam2DTop.GetComponent<Camera>())
            {
                position = VectorFunctions.Switch3D2D(position);
            }

            Bounds viewportBounds =
                GetViewportBounds(camera, firstPosition, Input.mousePosition);

            return viewportBounds.Contains(
                camera.WorldToViewportPoint(position));
        }

        /// <summary>
        /// Get bounds from camera and rectangle positions
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="screenPosition1"></param>
        /// <param name="screenPosition2"></param>
        /// <returns></returns>
        public static Bounds GetViewportBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2)
        {
            var v1 = camera.ScreenToViewportPoint(screenPosition1);
            var v2 = camera.ScreenToViewportPoint(screenPosition2);
            var min = Vector3.Min(v1, v2);
            var max = Vector3.Max(v1, v2);
            var bounds = new Bounds();

            // 3D
            if (camera.gameObject.layer == (int)ErgoLayers.ThreeD)
            {
                min.z = camera.nearClipPlane;
                max.z = camera.farClipPlane;
            }
            else
            {
                // 2D ?
                min.z = -100;
                max.z = 100;
                //min.y *= -1f;
                //max.y *= -1f;
            }


            bounds.SetMinMax(min, max);

            return bounds;

        }

        /// <summary>
        /// Draws border for rectangle selection
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="thickness"></param>
        /// <param name="color"></param>
        public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
        {
            // Top
            DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
            // Left
            DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
            // Right
            DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
            // Bottom
            DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
        }

        /// <summary>
        /// Get screen rectangle to help setting viewport
        /// </summary>
        /// <param name="screenPosition1"></param>
        /// <param name="screenPosition2"></param>
        /// <returns></returns>
        public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
        {
            // Move origin from bottom left to top left
            screenPosition1.y = Screen.height - screenPosition1.y;
            screenPosition2.y = Screen.height - screenPosition2.y;
            // Calculate corners
            var topLeft = Vector3.Min(screenPosition1, screenPosition2);
            var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
            // Create Rect
            return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        }

        public static Texture2D WhiteTexture
        {
            get
            {
                if (_whiteTexture == null)
                {
                    _whiteTexture = new Texture2D(1, 1);
                    _whiteTexture.SetPixel(0, 0, Color.white);
                    _whiteTexture.Apply();
                }

                return _whiteTexture;
            }
        }

        /// <summary>
        /// Draw rectangle
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="color"></param>
        public static void DrawScreenRect(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, WhiteTexture);
            GUI.color = Color.white;
        }
    }
}