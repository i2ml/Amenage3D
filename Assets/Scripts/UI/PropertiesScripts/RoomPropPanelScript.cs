using ErgoShop.Managers;
using ErgoShop.POCO;
using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    /// <summary>
    ///     UI Properties for selected element
    /// </summary>
    public class RoomPropPanelScript : PropertiesBehaviour
    {
        // Bindings for room properties
        public GameObject roomPropertiesForm;

        //public GameObject wallColorsForRoomGameObject;
        //public GameObject roomGroundColorGameObject;
        public InputField roomNameInput;
        public Toggle roomIsRectToggle;

        public CUIColorPicker groundColorPicker;
        public Image groundImageToggle;
        public CUIColorPicker wallsColorPicker;
        public Image wallsImageToggle;

        private bool m_needsUpdate;

        public static RoomPropPanelScript Instance { get; private set; }

        // Start is called before the first frame update
        private void Start()
        {
            Instance = this;
        }

        // Update is called once per frame
        private void Update()
        {
            if (CheckPropertiesBindings(SelectedObjectManager.Instance.currentRoomData))
                UIManager.Instance.instructionsText.text =
                    "F pour centrer la caméra sur la pièce. Suppr pour supprimer.";
            base.Update();
        }

        public void UpdateRoomProperties()
        {
            m_needsUpdate = true;
        }

        //private void ResetColors()
        //{
        //    for (int i = 0; i < roomGroundColorGameObject.transform.childCount; i++)
        //    {
        //        roomGroundColorGameObject.transform.GetChild(i).GetComponent<UnityEngine.UI.Outline>().enabled = false;
        //    }
        //    for (int i = 0; i < wallColorsForRoomGameObject.transform.childCount; i++)
        //    {
        //        wallColorsForRoomGameObject.transform.GetChild(i).GetComponent<UnityEngine.UI.Outline>().enabled = false;
        //    }
        //}

        // return true if properties form is shown
        private bool CheckPropertiesBindings(Room roomData)
        {
            if (roomData != null && !roomPropertiesForm.activeInHierarchy)
            {
                roomPropertiesForm.SetActive(true);
            }
            else if (roomData == null || roomData.Walls.Count == 0)
            {
                roomPropertiesForm.SetActive(false);
                return false;
            }

            // ROOM PROPERTIES
            if (roomPropertiesForm.activeInHierarchy)
            {
                //ResetColors();
                if (m_needsUpdate)
                {
                    // Room name
                    roomNameInput.text = roomData.Name;
                    roomIsRectToggle.isOn = roomData.LockAngles;
                    var model = roomData.Walls[0].Color;
                    if (roomData.Walls.TrueForAll(w => w.Color == model))
                    {
                        wallsImageToggle.gameObject.SetActive(true);
                        wallsColorPicker.Color = new Color(roomData.Walls[0].Color.r, roomData.Walls[0].Color.g,
                            roomData.Walls[0].Color.b);
                        wallsImageToggle.color = wallsColorPicker.Color;
                    }
                    else
                    {
                        wallsColorPicker.gameObject.SetActive(false);
                        wallsImageToggle.gameObject.SetActive(false);
                    }

                    groundImageToggle.color = new Color(roomData.Ground.Color.r, roomData.Ground.Color.g,
                        roomData.Ground.Color.b);
                    groundColorPicker.Color = groundImageToggle.color;
                }

                wallsImageToggle.color = new Color(wallsColorPicker.Color.r, wallsColorPicker.Color.g,
                    wallsColorPicker.Color.b);
                groundImageToggle.color = new Color(groundColorPicker.Color.r, groundColorPicker.Color.g,
                    groundColorPicker.Color.b);

                if (roomData.Walls[0].Color != wallsImageToggle.color)
                {
                    foreach (var w in roomData.Walls) w.Color = wallsImageToggle.color;
                    WallsCreator.Instance.AdjustAllWalls();
                }

                if (roomData.Ground.Color != groundImageToggle.color)
                {
                    roomData.Ground.Color = groundImageToggle.color;
                    WallsCreator.Instance.AdjustGroundsAndCeils();
                }

                m_needsUpdate = false;

                //// Ground Color
                //Color col = roomData.Ground.Color;
                //for (int i = 0; i < roomGroundColorGameObject.transform.childCount; i++)
                //{
                //    if (roomGroundColorGameObject.transform.GetChild(i).GetComponent<Button>().colors.normalColor == col)
                //    {
                //        roomGroundColorGameObject.transform.GetChild(i).GetComponent<UnityEngine.UI.Outline>().enabled = true;
                //    }
                //}
                //col = roomData.Walls[0].Color;
                //// Walls Color
                //for (int i = 0; i < wallColorsForRoomGameObject.transform.childCount; i++)
                //{
                //    if (wallColorsForRoomGameObject.transform.GetChild(i).GetComponent<Button>().colors.normalColor == col)
                //    {
                //        wallColorsForRoomGameObject.transform.GetChild(i).GetComponent<UnityEngine.UI.Outline>().enabled = true;
                //    }
                //}
            }

            return roomPropertiesForm.activeInHierarchy;
        }

        public void ShowWallToggle()
        {
            wallsColorPicker.Color = wallsImageToggle.color;
        }

        public void ShowGroundToggle()
        {
            groundColorPicker.Color = groundImageToggle.color;
        }

        public override void Hide()
        {
            roomPropertiesForm.SetActive(false);
        }
    }
}