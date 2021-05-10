using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    /// <summary>
    ///     Update the content size so the vertical layout can display its children correctly
    /// </summary>
    [ExecuteInEditMode]
    public class ElementsScrollScript : MonoBehaviour
    {
        public RectTransform ElementsScroll;
        public RectTransform Content;

        /// <summary>
        ///     Update the content size so the vertical layout can display its children correctly
        /// </summary>
        private void Update()
        {
                ProcessChild();
        }

        private short ProcessChild()
        {
            short children = 0;
            for (short i = 0; i < Content.childCount; i++)
            {
                Transform Tchild = Content.GetChild(i);
                if (Tchild.gameObject.activeInHierarchy)
                {
                    children++;
                }
            }
            Content.sizeDelta = new Vector2(0, Mathf.Max(children * 30f, ElementsScroll.sizeDelta.y));
            return children;
        }
    }
}