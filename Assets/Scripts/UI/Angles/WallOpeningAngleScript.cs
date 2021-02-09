using System.Collections.Generic;
using ErgoShop.POCO;
using ErgoShop.Utils;
using UnityEngine;

namespace ErgoShop.UI
{
    /// <summary>
    ///     Draw the arc representing the door or window opening
    /// </summary>
    public class WallOpeningAngleScript : MonoBehaviour
    {
        public LineRenderer LineRend;

        public bool is3D;

        private float m_angle;

        private float m_radius;
        private float m_segments;

        private float m_startAngle;

        public WallOpening wallOpening;

        // Start is called before the first frame update
        private void Start()
        {
            LineRend = GetComponent<LineRenderer>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (wallOpening == null) Destroy(gameObject);
            if (!wallOpening.associated2DObject) Destroy(gameObject);
        }

        /// <summary>
        ///     Draw the Arc
        /// </summary>
        public void DrawAngle()
        {
            var arcPoints = new List<Vector3>();

            m_radius = wallOpening.Size.x;

            m_startAngle = -wallOpening.associated3DObject.transform.eulerAngles.y;

            if (wallOpening.IsDouble)
            {
                m_radius = m_radius / 2f;
                if (wallOpening.IsPull)
                    m_startAngle -= 90f;
                else
                    m_startAngle -= 180f;
            }
            else
            {
                if (wallOpening.IsLeft) m_startAngle -= 90f;
                if (wallOpening.IsPull)
                {
                    if (wallOpening.IsLeft)
                        m_startAngle -= 90f;
                    else
                        m_startAngle += 90f;
                }
            }

            var centers = wallOpening.GetCorners();

            m_angle = 90f;

            m_segments = Mathf.RoundToInt(m_angle);
            var curAngle = m_startAngle;

            float buff = 0;

            foreach (var c in centers)
            {
                var center = c;
                //if (is3D) center = VectorFunctions.Switch2D3D(center, 0.05f);

                curAngle = m_startAngle + buff;

                for (var i = 0; i <= m_segments; i++)
                {
                    var x = Mathf.Cos(Mathf.Deg2Rad * curAngle) * m_radius;
                    var y = Mathf.Sin(Mathf.Deg2Rad * curAngle) * m_radius;

                    var pos = new Vector3(x, y, -0.00001f) + center;
                    if (is3D) pos = VectorFunctions.Switch2D3D(pos, 0.05f);
                    arcPoints.Add(pos);

                    curAngle += m_angle / m_segments;
                }

                buff = wallOpening.IsPull ? 90f : -90f;
            }

            var col = Color.white * 0.6f * (wallOpening.IsWindow ? 1f : 0.6f);
            LineRend.positionCount = arcPoints.Count;
            LineRend.SetPositions(arcPoints.ToArray());
            LineRend.startColor = col;
            LineRend.endColor = col;
            LineRend.startWidth = 0.1f;
            LineRend.endWidth = 0.1f;

            LineRend.gameObject.SetActive(m_angle != -999);
        }
    }
}