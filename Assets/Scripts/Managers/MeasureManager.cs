using System.Collections.Generic;
using ErgoShop.UI;
using ErgoShop.Utils;
using UnityEngine;

namespace ErgoShop.Managers
{
    /// <summary>
    ///     Manager to do user measures in both 2d and 3d view
    /// </summary>
    public class MeasureManager : MonoBehaviour
    {
        public CotationsScript measure2D, measure3D;
        public CotationsScript measure3DH, measure3DV;

        public GameObject erase2DBTN, erase3DBTN;
        private bool m_goPutFirstPoint2D, m_goPutFirstPoint3D;
        private bool m_goPutSecondPoint2D, m_goPutSecondPoint3D;

        public static MeasureManager Instance { get; private set; }

        public bool IsMesuring { get; set; }

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        private void Start()
        {
            IsMesuring = false;
        }

        /// <summary>
        ///     Update checks each frame if we are measuring
        /// </summary>
        private void Update()
        {
            //if (!measure2D.gameObject.activeInHierarchy)
            //{
            //    measure2D.start = Vector3.zero;
            //    measure2D.end = v
            //}
            erase2DBTN.SetActive(measure2D.gameObject.activeInHierarchy);
            erase3DBTN.SetActive(measure3D.gameObject.activeInHierarchy);

            // second before first, to prevent dbl click
            PutSecondPoint2D();
            PutSecondPoint3D();
            PutFirstPoint2D();
            PutFirstPoint3D();

            CancelMeasure();
            var m3 = measure3D.gameObject.activeInHierarchy;
            measure3DH.gameObject.SetActive(m3);
            measure3DV.gameObject.SetActive(m3);
            // rectangle triangle to show horizontal length and vertical length
            if (m3)
            {
                if (measure3D.end.y > measure3D.start.y)
                {
                    measure3DH.start = measure3D.start;
                    measure3DH.end = new Vector3(measure3D.end.x, measure3D.start.y, measure3D.end.z);

                    measure3DV.start = measure3DH.end;
                    measure3DV.end = measure3D.end;
                }
                else
                {
                    measure3DV.start = measure3D.start;
                    measure3DV.end = new Vector3(measure3D.end.x, measure3D.start.y, measure3D.end.z);
                    measure3DH.start = measure3DV.end;
                    measure3DH.end = measure3D.end;
                }
            }

            // Rotation
            var camTrans = GlobalManager.Instance.GetActiveCamera().transform;
            var cots = new List<CotationsScript> {measure3D, measure3DH, measure3DV};
            foreach (var c in cots)
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
                c.cotationTM.transform.rotation = camTrans.rotation;
            }
        }

        /// <summary>
        ///     Right click to cancel a measure (hide it)
        /// </summary>
        public void CancelMeasure()
        {
            if (Input.GetMouseButtonDown(1) && IsMesuring) Hide();
        }

        /// <summary>
        ///     Start 2D measure
        /// </summary>
        public void PutFirstPoint2D()
        {
            if (m_goPutFirstPoint2D)
                if (InputFunctions.IsClickingOutsideUI())
                {
                    measure2D.gameObject.SetActive(true);
                    measure2D.start =
                        InputFunctions.GetWorldPoint2D(GlobalManager.Instance.cam2DTop.GetComponent<Camera>());
                    m_goPutSecondPoint2D = true;
                }
        }

        /// <summary>
        ///     End 2D measure
        /// </summary>
        public void PutSecondPoint2D()
        {
            if (m_goPutSecondPoint2D)
            {
                measure2D.end = InputFunctions.GetWorldPoint2D(GlobalManager.Instance.cam2DTop.GetComponent<Camera>());
                if (InputFunctions.IsClickingOutsideUI())
                {
                    IsMesuring = false;
                    m_goPutFirstPoint2D = m_goPutFirstPoint3D = m_goPutSecondPoint2D = m_goPutSecondPoint3D = false;
                }
            }
        }

        /// <summary>
        ///     Start 3d measure
        /// </summary>
        public void PutFirstPoint3D()
        {
            if (m_goPutFirstPoint3D)
                if (InputFunctions.IsClickingOutsideUI())
                {
                    measure3D.gameObject.SetActive(true);
                    measure3D.start = InputFunctions.GetWorldPoint(GlobalManager.Instance.cam3D.GetComponent<Camera>());
                    m_goPutSecondPoint3D = true;
                }
        }

        /// <summary>
        ///     End 3d measure
        /// </summary>
        public void PutSecondPoint3D()
        {
            if (m_goPutSecondPoint3D)
            {
                measure3D.end = InputFunctions.GetWorldPoint(GlobalManager.Instance.cam3D.GetComponent<Camera>());
                if (InputFunctions.IsClickingOutsideUI())
                {
                    IsMesuring = false;
                    m_goPutFirstPoint2D = m_goPutFirstPoint3D = m_goPutSecondPoint2D = m_goPutSecondPoint3D = false;
                }
            }
        }

        /// <summary>
        ///     Hide the sprite & text for 2D
        /// </summary>
        public void Hide2D()
        {
            measure2D.gameObject.SetActive(false);
            m_goPutFirstPoint2D = m_goPutFirstPoint3D = m_goPutSecondPoint2D = m_goPutSecondPoint3D = false;
            IsMesuring = false;
        }

        /// <summary>
        ///     Hide the sprite & text for 3D
        /// </summary>
        public void Hide3D()
        {
            measure3D.gameObject.SetActive(false);
            m_goPutFirstPoint2D = m_goPutFirstPoint3D = m_goPutSecondPoint2D = m_goPutSecondPoint3D = false;
            IsMesuring = false;
        }

        /// <summary>
        ///     Hide all 2d and 3d
        /// </summary>
        private void Hide()
        {
            measure2D.gameObject.SetActive(false);
            measure3D.gameObject.SetActive(false);
            m_goPutFirstPoint2D = m_goPutFirstPoint3D = m_goPutSecondPoint2D = m_goPutSecondPoint3D = false;
            IsMesuring = false;
        }

        /// <summary>
        ///     When users click on measure button 2d
        ///     Will trigger the PutFirstPoint2D() method when Update
        /// </summary>
        public void Measure2D()
        {
            GlobalManager.Instance.Set2DTopMode();
            IsMesuring = true;
            m_goPutFirstPoint2D = true;
        }

        /// <summary>
        ///     When users click on measure button 3d
        ///     Will trigger the PutFirstPoint3D() method when Update
        /// </summary>
        public void Measure3D()
        {
            if (!GlobalManager.Instance.Is3D()) GlobalManager.Instance.Set3DMode();
            IsMesuring = true;
            m_goPutFirstPoint3D = true;
        }
    }
}