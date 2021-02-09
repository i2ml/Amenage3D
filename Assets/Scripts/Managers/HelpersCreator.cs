using System.Collections.Generic;
using System.Linq;
using ErgoShop.Operations;
using ErgoShop.POCO;
using ErgoShop.UI;
using ErgoShop.Utils;
using ErgoShop.Utils.Extensions;
using UnityEngine;

namespace ErgoShop.Managers
{
    /// <summary>
    ///     Manager to create HelperElements. For now the only HelperElement is a textzone, but it can evolve
    /// </summary>
    public class HelpersCreator : CreatorBehaviour
    {
        /// <summary>
        ///     Prefab to instantiate a textzone
        /// </summary>
        public GameObject textZonePrefab;

        private HelperElement currentElement;

        /// <summary>
        ///     End of dragging to define a rectangle startpoint->endpoint
        /// </summary>
        private Vector3 m_endPoint;

        private List<HelperElement> m_helpers;
        private bool m_isCreating;

        /// <summary>
        ///     Currently dragging?
        /// </summary>
        private bool m_isDragging;

        /// <summary>
        ///     Info to drag a text zone
        /// </summary>
        private Vector3 m_startPoint;

        public static HelpersCreator Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        private void Start()
        {
            m_helpers = new List<HelperElement>();
        }

        // Update is called once per frame
        private void Update()
        {
            CreateTextZone();
        }

        /// <summary>
        ///     Get all helpers
        /// </summary>
        /// <returns></returns>
        public List<HelperElement> GetHelpers()
        {
            return m_helpers;
        }

        public bool IsOccupied()
        {
            return m_isCreating;
        }

        /// <summary>
        ///     Load all helpers of a given floor
        /// </summary>
        /// <param name="floor"></param>
        public void LoadHelpersFromFloor(Floor floor)
        {
            while (m_helpers.Count > 0) DestroyHelpers(m_helpers.First());
            m_helpers = new List<HelperElement>();
            foreach (var tze in floor.TextZoneElements) m_helpers.Add(tze);
            foreach (var h in m_helpers) h.RebuildSceneData();
        }

        /// <summary>
        ///     Copy Paste an helper
        /// </summary>
        /// <param name="tz">For now, it can be only a textzone</param>
        /// <returns></returns>
        public override Element CopyPaste(Element tz)
        {
            var newTZ = tz.GetCopy() as TextZoneElement;
            m_helpers.Add(newTZ);
            newTZ.RebuildSceneData();
            return newTZ;
        }

        /// <summary>
        ///     Create a new text zone. Its a comment zone shown in both views.
        /// </summary>
        private void CreateTextZone()
        {
            if (m_isCreating)
            {
                if (!m_isDragging)
                {
                    if (InputFunctions.IsClickingOutsideUI())
                    {
                        m_startPoint = InputFunctions.GetWorldPoint2D(GlobalManager.Instance.GetActiveCamera());
                        m_endPoint = InputFunctions.GetWorldPoint2D(GlobalManager.Instance.GetActiveCamera());
                        m_isDragging = true;
                        currentElement = new TextZoneElement
                        {
                            Text = "Commentaire",
                            BackgroundColor = Color.gray,
                            TextColor = Color.black,
                            TextSize = 1
                        };
                        currentElement.associated2DObject = Instantiate(textZonePrefab);
                        currentElement.associated3DObject = Instantiate(textZonePrefab);
                        currentElement.associated2DObject.SetLayerRecursively((int) ErgoLayers.Top);
                        currentElement.associated3DObject.SetLayerRecursively((int) ErgoLayers.ThreeD);
                    }
                }
                else
                {
                    m_endPoint = InputFunctions.GetWorldPoint2D(GlobalManager.Instance.GetActiveCamera());

                    if (InputFunctions.IsClickingOutsideUI())
                    {
                        m_isCreating = false;
                        m_isDragging = false;
                        m_helpers.Add(currentElement);
                        OperationsBufferScript.Instance.AddAutoSave("Creation zone de texte");
                    }

                    currentElement.associated2DObject.transform.position = (m_startPoint + m_endPoint) / 2f;
                    currentElement.Position = VectorFunctions.Switch2D3D((m_startPoint + m_endPoint) / 2f);
                    currentElement.associated3DObject.transform.position = currentElement.Position;

                    (currentElement as TextZoneElement).Size = VectorFunctions.Switch2D3D(currentElement
                        .associated2DObject.GetComponent<TextZoneScript>().SetSize(m_startPoint, m_endPoint));
                    currentElement.associated3DObject.GetComponent<TextZoneScript>().SetSize(m_startPoint, m_endPoint);

                    // CANCEL
                    if (Input.GetMouseButtonDown(1))
                    {
                        m_isCreating = false;
                        m_isDragging = false;
                        Destroy(currentElement.associated2DObject);
                        currentElement = null;
                    }
                }
            }
        }

        /// <summary>
        ///     First click : start of the text zone
        /// </summary>
        public void StartCreateTextZone()
        {
            m_isCreating = true;
            UIManager.Instance.instructionsText.text = "1er clic : début de la zone, 2e clic : fin de la zone de texte";
            GlobalManager.Instance.Set2DTopMode();
        }

        /// <summary>
        ///     Destroy gameobject and helper data
        /// </summary>
        /// <param name="h">HelperElement data</param>
        public void DestroyHelpers(HelperElement h)
        {
            m_helpers.Remove(h);
            Destroy(h.associated2DObject);
            Destroy(h.associated3DObject);
        }

        /// <summary>
        ///     Seek in all helpers if associated 2d or 3d object is go, and return the good one
        /// </summary>
        /// <param name="go">associated object 2d 3d</param>
        /// <returns>HelperElement data corresponding</returns>
        public HelperElement GetHelperFromGameObject(GameObject go)
        {
            foreach (var h in m_helpers)
                if (h.associated2DObject == go || h.associated3DObject == go)
                    return h;
            return null;
        }

        /// <summary>
        ///     Build text zone from data
        /// </summary>
        /// <param name="tz">TextZoneElement</param>
        public void RebuildTextZone(TextZoneElement tz)
        {
            Destroy(tz.associated2DObject);
            Destroy(tz.associated3DObject);
            tz.associated2DObject = Instantiate(textZonePrefab);
            tz.associated2DObject.SetLayerRecursively((int) ErgoLayers.Top);
            tz.associated2DObject.transform.position = VectorFunctions.Switch3D2D(tz.Position);
            tz.associated2DObject.GetComponent<TextZoneScript>().SetSize(VectorFunctions.Switch3D2D(tz.Size));
            tz.associated2DObject.GetComponent<TextZoneScript>().bg.color = tz.BackgroundColor;
            tz.associated2DObject.GetComponent<TextZoneScript>().tm.color = tz.TextColor;
            tz.associated2DObject.GetComponent<TextZoneScript>().tm.text = tz.Text;
            tz.associated2DObject.GetComponent<TextZoneScript>().textSize = tz.TextSize;

            tz.associated3DObject = Instantiate(textZonePrefab);
            tz.associated3DObject.SetLayerRecursively((int) ErgoLayers.ThreeD);
            tz.associated3DObject.transform.position = tz.Position;
            tz.associated3DObject.GetComponent<TextZoneScript>().SetSize(VectorFunctions.Switch3D2D(tz.Size));
            tz.associated3DObject.GetComponent<TextZoneScript>().bg.color = tz.BackgroundColor;
            tz.associated3DObject.GetComponent<TextZoneScript>().tm.color = tz.TextColor;
            tz.associated3DObject.GetComponent<TextZoneScript>().tm.text = tz.Text;
            tz.associated3DObject.GetComponent<TextZoneScript>().textSize = tz.TextSize;
        }

        /// <summary>
        ///     Delete all gamobjects and data
        /// </summary>
        public override void DestroyEverything()
        {
            foreach (var h in m_helpers)
            {
                Destroy(h.associated2DObject);
                Destroy(h.associated3DObject);
            }

            m_helpers.Clear();
        }
    }
}