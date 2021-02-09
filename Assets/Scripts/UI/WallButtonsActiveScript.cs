using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    /// <summary>
    ///     Change color when creating wall/room
    /// </summary>
    public class WallButtonsActiveScript : MonoBehaviour
    {
        public Button wall, polyWall, room, door, window;

        private ColorBlock cb, activeCB;

        // Start is called before the first frame update
        private void Awake()
        {
            cb = new ColorBlock
            {
                normalColor = Color.white,
                colorMultiplier = 1,
                disabledColor = Color.gray,
                fadeDuration = 0.1f,
                highlightedColor = new Color(0, 160 / 255f, 1f),
                pressedColor = new Color(14 / 255f, 166 / 255f, 0 / 255f)
            };

            activeCB = new ColorBlock
            {
                normalColor = cb.pressedColor,
                highlightedColor = cb.pressedColor,
                pressedColor = cb.pressedColor,
                disabledColor = cb.disabledColor,
                fadeDuration = cb.fadeDuration,
                colorMultiplier = cb.colorMultiplier
            };
        }

        private void Update()
        {
        }

        private void OnEnable()
        {
            DesactiveDoor();
            DesactivePolyWall();
            DesactiveRoom();
            DesactiveWall();
            DesactiveWindow();
        }

        public void ActiveWall()
        {
            wall.colors = activeCB;
        }

        public void DesactiveWall()
        {
            wall.colors = cb;
        }

        public void ActivePolyWall()
        {
            polyWall.colors = activeCB;
        }

        public void DesactivePolyWall()
        {
            polyWall.colors = cb;
        }

        public void ActiveRoom()
        {
            room.colors = activeCB;
        }

        public void DesactiveRoom()
        {
            room.colors = cb;
        }

        public void ActiveDoor()
        {
            door.colors = activeCB;
        }

        public void DesactiveDoor()
        {
            door.colors = cb;
        }

        public void ActiveWindow()
        {
            window.colors = activeCB;
        }

        public void DesactiveWindow()
        {
            window.colors = cb;
        }
    }
}