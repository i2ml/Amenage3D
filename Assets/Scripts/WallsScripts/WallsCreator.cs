using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ErgoShop.Operations;
using ErgoShop.POCO;
using ErgoShop.UI;
using ErgoShop.Utils;
using ErgoShop.Utils.Extensions;
using ProBuilder2.MeshOperations;
using UnityEngine;

namespace ErgoShop.Managers
{
    /// <summary>
    ///     CREATOR FOR WALLS AND ROOMS
    /// </summary>
    public class WallsCreator : CreatorBehaviour
    {
        public int LastIndexWall = 0;
        public int LastIndexRoom = 0;
        public int MaxIndex = 0;

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        private void Start()
        {
            m_wallsData = new List<Wall>();
            m_roomsData = new List<Room>();
            m_anglesData = new List<AngleScript>();
            m_woAnglesData = new List<WallOpeningAngleScript>();
            m_woAngles3DData = new List<WallOpeningAngleScript>();
            m_currentWallIdx = 0;
            m_currentRoomdx = 0;

            m_potentialPolyRoom = new List<Wall>();

            SetCreationModeNone();
        }

        // Update is called once per frame
        private void Update()
        {
            if (InputFunctions.IsMouseOutsideUI())
            {
                UpdateIsOneWall();

                foreach (Wall w in m_wallsData)
                {
                    UpdateWallCotation(w);
                }

                UpdateCeils();
                SetUIZoomWidthsAndAngles();

                Creation();
            }
        }

        #region public fields

        public float angleSnap;
        public float wallHeight, wallThickness;
        public Transform room3D, room2D;

        // Editor affectation
        public GameObject wall3DPrefab,
            door3DPrefab,
            door2DPrefab,
            simpleWindow3DPrefab,
            window3DPrefab,
            window2DPrefab;

        public GameObject roomTextPrefab;
        public GameObject wallTextPrefab;
        public WallButtonsActiveScript wb;
        public GameObject cotationPrefab;
        public GameObject angleScriptPrefab;
        public GameObject polyShapePrefab;
        public GameObject wallOpeningAngleScriptPrefab;
        public GameObject wallOpeningAngleScript3DPrefab;

        public static WallsCreator Instance { get; private set; }

        public bool isOneWall = false;
        #endregion

        #region private fields

        // private
        private Vector3 m_startPosition, m_endPosition;
        private Wall m_currentWallData;
        private Room m_currentRoomData;
        private WallOpening m_currentWallOpeningData;

        private int m_currentWallIdx;
        private int m_currentRoomdx;

        private bool m_polygonalRoom;
        private Vector3 m_beginPolyRoom;
        private List<Wall> m_potentialPolyRoom;

        private readonly float snapTol = 0.5f;

        // Mouse pressed and wall being create
        private bool m_creating, m_startedWall;

        private WallType m_creationMode;

        private List<Wall> m_wallsData;
        private List<Room> m_roomsData;
        private List<AngleScript> m_anglesData;
        private List<WallOpeningAngleScript> m_woAnglesData;
        private List<WallOpeningAngleScript> m_woAngles3DData;

        #endregion

        #region private methods

        /// <summary>
        /// Find if is one wall or not 
        /// </summary>
        private void UpdateIsOneWall()
        {
            if (m_wallsData.Count <= 0)
            {
                isOneWall = false;
            }
            else
            {
                isOneWall = true;
            }
        }

        /// <summary>
        ///     Show Ceils only if camera IN room
        /// </summary>
        private void UpdateCeils()
        {
            foreach (Room r in m_roomsData)
            {
                r.Ceil.planeGenerated.SetActive(GlobalManager.Instance.cam3D.transform.position.y < r.Ceil.planeGenerated.transform.position.y);
            }
        }

        /// <summary>
        ///     Create wall and rooms
        /// </summary>
        private void Creation()
        {
            if (InputFunctions.IsMouseOutsideUI())
            {
                switch (m_creationMode)
                {
                    case WallType.Room:
                        wb.ActiveRoom();
                        ProcessRoomCreation();
                        break;
                    case WallType.Wall:
                        if (m_polygonalRoom)
                            wb.ActivePolyWall();
                        else
                            wb.ActiveWall();
                        ProcessWallCreation();
                        break;
                    case WallType.Door:
                        wb.ActiveDoor();
                        ProcessWallOpening(false);
                        break;
                    case WallType.Window:
                        wb.ActiveWindow();
                        ProcessWallOpening(true);
                        break;
                    case WallType.None:
                        wb.DesactiveDoor();
                        wb.DesactiveWindow();
                        wb.DesactiveWall();
                        wb.DesactivePolyWall();
                        wb.DesactiveRoom();
                        break;
                }
                UpdateTextFromCreationMode();
            }
        }

        /// <summary>
        ///     Update instructions in screen bottom
        /// </summary>
        private void UpdateTextFromCreationMode()
        {
            switch (m_creationMode)
            {
                case WallType.Door:
                case WallType.Window:
                    UIManager.Instance.instructionsText.text =
                        "Clic gauche pour placer l'ouverture. Clic droit pour annuler.";
                    break;
                case WallType.Wall:
                    if (m_creating)
                        UIManager.Instance.instructionsText.text = m_polygonalRoom
                            ? "Clic gauche pour placer la seconde extremité, clic droit pour annuler."
                            : "Lachez le clic pour terminer le mur";
                    else
                        UIManager.Instance.instructionsText.text = m_polygonalRoom
                            ? "Clic gauche pour passer au mur suivant, clic droit pour annuler. Un clic gauche sur l'extremité du premier mur termine la pièce."
                            : "Laissez le clic gauche appuyé et tirez un trait, clic droit pour annuler.";
                    break;
                case WallType.Room:
                    UIManager.Instance.instructionsText.text =
                        "Laissez le clic gauche appuyé et tirez un rectangle, lachez le clic pour placer la pièce.";
                    break;
                case WallType.None:
                    break;
            }
        }

        /// <summary>
        ///     Creation wall opening
        /// </summary>
        /// <param name="isWindow"></param>
        private void ProcessWallOpening(bool isWindow)
        {
            // Door always along a wall
            AdjustWallOpening(isWindow);

            // Validate door position
            if (Input.GetMouseButtonUp(0) && InputFunctions.IsMouseOutsideUI()) SetWallOpeningEnd(isWindow);

            // Cancel    
            if (Input.GetMouseButtonDown(1))
            {
                CancelWallOpening();
            }

            //Cancel
            if (Input.GetButtonDown("Cancel"))
            {
                Debug.Log("yup");
                CancelWallOpening();
            }
            else
            {
                Debug.Log(Input.GetButtonDown("Cancel"));
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="isWindow"></param>
        private void SetWallOpeningEnd(bool isWindow)
        {
            m_currentWallOpeningData.Wall.Openings.Add(m_currentWallOpeningData);

            m_currentWallOpeningData.associated2DObject.transform.parent =
                m_currentWallOpeningData.Wall.associated2DObject.transform;
            UpdateWallOpeningPosition(m_currentWallOpeningData);
            SetCreationModeNone();

            OperationsBufferScript.Instance.AddAutoSave("Ajout " +
                                                        (m_currentWallOpeningData.IsWindow ? "Fenetre" : "Porte"));
            m_currentWallOpeningData = null;

            AdjustAllWalls();
        }

        /// <summary>
        ///     Create gameobjects
        /// </summary>
        /// <param name="wo"></param>
        public void InstantiateWallOpening(WallOpening wo)
        {
            if (wo.associated2DObject) Destroy(wo.associated2DObject);
            if (wo.associated3DObject) Destroy(wo.associated3DObject);

            var wOpening2D = wo.IsWindow ? Instantiate(window2DPrefab) : Instantiate(door2DPrefab);
            wOpening2D.SetLayerRecursively((int)ErgoLayers.Top);
            wOpening2D.tag = "WallOpening";
            wo.associated2DObject = wOpening2D;

            var wOpening3D = wo.IsWindow
                ? wo.IsDouble ? Instantiate(window3DPrefab) : Instantiate(simpleWindow3DPrefab)
                : Instantiate(door3DPrefab);
            wOpening3D.SetLayerRecursively((int)ErgoLayers.ThreeD);
            wOpening3D.tag = "WallOpening";
            wo.associated3DObject = wOpening3D;
        }

        /// <summary>
        ///     Adjust during creation (first move)
        /// </summary>
        /// <param name="isWindow"></param>
        private void AdjustWallOpening(bool isWindow)
        {
            // Tricks to get mesh size
            GameObject winwin = Instantiate(window3DPrefab);
            GameObject doordoor = Instantiate(door3DPrefab);
            Vector3 openingSize = isWindow ? winwin.MeshSize() : doordoor.MeshSize();

            Destroy(winwin);
            Destroy(doordoor);

            openingSize = VectorFunctions.InvertXZ(openingSize);
            // 
            float heightWindow = isWindow ? 1.1f : 0f;

            // New one, create it !
            if (m_currentWallOpeningData == null)
            {
                m_currentWallOpeningData = new WallOpening
                {
                    IsWindow = isWindow,
                    WindowHeight = heightWindow,
                    Size = openingSize,
                    IsDouble = isWindow
                };
                m_currentWallOpeningData.RebuildSceneData();
            }

            UpdateWallOpeningPosition(m_currentWallOpeningData);
        }

        /// <summary>
        ///     create wall
        /// </summary>
        private void ProcessWallCreation()
        {
            var endWall = m_polygonalRoom ? Input.GetMouseButtonDown(0) : Input.GetMouseButtonUp(0);
            if (!m_startedWall && Input.GetMouseButtonDown(0) && InputFunctions.IsMouseOutsideUI())
            {
                SetWallStart();
            }
            else if (endWall && InputFunctions.IsMouseOutsideUI())
            {
                SetWallEnd();
            }
            else
            {
                if (m_creating) AdjustWall();
            }


            if (Input.GetMouseButtonDown(1)) CancelWall();
        }

        /// <summary>
        ///     create room
        /// </summary>
        private void ProcessRoomCreation()
        {
            if (Input.GetMouseButtonDown(0) && InputFunctions.IsMouseOutsideUI())
            {
                SetRoomStart();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                SetRoomEnd();
            }
            else
            {
                if (m_creating)
                {
                    AdjustRoom();
                }
            }

            //Annule la creation de la room 
            if (Input.GetMouseButtonDown(1) || Input.GetButtonDown("Cancel"))
            {
                CancelRoom();
            }

            if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) &&
                !InputFunctions.IsMouseOutsideUI())
            {
                CancelRoom();
            }


        }

        /// <summary>
        ///     cancel wall
        /// </summary>
        public void CancelWallOpening()
        {

            if (m_currentWallOpeningData != null)
            {
                DestroyWallOpening(m_currentWallOpeningData);
                //Destroy(m_currentWallOpeningData.associated2DObject);
                //Destroy(m_currentWallOpeningData.associated3DObject);                        

                //if (m_currentWallOpeningData.Wall != null)
                //{
                //    m_currentWallOpeningData.Wall.Openings.Remove(m_currentWallOpeningData);
                //}
                //m_currentWallOpeningData = null;
                SetCreationModeNone();
                //AdjustAllWalls();
            }
        }

        /// <summary>
        ///     cancel room by destroying current walls
        /// </summary>
        public void CancelRoom()
        {
            if (m_currentRoomData != null)
                for (var i = 0; i < 4; i++)
                    DestroyWall(m_currentRoomData.Walls[i]);
            m_currentRoomData = null;
            SetCreationModeNone();
        }

        /// <summary>
        ///     cancel wall by destroying gamobjct
        /// </summary>
        private void CancelWall()
        {
            if (m_polygonalRoom)
            {
                foreach (var w in m_potentialPolyRoom) DestroyWall(w);
                m_potentialPolyRoom.Clear();
            }
            else
            {
                if (m_currentWallData != null) DestroyWall(m_currentWallData);
            }

            m_currentWallData = null;
            SetCreationModeNone();
        }

        private void SetRoomStart(bool setStart = true)
        {
            m_creating = true;
            if (setStart) m_startPosition = InputFunctions.GetWorldPoint(GlobalManager.Instance.GetActiveCamera());
            m_currentRoomData = new Room
            {
                Height = wallHeight,
                Name = "Piece",
                LockAngles = true
            };
            m_currentRoomData.Walls = new List<Wall>();
            LastIndexRoom = MaxIndex;
            for (var i = 0; i < 4; i++)
            {

                m_currentRoomData.Walls.Add(new Wall());
                m_currentRoomData.Walls[i].P1 = m_startPosition;
                m_currentRoomData.Walls[i].Height = wallHeight;
                m_currentRoomData.Walls[i].associated2DObject.transform.parent = room2D;
                m_currentRoomData.Walls[i].associated3DObject.transform.parent = room3D;
                m_currentRoomData.Walls[i].Index = 0;

                if (LastIndexRoom == 0 && m_currentRoomdx == 0)
                {
                    m_currentRoomData.Walls[i].Index = m_currentRoomdx;
                    m_currentRoomdx++;
                }
                else if (LastIndexRoom == 0)
                {
                    LastIndexRoom = m_currentRoomdx;
                    m_currentRoomData.Walls[i].Index = LastIndexRoom;
                }
                else if (m_currentRoomData.Walls[i].Index == 0)
                {
                    LastIndexRoom++;
                    m_currentRoomData.Walls[i].Index = LastIndexRoom;
                }

                m_wallsData.Add(m_currentRoomData.Walls[i]);
            }
            MaxIndex = m_currentRoomData.Walls[3].Index;
            // stick walls
            m_currentRoomData.Walls[0].linkedP1.Add(m_currentRoomData.Walls[3]);
            m_currentRoomData.Walls[0].linkedP2.Add(m_currentRoomData.Walls[1]);
            m_currentRoomData.Walls[1].linkedP1.Add(m_currentRoomData.Walls[0]);
            m_currentRoomData.Walls[1].linkedP2.Add(m_currentRoomData.Walls[2]);
            m_currentRoomData.Walls[2].linkedP1.Add(m_currentRoomData.Walls[1]);
            m_currentRoomData.Walls[2].linkedP2.Add(m_currentRoomData.Walls[3]);

            m_currentRoomData.Walls[3].linkedP1.Add(m_currentRoomData.Walls[2]);
            m_currentRoomData.Walls[3].linkedP2.Add(m_currentRoomData.Walls[0]);
        }

        private void SetWallStart()
        {
            m_creating = true;
            m_startedWall = true;

            // Data
            m_currentWallData = new Wall
            {
                Color = new Color(200f / 255f, 200f / 255f, 200f / 255f)
            };

            m_currentWallData.associated2DObject.transform.parent = room2D;
            m_currentWallData.associated3DObject.transform.parent = room3D;

            m_startPosition =
                SnapStartingPosition(InputFunctions.GetWorldPoint(GlobalManager.Instance.GetActiveCamera()), 1, true);
            m_currentWallData.P1 = m_startPosition;
            m_currentWallData.Height = wallHeight;
            //m_currentWallIdx

            LastIndexWall = MaxIndex;
            m_currentWallData.Index = 0;
            if (LastIndexWall == 0 && m_currentWallIdx == 0)
            {
                m_currentWallData.Index = m_currentWallIdx;
                m_currentWallIdx++;
            }
            else if (LastIndexWall == 0)
            {
                LastIndexWall = m_currentWallIdx;
                m_currentWallData.Index = LastIndexWall + 1;
            }
            else if (m_currentWallData.Index == 0)
            {
                m_currentWallData.Index = LastIndexWall + 1;
            }
            else
            {
                m_currentWallData.Index++;
            }

            MaxIndex = m_currentWallData.Index;

            //Debug.Log(LastIndexWall + "  /  " + m_currentWallIdx + "  /  " + m_currentWallData.Index);

            if (m_polygonalRoom && m_potentialPolyRoom.Count == 0) m_beginPolyRoom = m_startPosition;
            if (m_polygonalRoom) m_potentialPolyRoom.Add(m_currentWallData);

            // Collections
            m_wallsData.Add(m_currentWallData);
        }

        private void SetRoomEnd()
        {
            m_creating = false;
            if (Vector3.Distance(m_startPosition, m_endPosition) < wallThickness)
            {
                CancelRoom();
            }
            else
            {
                m_roomsData.Add(m_currentRoomData);
                SelectedObjectManager.Instance.Select(m_currentRoomData);
                m_currentRoomData = null;
                AdjustGroundsAndCeils();
            }

            AdjustAllWalls();

            m_currentWallData = null;
            SetCreationModeNone();
            OperationsBufferScript.Instance.AddAutoSave("Création pièce");
        }

        private void SetWallEnd()
        {
            m_creating = false;
            m_startedWall = false;
            //m_endPosition = GetWorldPoint2D();
            if (m_currentWallData != null)
            {
                // too short or angle < 5 = cancel            
                if (m_currentWallData.linkedP1.Count > 0)
                {
                    var a = WallFunctions.GetAngleBetweenWalls(m_currentWallData, m_currentWallData.linkedP1[0]);
                    if (a < 5 && a > -5) CancelWall();
                }
                else if (m_currentWallData.Length < wallThickness)
                {
                    CancelWall();
                }

                AdjustAllWalls();
            }

            if (m_polygonalRoom)
            {
                if (m_endPosition != m_beginPolyRoom)
                {
                    SetWallStart();
                }
                else
                {
                    if (m_potentialPolyRoom.Count >= 3)
                    {
                        var r = new Room();
                        r.LockAngles = false;
                        r.Walls = new List<Wall>();
                        r.Walls.AddRange(m_potentialPolyRoom);

                        r.Height = r.Walls[0].Height;
                        r.Name = "Pièce";
                        m_roomsData.Add(r);
                        AdjustGroundsAndCeils();
                        SelectedObjectManager.Instance.Select(r);
                    }
                    else
                    {
                        foreach (var pw in m_potentialPolyRoom)
                        {
                            m_wallsData.Remove(pw);
                            DestroyWall(pw);
                        }
                    }

                    m_potentialPolyRoom.Clear();
                    m_currentWallData = null;
                    SetCreationModeNone();
                    OperationsBufferScript.Instance.AddAutoSave("Création pièce polygonale");
                }
            }
            else
            {
                m_currentWallData = null;
                SetCreationModeNone();
                OperationsBufferScript.Instance.AddAutoSave("Création mur");
            }
        }

        private void SetCreationModeNone()
        {
            m_creationMode = WallType.None;
            UIManager.Instance.instructionsText.text = "";
        }

        private void StickWalls(Wall oldWall, Wall newWall)
        {
            if (oldWall == newWall) { return; }
            if (oldWall.Index == newWall.Index) { return; }
            if (oldWall == null || newWall == null) Debug.Log("STICKWALL NULL " + oldWall + " " + newWall);
            // stick walls to middle of their points (tolerance in meters)
            float stickTol = m_creating ? wallThickness : 0;

            Vector3 common = WallFunctions.GetCommonPosition(oldWall, newWall);

            // try to stick those with same extremities
            if (!common.Equals(Vector3.positiveInfinity) && !oldWall.Equals(newWall))
            {
                if (oldWall.P1.Equals(common) && !oldWall.linkedP1.Contains(newWall))
                {
                    oldWall.linkedP1.Add(newWall);
                }
                else if (oldWall.P2.Equals(common) && !oldWall.linkedP2.Contains(newWall))
                {
                    oldWall.linkedP2.Add(newWall);
                }

                if (newWall.P1.Equals(common) && !newWall.linkedP1.Contains(oldWall))
                {
                    newWall.linkedP1.Add(oldWall);
                }
                else if (newWall.P2.Equals(common) && !newWall.linkedP2.Contains(oldWall))
                {
                    newWall.linkedP2.Add(oldWall);
                }
            }
            // else try to stick those that are very close
            else if (!m_creating)
            {
                Vector3 middle = WallFunctions.GetCommonPosition(oldWall, newWall, stickTol);
                if (middle.Equals(Vector3.positiveInfinity) || oldWall.Equals(newWall)) { return; }

                // Stick new wall to middle
                if (Vector3.Distance(oldWall.P1, middle) < stickTol && !oldWall.linkedP1.Contains(newWall))
                {
                    oldWall.P1 = middle;
                }
                else if (Vector3.Distance(oldWall.P2, middle) < stickTol && !oldWall.linkedP2.Contains(newWall))
                {
                    oldWall.P2 = middle;
                }

                //Stick old wall to middle
                if (Vector3.Distance(newWall.P1, middle) < stickTol && !newWall.linkedP1.Contains(oldWall))
                {
                    newWall.P1 = middle;
                }
                else if (Vector3.Distance(newWall.P2, middle) < stickTol && !newWall.linkedP2.Contains(oldWall))
                {
                    newWall.P2 = middle;
                }
            }
        }

        public void CreateRoomFromWidthAndHeight()
        {
            var h = RectRoomFormScript.Instance.GetHeight();
            var w = RectRoomFormScript.Instance.GetWidth();

            m_startPosition = Vector3.zero;
            m_endPosition = m_startPosition +
                            new Vector3(h / 100f + wallThickness + 0.005f, w / 100f + wallThickness + 0.005f);

            SetRoomStart(false);
            AdjustRoom(false);
            SetRoomEnd();
        }

        private void AdjustRoom(bool setEnd = true)
        {
            if (setEnd) m_endPosition = InputFunctions.GetWorldPoint(GlobalManager.Instance.GetActiveCamera());

            var x2y1 = new Vector3(m_endPosition.x, m_startPosition.y);
            var x1y2 = new Vector3(m_startPosition.x, m_endPosition.y);

            m_currentRoomData.Walls[0].P1 = m_startPosition;
            m_currentRoomData.Walls[0].P2 = x2y1;

            m_currentRoomData.Walls[1].P1 = x2y1;
            m_currentRoomData.Walls[1].P2 = m_endPosition;

            m_currentRoomData.Walls[2].P1 = m_endPosition;
            m_currentRoomData.Walls[2].P2 = x1y2;

            m_currentRoomData.Walls[3].P1 = x1y2;
            m_currentRoomData.Walls[3].P2 = m_startPosition;

            for (var i = 0; i < 4; i++) m_currentRoomData.Walls[i].Thickness = wallThickness;

            AdjustAllWalls();
        }

        /// <summary>
        /// //////////////////////////////////
        /// </summary>
        public void AdjustWall()
        {
            if (m_currentWallData == null) return;
            m_endPosition = InputFunctions.GetWorldPoint(GlobalManager.Instance.GetActiveCamera());
            m_endPosition = SnapPosition(m_endPosition, m_startPosition);

            m_currentWallData.P2 = m_endPosition;
            m_currentWallData.Thickness = wallThickness;

            AdjustAllWalls();
        }

        public void AdjustGroundsAndCeils()
        {
            foreach (Room room in m_roomsData)
            {
                List<Vector3> vertices = WallFunctions.GetDistinctPositionsSortedForOneRoom(room.Walls);

                if (room.associated2DObject == null)
                {
                    room.associated2DObject = Instantiate(roomTextPrefab);
                    room.associated2DObject.layer = (int)ErgoLayers.Top;
                    room.associated2DObject.transform.parent = room2D;
                }

                room.associated2DObject.GetComponent<TextMesh>().text = room.Name;
                room.associated2DObject.transform.position = room.GetCenter();

                if (room.Ground != null && room.Ground.planeGenerated != null)
                {
                    Destroy(room.Ground.planeGenerated.gameObject);
                }
                if (room.Ground == null)
                {
                    room.Ground = new Ground
                    {
                        Color = new Color(200 / 255f, 200 / 255f, 200 / 255f)
                    };
                }

                
                if (room.Ceil != null && room.Ceil.planeGenerated != null)
                {
                    room.Ceil.planeGenerated.SetActive(false);
                    Destroy(room.Ceil.planeGenerated);
                }
                if (room.Ceil == null)
                {
                    room.Ceil = new Ceil
                    {
                        Color = Color.white
                    };
                    room.Ceil.planeGenerated.SetActive(false);
                }

                Vector3 center3D = VectorFunctions.Switch2D3D(VectorFunctions.GetCenter(vertices));
                List<Vector3> vs = new List<Vector3>();
                foreach (Vector3 v in vertices)
                {
                    Vector3 nv = VectorFunctions.Switch2D3D(v);
                    nv -= center3D;
                    vs.Add(nv);
                }

                //var triangles3D = VectorFunctions.TriangulateConvex(VectorFunctions.SortVerticesConvex(Vector3.up, vs));
                //var triangles3DCeil = VectorFunctions.TriangulateConvex(VectorFunctions.SortVerticesConvex(Vector3.down, vs));

                if (room.Ground.planeGenerated)
                {
                    Destroy(room.Ground.planeGenerated);
                }

                room.Ground.planeGenerated = Instantiate(polyShapePrefab);
                room.Ground.planeGenerated.layer = (int)ErgoLayers.ThreeD;
                room.Ground.planeGenerated.transform.parent = room3D;
                room.Ground.planeGenerated.transform.localPosition = Vector3.up * 0.001f + center3D;
                room.Ground.planeGenerated.tag = "Ground";
                room.Ground.planeGenerated.name = "Ground";

                if (room.Ceil.planeGenerated)
                {
                    Destroy(room.Ceil.planeGenerated);
                }
                room.Ceil.planeGenerated = Instantiate(polyShapePrefab);
                room.Ceil.planeGenerated.SetActive(false);
                room.Ceil.planeGenerated.layer = (int)ErgoLayers.ThreeD;
                room.Ceil.planeGenerated.transform.parent = room3D;
                room.Ceil.planeGenerated.transform.localPosition = Vector3.up * room.Height + center3D;
                Destroy(room.Ceil.planeGenerated.GetComponent<MeshCollider>());
                room.Ceil.planeGenerated.name = "Ceil";
                //room.Ceil.planeGenerated.tag = "Ceil";

                pb_Object pbog = room.Ground.planeGenerated.GetComponent<pb_Object>();
                pbog.CreateShapeFromPolygon(vs, 0.0f, false);

                pbog.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
                pbog.GetComponent<MeshRenderer>().material.color = room.Ground.Color;

                pb_Object pboc = room.Ceil.planeGenerated.GetComponent<pb_Object>();
                pboc.CreateShapeFromPolygon(vs, 0.1f, false);

                pboc.GetComponent<MeshRenderer>().material = new Material(Shader.Find("Diffuse"));
                pboc.GetComponent<MeshRenderer>().material.color = room.Ceil.Color;

                //Debug.Log(pboc.faceCount);
                //float minY=float.PositiveInfinity;
                //List<int> indexes=new List<int>();
                //foreach(var v in pboc.vertices)
                //{
                //    if(v.y < minY)
                //    {
                //        minY = v.y;
                //    }
                //}

                //for(int i = 0 ; i < pboc.vertices.Length; i++)
                //{
                //    if(pboc.vertices[i].y != minY)
                //    {
                //        indexes.Add(i);
                //    }
                //}

                //pboc.DeleteVerticesWithIndices(indexes);
                //pboc.RemoveDegenerateTriangles(out int[] removed);
                //pboc.ToMesh();
                pboc.Refresh();
                pbog.Refresh();
            }
        }

        public void AdjustAllWalls()
        {
            foreach (Wall w in m_wallsData)
            {
                w.Position = VectorFunctions.Switch2D3D(w.Center, w.Height / 2f);

                w.linkedP1.Clear();
                w.linkedP2.Clear();
                foreach (Wall w2 in m_wallsData)
                {
                    StickWalls(w, w2);
                }

                AdjustWallVertices(w);

                if (w.linkedP1.Count == 0 && w.linkedP2.Count == 0)
                {
                    foreach (WallOpening wo in w.Openings)
                    {
                        UpdateWallOpeningAngles(wo);
                    }
                }
            }

            //gestion 2D
            foreach (Room r in m_roomsData)
            {
                if (!r.associated2DObject)
                {
                    r.associated2DObject = Instantiate(roomTextPrefab);
                    r.associated2DObject.layer = (int)ErgoLayers.Top;
                    r.associated2DObject.transform.parent = room2D;
                    r.associated2DObject.GetComponent<TextMesh>().text = r.Name;
                }

                r.associated2DObject.transform.position = r.GetCenter();
            }
        }

        // Draw angles and update texts and "fake" ui scales
        public void SetUIZoomWidthsAndAngles()
        {
            var zoom = Mathf.Abs(GlobalManager.Instance.cam2DTop.transform.position.z / 10f);
            foreach (AngleScript a in m_anglesData)
            {
                a.LineRend.startWidth = zoom / 20f;
                a.LineRend.endWidth = zoom / 20f;
                a.Radius = zoom / 2f; // + a.w1.Thickness + a.w2.Thickness;
                a.AngleTM.transform.localScale = Vector3.one * zoom;
                a.DrawAngle();
            }

            foreach (Room r in m_roomsData)
            {
                r.associated2DObject.transform.localScale = Vector3.one * zoom;
            }
        }

        private void AdjustWallVertices(Wall w)
        {
            ClearWallGraphics(w);
            w.vertices2D.Clear();
            Vector3 a0 = w.P1;
            Vector3 b1 = w.P2;

            if (w.Openings.Count == 0)
            {
                w.vertices2D.AddRange(WallFunctions.MakeGraphicalWall(w.P1, w.P2, w));
            }
            else if (w.Openings.Count == 1)
            {
                WallOpening wo = w.Openings[0];

                Vector3 a1, b0;
                a1 = wo.Position - wo.Size.x * ((b1 - a0).normalized / 2);
                b0 = wo.Position + wo.Size.x * ((b1 - a0).normalized / 2);

                w.vertices2D.AddRange(WallFunctions.MakeGraphicalWall(a0, a1, w));
                w.vertices2D.AddRange(WallFunctions.MakeGraphicalWall(b0, b1, w));
            }
            else
            {
                w.Openings = WallFunctions.SortOpenings(w.Openings);

                for (var i = 0; i < w.Openings.Count; i++)
                {
                    var wo = w.Openings[i];

                    Vector3 a1, b0;

                    a1 = wo.Position - wo.Size.x * ((b1 - a0).normalized / 2);
                    b0 = wo.Position + wo.Size.x * ((b1 - a0).normalized / 2);

                    // first = first wall
                    if (i == 0)
                    {
                        w.vertices2D.AddRange(WallFunctions.MakeGraphicalWall(a0, a1, w));
                    }
                    // middle or end = wall between i-1 and itself       
                    else if (i <= w.Openings.Count - 1)
                    {
                        var b0p = w.Openings[i - 1].Position + w.Openings[i - 1].Size.x * ((b1 - a0).normalized / 2);
                        w.vertices2D.AddRange(WallFunctions.MakeGraphicalWall(b0p, a1, w));
                    }

                    // last = last wall
                    if (i == w.Openings.Count - 1) w.vertices2D.AddRange(WallFunctions.MakeGraphicalWall(b0, b1, w));
                }
            }

            foreach (var wo in w.Openings)
            {
                wo.RebuildSceneData();
                wo.associated2DObject.transform.position = wo.Position + Vector3.forward * -0.01f;
                var rot = Quaternion.FromToRotation(Vector3.right, w.Direction);
                wo.associated2DObject.transform.rotation = rot;
                wo.EulerAngles = rot.eulerAngles;
                wo.Rotation = wo.EulerAngles.z;
                // Remove the -180° bug with y (no outline)
                wo.associated2DObject.transform.eulerAngles = new Vector3(wo.associated2DObject.transform.eulerAngles.x,
                    0,
                    wo.associated2DObject.transform.eulerAngles.z);


                wo.associated2DObject.GetComponent<SpriteRenderer>().size = new Vector2(wo.Size.x, wo.Size.z);
                wo.associated2DObject.GetComponent<BoxCollider2D>().size = new Vector2(wo.Size.x, wo.Size.z);
                wo.associated2DObject.transform.parent = w.associated2DObject.transform;

                wo.associated3DObject.layer = room3D.gameObject.layer;

                wo.associated3DObject.transform.position = VectorFunctions.Switch2D3D(wo.Position) +
                                                           (wo.IsWindow ? wo.WindowHeight * Vector3.up : Vector3.zero);

                Vector3 a1, b0;

                a1 = wo.Position - wo.Size.x * ((b1 - a0).normalized / 2);
                b0 = wo.Position + wo.Size.x * ((b1 - a0).normalized / 2);

                var rotDoor = Quaternion.FromToRotation(Vector3.forward, VectorFunctions.Switch2D3D(b0 - a1));
                wo.associated3DObject.transform.rotation = rotDoor;
                wo.associated3DObject.transform.localEulerAngles = new Vector3(
                    wo.associated3DObject.transform.localEulerAngles.x,
                    wo.associated3DObject.transform.localEulerAngles.y, 0);
                wo.associated3DObject.transform.parent = w.associated3DObject.transform;

                var s = wo.Size;
                var ms = wo.MeshSize;

                ms = VectorFunctions.InvertXZ(ms);

                wo.associated3DObject.transform.localScale =
                    new Vector3(1, s.y / ms.y, wo.IsLeft ? -s.x / ms.x : s.x / ms.x);

                CreateWallPartsForWallOpening(wo);
            }

            foreach (var w2d in w.walls2D)
                w2d.gameObject.transform.localEulerAngles = new Vector3(
                    w2d.gameObject.transform.localEulerAngles.x,
                    0,
                    w2d.gameObject.transform.localEulerAngles.z);
        }

        // Remake wall part above door
        private void CreateWallPartsForWallOpening(WallOpening wopening)
        {
            if (wopening == null) { return; }


            // UP
            GameObject w = Instantiate(wall3DPrefab, wopening.Wall.associated3DObject.transform);
            // deltY = distance from object top to wallHeight
            var deltY = wopening.Wall.Height - wopening.Size.y;
            w.transform.localPosition = VectorFunctions.Switch2D3D(wopening.Position) +
                                        (wopening.Size.y + deltY / 2f + wopening.WindowHeight / 2f) * Vector3.up;
            w.transform.localEulerAngles =
                wopening.associated3DObject.transform.localEulerAngles +
                90f * Vector3.up; //wopening.associated3DObject.transform.localEulerAngles; // 90f * Vector3.up;
            w.transform.localScale =
                wopening.Size.x * Vector3.right
                + deltY * Vector3.up - wopening.WindowHeight * Vector3.up
                + wopening.Wall.Thickness * Vector3.forward;
            var renderer = w.GetComponent<MeshRenderer>();
            renderer.material = wopening.Wall.walls3D[0].gameObject.GetComponent<MeshRenderer>().material;

            //wopening.WallParts.Add(w);

            // DOWN
            if (wopening.IsWindow)
            {
                var w2 = Instantiate(wall3DPrefab, wopening.Wall.associated3DObject.transform);

                w2.transform.localPosition = VectorFunctions.Switch2D3D(wopening.Position) +
                                             wopening.WindowHeight / 2f * Vector3.up;
                w2.transform.localEulerAngles =
                    wopening.associated3DObject.transform.localEulerAngles + 90f * Vector3.up;
                w2.transform.localScale = Vector3.up * wopening.WindowHeight
                                          + wopening.Size.x * Vector3.right
                                          + wopening.Wall.Thickness * Vector3.forward;
                var renderer2 = w2.GetComponent<MeshRenderer>();
                renderer2.material = wopening.Wall.walls3D[0].gameObject.GetComponent<MeshRenderer>().material;
            }
        }

        private Vector3 SnapStartingPosition(Vector3 pos, float dist = 1, bool isStart = false)
        {
            // Snap to another wall ending
            foreach (var wd in m_wallsData)
                if (wd != m_currentWallData)
                {
                    if (Vector3.Distance(wd.P2, pos) < dist) return wd.P2;
                    if (Vector3.Distance(wd.P1, pos) < dist) return wd.P1;
                }

            return pos;
        }

        private Vector3 SnapPosition(Vector3 end, Vector3 begin, float angleTol = 5)
        {
            var otherWall = SnapStartingPosition(end);
            // If other wall return
            if (!otherWall.Equals(end)) return otherWall;
            // Else, try to fit to 90°
            var curDir = end - begin;
            var angle = Vector3.Angle(curDir, Vector3.up);
            // Is in range if between 0 and angletTol OR between 90-angleTol and 90
            var angle90 = angle % 90;

            // Close to 90° ?
            if (angle90 < angleTol || angle90 > 90 - angleTol)
            {
                var snappedDirection = VectorFunctions.SnapToAngle(curDir, 90);
                // Snap to close X / Y other wall ?
                var snapPos = begin + snappedDirection;

                foreach (var w in m_wallsData)
                    if (w != m_currentWallData)
                    {
                        if (Mathf.Abs(w.P1.x - snapPos.x) < snapTol) return new Vector3(w.P1.x, snapPos.y);
                        if (Mathf.Abs(w.P2.x - snapPos.x) < snapTol) return new Vector3(w.P2.x, snapPos.y);

                        if (Mathf.Abs(w.P1.y - snapPos.y) < snapTol) return new Vector3(snapPos.x, w.P1.y);
                        if (Mathf.Abs(w.P2.y - snapPos.y) < snapTol) return new Vector3(snapPos.x, w.P2.y);
                    }

                return snapPos;
            }

            // Else return the mouse position
            return end;
        }

        private void ClearWallGraphics(Wall w)
        {
            foreach (var go in w.walls2D) Destroy(go.gameObject);
            foreach (var go in w.walls3D) Destroy(go.gameObject);

            for (var i = 0; i < w.associated2DObject.transform.childCount; i++)
            {
                var child = w.associated2DObject.transform.GetChild(i);
                if (child.tag != "WallOpening") Destroy(child.gameObject);
            }

            for (var i = 0; i < w.associated3DObject.transform.childCount; i++)
            {
                var child = w.associated3DObject.transform.GetChild(i);
                if (child.tag != "WallOpening") Destroy(child.gameObject);
            }

            w.walls2D.Clear();
            w.walls3D.Clear();
        }


        public void DestroyWallOpening(WallOpening wo)
        {
            Destroy(wo.associated2DObject);
            Destroy(wo.associated3DObject);

            wo.Wall.Openings.Remove(wo);

            WallOpeningAngleScript angleToRemove = null;
            WallOpeningAngleScript angle3DToRemove = null;

            foreach (WallOpeningAngleScript a in m_woAnglesData)
            {
                if (a.wallOpening == wo)
                {
                    angleToRemove = a;
                    m_woAnglesData.Remove(angleToRemove);
                    Destroy(angleToRemove.gameObject);
                }
            }

            foreach (WallOpeningAngleScript a in m_woAngles3DData)
            {
                if (a.wallOpening == wo)
                {
                    angle3DToRemove = a;
                    m_woAngles3DData.Remove(angle3DToRemove);
                    Destroy(angle3DToRemove.gameObject);
                }
            }

            wo = null;
            AdjustAllWalls();
        }

        public void DestroyWall(Wall w, bool updateRoom = true)
        {
            Debug.LogWarning("Destroying Wall");

            if (updateRoom) UpdateRoomsFromDestroyedWall(w);

            ClearWallGraphics(w);

            Destroy(w.text2D);

            m_wallsData.Remove(w);

            w.Openings.Clear();

            Destroy(w.associated2DObject);
            Destroy(w.associated3DObject);

            var ans = new List<AngleScript>();
            foreach (var a in m_anglesData)
                if (a.w1 == w || a.w2 == w)
                {
                    ans.Add(a);
                    Destroy(a.gameObject);
                }

            foreach (var a in ans) m_anglesData.Remove(a);

            w.linkedP1.Clear();
            w.linkedP2.Clear();
            w.walls2D.Clear();
            w.walls3D.Clear();

            Destroy(w.cotOne?.gameObject);
            Destroy(w.cotTwo?.gameObject);

            w = null;

            AdjustAllWalls();

            // TODO TO DO MOVE MERGING IN MANUAL METHOD ?
            //Dictionary<Wall, Wall> mergeWallsDico = new Dictionary<Wall, Wall>();
            //foreach (var a in m_anglesData)
            //{
            //    if (WallFunctions.GetAngleBetweenWalls(a.w1, a.w2) > 179 || WallFunctions.GetAngleBetweenWalls(a.w1, a.w2) < -179)
            //    {
            //        if (WallFunctions.IsCommonUnique(a.w1, a.w2))
            //        {
            //            mergeWallsDico.Add(a.w1, a.w2);
            //            //WallFunctions.MergeWalls(a.w1, a.w2);
            //            //DestroyWall(a.w2);
            //        }
            //    }
            //}
            //foreach (var m in mergeWallsDico)
            //{
            //    WallFunctions.MergeWalls(m.Key, m.Value);
            //    DestroyWall(m.Value);
            //}
        }

        // Instantiate if not exists
        private void UpdateAngles(Wall w1, Wall w2)
        {
            var angle = m_anglesData.Where(
                a =>
                    a.w1 == w1 && a.w2 == w2
                    ||
                    a.w2 == w1 && a.w1 == w2
            ).FirstOrDefault();

            if (angle == null)
            {
                var a = Instantiate(angleScriptPrefab).GetComponent<AngleScript>();
                a.w1 = w1;
                a.w2 = w2;
                m_anglesData.Add(a);
            }

            foreach (var wo in w1.Openings) UpdateWallOpeningAngles(wo);
            foreach (var wo in w2.Openings) UpdateWallOpeningAngles(wo);
        }

        // Instantiate if not exists
        private void UpdateWallOpeningAngles(WallOpening wo)
        {
            // 2D
            var angles = m_woAnglesData.Where(a => a.wallOpening == wo);
            if (angles.Count() == 0)
            {
                var a = Instantiate(wallOpeningAngleScriptPrefab).GetComponent<WallOpeningAngleScript>();
                a.wallOpening = wo;
                m_woAnglesData.Add(a);
            }
            else
            {
                angles.FirstOrDefault().DrawAngle();
            }

            // 3D
            var angles3D = m_woAngles3DData.Where(a => a.wallOpening == wo);
            if (angles3D.Count() == 0)
            {
                var a3D = Instantiate(wallOpeningAngleScript3DPrefab).GetComponent<WallOpeningAngleScript>();
                a3D.wallOpening = wo;
                m_woAngles3DData.Add(a3D);
            }
            else
            {
                angles3D.FirstOrDefault().DrawAngle();
            }
        }

        private void UpdateWallCotation(Wall w)
        {
            //if (w.ShowDetailedCotations) // && w.Openings.Count > 0)
            //{
            //    UpdateDetailedWallCotation(w);
            //}
            //else
            //{
            //    UpdateSimplifiedWallCotation(w);
            //}

            UpdateSimplifiedWallCotation(w);

            if (w.cotOne.Length == w.cotTwo.Length)
            {
                w.cotTwo.IsExterior = true;
            }
            // Decalage
            if (!w.cotOne.IsExterior && !w.cotTwo.IsExterior)
            {
                w.cotOne.decalTextOffset = 0.2f;
                w.cotTwo.decalTextOffset = 0.2f;
            }
            else
            {
                w.cotOne.decalTextOffset = 0f;
                w.cotTwo.decalTextOffset = 0f;
            }
        }

        private void UpdateDetailedWallCotation(Wall w)
        {
            w.cotOne.gameObject.SetActive(false);
            w.cotTwo.gameObject.SetActive(false);

            var needAdd = false;

            // Need openings + 1
            var totalCotsToHave = w.Openings.Count + 1; // + (m_currentWallOpeningData == null ? 0 : 1);
            if (w.detailedCotations.Count != totalCotsToHave)
            {
                needAdd = true;
                Debug.Log("Reset detailed cotations because " + w.detailedCotations.Count + " versus " + totalCotsToHave);
                foreach (var cot in w.detailedCotations) Destroy(cot.gameObject);
                foreach (var cot in w.detailedCotations2) Destroy(cot.gameObject);
                w.detailedCotations.Clear();
                w.detailedCotations2.Clear();
            }
            else
            {
                foreach (var cot in w.detailedCotations) cot.gameObject.SetActive(true);
                foreach (var cot in w.detailedCotations2) cot.gameObject.SetActive(true);
            }

            if (w.vertices2D.Count < 4 * totalCotsToHave) return;

            // Wall cotations
            for (var i = 0; i < totalCotsToHave; i++)
            {
                if (needAdd)
                {
                    w.detailedCotations.Add(Instantiate(cotationPrefab).GetComponent<CotationsScript>());
                    w.detailedCotations2.Add(Instantiate(cotationPrefab).GetComponent<CotationsScript>());
                }

                w.detailedCotations[i].start = w.vertices2D[i * 4 + 0];
                w.detailedCotations[i].end = w.vertices2D[(i * 4 + 1) % w.vertices2D.Count];
                w.detailedCotations2[i].start = w.vertices2D[(i * 4 + 2) % w.vertices2D.Count];
                w.detailedCotations2[i].end = w.vertices2D[(i * 4 + 3) % w.vertices2D.Count];

                var center = w.detailedCotations[i].start
                             + w.detailedCotations[i].end
                             + w.detailedCotations2[i].start
                             + w.detailedCotations2[i].end;
                center /= 4f;

                w.detailedCotations[i].Sort(w.Direction);
                w.detailedCotations2[i].Sort(w.Direction);

                w.detailedCotations[i].SetPerp(w, center);
                w.detailedCotations2[i].SetPerp(w, center);

                var p = w.detailedCotations[i].Perp * 0.1f;
                var p2 = w.detailedCotations2[i].Perp * 0.1f;

                w.detailedCotations[i].start = w.detailedCotations[i].start + p;
                w.detailedCotations[i].end = w.detailedCotations[i].end + p;
                w.detailedCotations2[i].start = w.detailedCotations2[i].start + p2;
                w.detailedCotations2[i].end = w.detailedCotations2[i].end + p2;

                w.detailedCotations[i].myElement = w;
                w.detailedCotations2[i].myElement = w;
            }
        }

        public override Element CopyPaste(Element m_copiedElement)
        {
            Element e = null;
            if (m_copiedElement is Room)
            {
                var r = m_copiedElement.GetCopy() as Room;
                m_roomsData.Add(r);
                e = r;
            }
            else if (m_copiedElement is Wall)
            {
                var w = m_copiedElement.GetCopy() as Wall;
                m_wallsData.Add(w);
                e = w;
            }
            else
            {
                var wo = m_copiedElement.GetCopy() as WallOpening;
                wo.Wall.Openings.Add(wo);
                e = wo;
            }

            AdjustAllWalls();
            return e;
        }

        private void UpdateSimplifiedWallCotation(Wall w)
        {
            foreach (var cot in w.detailedCotations) cot.gameObject.SetActive(false);
            foreach (var cot in w.detailedCotations2) cot.gameObject.SetActive(false);

            if (!w.cotOne) w.cotOne = Instantiate(cotationPrefab).GetComponent<CotationsScript>();
            if (!w.cotTwo) w.cotTwo = Instantiate(cotationPrefab).GetComponent<CotationsScript>();

            w.cotOne.myElement = w;
            w.cotTwo.myElement = w;

            w.cotOne.gameObject.SetActive(true);
            w.cotTwo.gameObject.SetActive(true);

            List<Vector3> fourVertices = WallFunctions.GetFourVerticesFromWall(w);
            if (fourVertices == null || fourVertices.Count < 4) return;

            w.cotOne.start = fourVertices[0];
            w.cotOne.end = fourVertices[1];

            w.cotTwo.start = fourVertices[2];
            w.cotTwo.end = fourVertices[3];

            w.cotOne.Sort(w.Direction);
            w.cotTwo.Sort(w.Direction);

            w.cotOne.SetPerp(w);
            w.cotTwo.SetPerp(w);

            Vector3 p = w.cotOne.Perp * 0.1f;
            Vector3 p2 = w.cotTwo.Perp * 0.1f;

            w.cotOne.start = w.cotOne.start + p;
            w.cotOne.end = w.cotOne.end + p;
            w.cotTwo.start = w.cotTwo.start + p2;
            w.cotTwo.end = w.cotTwo.end + p2;

            var roomsForW = WallFunctions.GetRoomsFromWall(w, m_roomsData);

            // each room : check if cotone and cottwo are sided in room, if not, hide them
            bool showOne = false, showTwo = false;
            foreach (var r in roomsForW)
                if (Vector3.Distance(w.cotOne.Center, r.GetCenter())
                    < Vector3.Distance(w.cotTwo.Center, r.GetCenter()))
                    showOne = true;
                else if (Vector3.Distance(w.cotOne.Center, r.GetCenter())
                         > Vector3.Distance(w.cotTwo.Center, r.GetCenter()))
                    showTwo = true;
            if (roomsForW.Count == 0)
            {
                showOne = true;
                showTwo = true;
            }

            w.cotOne.IsExterior = !showOne;
            w.cotTwo.IsExterior = !showTwo;
            // Force hiding if same length


            // TODO TO DO traiter le cas ou un a un raycast d'un seul côté à faire
            // If linked walls do raycast
            if (w.linkedP1.Count > 1 || w.linkedP2.Count > 1)
            {
                var res = ShootRayFrom(w.cotOne.Center, -w.Direction, w);
                var res2 = ShootRayFrom(w.cotOne.Center, w.Direction, w);

                var rest = ShootRayFrom(w.cotTwo.Center, -w.Direction, w);
                var rest2 = ShootRayFrom(w.cotTwo.Center, w.Direction, w);

                if (res == Vector3.positiveInfinity) res = w.cotOne.start;
                if (res2 == Vector3.positiveInfinity) res2 = w.cotOne.end;
                if (rest == Vector3.positiveInfinity) rest = w.cotTwo.start;
                if (rest2 == Vector3.positiveInfinity) rest2 = w.cotTwo.end;

                if (Vector3.Distance(res, res2) < w.cotOne.Length) w.cotOne.start = res;
                if (Vector3.Distance(res, res2) < w.cotOne.Length) w.cotOne.end = res2;
                if (Vector3.Distance(rest, rest2) < w.cotTwo.Length) w.cotTwo.start = rest;
                if (Vector3.Distance(rest, rest2) < w.cotTwo.Length) w.cotTwo.end = rest2;
            }
        }

        private Vector3 ShootRayFrom(Vector3 origin, Vector3 dir, Wall w)
        {
            var hits = Physics2D.RaycastAll(origin, dir, Mathf.Infinity, 1 << (int)ErgoLayers.Top);
            foreach (var hit in hits)
                if (hit.collider.gameObject.tag == "Wall")
                {
                    var ok = true;
                    bool okl1 = false, okl2 = false;
                    foreach (var p in w.walls2D)
                        if (p.gameObject == hit.collider.gameObject)
                            ok = false;
                    foreach (var l in w.linkedP1)
                        if (l.walls2D.Select(p => p.gameObject).Contains(hit.collider.gameObject))
                            okl1 = true;
                    foreach (var l in w.linkedP2)
                        if (l.walls2D.Select(p => p.gameObject).Contains(hit.collider.gameObject))
                            okl2 = true;

                    if (ok && (okl1 || okl2))
                        //Debug.DrawLine(origin, hit.point, Color.magenta);
                        return hit.point;
                }

            return Vector3.positiveInfinity;
        }


        /// <summary>
        ///     If a wall gets destroyed, check rooms and disband those who had this wall
        /// </summary>
        /// <param name="w"></param>
        private void UpdateRoomsFromDestroyedWall(Wall w)
        {
            var badRooms = new List<Room>();
            foreach (var r in m_roomsData)
                if (r.Walls.Contains(w))
                    badRooms.Add(r);

            foreach (var r in badRooms) UpdateRoomFromWalls(r.Walls, false, r);
        }

        public void DestroyRoom(Room r)
        {
            if (r == null)
            {
                r = m_roomsData.First();
            }

            foreach (var w in r.Walls) UpdateRoomFromWalls(r.Walls, false, r);

            foreach (var w in r.Walls)
                if (WallHasNoRoom(w))
                    DestroyWall(w);

            m_roomsData.Remove(r);
            r.Walls.Clear();
            Destroy(r.associated2DObject);
            r = null;

            AdjustAllWalls();
            AdjustGroundsAndCeils();
        }

        public void CheckRoomsFusion(float dist = 0.5f)
        {
            foreach (var r1 in m_roomsData)
            {
                foreach (var r2 in m_roomsData)
                {
                    if (r1 != r2)
                    {
                        foreach (var w1 in r1.Walls)
                        {
                            foreach (var w2 in r2.Walls)
                            {
                                if (w1 != w2)
                                {
                                    if (WallFunctions.IsWallContainingOther(w1, w2, dist))
                                    {
                                        UIManager.Instance.ShowMergeRoomsMessage();
                                        StartCoroutine(WaitForMerge(r1, r2, w1, w2, dist));

                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private IEnumerator WaitForMerge(Room r1, Room r2, Wall w1, Wall w2, float dist)
        {
            while (!CustomConfPopinScript.Instance.MadeChoice) { yield return null; }
            CustomConfPopinScript.Instance.MadeChoice = false;

            if (CustomConfPopinScript.Instance.IsYes) { WallFunctions.MergeRoomsCommonWall(r1, r2, w1, w2, dist); }

        }

        private bool WallHasNoRoom(Wall w)
        {
            foreach (var r in m_roomsData)
                if (r.Walls.Contains(w))
                    return false;
            return true;
        }

        private Quaternion RotationSnap(Quaternion rot)
        {
            var euler = rot.eulerAngles;
            euler = new Vector3(euler.x, Mathf.Floor(euler.y / angleSnap) * angleSnap, euler.z);
            return Quaternion.Euler(euler);
        }

        #endregion

        #region public methods

        public override void DestroyEverything()
        {
            while (m_roomsData.Count > 0) DestroyRoom(m_roomsData.First());
            while (m_wallsData.Count > 0) DestroyWall(m_wallsData.First());
        }

        public void CreateWall(bool polygonalRoom)
        {
            GlobalManager.Instance.Set2DTopMode(false);
            m_creationMode = WallType.Wall;
            m_polygonalRoom = polygonalRoom;
        }

        public void CreateDoor()
        {
            GlobalManager.Instance.Set2DTopMode(false);
            m_creationMode = WallType.Door;
        }

        public void CreateWindow()
        {
            GlobalManager.Instance.Set2DTopMode(false);
            m_creationMode = WallType.Window;
        }

        public void CreateRectRoom()
        {
            GlobalManager.Instance.Set2DTopMode(false);
            m_creationMode = WallType.Room;
        }

        public bool IsCreating()
        {
            return m_creationMode != WallType.None;
        }

        public WallOpening GetWallOpeningFromGameObject(GameObject go)
        {
            var associated = WallFunctions.SeekAssociatedObjectFromGameObject(go);
            Wall goodWall = null;
            foreach (var w in m_wallsData)
                if (w.associated2DObject == associated
                    || w.associated3DObject == associated)
                    goodWall = w;
            if (goodWall != null)
                foreach (var wo in goodWall.Openings)
                    if (wo.associated3DObject == go || wo.associated2DObject == go)
                        return wo;
            return null;
        }

        public Wall GetWallFromGameObject(GameObject go)
        {
            var associated = WallFunctions.SeekAssociatedObjectFromGameObject(go);
            foreach (var w in m_wallsData)
                if (w.associated2DObject == associated
                    || w.associated3DObject == associated)
                    return w;
            return null;
        }

        public Room GetRoomFromRoomTextOrGround(GameObject go)
        {
            foreach (var r in m_roomsData)
            {
                if (r.associated2DObject == go) return r;
                if (r.Ground.planeGenerated.gameObject == go) return r;
            }

            return null;
        }

        public void UpdateWallLength(Wall w, float newLength, bool isInside)
        {
            float oldLength = 0;

            if (isInside)
                oldLength = w.InsideLength;
            else
                oldLength = w.OutsideLength;

            Debug.Log("Wants new length : " + newLength);
            newLength = Mathf.Clamp(newLength, 2, 50);

            // Update a wall length
            var moveLength = (oldLength - newLength) / 2f;

            var newP1 = w.P1 + moveLength * w.Direction;
            var newP2 = w.P2 + moveLength * w.Direction * -1;


            // P1 locked ?
            foreach (var linked in w.linkedP1)
            {
                var linkedRooms = WallFunctions.GetRoomsFromWall(linked, m_roomsData);
                if (linkedRooms.Count > 0 && linkedRooms.Where(ro => ro.LockAngles).Count() > 0)
                {
                    newP1 = w.P1;
                    newP2 = w.P2 + moveLength * 2f * w.Direction * -1;
                }
            }

            // P2 locked ?
            foreach (var linked in w.linkedP2)
            {
                var linkedRooms = WallFunctions.GetRoomsFromWall(linked, m_roomsData);
                if (linkedRooms.Count > 0 && linkedRooms.Where(ro => ro.LockAngles).Count() > 0)
                {
                    newP1 = w.P1 + moveLength * 2f * w.Direction;
                    newP2 = w.P2;
                }
            }

            // CAN AUTO LOCK IF P1 LINKED AND NOT P2
            //               IF P2 LINKED AND NOT P1
            if (w.linkedP1.Count == 0 && w.linkedP2.Count > 0)
            {
                newP1 = w.P1 + moveLength * 2f * w.Direction;
                newP2 = w.P2;
            }

            if (w.linkedP2.Count == 0 && w.linkedP1.Count > 0)
            {
                newP1 = w.P1;
                newP2 = w.P2 + moveLength * 2f * w.Direction * -1;
            }

            var linkedR = WallFunctions.GetRoomsFromWall(w, m_roomsData);
            var isRect = linkedR.Count > 0 ? linkedR.Where(ro => ro.LockAngles).Count() > 0 : false;
            WallFunctions.SetNewPointsForWall(w, newP1, newP2, isRect, true);

            // Graphical
            AdjustAllWalls();
            AdjustGroundsAndCeils();
        }

        public void UpdateWallPerp(Wall w, Vector3 destination)
        {
            var projP1 = Vector3.Project(w.P1 - destination, w.Direction) + destination;
            var projP2 = Vector3.Project(w.P2 - destination, w.Direction) + destination;

            // Update linked walls
            var linkedR = WallFunctions.GetRoomsFromWall(w, m_roomsData);
            var isRect = linkedR.Count > 0 ? linkedR.Where(ro => ro.LockAngles).Count() > 0 : false;

            WallFunctions.SetNewPointsForWall(w, projP1, projP2, false, true);
            // Graphical
            AdjustAllWalls();
            AdjustGroundsAndCeils();
        }

        public void UpdateBothWallPoints(Wall w, Vector2 newP1, Vector2 newP2)
        {
            List<Room> linkedR = WallFunctions.GetRoomsFromWall(w, m_roomsData);
            bool isRect = linkedR.Count > 0 ? linkedR.Where(ro => ro.LockAngles).Count() > 0 : false;

            WallFunctions.SetNewPointsForWall(w, newP1, newP2, isRect, true);
            // Graphical
            AdjustAllWalls(); // here is expensive add() 
            AdjustGroundsAndCeils();
        }

        public void UpdateWallPoint(Wall w, Vector2 newP, bool isP1)
        {
            var linkedR = WallFunctions.GetRoomsFromWall(w, m_roomsData);
            var isRect = linkedR.Count > 0 ? linkedR.Where(ro => ro.LockAngles).Count() > 0 : false;
            if (isP1)
            {
                WallFunctions.SetNewPointsForWall(w, newP, w.P2, isRect, true);
                w.P1 = newP;
            }
            else
            {
                WallFunctions.SetNewPointsForWall(w, w.P1, newP, isRect, true);
                w.P2 = newP;
            }

            // Graphical
            AdjustAllWalls();
            AdjustGroundsAndCeils();

            WallPropPanelScript.Instance.UpdateWallProperties();
        }

        /// <summary>
        ///     Recompute position after a cotation edit
        /// </summary>
        /// <param name="wo"></param>
        /// <param name="newPosition"></param>
        public void UpdateWallOpeningPosition(WallOpening wo, Vector2 newPosition)
        {
            wo.SetPosition(newPosition);
            OperationsBufferScript.Instance.AddAutoSave("Déplacement " + (wo.IsWindow ? "Fenetre" : "Porte"));
            AdjustAllWalls();
        }

        /// <summary>
        ///     Generic move function (can put the wall opening on another wall)
        /// </summary>
        /// <param name="wo"></param>
        /// <param name="newPosition"></param>
        public void UpdateWallOpeningPosition(WallOpening wo)
        {
            //Debug.Log("MOVE WALLOPENING");
            Vector3 pos;
            //test if we are in 2d or 3D
            if (GlobalManager.Instance.GetActiveCamera().gameObject.layer != (int)ErgoLayers.ThreeD)
            {
                pos = InputFunctions.GetWorldPoint(GlobalManager.Instance.GetActiveCamera());
            }
            else
            {
                pos = InputFunctions.GetWorldPoint3D(GlobalManager.Instance.GetActiveCamera(), true);
                pos = VectorFunctions.Switch3D2D(pos);
            }

            Wall closestWall = null;
            var closestProj = Vector3.positiveInfinity;
            foreach (var wd in m_wallsData)
                if (closestWall == null)
                {
                    closestWall = wd;
                    closestProj = Math3d.ProjectPointOnLineSegment(wd.P1, wd.P2, pos);
                }
                else
                {
                    var proj = Math3d.ProjectPointOnLineSegment(wd.P1, wd.P2, pos);
                    if (Vector2.Distance(pos, proj)
                        < Vector2.Distance(pos, closestProj))
                    {
                        closestWall = wd;
                        closestProj = proj;
                    }
                }

            if (wo.Wall != null) wo.Wall.Openings.Remove(wo);
            wo.Wall = closestWall;
            if (!closestWall.Openings.Contains(wo)) closestWall.Openings.Add(wo);
            wo.Size = new Vector3(wo.Size.x, wo.Size.y, wo.Wall.Thickness);

            wo.SetPosition(closestProj); // + closestWall.Direction * wo.Size.x);

            foreach (var w in m_wallsData) w.ShowDetailedCotations = false;
            closestWall.ShowDetailedCotations = true;

            // Graphical
            AdjustAllWalls();
        }

        public Room UpdateRoomFromWalls(List<Wall> walls, bool makeRoom, Room previousRoom)
        {
            if (!makeRoom)
            {
                if (previousRoom == null) return null;
                if (previousRoom.Ground != null)
                {
                    Destroy(previousRoom.Ground.planeGenerated);
                    Destroy(previousRoom.Ceil.planeGenerated);
                }

                m_roomsData.Remove(previousRoom);
                Destroy(previousRoom.associated2DObject);
                return null;
            }

            var r = new Room
            {
                Name = "Piece " + (m_roomsData.Count + 1),
                LockAngles = false
            };
            r.Walls = new List<Wall>();
            foreach (var w in walls) r.Walls.Add(w);
            r.Height = walls[0].Height;
            m_roomsData.Add(r);
            AdjustGroundsAndCeils();
            return r;
        }

        public List<Room> GetRooms()
        {
            return m_roomsData;
        }

        public List<Wall> GetWalls()
        {
            return m_wallsData;
        }

        public List<WallOpening> GetWallOpenings()
        {
            var wos = new List<WallOpening>();
            foreach (var w in m_wallsData)
                foreach (var o in w.Openings)
                    wos.Add(o);
            return wos;
        }

        public void LoadRoomsAndWallsFromFloor(Floor floor)
        {
            DestroyEverything();
            m_roomsData = new List<Room>();
            m_wallsData = new List<Wall>();
            // PB DE REFERENCE SUR LES MURS COMMUNS : REBUILD (COMME DANS LE SAVE ?)

            var listIndex = new List<int>();

            foreach (var r in floor.Rooms)
            {
                m_roomsData.Add(r);
                var indexesToUpdate = new List<int>();
                var wallsToUpdateInRooms = new List<Wall>();



                foreach (var w in r.Walls)
                {
                    if (!m_wallsData.Select(wa => wa.Index).Contains(w.Index))
                    {
                        m_wallsData.Add(w);
                    }
                    else
                    {
                        wallsToUpdateInRooms.Add(m_wallsData.Where(wa => wa.Index == w.Index).First());
                        indexesToUpdate.Add(r.Walls.IndexOf(w));
                    }

                    listIndex.Add(w.Index);
                }

                for (var i = 0; i < wallsToUpdateInRooms.Count; i++)
                {
                    r.Walls[indexesToUpdate[i]] = wallsToUpdateInRooms[i];
                }
            }

            if (m_wallsData.Count != 0)
            {
                // Debug.Log("el ROOOMM :" + m_wallsData[m_wallsData.Count - 1].Index);
                LastIndexRoom = m_wallsData[m_wallsData.Count - 1].Index;
            }

            foreach (var w in floor.Walls)
            {
                if (!m_wallsData.Select(wa => wa.Index).Contains(w.Index))
                {
                    m_wallsData.Add(w);
                    listIndex.Add(w.Index);
                }
            }


            //Debug.Log("WALLLL:" + m_wallsData[m_wallsData.Count - 1].Index);
            LastIndexWall = m_wallsData[m_wallsData.Count - 1].Index;


            listIndex.Sort();
            MaxIndex = listIndex[listIndex.Count - 1];
            //Debug.Log("MaxIndex : " + MaxIndex);


            foreach (var w in m_wallsData)
            {
                foreach (var wo in w.Openings) wo.Wall = w;
                w.RebuildSceneData();
                w.associated2DObject.transform.parent = room2D;
                w.associated3DObject.transform.parent = room3D;
            }

            AdjustAllWalls();
            AdjustGroundsAndCeils();

            SetCreationModeNone();


            //  Debug.Log("room : " + m_roomsData.Count + "Walls :" + m_wallsData.Count);
        }

        public void SetThickness(string s)
        {
            float res = 0;
            if (ParsingFunctions.ParseFloatCommaDot(s, out res)) wallThickness = res / 100f;
        }

        public void SetWallsHeight(string s)
        {
            float res = 0;
            if (ParsingFunctions.ParseFloatCommaDot(s, out res)) wallHeight = res / 100f;
        }

        public void ReplaceWall(Wall wall, Wall[] walls)
        {
            Debug.Log("Replace Wall");
            // Room
            var r = WallFunctions.GetRoomsFromWall(wall, m_roomsData);
            if (r.Count == 1)
            {
                r[0].Walls.Remove(wall);
                r[0].Walls.AddRange(walls);
            }

            m_wallsData.AddRange(walls);
            DestroyWall(wall, false);
            AdjustAllWalls();
        }

        #endregion
    }
}