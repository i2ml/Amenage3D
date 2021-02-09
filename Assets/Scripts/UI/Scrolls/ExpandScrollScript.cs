using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    /// <summary>
    ///     Show / hide list without desactivate gameobject
    /// </summary>
    public class ExpandScrollScript : MonoBehaviour
    {
        public GameObject Scroll;

        private bool m_expanded;

        private Vector2 m_scrollPosition;

        private void Start()
        {
            m_scrollPosition = Scroll.transform.localPosition;
            MinimizeScroll();
        }

        public void ToggleScroll()
        {
            if (m_expanded)
                MinimizeScroll();
            else
                ExpandScroll();
        }

        public void ExpandScroll()
        {
            if (transform.GetChild(0).GetComponent<Text>()) transform.GetChild(0).GetComponent<Text>().text = "-";
            Scroll.transform.localPosition = m_scrollPosition;
            m_expanded = true;
        }

        public void MinimizeScroll()
        {
            if (transform.GetChild(0).GetComponent<Text>()) transform.GetChild(0).GetComponent<Text>().text = "+";
            Scroll.transform.position = Vector2.up * 10000;
            m_expanded = false;
        }
    }
}