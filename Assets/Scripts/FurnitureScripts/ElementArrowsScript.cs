using ErgoShop.Managers;
using ErgoShop.Operations;
using ErgoShop.POCO;
using ErgoShop.UI;
using ErgoShop.Utils;
using ErgoShop.Utils.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ErgoShop.Interactable
{
    /// <summary>
    /// Behaviour to put handles on a selected element, to change its size.
    /// Used in 2D View
    /// </summary>
    public class ElementArrowsScript : MonoBehaviour
    {
        /// <summary>
        /// the 8 buttons
        /// </summary>
        public GameObject up, down, left, right, upLeft, upRight, downLeft, downRight;

        private List<GameObject> m_allArrows;
        private GameObject currentArrow;

        private Vector3 m_prevPos;
        private Vector3 oUp, oDown, oLeft, oRight, oUpLeft, oUpRight, oDownLeft, oDownRight;

        // Check if this script is used to prevent other "click" events
        public bool isMoving;

        // Instance
        public static ElementArrowsScript Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            m_allArrows = new List<GameObject>
            {
                up, down, left, right, upLeft, upRight, downLeft, downRight
            };
        }

        /// <summary>
        /// adjust buttons position according to element bounds
        /// </summary>
        /// <param name="me">Element</param>
        /// <param name="pos">Element position (2d)</param>
        /// <param name="size">Element size (2d)</param>
        void SetOrigins(MovableElement me, Vector3 pos, Vector3 size)
        {
            oUp = pos + size.y * me.associated2DObject.transform.up;
            oDown = pos - size.y * me.associated2DObject.transform.up;
            oLeft = pos - size.x * me.associated2DObject.transform.right;
            oRight = pos + size.x * me.associated2DObject.transform.right;
            oUpLeft = pos - size.x * me.associated2DObject.transform.right + size.y * me.associated2DObject.transform.up;
            oUpRight = pos + size.x * me.associated2DObject.transform.right + size.y * me.associated2DObject.transform.up;
            oDownLeft = pos - size.x * me.associated2DObject.transform.right - size.y * me.associated2DObject.transform.up;
            oDownRight = pos + size.x * me.associated2DObject.transform.right - size.y * me.associated2DObject.transform.up;
        }

        // Update is called once per frame
        void Update()
        {
            if (GlobalManager.Instance.GetActiveCamera().tag != "Cam2D")
            {
                foreach (var g in m_allArrows)
                {
                    g.SetActive(false);
                }
                return;
            }
            bool show = SelectedObjectManager.Instance.currentSelectedElements.Where(e => e is MovableElement).Count() == 1;
            // Ajout exceptions : murs et ouvertures de murs
            show = show && SelectedObjectManager.Instance.currentWallsData.Count == 0
                && SelectedObjectManager.Instance.currentWallOpenings.Count == 0
                && SelectedObjectManager.Instance.currentStairs.Count == 0;
            foreach (var g in m_allArrows)
            {
                g.SetActive(show);
            }

            if (show)
            {
                MovableElement me = SelectedObjectManager.Instance.currentSelectedElements.First() as MovableElement;
                if (SelectedObjectManager.Instance.currentSelectedElements.Count() == 0 || me == null) return;
                Vector3 pos = me.associated2DObject.transform.position;//VectorFunctions.Switch3D2D(me.Position);
                Vector3 size = VectorFunctions.Switch3D2D(me.Size / 2f);

                SetOrigins(me, pos, size);

                if (up != currentArrow)
                {
                    up.transform.position = oUp;
                }
                if (down != currentArrow)
                {
                    down.transform.position = oDown;
                }
                if (left != currentArrow)
                {
                    left.transform.position = oLeft;
                }
                if (right != currentArrow)
                {
                    right.transform.position = oRight;
                }
                if (upLeft != currentArrow)
                {
                    upLeft.transform.position = oUpLeft;
                }
                if (upRight != currentArrow)
                {
                    upRight.transform.position = oUpRight;
                }
                if (downLeft != currentArrow)
                {
                    downLeft.transform.position = oDownLeft;
                }
                if (downRight != currentArrow)
                {
                    downRight.transform.position = oDownRight;
                }
            }
            if (show)
            {
                foreach (var go in m_allArrows)
                {
                    go.transform.localScale = Vector3.one * Mathf.Abs(GlobalManager.Instance.cam2DTop.transform.position.z / 10f);
                }
                ClickArrow();
            }
            else
            {
                isMoving = false;
            }

        }

        /// <summary>
        /// Updates arrow according to movement
        /// </summary>
        /// <param name="dif"></param>
        /// <param name="inDirection"></param>
        /// <param name="inSizeCompo"></param>
        /// <param name="direction"></param>
        /// <param name="sizeCompo"></param>
        private void SetSizeCompoAndDirection(Vector3 dif, Vector3 inDirection, Vector3 inSizeCompo, out Vector3 direction, out Vector3 sizeCompo)
        {
            if (dif.normalized == inDirection.normalized)
            {
                sizeCompo = inSizeCompo;
                direction = inDirection;
            }
            else
            {
                sizeCompo = -inSizeCompo;
                direction = -inDirection;
            }
        }

        /// <summary>
        /// Main method, called when the users stars dragging one of the buttons
        /// </summary>
        private void ClickArrow()
        {
            MovableElement me = SelectedObjectManager.Instance.currentSelectedElements.First() as MovableElement;

            // Press on arrow = select arrow
            if (InputFunctions.IsClickingOutsideUI())
            {
                GameObject go = InputFunctions.GetHoveredObject2D(GlobalManager.Instance.GetActiveCamera());
                Debug.Log("CLICK ARROW " + go);
                if (!go || go.tag != "ElementArrow")
                {
                    isMoving = false;
                    return;
                }
                else
                {
                    currentArrow = go;
                }
            }
            // Release = no arrow
            if (Input.GetMouseButtonUp(0) && currentArrow != null)
            {
                currentArrow = null;
                OperationsBufferScript.Instance.AddAutoSave("Etirement");
            }

            // if arrow
            if (currentArrow)
            {
                Vector3 proj = Vector3.zero;
                Vector3 dif = Vector3.zero;
                Vector3 sizeCompo = Vector3.zero;
                float dist = 0;
                Vector3 direction = Vector3.zero;
                Vector3 mousePos = InputFunctions.GetWorldPoint2D(GlobalManager.Instance.GetActiveCamera());
                if (m_prevPos == mousePos) { return; }
                Debug.Log("FLECHE " + currentArrow.name);
                switch (currentArrow.name)
                {
                    case "Up":
                        direction = me.associated2DObject.transform.up;
                        proj = Vector3.Project(mousePos - oUp, direction) + oUp;
                        dif = proj - oUp;
                        dist = Vector3.Distance(proj, oUp);
                        sizeCompo = Vector3.up;
                        break;
                    case "Down":
                        direction = -me.associated2DObject.transform.up;
                        proj = Vector3.Project(mousePos - oDown, direction) + oDown;
                        dif = proj - oDown;
                        dist = Vector3.Distance(proj, oDown);
                        sizeCompo = Vector3.up;
                        break;
                    case "Left":
                        direction = -me.associated2DObject.transform.right;
                        proj = Vector3.Project(mousePos - oLeft, direction) + oLeft;
                        dif = proj - oLeft;
                        dist = Vector3.Distance(proj, oLeft);
                        sizeCompo = Vector3.right;
                        break;
                    case "Right":
                        direction = me.associated2DObject.transform.right;
                        proj = Vector3.Project(mousePos - oRight, direction) + oRight;
                        dif = proj - oRight;
                        dist = Vector3.Distance(proj, oRight);
                        sizeCompo = Vector3.right;
                        break;
                    case "UpLeft":
                        direction = -me.associated2DObject.transform.right + me.associated2DObject.transform.up;
                        proj = Vector3.Project(mousePos - oUpLeft, direction) + oUpLeft;
                        dif = proj - oUpLeft;
                        dist = Vector3.Distance(proj, oUpLeft);
                        sizeCompo = Vector3.up + Vector3.right;
                        break;
                    case "UpRight":
                        direction = me.associated2DObject.transform.right + me.associated2DObject.transform.up;
                        proj = Vector3.Project(mousePos - oUpRight, direction) + oUpRight;
                        dif = proj - oUpRight;
                        dist = Vector3.Distance(proj, oUpRight);
                        sizeCompo = Vector3.up + Vector3.right;
                        break;
                    case "DownLeft":
                        direction = -me.associated2DObject.transform.right - me.associated2DObject.transform.up;
                        proj = Vector3.Project(mousePos - oDownLeft, direction) + oDownLeft;
                        dif = proj - oDownLeft;
                        dist = Vector3.Distance(proj, oDownLeft);
                        sizeCompo = Vector3.up + Vector3.right;
                        break;
                    case "DownRight":
                        direction = me.associated2DObject.transform.right - me.associated2DObject.transform.up;
                        proj = Vector3.Project(mousePos - oDownRight, direction) + oDownRight;
                        dif = proj - oDownRight;
                        dist = Vector3.Distance(proj, oDownRight);
                        sizeCompo = Vector3.up + Vector3.right;
                        break;
                    default:
                        break;
                }
                SetSizeCompoAndDirection(dif, direction, sizeCompo, out direction, out sizeCompo);
                me.SetPosition(me.Position + VectorFunctions.Switch2D3D(dist * direction / 4f));
                me.SetSize(me.Size + VectorFunctions.Switch2D3D(dist * sizeCompo / 2f));
                if (me is Furniture) (me as Furniture).AdjustSpriteSize();
                currentArrow.transform.position = new Vector3(proj.x, proj.y, me.associated2DObject.transform.position.z);
                isMoving = true;
                m_prevPos = proj;
                FurniturePropScript.Instance.UpdateFurnitureProperties();
            }
            else isMoving = false;
        }
    }
}