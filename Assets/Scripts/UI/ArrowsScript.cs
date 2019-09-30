using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoShop.UI
{
    public class ArrowsScript : MonoBehaviour
    {
        public GameObject leftArrow, rightArrow;

        protected float m_arrowOffset;

        protected Vector3 m_decal = Vector3.forward * -0.02f;

        protected GameObject currentArrow;

        protected void Start()
        {
            m_arrowOffset = 0f;
        }
    }
}