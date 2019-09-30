using ErgoShop.Managers;
using ErgoShop.POCO;
using ErgoShop.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ErgoShop.UI
{
    /// <summary>
    /// Compute and show angle between two walls
    /// </summary>
    public class AngleScript : MonoBehaviour
    {
        private int m_segments;
        public LineRenderer LineRend;
        public TextMeshPro AngleTM;
        public float Radius { get; set; }
        private float m_angle;

        private float m_directionw1w2 = 1f;

        public Wall w1, w2;

        // Start is called before the first frame update
        void Start()
        {
            LineRend = GetComponent<LineRenderer>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// METHOD TO UPDATE WALL POSITIONS ACCORDING TO WANTED ANGLE
        /// NOT USED CURRENTLY, WAS CANCELLED
        /// </summary>
        /// <param name="angle"></param>
        public void SetAngle(float angle)
        {
            float diff = angle - m_angle;
            if (diff == 0) return;
            Vector3 v = WallFunctions.GetCommonPosition(w1, w2);

            Quaternion q = Quaternion.Euler(0, 0, diff / 2f * m_directionw1w2);
            Quaternion qInv = Quaternion.Euler(0, 0, -diff / 2f * m_directionw1w2);

            Vector3 neww1dir = q * w1.Direction;
            Vector3 neww2dir = qInv * w2.Direction;

            Vector3 neww1P1 = w1.P2 - (neww1dir * w1.Length);
            Vector3 neww1P2 = w1.P1 + (neww1dir * w1.Length);
            Vector3 neww2P1 = w2.P2 - (neww2dir * w2.Length);
            Vector3 neww2P2 = w2.P1 + (neww2dir * w2.Length);

            if (w1.P1 == v)
            {
                if (w2.P1 == v)
                {
                    // Update w1p2 and w2p2

                    WallFunctions.SetNewPointsForWall(w1, w1.P1, neww1P2, false, true);
                    WallFunctions.SetNewPointsForWall(w2, w2.P1, neww2P2, false, true);
                }
                else
                {
                    // Update w1p2 and w2p1
                    WallFunctions.SetNewPointsForWall(w1, w1.P1, neww1P2, false, true);
                    WallFunctions.SetNewPointsForWall(w1, neww2P1, w2.P2, false, true);
                }
            }
            else
            {
                if (w2.P1 == v)
                {
                    // Update w1p1 and w2p2
                    WallFunctions.SetNewPointsForWall(w1, neww1P1, w1.P2, false, true);
                    WallFunctions.SetNewPointsForWall(w1, w2.P1, neww2P2, false, true);
                }
                else
                {
                    // Update w1p1 and w2p1
                    WallFunctions.SetNewPointsForWall(w1, neww1P1, w1.P2, false, true);
                    WallFunctions.SetNewPointsForWall(w1, neww2P1, w2.P2, false, true);
                }
            }


            m_angle = angle;
        }

        /// <summary>
        /// Draw the computed angle
        /// </summary>
        public void DrawAngle()
        {
            m_angle = WallFunctions.GetAngleBetweenWalls(w1, w2);

            float startAngle = 0;
            Vector3 common = WallFunctions.GetCommonPosition(w1, w2);

            if (common == w1.P1)
            {
                startAngle = Vector3.SignedAngle(Vector3.left, -w1.Direction, Vector3.forward);
            }
            else if (common == w1.P2)
            {
                startAngle = Vector3.SignedAngle(Vector3.left, w1.Direction, Vector3.forward);
            }

            List<Vector3> arcPoints = new List<Vector3>();

            m_segments = Mathf.RoundToInt(m_angle);

            if (m_segments < 0)
            {
                m_directionw1w2 = 1f;
                startAngle = Vector3.SignedAngle(Vector3.left, w2.Direction, Vector3.forward);
                m_angle = WallFunctions.GetAngleBetweenWalls(w2, w1);
                m_segments = Mathf.RoundToInt(m_angle);

                if (common == w2.P1)
                {
                    startAngle = Vector3.SignedAngle(Vector3.left, -w2.Direction, Vector3.forward);
                }
                else if (common == w2.P2)
                {
                    startAngle = Vector3.SignedAngle(Vector3.left, w2.Direction, Vector3.forward);
                }
            }
            else
            {
                m_directionw1w2 = -1f;
            }

            Vector3 center = common;
            float sin = Mathf.Sin(Mathf.Deg2Rad * m_angle);
            float cos = Mathf.Cos(Mathf.Deg2Rad * m_angle);
            center += w1.Thickness / 2f * w1.Perpendicular * sin + w2.Thickness / 2f * w2.Perpendicular * sin;
            center += w1.Thickness / 2f * w1.Direction * cos * m_directionw1w2 + w2.Thickness / 2f * w2.Direction * cos * -m_directionw1w2;

            //if (Input.GetKeyDown(KeyCode.D))
            //{
            //    var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //    go.transform.position = v;
            //    go.transform.localScale = Vector3.one * 0.5f;
            //}

            float angle = startAngle;

            for (int i = 0; i <= m_segments; i++)
            {
                float x = Mathf.Cos(Mathf.Deg2Rad * angle) * Radius;
                float y = Mathf.Sin(Mathf.Deg2Rad * angle) * Radius;

                var pos = new Vector3(x, y, -0.00001f) + center;
                arcPoints.Add(pos);

                if (i == m_segments / 2)
                {
                    AngleTM.transform.position = pos + (pos - center) * 0.15f;
                    AngleTM.text = m_segments + "";
                }

                angle += (m_angle / m_segments);
            }

            LineRend.positionCount = arcPoints.Count;
            LineRend.SetPositions(arcPoints.ToArray());
            LineRend.startColor = Color.white * 0.1f;
            LineRend.endColor = Color.white * 0.1f;


            LineRend.gameObject.SetActive(m_angle != -999 && AngleTM.text != "180");
            AngleTM.gameObject.SetActive(m_angle != -999 && AngleTM.text != "180");

            // Show cotations ? (from room params)

            List<Room> r = WallFunctions.GetRoomsFromWall(w1, WallsCreator.Instance.GetRooms());
            List<Room> r2 = WallFunctions.GetRoomsFromWall(w2, WallsCreator.Instance.GetRooms());

            if (r.Count > 0 && r2.Count > 0 && r[0] == r2[0])
            {
                LineRend.gameObject.SetActive(LineRend.gameObject.activeInHierarchy && r[0].ShowCotations);
                AngleTM.gameObject.SetActive(AngleTM.gameObject.activeInHierarchy && r[0].ShowCotations);
            }

        }
    }
}