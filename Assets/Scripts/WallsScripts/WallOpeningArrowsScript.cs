using ErgoShop.Managers;
using ErgoShop.POCO;
using ErgoShop.Utils;
using UnityEngine;

namespace ErgoShop.UI
{
    public class WallOpeningArrowsScript : ArrowsScript
    {
        public bool isMoving;

        private WallOpening wo;

        public static WallOpeningArrowsScript Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        // Update is called once per frame
        private void Update()
        {
            if (GlobalManager.Instance.GetActiveCamera().tag != "Cam2D")
            {
                leftArrow.SetActive(false);
                rightArrow.SetActive(false);
                return;
            }

            var show = SelectedObjectManager.Instance.currentWallOpenings.Count > 0;
            leftArrow.SetActive(show);
            rightArrow.SetActive(show);
            if (show)
            {
                wo = SelectedObjectManager.Instance.currentWallOpenings[0];
                if (leftArrow != currentArrow)
                    leftArrow.transform.position = wo.Position + m_decal - wo.Wall.Direction * wo.Size.x / 2f;
                //p1Arrow.transform.rotation = Quaternion.FromToRotation(Vector3.right, -wo.Wall.Direction);
                if (rightArrow != currentArrow)
                    rightArrow.transform.position = wo.Position + m_decal + wo.Wall.Direction * wo.Size.x / 2f;
                //p2Arrow.transform.rotation = Quaternion.FromToRotation(Vector3.right, wo.Wall.Direction);
                leftArrow.transform.localScale =
                    Vector3.one * Mathf.Abs(GlobalManager.Instance.cam2DTop.transform.position.z / 10f);
                rightArrow.transform.localScale =
                    Vector3.one * Mathf.Abs(GlobalManager.Instance.cam2DTop.transform.position.z / 10f);
            }

            if (show)
                ClickArrow();
            else
                isMoving = false;
        }

        private void ClickArrow()
        {
            // Press on arrow = select arrow
            if (Input.GetMouseButtonDown(0))
            {
                var go = InputFunctions.GetHoveredObject2D(GlobalManager.Instance.GetActiveCamera());
                if (!go || go.tag != "WallArrow")
                {
                    isMoving = false;
                    return;
                }

                currentArrow = go;
            }

            // Release = no arrow
            if (Input.GetMouseButtonUp(0)) currentArrow = null;

            // if arrow
            if (currentArrow)
            {
                var proj = Vector3.zero;
                var mousePos = InputFunctions.GetWorldPoint2D(GlobalManager.Instance.GetActiveCamera());
                var d = 0f;
                switch (currentArrow.name)
                {
                    case "P1o":
                        proj = Vector3.Project(mousePos - wo.Position, wo.Wall.Direction) + wo.Position;
                        d = Vector3.Distance(proj, wo.Position);
                        wo.Size = new Vector3(d, wo.Size.y, wo.Size.z);
                        wo.SetPosition(wo.Position - d * wo.Wall.Direction);
                        WallsCreator.Instance.AdjustAllWalls();
                        WallOpeningPropScript.Instance.UpdateWallOpeningProperties();

                        break;
                    case "P2o":
                        proj = Vector3.Project(mousePos - wo.Position, -wo.Wall.Direction) + wo.Position;
                        d = Vector3.Distance(proj, wo.Position);
                        wo.Size = new Vector3(d, wo.Size.y, wo.Size.z);
                        wo.SetPosition(wo.Position + d * wo.Wall.Direction);
                        WallsCreator.Instance.AdjustAllWalls();
                        WallOpeningPropScript.Instance.UpdateWallOpeningProperties();

                        break;
                }

                currentArrow.transform.position = proj + m_decal;
                isMoving = true;
            }
            else
            {
                isMoving = false;
            }
        }
    }
}