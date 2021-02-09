using UnityEngine;

namespace ErgoShop.UI
{
    public class ArrowsScript : MonoBehaviour
    {
        public GameObject leftArrow, rightArrow;

        protected GameObject currentArrow;

        protected float m_arrowOffset;

        protected Vector3 m_decal = Vector3.forward * -0.02f;

        protected void Start()
        {
            m_arrowOffset = 0f;
        }
    }
}