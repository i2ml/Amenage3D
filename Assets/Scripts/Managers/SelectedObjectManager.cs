using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ErgoShop.Cameras;
using ErgoShop.Interactable;
using ErgoShop.Operations;
using ErgoShop.POCO;
using ErgoShop.UI;
using ErgoShop.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.Managers
{
    /// <summary>
    ///     Select objects and update properties
    ///     Works with EVERY type of Element
    /// </summary>
    public class SelectedObjectManager : MonoBehaviour
    {
        /// <summary>
        ///     Element hovered by the mouse, outlined in blue
        /// </summary>
        public GameObject currentHoveredElementGo;

        public CotationsScript currentCotation;


        public float doubleClickSpeed = 0.25f;

        public float moveFurnitureTimer;

        /// <summary>
        ///     Prefab to show object measures
        /// </summary>
        public GameObject cotationPrefab;

        /// <summary>
        ///     automatic measures in 3D
        /// </summary>
        private CurrentCotation3DScript cotations3D;

        public List<CharacterElement> currentCharacters;
        public List<ElementGroup> currentElementGroups;
        public List<Furniture> currentFurnitureData;
        public List<HelperElement> currentHelperElements;

        public Room currentRoomData;

        /// <summary>
        ///     Selected elements of several types
        /// </summary>
        public List<Element> currentSelectedElements;

        public List<Stairs> currentStairs;
        public List<WallOpening> currentWallOpenings;

        // All those lists + the room are the selected elements, filtered by type

        public List<Wall> currentWallsData;

        /// <summary>
        ///     Elements the users wants to copy paste
        /// </summary>
        private List<Element> m_copiedElements;

        private Furniture m_currentPlacingFurniture;
        private float m_currentTimerMove;

        /// <summary>
        ///     Contains the elements the user wants to move with mouse
        /// </summary>
        private List<MovableElement> m_elementsToMove;

        private bool m_firstClick, m_doubleClick;
        private Vector3 m_startingMovePos, m_prevPos;

        private float m_timerDoucleClick;

        public static SelectedObjectManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        private void Start()
        {
            currentWallsData = new List<Wall>();
            currentWallOpenings = new List<WallOpening>();
            currentFurnitureData = new List<Furniture>();
            currentStairs = new List<Stairs>();
            currentHelperElements = new List<HelperElement>();
            currentCharacters = new List<CharacterElement>();
            currentElementGroups = new List<ElementGroup>();

            currentSelectedElements = new List<Element>();
            m_copiedElements = new List<Element>();
            m_elementsToMove = new List<MovableElement>();
            m_timerDoucleClick = 0;
            cotations3D = FindObjectOfType<CurrentCotation3DScript>();
        }

        /// <summary>
        ///     Update method handles everything to do with a selected object :
        ///     1) Double click to focus view on selection
        ///     2) If we are currently selecting with a drag n drop, Reset selection and seek elements in the bounds of the
        ///     selection
        ///     3) Show hovered element, check if clicking to select it, and finally update our lists and measures
        ///     4) Check what objects need to be outlined
        ///     5) Move elements if user hold the click
        ///     6) Check CTRL C CTRL V
        ///     7) Check Suppr
        ///     8) Check focus
        ///     9) Check cancel selection
        ///     10) check cotation edition to update position
        /// </summary>
        private void Update()
        {
            if (!Instance) Instance = this;
            if (!WallsCreator.Instance.IsCreating() &&
                !HelpersCreator.Instance.IsOccupied() &&
                !WallArrowsScript.Instance.isMoving &&
                !ElementArrowsScript.Instance.isMoving &&
                !WallArrowsScript.Instance.isMoving)
            {
                //1
                DoubleClick();
                //2 
                if (SelectionManager.Instance.IsSelecting)
                {
                    ResetSelection();
                    foreach (var elem in AllElementsManager.Instance.AllElements)
                    {
                        if (elem is ElementGroup) continue;
                        if (SelectionManager.Instance.IsWithinSelectionBounds((elem as MovableElement).Position))
                        {
                            var eg = GroupsManager.Instance.GetGroupFromGameObject(currentHoveredElementGo);
                            if (eg != null)
                                currentElementGroups.Add(eg);
                            else if (elem is Furniture) currentSelectedElements.Add(elem);
                        }
                    }

                    var ws = currentSelectedElements.OfType<Wall>();
                    if (ws != null) currentWallsData.AddRange(ws);
                    if (currentRoomData == null) currentRoomData = CheckRoomFromWalls(currentWallsData);
                    var fs = currentSelectedElements.OfType<Furniture>();
                    if (fs != null) currentFurnitureData.AddRange(fs);
                    var wos = currentSelectedElements.OfType<WallOpening>();
                    if (wos != null) currentWallOpenings.AddRange(wos);
                    var ss = currentSelectedElements.OfType<Stairs>();
                    if (ss != null) currentStairs.AddRange(ss);
                    var hs = currentSelectedElements.OfType<HelperElement>();
                    if (hs != null) currentHelperElements.AddRange(hs);
                    var ch = currentSelectedElements.OfType<CharacterElement>();
                    if (ch != null) currentCharacters.AddRange(ch);

                    if (Input.GetMouseButtonUp(0)) SelectionManager.Instance.IsSelecting = false;
                }
                else
                {
                    // 3
                    HoverOnObject();
                    ClickOnObject();
                    UpdateCotationsAndSelectedElements();
                }

                //4
                CheckAffectations();
                //5
                MoveElements();

                //6
                CopyElementsToClipboard();
                PasteElement();
                //7
                if (Input.GetKey(KeyCode.Delete) && (currentCotation == null ||
                                                     currentCotation.cotationField.transform.position ==
                                                     Vector3.one * 10000f)) DeleteSelectedElements();
                //8
                if (Input.GetKeyDown(KeyCode.F) && InputFunctions.IsMouseOutsideUI())
                {
                    FocusOnSelection();
                    StartCoroutine(ReFocusCoroutine());
                }

                //9
                if (Input.GetKeyDown(KeyCode.Escape)) ResetSelection();
            }

            //10
            // COTATION EDITION
            if (currentCotation && currentCotation.cotationField)
            {
                if (currentCotation.is3D)
                {
                    var uiPos = GlobalManager.Instance.cam3D.GetComponent<Camera>()
                        .WorldToScreenPoint(currentCotation.cotationTM.transform.position);
                    currentCotation.cotationField.transform.position = uiPos;
                }
                else
                {
                    var uiPos = GlobalManager.Instance.cam2DTop.GetComponent<Camera>()
                        .WorldToScreenPoint(currentCotation.cotationTM.transform.position);
                    currentCotation.cotationField.transform.position = uiPos;
                }
            }
        }

        /// <summary>
        ///     Add manually elements in selection
        /// </summary>
        /// <param name="s"></param>
        public void Select(Stairs s)
        {
            ResetSelection();
            currentStairs.Add(s);
            UIManager.Instance.ResetTopForms();
        }


        /// <summary>
        ///     Add manually elements in selection
        /// </summary>
        /// <param name="s"></param>
        public void Select(ElementGroup g)
        {
            ResetSelection();
            currentSelectedElements.AddRange(g.Elements);
            UIManager.Instance.ResetTopForms();
        }


        /// <summary>
        ///     Add manually elements in selection
        /// </summary>
        /// <param name="s"></param>
        public void Select(CharacterElement character)
        {
            ResetSelection();
            currentSelectedElements.Add(character);
            UIManager.Instance.ResetTopForms();
        }

        /// <summary>
        ///     Called by PasteElement
        /// </summary>
        /// <param name="elem"></param>
        /// <returns></returns>
        public Element CopyPasteElementWithManager(Element elem)
        {
            if (elem is WallOpening) // || elem is Room || elem is Wall)
                return WallsCreator.Instance.CopyPaste(elem);
            if (elem is Furniture)
                return FurnitureCreator.Instance.CopyPaste(elem);
            if (elem is TextZoneElement)
                return HelpersCreator.Instance.CopyPaste(elem);
            if (elem is Stairs)
                return StairsCreator.Instance.CopyPaste(elem);
            if (elem is CharacterElement) return CharactersCreator.Instance.CopyPaste(elem);
            return null;
        }

        #region private methods

        /// <summary>
        ///     Refocus 0.1 second after focus command
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReFocusCoroutine()
        {
            yield return new WaitForSeconds(0.1f);
            FocusOnSelection();
        }

        /// <summary>
        ///     Double click will focus view on selection
        /// </summary>
        private void DoubleClick()
        {
            if (Input.GetMouseButtonDown(0) && m_firstClick)
            {
                if (m_timerDoucleClick < doubleClickSpeed)
                {
                    m_doubleClick = true;
                    m_firstClick = false;
                    m_timerDoucleClick = 0f;
                }
            }
            else
            {
                m_doubleClick = false;
            }

            if (Input.GetMouseButtonDown(0) && !m_firstClick && m_timerDoucleClick != 0f)
            {
                m_firstClick = true;
                m_timerDoucleClick = 0f;
            }

            if (m_timerDoucleClick > doubleClickSpeed) m_firstClick = false;
            m_timerDoucleClick += Time.deltaTime;
        }

        /// <summary>
        ///     Outline selected objects
        /// </summary>
        private void CheckAffectations()
        {
            foreach (var elem in currentSelectedElements)
                if (!(elem is Room))
                {
                    OutlineFunctions.SetOutlineEnabled(elem.associated2DObject, true);
                    OutlineFunctions.SetOutlineEnabled(elem.associated3DObject, true);
                }
        }

        /// <summary>
        ///     Get hovered gameobject by mouse
        /// </summary>
        /// <returns></returns>
        private GameObject GetHoveredGameObject()
        {
            if (GlobalManager.Instance.GetCurrentMode() == ViewMode.ThreeD)
                return InputFunctions.GetHoveredObject(GlobalManager.Instance.GetActiveCamera());
            if (GlobalManager.Instance.GetCurrentMode() == ViewMode.Top)
                return InputFunctions.GetHoveredObject2D(GlobalManager.Instance.GetActiveCamera());
            return null;
        }

        /// <summary>
        ///     Outline in blue the object hovered
        /// </summary>
        private void HoverOnObject()
        {
            if (currentHoveredElementGo != null)
            {
                OutlineFunctions.SetOutlineEnabled(currentHoveredElementGo, false);
                var eg = GroupsManager.Instance.GetGroupFromGameObject(currentHoveredElementGo);
                if (eg != null)
                    foreach (var e in eg.Elements)
                    {
                        OutlineFunctions.SetOutlineEnabled(e.associated2DObject, false);
                        OutlineFunctions.SetOutlineEnabled(e.associated3DObject, false);
                    }
            }

            currentHoveredElementGo = GetHoveredGameObject();

            if (currentHoveredElementGo)
            {
                var goodTag = currentHoveredElementGo.tag == "Wall"
                              || currentHoveredElementGo.tag == "Furniture"
                              || currentHoveredElementGo.tag == "Stairs"
                              || currentHoveredElementGo.tag == "RoomText"
                              || currentHoveredElementGo.tag == "WallOpening";
                if (goodTag)
                    OutlineFunctions.SetOutlineEnabled(currentHoveredElementGo, true, 1);
                var eg = GroupsManager.Instance.GetGroupFromGameObject(currentHoveredElementGo);
                if (eg != null)
                    foreach (var e in eg.Elements)
                    {
                        OutlineFunctions.SetOutlineEnabled(e.associated2DObject, true, 1);
                        OutlineFunctions.SetOutlineEnabled(e.associated3DObject, true, 1);
                    }
            }
        }

        /// <summary>
        ///     If click on object : select it
        ///     If CTRL is maintained,
        /// </summary>
        private void ClickOnObject()
        {
            if (Input.GetMouseButtonUp(0))
            {
                var go = GetHoveredGameObject();
                // Check view mode and do raycast

                // If we hit a gameobject
                if (go)
                {
                    UpdateUIProperties();

                    if (!InputFunctions.CTRL()
                        && go.tag != "WallArrow" && go.tag != "Cotation" && go.tag != "ElementArrow")
                    {
                        //Debug.Log("RESET");
                        ResetSelection();
                        PropertiesFormManager.Instance.HideAllProperties();
                    }

                    // PRIORITY ONE IS GROUPS
                    var eg = GroupsManager.Instance.GetGroupFromGameObject(go);
                    if (eg != null)
                    {
                        currentElementGroups.Add(eg);
                    }
                    // ROOM
                    else if (go.tag == "RoomText" || go.tag == "Ground")
                    {
                        currentRoomData = WallsCreator.Instance.GetRoomFromRoomTextOrGround(go);
                        if (currentRoomData != null)
                        {
                            currentWallsData.Clear();
                            currentWallsData.AddRange(currentRoomData.Walls);
                            RoomPropPanelScript.Instance.UpdateRoomProperties();
                        }
                    }
                    // WALL
                    else if (go.tag == "Wall")
                    {
                        var potentialWall = WallsCreator.Instance.GetWallFromGameObject(go);
                        if (potentialWall != null)
                        {
                            if (currentWallsData.Contains(potentialWall))
                            {
                                OutlineFunctions.SetOutlineEnabled(potentialWall.associated2DObject, false);
                                OutlineFunctions.SetOutlineEnabled(potentialWall.associated3DObject, false);
                                potentialWall.ShowDetailedCotations = false;
                                currentWallsData.Remove(potentialWall);
                            }
                            else
                            {
                                currentWallsData.Add(potentialWall);
                                potentialWall.ShowDetailedCotations = false;
                            }

                            WallPropPanelScript.Instance.UpdateWallProperties();

                            if (currentRoomData == null) currentRoomData = CheckRoomFromWalls(currentWallsData);
                            ResetFurnitures();
                            ResetWallOpening();
                        }
                    }
                    // WALL OPENING
                    else if (go.tag == "WallOpening")
                    {
                        var potentialWo = WallsCreator.Instance.GetWallOpeningFromGameObject(go);
                        if (potentialWo != null)
                        {
                            Debug.Log("OUVERTURE");
                            currentWallOpenings.Add(potentialWo);
                            OutlineFunctions.SetOutlineEnabled(potentialWo.associated2DObject, true);
                            OutlineFunctions.SetOutlineEnabled(potentialWo.associated3DObject, true);
                            potentialWo.Wall.ShowDetailedCotations = true;

                            WallOpeningPropScript.Instance.UpdateWallOpeningProperties();
                            ResetFurnitures();
                            ResetWalls();
                        }
                    }
                    // STAIRS
                    else if (go.tag == "Stairs")
                    {
                        var potentialStairs = StairsCreator.Instance.GetStairsFromGameObject(go);
                        if (potentialStairs != null)
                        {
                            currentStairs.Add(potentialStairs);
                            OutlineFunctions.SetOutlineEnabled(potentialStairs.associated2DObject, true);
                            OutlineFunctions.SetOutlineEnabled(potentialStairs.associated3DObject, true);
                        }
                    }
                    // FURNITURE
                    else if (go.tag == "Furniture")
                    {
                        var potentialFurniture = FurnitureCreator.Instance.GetFurnitureFromGameObject(go);
                        if (potentialFurniture != null)
                        {
                            if (currentFurnitureData.Contains(potentialFurniture))
                            {
                                OutlineFunctions.SetOutlineEnabled(potentialFurniture.associated2DObject, false);
                                OutlineFunctions.SetOutlineEnabled(potentialFurniture.associated3DObject, false);
                                currentFurnitureData.Remove(potentialFurniture);
                            }
                            else
                            {
                                currentFurnitureData.Add(potentialFurniture);
                            }

                            if (currentFurnitureData.Count == 1)
                                FurniturePropScript.Instance.UpdateFurnitureProperties();

                            ResetWalls();
                            ResetWallOpening();
                        }
                    }
                    // HELPER ELEMENT
                    else if (go.tag == "Helper")
                    {
                        var potentialHelper = HelpersCreator.Instance.GetHelperFromGameObject(go);
                        if (potentialHelper != null)
                        {
                            currentHelperElements.Add(potentialHelper);
                            OutlineFunctions.SetOutlineEnabled(potentialHelper.associated2DObject, true);
                            OutlineFunctions.SetOutlineEnabled(potentialHelper.associated3DObject, true);
                            TextZonePropertiesScript.Instance.UpdateTextZoneProperties();
                        }
                    }
                    // CHARACTER
                    else if (go.tag == "Character")
                    {
                        var potentialCharacter = CharactersCreator.Instance.GetCharacterFromGameObject(go);
                        if (potentialCharacter != null)
                        {
                            currentCharacters.Add(potentialCharacter);
                            OutlineFunctions.SetOutlineEnabled(potentialCharacter.associated2DObject, true);
                            OutlineFunctions.SetOutlineEnabled(potentialCharacter.associated3DObject, true);
                            CharacterPropertiesScript.Instance.UpdateCharacterProperties();
                        }
                    }
                }
                else
                {
                    if (InputFunctions.IsMouseOutsideUI()) ResetSelection();
                }

                if (m_doubleClick && go)
                {
                    Debug.Log("dbl clic");
                    m_doubleClick = false;
                    FocusOnSelection();
                    StartCoroutine(ReFocusCoroutine());
                }
            }
        }

        /// <summary>
        ///     Update the UI shown according to selected element
        /// </summary>
        private void UpdateUIProperties()
        {
            TextZonePropertiesScript.Instance.UpdateTextZoneProperties();
            FurniturePropScript.Instance.UpdateFurnitureProperties();
            WallOpeningPropScript.Instance.UpdateWallOpeningProperties();
            WallPropPanelScript.Instance.UpdateWallProperties();
            RoomPropPanelScript.Instance.UpdateRoomProperties();
        }

        /// <summary>
        ///     Move elements if users clicks and hold on the selection
        /// </summary>
        private void MoveElements()
        {
            if (m_currentPlacingFurniture != null)
                PlaceFurniture();
            else if (InputFunctions.IsMouseOutsideUI())
                // (movableelements)
                if (currentSelectedElements.Where(e => e is MovableElement).Count() == currentSelectedElements.Count)
                {
                    // Click on current movable element = start to move it
                    var go = InputFunctions.GetHoveredObject(GlobalManager.Instance.GetActiveCamera());
                    if (Input.GetMouseButtonDown(0) && IsGoInSelectedElements(go))
                    {
                        foreach (var elem in currentSelectedElements)
                            if (!m_elementsToMove.Contains(elem))
                                m_elementsToMove.Add(elem as MovableElement);
                        m_startingMovePos = InputFunctions.GetWorldPoint(GlobalManager.Instance.GetActiveCamera());
                    }

                    // Cant move if several elements and one of them at least is on wall
                    var onlyOneWall = m_elementsToMove.Count == 1 ||
                                      m_elementsToMove.Where(m => m is Furniture && (m as Furniture).IsOnWall)
                                          .Count() == 0;
                    // while pressed, update furniture position
                    if (Input.GetMouseButton(0) && m_elementsToMove.Count > 0 &&
                        m_elementsToMove.Where(m => m.IsLocked).Count() == 0 && onlyOneWall)
                    {
                        if (m_currentTimerMove > moveFurnitureTimer)
                            foreach (var elem in m_elementsToMove)
                            {
                                Debug.Log("Moving");
                                if (elem.associated3DObject)
                                {
                                    var rb = elem.associated3DObject.GetComponent<Rigidbody>();
                                    if (rb) rb.constraints = RigidbodyConstraints.FreezeAll;
                                }

                                elem.Move(m_startingMovePos);
                            }
                        else
                            m_currentTimerMove += Time.deltaTime;

                        m_startingMovePos = InputFunctions.GetWorldPoint(GlobalManager.Instance.GetActiveCamera());
                    }

                    // End pressing = stop update furniture position
                    if (Input.GetMouseButtonUp(0) && m_elementsToMove.Count > 0)
                    {
                        Debug.Log("End moving");
                        if (m_elementsToMove.Any(elem => elem is Wall))
                        {
                            Debug.Log("Y AVAIT UN MUR AU MOINS");
                            WallsCreator.Instance.CheckRoomsFusion();
                        }

                        m_elementsToMove.Clear();
                        m_currentTimerMove = 0f;
                        OperationsBufferScript.Instance.AddAutoSave("Déplacement d'un élément");
                    }
                }
        }

        /// <summary>
        ///     Seek room containing all the walls
        /// </summary>
        /// <param name="currentWallsData">the walls</param>
        /// <returns></returns>
        private Room CheckRoomFromWalls(List<Wall> currentWallsData)
        {
            foreach (var r in WallsCreator.Instance.GetRooms())
            {
                var ok = true;
                foreach (var w in currentWallsData)
                    if (!r.Walls.Contains(w))
                        ok = false;
                if (ok) return r.Walls.Count == currentWallsData.Count ? r : null;
            }

            return null;
        }

        #endregion

        #region public methods

        /// <summary>
        ///     Update wall length coroutine
        /// </summary>
        /// <param name="w"></param>
        /// <param name="res"></param>
        /// <param name="updateMin"></param>
        /// <returns></returns>
        public IEnumerator SWLRoutine(Wall w, float res, bool updateMin)
        {
            yield return new WaitForSeconds(0.1f);

            WallsCreator.Instance.UpdateWallLength(w, res, updateMin);
            yield return new WaitForSeconds(0.1f);
            WallPropPanelScript.Instance.UpdateWallProperties();
        }

        /// <summary>
        ///     Update element size when updating ui
        /// </summary>
        public void UpdateCharacterSize()
        {
            foreach (var ch in currentCharacters)
            {
                var s = ch.Size;
                var ms = ch.MeshSize;
                var sOnMs = new Vector3(s.x / ms.x, s.y / ms.y, s.z / ms.z);

                ch.associated3DObject.transform.localScale = sOnMs;
                ch.associated2DObject.transform.localScale = VectorFunctions.Switch3D2D(sOnMs);
            }

            OperationsBufferScript.Instance.AddAutoSave("Changement taille personnage");
        }

        /// <summary>
        ///     Update element size when updating ui
        /// </summary>
        public void UpdateFurnitureSize()
        {
            foreach (var f in currentFurnitureData)
            {
                    var s = f.Size;
                    var ms = f.MeshSize;
                    var sOnMs = new Vector3(s.x / ms.x, s.y / ms.y, s.z / ms.z);

                    f.associated3DObject.transform.localScale = sOnMs;
                    f.associated2DObject.transform.localScale = VectorFunctions.Switch3D2D(sOnMs) / f.ScaleModifier;

                    f.AdjustSpriteSize();
            }

            OperationsBufferScript.Instance.AddAutoSave("Changement taille de " + currentFurnitureData.Count +
                                                        " meuble" + (currentFurnitureData.Count > 1 ? "s" : ""));
        }

        /// <summary>
        ///     Update element name when updating ui
        /// </summary>
        public void UpdateFurnitureName()
        {
            foreach (var f in currentFurnitureData) f.text2D.text = f.Name;
            OperationsBufferScript.Instance.AddAutoSave("Changement du nom de " + currentFurnitureData.Count +
                                                        " meuble" + (currentFurnitureData.Count > 1 ? "s" : ""));
        }

        /// <summary>
        ///     Not used !
        /// </summary>
        public void SetRectangularRoom()
        {
            if (currentRoomData.Walls.Count == 4) WallsCreator.Instance.AdjustAllWalls();
            OperationsBufferScript.Instance.AddAutoSave("?");
        }

        /// <summary>
        ///     Update all lists
        /// </summary>
        public void UpdateCotationsAndSelectedElements()
        {
            var decalX = Vector3.right * 0.1f;
            var decalY = Vector3.up * 0.1f;

            currentSelectedElements.Clear();
            if (currentRoomData != null) currentSelectedElements.Add(currentRoomData);
            currentSelectedElements.AddRange(currentWallsData);
            currentSelectedElements.AddRange(currentFurnitureData);
            currentSelectedElements.AddRange(currentWallOpenings);
            currentSelectedElements.AddRange(currentStairs);
            currentSelectedElements.AddRange(currentHelperElements);
            currentSelectedElements.AddRange(currentCharacters);

            foreach (var eg in currentElementGroups)
            {
                currentSelectedElements.AddRange(eg.Elements);
                var rd = eg.Elements.OfType<Room>();
                if (rd != null) currentRoomData = rd.FirstOrDefault();
                var ws = eg.Elements.OfType<Wall>();
                if (ws != null) currentWallsData.AddRange(ws);
                var fs = eg.Elements.OfType<Furniture>();
                if (fs != null) currentFurnitureData.AddRange(fs);
                var wos = eg.Elements.OfType<WallOpening>();
                if (wos != null) currentWallOpenings.AddRange(wos);
                var ss = eg.Elements.OfType<Stairs>();
                if (ss != null) currentStairs.AddRange(ss);
                var hs = eg.Elements.OfType<HelperElement>();
                if (hs != null) currentHelperElements.AddRange(hs);
                var ch = currentSelectedElements.OfType<CharacterElement>();
                if (ch != null) currentCharacters.AddRange(ch);
            }

            currentWallsData = currentWallsData.Distinct().ToList();
            currentFurnitureData = currentFurnitureData.Distinct().ToList();
            currentWallOpenings = currentWallOpenings.Distinct().ToList();
            currentStairs = currentStairs.Distinct().ToList();
            currentHelperElements = currentHelperElements.Distinct().ToList();
            currentCharacters = currentCharacters.Distinct().ToList();

            currentElementGroups = currentElementGroups.Distinct().ToList();
            currentSelectedElements = currentSelectedElements.Distinct().ToList();
        }

        /// <summary>
        ///     Focus 3d view face
        /// </summary>
        public void FocusOnSelectionFace()
        {
            Vector3 center = GetSelectedElementsCenter();
            center = new Vector3(center.x, 2f, center.z);

            GlobalManager.Instance.cam3D.GetComponent<Camera3DMove>().SetPosition(center + Vector3.forward * 5f);
            GlobalManager.Instance.cam3D.GetComponent<Camera3DMove>().SetRotation(new Vector3(0, 180, 0));
        }

        /// <summary>
        ///     Focus 3d/2d view top
        /// </summary>
        public void FocusOnSelection()
        {
            Vector3 center = GetSelectedElementsCenter();
            center += Vector3.right * -2f;

            if (GlobalManager.Instance.GetActiveCamera().gameObject == GlobalManager.Instance.cam2DTop)
            {
                GlobalManager.Instance.cam2DTop.GetComponent<Camera2DMove>().SetPosition(center);
            }
            else
            {
                GlobalManager.Instance.cam3D.GetComponent<Camera3DMove>().SetPosition(center + Vector3.up * 10f);
                GlobalManager.Instance.cam3D.GetComponent<Camera3DMove>().SetRotation(new Vector3(90, 0, 0));

                //GlobalManager.Instance.cam3D.transform.LookAt(center);
            }



            //Vector3 center2D = Vector3.zero;
            //bool lookAtWall = false;
            //Transform selectionTrans=null;
            //if (currentRoomData != null)
            //{
            //    center2D = currentRoomData.Walls.GetCenter();
            //    selectionTrans = currentRoomData.Ground.planeGenerated.gameObject.transform;
            //}
            //else if (currentWallsData.Count > 0)
            //{
            //    center2D = currentWallsData.GetCenter();
            //    lookAtWall = true;
            //}
            //else if (currentFurnitureData.Count > 0)
            //{
            //    center2D = currentFurnitureData.GetCenter();
            //    selectionTrans = currentFurnitureData[0].associated3DObject.transform;
            //}
            //else if (currentWallOpenings.Count > 0)
            //{
            //    center2D = currentWallOpenings[0].Position;
            //    selectionTrans = currentWallOpenings[0].associated3DObject.transform;
            //}
            //else return;

            //Camera cam = GlobalManager.Instance.GetActiveCamera();

            //center2D -= Vector3.right * 2f;
            //Debug.Log("Set center2D " + center2D);
            //GlobalManager.Instance.cam2DTop.GetComponent<Camera2DMove>().SetPosition(center2D);

            //GlobalManager.Instance.cam3D.GetComponent<Camera3DMove>().SetPosition(VectorFunctions.Switch2D3D(center2D, 8f));
            ////gm.cam3D.transform.LookAt(selectionTrans);
            //GlobalManager.Instance.cam3D.GetComponent<Camera3DMove>().SetRotation(GlobalManager.Instance.cam3D.transform.localEulerAngles);

            //if (lookAtWall)
            //{
            //    Debug.Log("LOOK At WALL");
            //    Vector3 pos;
            //    List<Room> rooms = WallFunctions.GetRoomsFromWall(currentWallsData[0], WallsCreator.Instance.GetRooms());
            //    List<Vector3> positions = new List<Vector3>();
            //    foreach (var r in rooms)
            //    {
            //        Vector3 perp = currentWallsData[0].GetInteriorSide(r);
            //        if (perp == Vector3.positiveInfinity) perp = currentWallsData[0].Perpendicular;
            //        positions.Add(VectorFunctions.Switch2D3D(currentWallsData[0].Center + perp * currentWallsData[0].Length * 0.5f, currentWallsData[0].Height / 2f));
            //    }
            //    // We keep the closest position from current camera position
            //    if (positions.Count > 0) {
            //        pos = positions[0];
            //        foreach (var p in positions)
            //        {
            //            if (Vector3.Distance(p, GlobalManager.Instance.cam3D.transform.position)
            //              < Vector3.Distance(pos, GlobalManager.Instance.cam3D.transform.position))
            //            {
            //                pos = p;
            //            }
            //        }       
            //    }
            //    else
            //    {
            //        pos = VectorFunctions.Switch2D3D(currentWallsData[0].Center + currentWallsData[0].Perpendicular * currentWallsData[0].Length * 0.5f, currentWallsData[0].Height / 2f);
            //    }
            //    GlobalManager.Instance.cam3D.GetComponent<Camera3DMove>().SetPosition(pos);
            //    GlobalManager.Instance.cam3D.transform.LookAt(VectorFunctions.Switch2D3D(currentWallsData[0].Center, currentWallsData[0].Height/2f));
            //    GlobalManager.Instance.cam3D.GetComponent<Camera3DMove>().SetRotation(GlobalManager.Instance.cam3D.transform.localEulerAngles);
            //}
            //else
            //{
            //    if (currentFurnitureData.Count > 0 && currentFurnitureData[0].IsOnWall)
            //    {
            //        Vector3 pos = currentFurnitureData[0].Position + currentFurnitureData[0].associated3DObject.transform.forward * 4f;
            //        GlobalManager.Instance.cam3D.GetComponent<Camera3DMove>().SetPosition(new Vector3(pos.x, 1.75f, pos.z) + GlobalManager.Instance.cam3D.transform.right*-0.3f);
            //        GlobalManager.Instance.cam3D.GetComponent<Camera3DMove>().SetRotation(currentFurnitureData[0].associated3DObject.transform.localEulerAngles + Vector3.up*180f);
            //    }
            //    else
            //    {
            //        GlobalManager.Instance.cam3D.GetComponent<Camera3DMove>().SetRotation(Vector3.right * 90f);
            //    }
            //}
        }

        /// <summary>
        ///     Return mean of every element position
        /// </summary>
        /// <returns></returns>
        private Vector3 GetSelectedElementsCenter()
        {
            Vector3 center = Vector3.up * 10;

            foreach (var e in currentSelectedElements)
                if (e is MovableElement)
                    center += (e as MovableElement).Position;

            center = new Vector3
                (
                    center.x / currentSelectedElements.Count(),
                    center.y / currentSelectedElements.Count(),
                    center.z / currentSelectedElements.Count()
                 );

            return center;
        }

        /// <summary>
        ///     seek room center and move camera in the center
        /// </summary>
        public void CenterCameraInRoom()
        {
            if (currentRoomData != null)
            {
                GlobalManager.Instance.cam3D.GetComponent<Camera3DMove>()
                    .SetPosition(currentRoomData.GetCenter() + Vector3.up * 3 * currentRoomData.Height);
                GlobalManager.Instance.cam3D.GetComponent<Camera3DMove>().SetRotation(Vector3.right * 90f);
            }
            else
            {
                if (WallsCreator.Instance.GetRooms().Count > 0)
                {
                    GlobalManager.Instance.cam3D.GetComponent<Camera3DMove>().SetPosition(
                        WallsCreator.Instance.GetRooms()[0].GetCenter() +
                        Vector3.up * 3 * WallsCreator.Instance.GetRooms()[0].Height);
                    GlobalManager.Instance.cam3D.GetComponent<Camera3DMove>().SetRotation(Vector3.right * 90f);
                }
            }
        }

        /// <summary>
        ///     Group or ungroup according to the checkbox state
        /// </summary>
        /// <param name="v">Checkbox isOn</param>
        public void ToggleGroupSelection(bool v)
        {
            if (v)
                GroupSelection();
            else
                UnGroupSelection();
            OperationsBufferScript.Instance.AddAutoSave(v ? "Groupement" : "Dégroupement");
        }

        /// <summary>
        ///     Group the selection by asking groupmanager
        /// </summary>
        private void GroupSelection()
        {
            currentElementGroups.Add(GroupsManager.Instance.CreateGroup(currentSelectedElements));
        }

        /// <summary>
        ///     unGroup the selection by asking groupmanager
        /// </summary>
        private void UnGroupSelection()
        {
            foreach (var eg in currentElementGroups) GroupsManager.Instance.RemoveGroup(eg);
            currentElementGroups.Clear();
            UpdateCotationsAndSelectedElements();
        }

        /// <summary>
        ///     Reset selection by clearing all lists
        /// </summary>
        public void ResetSelection()
        {
            ResetCotations();
            ResetWalls();
            ResetFurnitures();
            ResetWallOpening();
            ResetStairs();
            ResetHelpers();
            ResetCharacters();
            ResetGroups();

            if (currentCotation && currentCotation.cotationField)
                currentCotation.cotationField.transform.position = Vector3.one * 10000;

            currentCotation = null;
            currentSelectedElements.Clear();
            UpdateUIProperties();
        }

        /// <summary>
        ///     Destroy objects and data in selected elements
        /// </summary>
        public void DeleteSelectedElements()
        {
            //DeleteGroups();

            if (currentRoomData != null) DeleteRoom();

            DeleteWalls();
            DeleteFurnitures();
            DeleteWallOpening();
            DeleteStairs();
            DeleteHelpers();
            DeleteCharacters();

            OperationsBufferScript.Instance.AddAutoSave("Suppression");
        }

        /// <summary>
        ///     Show or Hide the room measures according to the checkbox state
        /// </summary>
        /// <param name="v">Checkbox isOn</param>
        public void ToggleRoomCotations(bool v)
        {
            v = !v;
            if (currentRoomData != null) currentRoomData.ShowCotations = v;
            OperationsBufferScript.Instance.AddAutoSave((v ? "Affichage" : "Masquage") + " cotation pièce " +
                                                        currentRoomData.Name);
        }

        /// <summary>
        ///     Show or Hide elements according to the checkbox state (the eye)
        /// </summary>
        /// <param name="v">Checkbox isOn</param>
        public void ToggleSelectedElementsVisibility(bool v)
        {
            v = !v;
            cotations3D.gameObject.SetActive(v);

            if (currentRoomData != null)
            {
                currentRoomData.Ground.planeGenerated.gameObject.SetActive(v);
                currentRoomData.Ceil.planeGenerated.gameObject.SetActive(v);
            }

            foreach (var elem in currentSelectedElements) elem.associated3DObject.gameObject.SetActive(v);
            OperationsBufferScript.Instance.AddAutoSave((v ? "Affichage" : "Masquage") + " d'éléments");
        }

        /// <summary>
        ///     Group or ungroup according to the checkbox state
        /// </summary>
        /// <param name="v">Checkbox isOn</param>
        public void ToggleSelectedElementsLockState(bool v)
        {
            foreach (var elem in currentSelectedElements)
                if (elem is MovableElement)
                    (elem as MovableElement).IsLocked = v;
            OperationsBufferScript.Instance.AddAutoSave((v ? "Verrouillage" : "Dévérouillage") + " d'éléments");
        }

        /// <summary>
        /// </summary>
        /// <returns> true if user is moving elements</returns>
        public bool IsOccupied()
        {
            return m_elementsToMove.Count > 0;
        }

        #endregion

        #region public wall methods

        /// <summary>
        ///     Add manually elements in selection
        /// </summary>
        /// <param name="s"></param>
        public void Select(Room r)
        {
            currentRoomData = r;
            currentWallsData.Clear();
            currentWallsData.AddRange(currentRoomData.Walls);
            RoomPropPanelScript.Instance.UpdateRoomProperties();
            UIManager.Instance.ResetTopForms();
        }


        /// <summary>
        ///     Reset selection
        /// </summary>
        public void ResetWalls()
        {
            foreach (var w in currentWallsData)
            {
                OutlineFunctions.SetOutlineEnabled(w.associated2DObject, false);
                OutlineFunctions.SetOutlineEnabled(w.associated3DObject, false);
                w.ShowDetailedCotations = false;
            }

            currentWallsData.Clear();
            currentRoomData = null;
        }

        /// <summary>
        ///     Create room from walls (without verification) or remove room data
        /// </summary>
        /// <param name="makeRoom">make or not room</param>
        public void SetRoomFromWalls(bool makeRoom)
        {
            currentRoomData = WallsCreator.Instance.UpdateRoomFromWalls(currentWallsData, makeRoom, currentRoomData);
        }

        /// <summary>
        ///     Update wall color from button color (which color is based on color picker)
        /// </summary>
        /// <param name="b"></param>
        public void SetWallsColor(Button b)
        {
            foreach (var w in currentWallsData) w.Color = b.colors.normalColor;
            WallsCreator.Instance.AdjustAllWalls();
            OperationsBufferScript.Instance.AddAutoSave("Changement de couleur des murs");
        }

        /// <summary>
        ///     Update ground color from button color (which color is based on color picker)
        /// </summary>
        /// <param name="b"></param>
        public void SetGroundColor(Button b)
        {
            currentRoomData.Ground.Color = b.colors.normalColor;
            WallsCreator.Instance.AdjustGroundsAndCeils();
            OperationsBufferScript.Instance.AddAutoSave("Changement de couleur du sol de " + currentRoomData.Name);
        }

        /// <summary>
        ///     delete
        /// </summary>
        public void DeleteWalls()
        {
            foreach (var w in currentWallsData) WallsCreator.Instance.DestroyWall(w);
            currentWallsData.Clear();
        }

        /// <summary>
        ///     delete
        /// </summary>
        public void DeleteRoom()
        {
            WallsCreator.Instance.DestroyRoom(currentRoomData);
            currentRoomData = null;
            currentWallsData.Clear();
        }

        /// <summary>
        ///     Update room name from ui
        /// </summary>
        /// <param name="t"></param>
        public void SetRoomName(string t)
        {
            currentRoomData.Name = t;
            currentRoomData.associated2DObject.GetComponent<TextMesh>().text = t;
            RoomPropPanelScript.Instance.UpdateRoomProperties();
            OperationsBufferScript.Instance.AddAutoSave("Renommage de la pièce en " + currentRoomData.Name);
        }

        /// <summary>
        ///     Lock angles or not (doesn't have to be 90)
        /// </summary>
        /// <param name="v"></param>
        public void SetRoomAngles90(bool v)
        {
            currentRoomData.LockAngles = v;
            RoomPropPanelScript.Instance.UpdateRoomProperties();
            OperationsBufferScript.Instance.AddAutoSave((v ? "Verrouillage" : "Dévérouillage") +
                                                        "des angles de la pièce");
        }

        #endregion

        #region public furniture methods

        /// <summary>
        ///     Select furniture and moves it if place
        /// </summary>
        /// <param name="f">Furniture data</param>
        /// <param name="place">move it or not</param>
        public void Select(Furniture f, bool place = true)
        {
            ResetSelection();
            currentFurnitureData.Clear();
            currentFurnitureData.Add(f);
            if (place) m_currentPlacingFurniture = f;
        }

        /// <summary>
        ///     Move furniture
        /// </summary>
        /// <param name="init"></param>
        public void PlaceFurniture(bool init = false)
        {
            if (init) m_prevPos = Vector3.zero;
            m_currentPlacingFurniture.Move(m_prevPos);
            if (Input.GetMouseButtonDown(0))
            {
                OperationsBufferScript.Instance.AddAutoSave("Placement du meuble " + m_currentPlacingFurniture.Name);

                m_currentPlacingFurniture = null;
            }

            m_prevPos = InputFunctions.GetWorldPoint(GlobalManager.Instance.GetActiveCamera());
        }

        /// <summary>
        ///     reset
        /// </summary>
        public void ResetFurnitures()
        {
            foreach (var f in currentFurnitureData)
            {
                OutlineFunctions.SetOutlineEnabled(f.associated2DObject, false);
                OutlineFunctions.SetOutlineEnabled(f.associated3DObject, false);
            }

            currentFurnitureData.Clear();
        }

        /// <summary>
        ///     hide cotation
        /// </summary>
        public void ResetCotations()
        {
            foreach (var elem in currentSelectedElements)
            {
                if (elem == null) continue;
                if (elem.widthCotation)
                {
                    elem.widthCotation.gameObject.SetActive(false);
                    elem.heightCotation.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        ///     reset
        /// </summary>
        public void ResetWallOpening()
        {
            foreach (var wo in currentWallOpenings)
            {
                OutlineFunctions.SetOutlineEnabled(wo.associated2DObject, false);
                OutlineFunctions.SetOutlineEnabled(wo.associated3DObject, false);
                wo.Wall.ShowDetailedCotations = false;
            }

            currentWallOpenings.Clear();
        }


        /// <summary>
        ///     reset
        /// </summary>
        public void ResetHelpers()
        {
            foreach (var help in currentHelperElements)
            {
                OutlineFunctions.SetOutlineEnabled(help.associated2DObject, false);
                OutlineFunctions.SetOutlineEnabled(help.associated3DObject, false);
            }

            currentHelperElements.Clear();
        }


        /// <summary>
        ///     reset
        /// </summary>
        public void ResetCharacters()
        {
            foreach (var chara in currentCharacters)
            {
                OutlineFunctions.SetOutlineEnabled(chara.associated2DObject, false);
                OutlineFunctions.SetOutlineEnabled(chara.associated3DObject, false);
            }

            currentCharacters.Clear();
        }


        /// <summary>
        ///     reset
        /// </summary>
        public void ResetGroups()
        {
            foreach (var eg in currentElementGroups)
                foreach (var elem in eg.Elements)
                {
                    OutlineFunctions.SetOutlineEnabled(elem.associated2DObject, false);
                    OutlineFunctions.SetOutlineEnabled(elem.associated3DObject, false);
                }

            currentElementGroups.Clear();
        }


        /// <summary>
        ///     reset
        /// </summary>
        public void ResetStairs()
        {
            foreach (var s in currentStairs)
            {
                OutlineFunctions.SetOutlineEnabled(s.associated2DObject, false);
                OutlineFunctions.SetOutlineEnabled(s.associated3DObject, false);
            }

            currentStairs.Clear();
        }


        /// <summary>
        ///     delete
        /// </summary>
        public void DeleteWallOpening()
        {
            foreach (var wo in currentWallOpenings) WallsCreator.Instance.DestroyWallOpening(wo);
            currentWallOpenings.Clear();
        }

        /// <summary>
        ///     delete
        /// </summary>
        public void DeleteStairs()
        {
            foreach (var s in currentStairs) StairsCreator.Instance.DestroyStairs(s);
            currentStairs.Clear();
        }

        /// <summary>
        ///     delete
        /// </summary>
        public void DeleteHelpers()
        {
            foreach (var h in currentHelperElements) HelpersCreator.Instance.DestroyHelpers(h);
            currentHelperElements.Clear();
        }

        /// <summary>
        ///     delete
        /// </summary>
        public void DeleteCharacters()
        {
            foreach (var h in currentCharacters) CharactersCreator.Instance.DestroyCharacter(h);
            currentCharacters.Clear();
        }

        /// <summary>
        ///     delete
        /// </summary>
        public void DeleteFurnitures()
        {
            foreach (var f in currentFurnitureData) FurnitureCreator.Instance.DestroyFurniture(f);
            currentFurnitureData.Clear();
        }

        /// <summary>
        ///     check if has selection or not
        /// </summary>
        /// <returns></returns>
        public bool HasNoSelection()
        {
            return currentSelectedElements.Count == 0;
        }

        /// <summary>
        /// </summary>
        /// <param name="cs"></param>
        public void SetCurrentCotation(CotationsScript cs)
        {
            currentCotation = cs;

            var uiPos = GlobalManager.Instance.cam2DTop.GetComponent<Camera>()
                .WorldToScreenPoint(cs.cotationTM.transform.position);
            cs.cotationField.transform.position = uiPos;
            cs.cotationField.text = cs.cotationTM.text;
            cs.cotationField.gameObject.SetActive(true);
        }

        /// <summary>
        ///     Update element position according to modified cotation from user
        /// </summary>
        /// <param name="value"></param>
        public void UpdateCurrentCotation(string value)
        {
            float res = 0;
            var ok = ParsingFunctions.ParseFloatCommaDot(value, out res);
            if (!ok) return;

            var diff = currentCotation.Length * 100f - res;

            if (currentCotation.cotationField == null)
                foreach (var inf in FindObjectsOfType<InputField>())
                    // ceinture bretelles
                    if (inf.name == "CotationField" || inf.tag == "Cotation")
                        currentCotation.cotationField = inf;

            if (currentCotation.myElement is Wall)
            {
                // OPENING DISTANCE
                if (currentWallOpenings != null && currentWallOpenings.Count > 0 &&
                    currentWallOpenings[0].Wall == currentCotation.myElement)
                {
                    var heading = currentCotation.cotationTM.transform.position - currentWallOpenings[0].Position;
                    var dot = Vector3.Dot(heading, currentWallOpenings[0].Wall.Direction);

                    float sens = dot < 0 ? 1 : -1;

                    Vector2 newPos = currentWallOpenings[0].Position -
                                     diff * currentWallOpenings[0].Wall.Direction * sens / 100f;
                    WallsCreator.Instance.UpdateWallOpeningPosition(currentWallOpenings[0], newPos);
                }
                // WALL
                else
                {
                    var w = currentCotation.myElement as Wall;

                    var updateMin = currentCotation.Length == Mathf.Min(w.cotOne.Length, w.cotTwo.Length);
                    WallsCreator.Instance.UpdateWallLength(w, res / 100f, updateMin);
                    StartCoroutine(SWLRoutine(w, res / 100f, updateMin));
                }
            }
            // MOVABLE ELEMENTS POSITIONS
            else if (currentFurnitureData.Count == 1 || currentStairs.Count == 1)
            {
                MovableElement me;
                if (currentFurnitureData.Count == 1) me = currentFurnitureData[0];
                else me = currentStairs[0];
                var dirForward = currentCotation.is3D ? Vector3.forward : Vector3.back;
                var dirBack = currentCotation.is3D ? Vector3.back : Vector3.forward;
                var dirUp = currentCotation.is3D ? Vector3.up : Vector3.forward;
                var dirDown = currentCotation.is3D ? Vector3.down : Vector3.back;
                var dirLeft = currentCotation.is3D ? me.associated3DObject.transform.right * -1f : Vector3.left;
                var dirRight = currentCotation.is3D ? me.associated3DObject.transform.right : Vector3.right;

                if (currentCotation.isUp)
                {
                    if (currentFurnitureData[0].IsOnWall)
                        me.Position = me.Position
                                      + dirUp * diff / 100f;
                    else
                        me.Position = me.Position + dirBack * diff / 100f;
                }
                else if (currentCotation.isDown)
                {
                    if (currentFurnitureData[0].IsOnWall)
                        me.Position = me.Position
                                      + dirDown * diff / 100f;
                    else
                        me.Position = me.Position + dirForward * diff / 100f;
                }
                else if (currentCotation.isLeft)
                {
                    if (currentFurnitureData[0].IsOnWall)
                        me.Position = me.Position
                                      + dirLeft * diff / 100f;
                    else
                        me.Position = me.Position + Vector3.left * diff / 100f;
                }
                else if (currentCotation.isRight)
                {
                    if (currentFurnitureData[0].IsOnWall)
                        me.Position = me.Position
                                      + dirRight * diff / 100f;
                    else
                        me.Position = me.Position + Vector3.right * diff / 100f;
                }

                me.associated3DObject.transform.position = me.Position;
                me.associated2DObject.transform.position =
                    VectorFunctions.GetExactPositionFrom3DObject(me.associated2DObject, me.associated3DObject,
                        me.Position);
                StartCoroutine(Adjust2DElementFrom3DCoroutine(me));
                //VectorFunctions.Switch3D2D(me.Position);
            }

            currentCotation.cotationField.transform.position = Vector3.one * 10000;
            OperationsBufferScript.Instance.AddAutoSave("Mise à jour d'une cotation");
        }

        /// <summary>
        ///     Updates 2d position from 3d position
        /// </summary>
        /// <param name="me"></param>
        /// <returns></returns>
        private IEnumerator Adjust2DElementFrom3DCoroutine(MovableElement me)
        {
            yield return new WaitForSeconds(0.1f);
            me.associated2DObject.transform.position =
                VectorFunctions.GetExactPositionFrom3DObject(me.associated2DObject, me.associated3DObject, me.Position);
        }

        /// <summary>
        ///     not used
        /// </summary>
        public void SplitWall()
        {
            if (currentWallsData.Count == 1)
            {
                var walls = WallFunctions.CutWall(currentWallsData[0], currentWallsData[0].Center);
                walls[0].RebuildSceneData();
                walls[1].RebuildSceneData();

                walls[0].associated2DObject.transform.parent = currentWallsData[0].associated2DObject.transform.parent;
                walls[0].associated3DObject.transform.parent = currentWallsData[0].associated3DObject.transform.parent;

                walls[1].associated2DObject.transform.parent = currentWallsData[0].associated2DObject.transform.parent;
                walls[1].associated3DObject.transform.parent = currentWallsData[0].associated3DObject.transform.parent;
                WallsCreator.Instance.ReplaceWall(currentWallsData[0], walls);
                ResetSelection();
            }
        }

        /// <summary>
        ///     seeks gameobject in selection
        /// </summary>
        /// <param name="go">gameobject</param>
        /// <returns>true if found in selection</returns>
        private bool IsGoInSelectedElements(GameObject go)
        {
            foreach (var e in currentSelectedElements)
                if (e.associated2DObject == go || e.associated3DObject == go)
                {
                    return true;
                }
                else
                {
                    for (var i = 0; i < e.associated2DObject.transform.childCount; i++)
                        if (e.associated2DObject.transform.GetChild(i).gameObject == go)
                            return true;
                    if (e.associated3DObject)
                        for (var i = 0; i < e.associated3DObject.transform.childCount; i++)
                            if (e.associated3DObject.transform.GetChild(i).gameObject == go)
                                return true;
                }

            return false;
        }

        /// <summary>
        ///     copy selected elements
        /// </summary>
        /// <param name="elem"></param>
        /// <returns>Description of elements to put in windows clipboard</returns>
        private string CopyElementToClipboard(Element elem)
        {
            m_copiedElements.Add(elem);
            return elem.GetDescription();
        }

        /// <summary>
        ///     We can copy everything except walls
        ///     The elements are copied both in software and also in windows clipboard (text)
        /// </summary>
        public void CopyElementsToClipboard()
        {
            //TODO TO DO IF GROUP COPY GROUP
            if (InputFunctions.CTRLC() && currentSelectedElements.Count > 0)
            {
                m_copiedElements.Clear();
                var clipboard = currentSelectedElements.Count() + " éléments : \n";
                foreach (var elem in currentSelectedElements)
                {
                    var potentialGroup = GroupsManager.Instance.GetGroupFromElement(elem);
                    if (potentialGroup != null)
                    {
                        if (!m_copiedElements.Contains(potentialGroup))
                            clipboard += CopyElementToClipboard(potentialGroup);
                    }
                    else
                    {
                        clipboard += CopyElementToClipboard(elem) + "\n";
                    }
                }

                GUIUtility.systemCopyBuffer = clipboard;
            }
        }

        /// <summary>
        ///     Instantiate a new element from copied ones
        /// </summary>
        public void PasteElement()
        {
            if (m_copiedElements.Count > 0 && InputFunctions.CTRLV())
            {
                foreach (var elem in m_copiedElements)
                    if (elem is ElementGroup)
                        GroupsManager.Instance.CopyPaste(elem);
                    else CopyPasteElementWithManager(elem);
                OperationsBufferScript.Instance.AddAutoSave("Collage d'éléments");
            }
        }

        #endregion
    }
}