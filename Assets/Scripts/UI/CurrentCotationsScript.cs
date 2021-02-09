using ErgoShop.Managers;
using ErgoShop.POCO;
using ErgoShop.Utils;
using ErgoShop.Utils.Extensions;
using UnityEngine;

namespace ErgoShop.UI
{
    /// <summary>
    ///     Handle 4 cotations around an element in 2D to compute north west east and south lengths between element and next
    ///     obstacles/walls
    /// </summary>
    public class CurrentCotationsScript : MonoBehaviour
    {
        public CotationsScript up, down, left, right;

        private GameObject m_element;

        // Start is called before the first frame update
        private void Start()
        {
            up.isUp = true;
            down.isDown = true;
            left.isLeft = true;
            right.isRight = true;
        }

        // Update is called once per frame
        private void Update()
        {
            var isFurniWall = false;
            Wall w = null;
            m_element = null;
            if (SelectedObjectManager.Instance.currentFurnitureData.Count == 1)
            {
                m_element = SelectedObjectManager.Instance.currentFurnitureData[0].associated2DObject;
                isFurniWall = SelectedObjectManager.Instance.currentFurnitureData[0].IsOnWall;
                if (isFurniWall) w = SelectedObjectManager.Instance.currentFurnitureData[0].AssociatedElement as Wall;
            }
            else if (SelectedObjectManager.Instance.currentStairs.Count == 1)
            {
                m_element = SelectedObjectManager.Instance.currentStairs[0].associated2DObject;
            }

            up.gameObject.SetActive(m_element);
            down.gameObject.SetActive(m_element);
            left.gameObject.SetActive(m_element);
            right.gameObject.SetActive(m_element);

            if (m_element)
            {
                var b = m_element.GetSpriteBounds();
                var upPoint = new Vector3(b.center.x, b.max.y);
                var downPoint = new Vector3(b.center.x, b.min.y);
                var leftPoint = new Vector3(b.min.x, b.center.y);
                var rightPoint = new Vector3(b.max.x, b.center.y);

                up.start = upPoint; //GetPointFromRay(upPoint, Vector3.down, null);
                down.start = downPoint; //GetPointFromRay(downPoint, Vector3.up, null);
                left.start = leftPoint; //GetPointFromRay(leftPoint, Vector3.right, null);
                right.start = rightPoint; //GetPointFromRay(rightPoint, Vector3.left, null);

                up.end = GetPointFromRayFromCot(up, Vector3.up, m_element);
                down.end = GetPointFromRayFromCot(down, Vector3.down, m_element);
                left.end = GetPointFromRayFromCot(left, Vector3.left, m_element);
                right.end = GetPointFromRayFromCot(right, Vector3.right, m_element);

                up.gameObject.SetActive(up.end != Vector3.positiveInfinity && up.Length > 0);
                down.gameObject.SetActive(down.end != Vector3.positiveInfinity && down.Length > 0);
                left.gameObject.SetActive(left.end != Vector3.positiveInfinity && left.Length > 0);
                right.gameObject.SetActive(right.end != Vector3.positiveInfinity && right.Length > 0);

                if (isFurniWall)
                {
                    up.gameObject.SetActive(up.Dir == w.Direction || up.Dir == -1 * w.Direction);
                    down.gameObject.SetActive(down.Dir == w.Direction || down.Dir == -1 * w.Direction);
                    left.gameObject.SetActive(left.Dir == w.Direction || left.Dir == -1 * w.Direction);
                    right.gameObject.SetActive(right.Dir == w.Direction || right.Dir == -1 * w.Direction);
                }
            }
        }

        private Vector3 GetPointFromRayFromCot(CotationsScript cot, Vector3 dir, GameObject avoid)
        {
            return GetPointFromRay(cot.start, dir, avoid);
        }

        //private Vector3 GetPointFromRayFromCot3D(CotationsScript cot, Vector3 dir, GameObject avoid)
        //{
        //    return GetPointFromRay3D(cot.start, dir, avoid);
        //}

        //private Vector3 GetPointFromRay3D(Vector3 origin, Vector2 dir, GameObject avoid)
        //{
        //    RaycastHit[] hits = Physics.RaycastAll(origin, dir, Mathf.Infinity, 1 << (int)ErgoLayers.ThreeD);
        //    foreach (var hit in hits)
        //    {
        //        if (hit.collider.gameObject != avoid && hit.collider.tag != "Cotation" && hit.collider.tag != "Helper")
        //        {
        //            Debug.Log("GO : " + hit.collider.gameObject.name + " ORIGIN : " + origin + " DIR : " + dir + " POINT : " + hit.point);
        //            return hit.point;
        //        }
        //    }
        //    return Vector3.positiveInfinity;
        //}

        private Vector3 GetPointFromRay(Vector3 origin, Vector2 dir, GameObject avoid)
        {
            var hits = Physics2D.RaycastAll(origin, dir, Mathf.Infinity, 1 << (int) ErgoLayers.Top);
            foreach (var hit in hits)
                if (hit.collider.gameObject != avoid
                    && hit.collider.name != "2D - Top"
                    && hit.collider.tag != "RoomText"
                    && hit.collider.tag != "Cotation"
                    && hit.collider.tag != "Helper"
                    && !hit.collider.tag.Contains("Arrow"))
                {
                    if (SettingsManager.Instance.SoftwareParameters.TagOnlyWall)
                    {
                        if (hit.collider.gameObject.tag == "Wall") return hit.point;
                    }
                    else
                    {
                        return hit.point;
                    }

                    //Debug.Log("GO : " + hit.collider.gameObject.name + " ORIGIN : " + origin + " DIR : " + dir + " POINT : " + hit.point);
                }

            return Vector3.positiveInfinity;
        }
    }
}