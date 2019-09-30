using ErgoShop.Managers;
using ErgoShop.POCO;
using ErgoShop.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    /// <summary>
    /// Display arrows around the size in centimeters we want to display
    /// The length is computed between start and end point.
    /// It can be to measure an object size, or a length
    /// </summary>
    public class CotationsScript : MonoBehaviour
    {
        public Transform arrow1, arrow2;
        public SpriteRenderer middle1, middle2;
        public TextMeshPro cotationTM;
        public InputField cotationField;
        public Vector3 textOffset;
        /// <summary>
        /// Startint point for the measure
        /// </summary>
        public Vector3 start;
        /// <summary>
        /// Ending point for the measure
        /// </summary>
        public Vector3 end;

        public float decalTextOffset;

        public Element myElement;

        public bool isUp, isDown, isLeft, isRight;

        public bool isMeasure;

        public bool IsExterior { get; set; }

        public bool is3D;

        // Start is called before the first frame update
        void Start()
        {
            foreach (InputField inf in FindObjectsOfType<InputField>())
            {
                // ceinture bretelles
                if (inf.name == "CotationField" || inf.tag == "Cotation")
                {
                    cotationField = inf;
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (myElement == null && !isUp && !isDown && !isLeft && !isRight && !isMeasure)
            {
                Destroy(gameObject);
            }

            if (!cotationField)
            {
                foreach (InputField inf in FindObjectsOfType<InputField>())
                {
                    // ceinture bretelles
                    if (inf.name == "CotationField" || inf.tag == "Cotation")
                    {
                        cotationField = inf;
                    }
                }
            }

            if (!is3D)
            {
                start.z = -0.05f;
                end.z = -0.05f;
            }

            bool show = Length > 0.005 && Length != float.PositiveInfinity && !IsExterior;
            /*start != Vector3.positiveInfinity && end != Vector3.positiveInfinity && */

            arrow1.gameObject.SetActive(show);
            arrow2.gameObject.SetActive(show);
            middle1.gameObject.SetActive(show);
            middle2.gameObject.SetActive(show);
            cotationTM.gameObject.SetActive(show);
            if (!show) return;


            float approxDist = Mathf.Floor(Vector3.Distance(start, end) * 100f);
            float offset = approxDist > 999 ? 0.4f : 0.3f;

            Vector3 dir = (end - start).normalized;
            transform.position = (start + end) / 2f;

            transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);


            arrow1.position = start;
            arrow2.position = end;

            Vector3 mid = (arrow1.position + arrow2.position) / 2f;
            float dist = Vector3.Distance(arrow1.position, arrow2.position);
            Vector2 midSize = new Vector2(0.025f, dist / 2f - offset);

            middle1.transform.position = (arrow1.position + mid) / 2f - dir * offset / 4f;
            middle1.size = midSize;

            middle2.transform.position = (arrow2.position + mid) / 2f + dir * offset / 4f;
            middle2.size = midSize;

            cotationTM.transform.position = mid + textOffset + Perp * decalTextOffset;

            if (!is3D)
            {
                cotationTM.transform.localEulerAngles = -transform.localEulerAngles;
            }

            //cotationTM.characterSize = 0.4f;
            cotationTM.text = approxDist + "";

            if (!isMeasure)
            {
                if (is3D)
                {
                    cotationTM.GetComponent<BoxCollider>().size = new Vector3(Mathf.Clamp(Mathf.Log10(approxDist) * 0.13f, 0f, 2f), 0.18f, 0.1f);
                    //cotationTM.transform.Translate(cotationTM.)
                }
                else
                {
                    cotationTM.GetComponent<BoxCollider2D>().size = new Vector2(Mathf.Clamp(Mathf.Log10(approxDist) * 0.1f, 0f, 2f), 0.18f);
                }
            }

            // Scale in function of cam Z in 2D
            if (!is3D)
            {
                cotationTM.transform.localScale = Vector3.one * Mathf.Abs((GlobalManager.Instance.cam2DTop.transform.position.z / 10f));
            }

            // Show cotations ? (from room params)
            if (myElement is Wall)
            {
                List<Room> r = WallFunctions.GetRoomsFromWall((myElement as Wall), WallsCreator.Instance.GetRooms());
                if (r.Count > 0)
                {
                    for (int i = 0; i < transform.childCount; i++)
                    {
                        transform.GetChild(i).gameObject.SetActive(transform.GetChild(i).gameObject.activeInHierarchy && r[0].ShowCotations);
                    }
                }
            }
            // Live Update
            if (Input.GetMouseButtonDown(0))
            {
                GameObject go;
                if (is3D)
                {
                    go = InputFunctions.GetHoveredObject(GlobalManager.Instance.cam3D.GetComponent<Camera>());
                }
                else
                {
                    go = InputFunctions.GetHoveredObject2D(GlobalManager.Instance.cam2DTop.GetComponent<Camera>());
                }
                if (go)
                {
                    if (go == cotationTM.gameObject)
                    {
                        SelectedObjectManager.Instance.SetCurrentCotation(this);
                    }
                }
            }

            // Rotation measure
            if (isMeasure)
            {
                Transform camTrans = GlobalManager.Instance.GetActiveCamera().transform;

                List<Transform> tr = new List<Transform>() { middle1.transform, middle2.transform };
                foreach (var t in tr)
                {
                    t.LookAt(camTrans);
                    t.localEulerAngles = new Vector3(0, t.localEulerAngles.y, 180);
                }

            }
        }



        public float Length
        {
            get
            {
                return Vector3.Distance(start, end);
            }
        }

        public Vector3 Center
        {
            get
            {
                return (start + end) / 2f;
            }
        }

        public Vector3 Dir
        {
            get
            {
                return (end - start).normalized;
            }
        }

        public Vector3 Perp { get; set; }

        public void SetPerp(Wall w)
        {
            Perp = (Center - w.Center).normalized;
        }

        public void SetPerp(Wall w, Vector3 cent)
        {
            Perp = (Center - cent).normalized;
        }

        /// <summary>
        /// Sort start and end according to wall P1 and P2
        /// </summary>
        /// <param name="wallDir"></param>
        public void Sort(Vector3 wallDir)
        {
            if (Dir != wallDir)
            {
                var tmp = start;
                start = end;
                end = tmp;
            }
        }
    }
}