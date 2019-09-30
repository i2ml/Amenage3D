using ErgoShop.Managers;
using ErgoShop.POCO;
using ErgoShop.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoShop.UI
{
    public class WallArrowsScript : ArrowsScript
    {
        public GameObject upPerpArrow, downPerpArrow;

        public bool isMoving;

        private Wall w;

        private Vector3 m_prevPos;

        public static WallArrowsScript Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            if (GlobalManager.Instance.GetActiveCamera().tag != "Cam2D")
            {
                leftArrow.SetActive(false);
                rightArrow.SetActive(false);
                upPerpArrow.SetActive(false);
                downPerpArrow.SetActive(false);
                return;
            }
            bool show = SelectedObjectManager.Instance.currentWallsData.Count == 1;
            leftArrow.SetActive(show);
            rightArrow.SetActive(show);
            upPerpArrow.SetActive(show);
            downPerpArrow.SetActive(show);
            if (show)
            {
                w = SelectedObjectManager.Instance.currentWallsData[0];
                if (leftArrow != currentArrow)
                {
                    leftArrow.transform.position = w.P1 + m_decal - w.Direction * m_arrowOffset;
                    //leftArrow.transform.rotation = Quaternion.FromToRotation(Vector3.right, -w.Direction);
                }
                if (rightArrow != currentArrow)
                {
                    rightArrow.transform.position = w.P2 + m_decal + w.Direction * m_arrowOffset;
                    //rightArrow.transform.rotation = Quaternion.FromToRotation(Vector3.right, w.Direction);
                }
                if (upPerpArrow != currentArrow)
                {
                    upPerpArrow.transform.position = w.Center + m_decal - w.Perpendicular * w.Thickness / 2f;
                    //upPerpArrow.transform.rotation = Quaternion.FromToRotation(Vector3.right, -w.Perpendicular);
                }
                if (downPerpArrow != currentArrow)
                {
                    downPerpArrow.transform.position = w.Center + m_decal + w.Perpendicular * w.Thickness / 2f;
                    //downPerpArrow.transform.rotation = Quaternion.FromToRotation(Vector3.right, w.Perpendicular);
                }
            }
            if (show)
            {
                List<GameObject> arrows = new List<GameObject> { leftArrow, rightArrow, upPerpArrow, downPerpArrow };
                foreach (var go in arrows)
                {
                    go.transform.localScale = Vector3.one * Mathf.Abs(GlobalManager.Instance.cam2DTop.transform.position.z / 10f);
                    // DEBUG
                    //if(go.name == "P1")
                    //{
                    //    go.transform.localScale *= 1.3f;
                    //}
                    //if(go.name == "P2")
                    //{
                    //    go.transform.localScale *= 0.7f;
                    //}
                }
                ClickArrow();
            }
            else
            {
                isMoving = false;
            }
        }

        private void ClickArrow()
        {
            // Press on arrow = select arrow
            if (Input.GetMouseButtonDown(0))
            {
                GameObject go = InputFunctions.GetHoveredObject2D(GlobalManager.Instance.GetActiveCamera());
                if (!go || go.tag != "WallArrow")
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
            if (Input.GetMouseButtonUp(0))
            {
                currentArrow = null;
            }

            // if arrow
            if (currentArrow)
            {
                Vector3 proj = Vector3.zero;
                Vector3 mousePos = InputFunctions.GetWorldPoint2D(GlobalManager.Instance.GetActiveCamera());
                if (m_prevPos == mousePos) { return; }
                switch (currentArrow.name)
                {
                    case "P1":
                        proj = Vector3.Project(mousePos - w.P1, w.Direction) + w.P1;
                        WallsCreator.Instance.UpdateWallPoint(w, proj + w.Direction * m_arrowOffset, true);
                        break;
                    case "P2":
                        proj = Vector3.Project(mousePos - w.P2, -w.Direction) + w.P2;
                        WallsCreator.Instance.UpdateWallPoint(w, proj - w.Direction * m_arrowOffset, false);
                        break;
                    case "UpPerp":
                        proj = Vector3.Project(mousePos - w.Center, -w.Perpendicular) + w.Center;
                        w.Thickness = Vector3.Distance(proj, w.Center) * 2f;
                        WallsCreator.Instance.AdjustAllWalls();
                        WallPropPanelScript.Instance.UpdateWallProperties();
                        break;
                    case "DownPerp":
                        proj = Vector3.Project(mousePos - w.Center, w.Perpendicular) + w.Center;
                        w.Thickness = Vector3.Distance(proj, w.Center) * 2f;
                        WallsCreator.Instance.AdjustAllWalls();
                        WallPropPanelScript.Instance.UpdateWallProperties();
                        break;
                    default:
                        break;
                }
                currentArrow.transform.position = proj + m_decal;
                isMoving = true;
                m_prevPos = mousePos;
            }
            else isMoving = false;
        }
    }
}