using cakeslice;
using UnityEngine;

namespace ErgoShop.Utils
{
    /// <summary>
    ///     Outline 2D and 3D objects
    /// </summary>
    public static class OutlineFunctions
    {
        /// <summary>
        ///     Outline a gameobject or hide an outline
        /// </summary>
        /// <param name="go"></param>
        /// <param name="enab">show or hide</param>
        /// <param name="color">0=green 1=blue 2=red</param>
        public static void SetOutlineEnabled(GameObject go, bool enab, int color = 0)
        {
            Outline outline;

            if (!go) return;
            if (go.GetComponent<Outline>() != null)
            {
                outline = go.GetComponent<Outline>();
                if (outline)
                {
                    outline.enabled = enab;
                    outline.color = color;
                }

                //outline.needsUpdate = enab;
            }
            else
            {
                if (go.GetComponent<Renderer>() != null)
                {
                    outline = go.AddComponent<Outline>();
                    if (outline)
                    {
                        outline.enabled = enab;
                        outline.color = color;
                    }
                }

                //outline.OutlineColor = Color.green;
                //outline.OutlineWidth = 5f;
                //outline.OutlineMode = Outline.Mode.OutlineAll;
                //outline.needsUpdate = enab;
            }

            for (var i = 0; i < go.transform.childCount; i++)
                SetOutlineEnabled(go.transform.GetChild(i).gameObject, enab, color);
        }
    }
}