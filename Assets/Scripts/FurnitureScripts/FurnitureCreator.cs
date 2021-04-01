using System.Collections.Generic;
using System.Linq;
using ErgoShop.POCO;
using ErgoShop.UI;
using ErgoShop.Utils;
using ErgoShop.Utils.Extensions;
using UnityEngine;

namespace ErgoShop.Managers
{
    /// <summary>
    ///     Furnitures manager. Handle creation, movment, delete.
    /// </summary>
    public class FurnitureCreator : CreatorBehaviour
    {
        // Instance
        public static FurnitureCreator Instance;

        // Unity hierarchy
        public Transform parent2DTop, parent3D;

        // main ground if no room
        public GameObject ground3D;

        // Default sprite used for 2D view
        public GameObject backUpFurniture2DTop;

        // script used to instantiate default furnitures (NOT CUSTOM ONES)
        private FurnitureScript m_currentFurniScript;

        // current furniture being created
        private Furniture m_currentFurniture;

        // current furniture being created associated gameobjects
        private GameObject m_currentFurniture2DTop, m_currentFurniture3D;

        // ALL FURNITURES INSTIANTED FOR THE CURRENT FLOOR
        private List<Furniture> m_furnituresData;

        private WallsCreator Sc_WallsCreator;

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        private void Start()
        {
            m_currentFurniScript = null;
            m_furnituresData = new List<Furniture>();

            Sc_WallsCreator = FindObjectOfType<WallsCreator>();
            if (Sc_WallsCreator == null)
            {
                Debug.LogWarning("FurnitureCreator 'Sc_WallCreator' is null");
            }
        }

        // Update is called once per frame
        private void Update()
        {
            // RIGIDBODIES
            foreach (var f in m_furnituresData)
                if (f != m_currentFurniture)
                {
                    var rb = f.associated3DObject.GetComponent<Rigidbody>();
                    // Freeze all except if its current selected furniture
                    if (SelectedObjectManager.Instance.currentFurnitureData.Count == 1
                        && SelectedObjectManager.Instance.currentFurnitureData[0] == f)
                    {
                        if (f.IsOnWall)
                        {
                            rb.constraints = RigidbodyConstraints.FreezeRotation;
                            rb.useGravity = false;
                        }
                        else
                        {
                            if (f.CanBePutOnFurniture)
                                rb.constraints = RigidbodyConstraints.FreezeRotation;
                            else
                                rb.constraints = RigidbodyConstraints.FreezeRotation |
                                                 RigidbodyConstraints.FreezePositionY;
                        }
                    }
                    else
                    {
                        rb.constraints = RigidbodyConstraints.FreezeAll;
                    }

                    // update 2D position if moving
                    if (f.associated3DObject?.GetComponent<Rigidbody>().velocity.magnitude > 0)
                    {
                        f.Position = f.associated3DObject.transform.position;
                        f.associated2DObject.transform.position =
                            VectorFunctions.GetExactPositionFrom3DObject(f.associated2DObject, f.associated3DObject,
                                f.Position);
                        //VectorFunctions.Switch3D2D(f.Position);
                        rb.velocity *= 0.5f;
                    }

                    f.text2D.transform.position = f.associated2DObject.transform.position;
                }

            // In creation -> check view then position it
            if (m_currentFurniture != null)
                m_currentFurniture.Move(InputFunctions.GetWorldPoint(GlobalManager.Instance.GetActiveCamera()));
            // Validate
            if (Input.GetMouseButtonDown(0) && m_currentFurniture != null)
            {
                m_currentFurniture.Position = m_currentFurniture3D.transform.position;
                m_currentFurniture = null;

                if (m_currentFurniScript)
                {
                    Destroy(m_currentFurniScript.gameObject);
                    m_currentFurniScript = null;
                }

                //m_currentFurniture3D.GetComponent<MeshCollider>().enabled = true;
            }

            // if (Input.GetMouseButtonDown(1)) CancelFurniture();
            if (Input.GetButtonDown("Cancel")) { CancelFurniture(); }
        }

        /// <summary>
        ///     Destroy current furniture to cancel its creation
        /// </summary>
        private void CancelFurniture()
        {
            if (m_currentFurniScript)
            {
                //Destroy(m_currentFurniture2DFace);
                Destroy(m_currentFurniture2DTop);
                Destroy(m_currentFurniture3D);
                Destroy(m_currentFurniScript.gameObject);
                Destroy(m_currentFurniture.text2D);
                m_furnituresData.Remove(m_currentFurniture);
                FurnitureListScroll.Instance.UpdateList();
                m_currentFurniture = null;
                m_currentFurniScript = null;
            }
        }

        /// <summary>
        ///     Seek all associated objects in furnitures to find the Furniture object concerned
        /// </summary>
        /// <param name="go">Associated Gameobject, can be 2D or 3D</param>
        /// <returns>The Furniture data or null if not found</returns>
        public Furniture GetFurnitureFromGameObject(GameObject go)
        {
            if (!go || !go.transform || !go.transform.parent || !go.transform.parent.gameObject) return null;
            if (go.tag != "Furniture") return GetFurnitureFromGameObject(go.transform.parent.gameObject);
            foreach (var f in m_furnituresData)
            {
                if (f.associated2DObject == go) return f;
                if (f.associated3DObject == go) return f;
            }

            return null;
        }

        /// <summary>
        ///     Destroy a Furniture by destroying gameobject and set data to null
        /// </summary>
        /// <param name="f">The furniture</param>
        public void DestroyFurniture(Furniture f)
        {
            m_furnituresData.Remove(f);
            FurnitureListScroll.Instance.UpdateList();
            Destroy(f.text2D);
            Destroy(f.associated2DObject);
            Destroy(f.associated3DObject);
            Destroy(f.widthCotation?.gameObject);
            Destroy(f.heightCotation?.gameObject);
            f = null;
        }

        /// <summary>
        ///     Get all furnitures
        /// </summary>
        /// <returns>all current floor furnitures</returns>
        public List<Furniture> GetFurnitures()
        {
            return m_furnituresData;
        }

        /// <summary>
        ///     Add a custom furniture contained in %appdata%/ErgoShop/Imports on scene
        /// </summary>
        /// <param name="modelGO">the gameobject returned by the obj importer</param>
        /// <param name="cf">Data containing path and name</param>
        public void AddCustomFurniture(GameObject modelGO, CustomFurniture cf)
        {

            AddCollider(modelGO);

            m_currentFurniture3D = modelGO;
            m_currentFurniture2DTop = Instantiate(backUpFurniture2DTop, parent2DTop);
            m_currentFurniture3D.transform.parent = parent3D;
            m_currentFurniture3D.tag = "Furniture";
            m_currentFurniture2DTop.tag = "Furniture";
            m_currentFurniture2DTop.SetLayerRecursively(9);
            m_currentFurniture3D.SetLayerRecursively(10);
            m_currentFurniture = new Furniture
            {
                associated2DObject = m_currentFurniture2DTop,
                associated3DObject = m_currentFurniture3D,
                Name = cf.Name,
                Position = m_currentFurniture3D.transform.position,
                Rotation = 0,
                IsCustom = true,
                CustomPath = cf.Path,
                Type = "Perso",
                IsOnWall = false,
                CanBePutOnFurniture = false,
                ScaleModifier = 1f
            };
            m_currentFurniture.Size = m_currentFurniture.MeshSize * m_currentFurniture.ScaleModifier;

            InitFurnitureText(m_currentFurniture);

            SetFurni2DSize();

            m_currentFurniture3D.transform.localScale = m_currentFurniture.ScaleModifier * Vector3.one;

            m_furnituresData.Add(m_currentFurniture);
            FurnitureListScroll.Instance.UpdateList();
            SelectedObjectManager.Instance.Select(m_currentFurniture);
            SelectedObjectManager.Instance.PlaceFurniture(true);
        }

        /// <summary>
        ///     Since custom furnitures are not prepared like the default ones, we need to add colliders and rigidbody.
        ///     For now the custom furnitures can only by "ground" ones
        ///     TO DO for later : add bool to set constraints for on wall furnitures
        /// </summary>
        /// <param name="modelGO">the gameobject returned by the obj importer</param>
        private void AddCollider(GameObject modelGO)
        {
            modelGO.AddComponent<BoxCollider>();
            modelGO.AddComponent<Rigidbody>();
            modelGO.GetComponent<Rigidbody>().constraints =
                RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotation;
        }

        /// <summary>
        ///     Sets the text mesh in 2D view
        /// </summary>
        /// <param name="f">Furniture data</param>
        private void InitFurnitureText(Furniture f)
        {
            f.text2D = f.associated2DObject.transform.GetChild(0).GetComponent<TextMesh>();
            f.text2D.text = f.Name;
            f.text2D.transform.parent = f.associated2DObject.transform.parent;
            f.text2D.transform.rotation = Quaternion.identity;
            f.text2D.transform.localScale = Vector3.one * 0.05f;
        }

        /// <summary>
        ///     Adapt sprite scaling according to mesh size and user's wanted size
        /// </summary>
        private void SetFurni2DSize()
        {
            var s = m_currentFurniture.Size;
            var ms = m_currentFurniture.MeshSize;
            var sOnMs = new Vector3(s.x / ms.x, s.y / ms.y, s.z / ms.z);
            m_currentFurniture.associated2DObject.transform.localScale =
                VectorFunctions.Switch3D2D(sOnMs);
            m_currentFurniture.AdjustSpriteSize();
        }

        /// <summary>
        ///     Add a furniture (not custom) on scene
        /// </summary>
        /// <param name="furniture">Gameobject containing data in a FurnitureScript (prefab data)</param>
        public void AddFurniture(GameObject furniture)
        {
            FurnitureScript Sc_FurnitureScript = furniture.GetComponent<FurnitureScript>();
            if (Sc_FurnitureScript == null)
            {
                Debug.LogError("FurnitureCreator 'Sc_FurnitureScript' is null");
            }
            else
            {
                if (!Sc_WallsCreator.isOneWall && Sc_FurnitureScript.isOnWall)
                {
                    //To Do..
                    //Management of the case where the object needs a wall but there is not one
                    Debug.Log("The object needs a wall");
                }
                else
                {
                    m_currentFurniScript = Instantiate(furniture).GetComponent<FurnitureScript>();

                    if (!m_currentFurniScript.furniture2DTop)
                        m_currentFurniture2DTop = Instantiate(backUpFurniture2DTop, parent2DTop);
                    else
                        m_currentFurniture2DTop = Instantiate(m_currentFurniScript.furniture2DTop, parent2DTop);
                    //m_currentFurniture2DFace = Instantiate(m_currentFurniScript.furniture2DFace, parent2DFace);
                    m_currentFurniture3D = Instantiate(m_currentFurniScript.furniture3D, parent3D);

                    m_currentFurniture3D.tag = "Furniture";
                    m_currentFurniture2DTop.tag = "Furniture";

                    m_currentFurniture2DTop.SetLayerRecursively(9);
                    m_currentFurniture3D.SetLayerRecursively(10);

                    m_currentFurniture = new Furniture
                    {
                        associated2DObject = m_currentFurniture2DTop,
                        associated3DObject = m_currentFurniture3D,
                        Name = m_currentFurniScript.furnitureName,
                        Position = m_currentFurniture3D.transform.position,
                        Rotation = 0,
                        PrefabName = m_currentFurniScript.resourceName,
                        Type = m_currentFurniScript.furnitureType,
                        IsOnWall = m_currentFurniScript.isOnWall,
                        CanBePutOnFurniture = m_currentFurniScript.canBePutOnFurniture,
                        ScaleModifier = m_currentFurniScript.scaleRatio
                    };

                    m_currentFurniture.Size = m_currentFurniture.MeshSize * m_currentFurniture.ScaleModifier;

                    InitFurnitureText(m_currentFurniture);

                    SetFurni2DSize();

                    m_currentFurniture3D.transform.localScale = m_currentFurniture.ScaleModifier * Vector3.one;

                    m_furnituresData.Add(m_currentFurniture);
                    FurnitureListScroll.Instance.UpdateList();
                    SelectedObjectManager.Instance.Select(m_currentFurniture);
                    SelectedObjectManager.Instance.PlaceFurniture(true);
                    //OperationsBufferScript.Instance.AddAutoSave("Ajout de " + m_currentFurniture.Name);
                }
            }
        }

        /// <summary>
        ///     Destroy every furniture (gameobjects and data)
        /// </summary>
        public override void DestroyEverything()
        {
            while (m_furnituresData.Count > 0) DestroyFurniture(m_furnituresData.First());
            m_furnituresData = new List<Furniture>();
            FurnitureListScroll.Instance.UpdateList();
        }

        /// <summary>
        ///     Load furnitures from floor data
        /// </summary>
        /// <param name="floor">Floor</param>
        public void LoadFurnituresFromFloor(Floor floor)
        {
            // reset everything before loading
            DestroyEverything();
            foreach (var f in floor.Furnitures)
            {
                m_furnituresData.Add(f);
                RebuildFurniture(f);
            }

            // Update element list
            FurnitureListScroll.Instance.UpdateList();
        }

        /// <summary>
        ///     Force a furniture to rebuild gameobjects (scene data) and adjust the transforms
        /// </summary>
        /// <param name="f">Furniture data</param>
        private void RebuildFurniture(Furniture f)
        {
            f.RebuildSceneData();

            if (f.IsCustom)
            {
                f.associated3DObject = ImportManager.Instance.GetGameObjectFromCustomObject(new CustomFurniture
                {
                    Path = f.CustomPath,
                    Name = f.Name
                });
                f.associated3DObject.transform.parent = parent3D;
                AddCollider(f.associated3DObject);
                f.associated2DObject = Instantiate(backUpFurniture2DTop, parent2DTop);
            }
            else
            {
                // 3D
                f.associated3DObject = Instantiate(f.associated3DObject, parent3D);
                // 2D
                if (f.associated2DObject)
                    f.associated2DObject = Instantiate(f.associated2DObject, parent2DTop);
                else
                    f.associated2DObject = Instantiate(backUpFurniture2DTop, parent2DTop);
            }

            var s = f.Size;
            var ms = f.MeshSize;

            f.associated3DObject.transform.position = f.Position;
            f.associated3DObject.transform.localEulerAngles =
                f.EulerAngles;
            f.associated3DObject.transform.localScale =
                new Vector3(s.x / ms.x, s.y / ms.y, s.z / ms.z);
            f.associated3DObject.SetLayerRecursively((int)ErgoLayers.ThreeD);
            //f.associated3DObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;


            f.associated2DObject.transform.position =
                VectorFunctions
                    .Switch3D2D(f
                        .Position); // VectorFunctions.GetExactPositionFrom3DObject(f.associated2DObject, f.associated3DObject, f.Position);
            f.associated2DObject.transform.localEulerAngles =
                f.EulerAngles.y * Vector3.forward * -1f;
            f.associated2DObject.transform.localScale = VectorFunctions.Switch3D2D(new Vector3(
                f.Size.x / f.MeshSize.x,
                f.Size.y / f.MeshSize.y,
                f.Size.z / f.MeshSize.z
            ));

            f.associated2DObject.SetLayerRecursively((int)ErgoLayers.Top);

            f.associated3DObject.tag = "Furniture";
            f.associated2DObject.tag = "Furniture";

            InitFurnitureText(f);
            f.AdjustSpriteSize();
        }

        /// <summary>
        ///     Paste a copied furniture by getting a copy and instantiate it, and rebuilding gameobjects
        /// </summary>
        /// <param name="m_copiedElement">Copied furniture</param>
        /// <returns>The new furniture, identical to the copied one</returns>
        public override Element CopyPaste(Element m_copiedElement)
        {
            if (m_copiedElement is Furniture)
            {
                var f = m_copiedElement as Furniture;
                var newOne = f.GetCopy() as Furniture;
                RebuildFurniture(newOne);
                m_furnituresData.Add(newOne);
                FurnitureListScroll.Instance.UpdateList();
                return newOne;
            }

            return null;
        }
    }
}