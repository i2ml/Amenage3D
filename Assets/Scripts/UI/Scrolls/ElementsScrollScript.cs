using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoShop.UI
{
    /// <summary>
    /// Update the content size so the vertical layout can display its children correctly
    /// </summary>
    [ExecuteInEditMode]
    public class ElementsScrollScript : MonoBehaviour
    {
        public RectTransform ElementsScroll;
        public RectTransform Content;

        // Start is called before the first frame update
        void Start()
        {

        }

        /// <summary>
        /// Update the content size so the vertical layout can display its children correctly
        /// </summary>
        void Update()
        {
            int children = 0;
            for (int i = 0; i < Content.childCount; i++)
            {
                if (Content.GetChild(i).gameObject.activeInHierarchy)
                {
                    children++;
                }
            }
            float height = Mathf.Max(children * 30f, ElementsScroll.sizeDelta.y);
            Content.sizeDelta = new Vector2(0, height);
        }
    }
}