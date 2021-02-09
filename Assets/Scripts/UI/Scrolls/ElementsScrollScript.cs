using UnityEngine;

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

        // Start is called before the first frame update
        private void Start()
        {
        }

        /// <summary>
        ///     Update the content size so the vertical layout can display its children correctly
        /// </summary>
        private void Update()
        {
            var children = 0;
            for (var i = 0; i < Content.childCount; i++)
                if (Content.GetChild(i).gameObject.activeInHierarchy)
                    children++;
            var height = Mathf.Max(children * 30f, ElementsScroll.sizeDelta.y);
            Content.sizeDelta = new Vector2(0, height);
        }
    }
}