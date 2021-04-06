using System.Collections.Generic;
using ErgoShop.Managers;
using ErgoShop.POCO;
using ErgoShop.Utils;
using ErgoShop.Utils.Extensions;
using UnityEngine;

namespace ErgoShop.UI
{
    /// <summary>
    ///     Handle 4 cotations around an element in 3D to compute north west east and south lengths between element and next
    ///     obstacles/walls
    /// </summary>
    public class CurrentCotation3DScript : MonoBehaviour
    {
        public CotationsScript up, down, left, right;

        private GameObject m_element;

        private Wall m_wall;

        // Start is called before the first frame update
        private void Start()
        {
            up.isUp = true;
            down.isDown = true;
            left.isLeft = true;
            right.isRight = true;

            up.is3D = down.is3D = left.is3D = right.is3D = true;
        }

        // Update is called once per frame
        private void Update()
        {
            m_element = null;
            if (SelectedObjectManager.Instance.currentFurnitureData.Count == 1)
            {
                m_element = SelectedObjectManager.Instance.currentFurnitureData[0].associated3DObject;
                m_wall = SelectedObjectManager.Instance.currentFurnitureData[0].AssociatedElement as Wall;
            }

            up.gameObject.SetActive(m_element);
            down.gameObject.SetActive(m_element);
            left.gameObject.SetActive(m_element);
            right.gameObject.SetActive(m_element);

            if (m_element)
            {
                var b = m_element.GetMeshBounds();
                var upPoint = Vector3.positiveInfinity;
                var downPoint = Vector3.positiveInfinity;
                var leftPoint = Vector3.positiveInfinity;
                var rightPoint = Vector3.positiveInfinity;
                if (SelectedObjectManager.Instance.currentFurnitureData[0].IsOnWall)
                {
                    upPoint = new Vector3(b.center.x, b.max.y, b.center.z);
                    downPoint = new Vector3(b.center.x, b.min.y, b.center.z);
                }
                else
                {
                    upPoint = new Vector3(b.center.x, b.center.y, b.min.z);
                    downPoint = new Vector3(b.center.x, b.center.y, b.max.z);
                }

                leftPoint = b.center + m_element.transform.right * b.size.x / 2f * -1f;
                rightPoint = b.center + m_element.transform.right * b.size.x / 2f;

                up.start = upPoint; //GetPointFromRay(upPoint, Vector3.down, null);
                down.start = downPoint; //GetPointFromRay(downPoint, Vector3.up, null);
                left.start = leftPoint; //GetPointFromRay(leftPoint, Vector3.right, null);
                right.start = rightPoint; //GetPointFromRay(rightPoint, Vector3.left, null);

                if (SelectedObjectManager.Instance.currentFurnitureData[0].IsOnWall)
                {
                    up.end = GetPointFromRayFromCot3D(up, Vector3.up, m_element);
                    down.end = GetPointFromRayFromCot3D(down, Vector3.down, m_element);
                    left.end = GetPointFromRayFromCot3D(left, VectorFunctions.Switch2D3D(m_wall.Direction), m_element);
                    right.end = GetPointFromRayFromCot3D(right, VectorFunctions.Switch2D3D(-m_wall.Direction),
                        m_element);
                }
                else
                {
                    up.end = GetPointFromRayFromCot3D(up, Vector3.back, m_element);
                    down.end = GetPointFromRayFromCot3D(down, Vector3.forward, m_element);
                    left.end = GetPointFromRayFromCot3D(left, Vector3.left, m_element); //m_element.transform.right*-1f
                    right.end = GetPointFromRayFromCot3D(right, Vector3.right, m_element); //m_element.transform.right
                }

                up.gameObject.SetActive(up.end != Vector3.positiveInfinity && up.Length > 0 &&
                                        up.Length != float.PositiveInfinity);
                down.gameObject.SetActive(down.end != Vector3.positiveInfinity && down.Length > 0 &&
                                          down.Length != float.PositiveInfinity);
                left.gameObject.SetActive(left.end != Vector3.positiveInfinity && left.Length > 0 &&
                                          left.Length != float.PositiveInfinity);
                right.gameObject.SetActive(right.end != Vector3.positiveInfinity && right.Length > 0 &&
                                           right.Length != float.PositiveInfinity);

                // ROTATION FACE TO CAM IN 3D

                var cs = new List<CotationsScript> {up, down, left, right};

                var camTrans = GlobalManager.Instance.GetActiveCamera().transform;

                foreach (var c in cs)
                {
                    var tr = new List<Transform> {c.middle1.transform, c.middle2.transform};
                    foreach (var t in tr)
                    {
                        t.LookAt(camTrans);
                        t.localEulerAngles = new Vector3(0, t.localEulerAngles.y, 180);
                    }

                    c.arrow1.localEulerAngles = new Vector3(c.arrow1.localEulerAngles.x,
                        c.middle1.transform.localEulerAngles.y, c.arrow1.localEulerAngles.z);
                    c.arrow2.localEulerAngles = new Vector3(c.arrow2.localEulerAngles.x,
                        c.middle1.transform.localEulerAngles.y, c.arrow2.localEulerAngles.z);
                    //c.cotationTM.transform.LookAt(camTrans);
                    //c.cotationTM.transform.rotation *= Quaternion.Euler(Vector3.up * 180f);
                    //c.arrow2.localEulerAngles = new Vector3(0,c.arrow2.localEulerAngles.y, c.arrow2.localEulerAngles.z+180);
                    c.cotationTM.transform.rotation = camTrans.rotation;
                }
            }
            else
            {
                up.gameObject.SetActive(false);
                down.gameObject.SetActive(false);
                left.gameObject.SetActive(false);
                right.gameObject.SetActive(false);
            }
        }


        private Vector3 GetPointFromRayFromCot3D(CotationsScript cot, Vector3 dir, GameObject avoid)
        {
            return GetPointFromRay3D(cot.start, dir, avoid);
        }

        private Vector3 GetPointFromRay3D(Vector3 origin, Vector3 dir, GameObject avoid)
        {
            var hits = Physics.RaycastAll(origin, dir, Mathf.Infinity, 1 << (int) ErgoLayers.ThreeD);
            var goodHits = new List<Vector3>();
            foreach (var hit in hits)
                if (hit.collider.gameObject != avoid && hit.collider.tag != "Cotation")
                {
                    // check settings to find good collider
                    if (SettingsManager.Instance.SoftwareParameters.TagOnlyWall)
                    {
                        if (hit.collider.gameObject.tag == "Wall") goodHits.Add(hit.point);
                    }
                    else
                    {
                        //Debug.Log("GO : " + hit.collider.gameObject.name + " ORIGIN : " + origin + " DIR : " + dir + " POINT : " + hit.point);
                        goodHits.Add(hit.point);
                    }
                }
                else
                {
                    //Debug.Log("Ignored " + hit.collider.name);
                }

            var bestHit = Vector3.positiveInfinity;

            foreach (var gh in goodHits)
                if (Vector3.Distance(gh, origin) < Vector3.Distance(bestHit, origin))
                    bestHit = gh;

            return bestHit;
        }
    }
}