using ErgoShop.Managers;
using ErgoShop.Utils;
using UnityEngine;

namespace ErgoShop.UI
{
    /// <summary>
    ///     Script to handle the behaviour of a text zone
    ///     The 3D version always faces the camera
    ///     Also updates collider
    /// </summary>
    public class TextZoneScript : MonoBehaviour
    {
        public SpriteRenderer bg;
        public TextMesh tm;

        public float textSize;

        // Start is called before the first frame update
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            if (textSize == 0) textSize = 1f;
            var s = Mathf.CeilToInt(bg.size.magnitude) * 2;
            tm.fontSize = 144;
            tm.characterSize = 0.001f * s * textSize;


            if (gameObject.layer == (int) ErgoLayers.ThreeD)
            {
                if (GetComponent<BoxCollider2D>() || GetComponent<BoxCollider>() == null)
                {
                    Destroy(GetComponent<BoxCollider2D>());
                    gameObject.AddComponent<BoxCollider>();
                }

                if (GetComponent<BoxCollider>()) GetComponent<BoxCollider>().size = bg.size;
                transform.localEulerAngles = new Vector3(
                    GlobalManager.Instance.cam3D.transform.localEulerAngles.x,
                    GlobalManager.Instance.cam3D.transform.localEulerAngles.y,
                    0
                );
                transform.position = new Vector3(
                    transform.position.x,
                    bg.size.y / 2f,
                    transform.position.z);
            }
            else
            {
                GetComponent<BoxCollider2D>().size = bg.size;
            }
        }

        public Vector2 SetSize(Vector2 a, Vector2 b)
        {
            bg.size = new Vector2(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
            return bg.size;
        }

        public void SetSize(Vector2 a)
        {
            bg.size = a;
        }
    }
}