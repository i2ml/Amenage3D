using System.Collections.Generic;
using ErgoShop.Managers;
using Newtonsoft.Json;
using UnityEngine;

namespace ErgoShop.POCO
{
    /// <summary>
    ///     An element group is an element containing other elements
    ///     Its used to select several elements easily
    /// </summary>
    public class ElementGroup : Element
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        public ElementGroup()
        {
            Elements = new List<Element>();
            ElementsIds = new List<int>();
        }

        /// <summary>
        ///     Elements. Not saved in json file because we only store the ids to rebuild references later
        /// </summary>
        [JsonIgnore]
        public List<Element> Elements { get; set; }

        /// <summary>
        ///     Ids for saving
        /// </summary>
        public List<int> ElementsIds { get; set; }

        /// <summary>
        ///     Copy all elements and make a new group containing those new elements
        /// </summary>
        /// <returns>the new group</returns>
        public override Element GetCopy()
        {
            var copy = new ElementGroup
            {
                Elements = new List<Element>(),
                ElementsIds = new List<int>()
            };
            foreach (var e in Elements)
            {
                var copyElement = SelectedObjectManager.Instance.CopyPasteElementWithManager(e);
                copy.Elements.Add(copyElement);
            }

            return copy;
        }

        /// <summary>
        ///     Rebuild 2D and 3D objects from data
        /// </summary>
        public override void RebuildSceneData()
        {
            foreach (var e in Elements) e.RebuildSceneData();
        }

        /// <summary>
        ///     Seek a group from a gameobject associated with any element in this group
        /// </summary>
        /// <param name="go"></param>
        /// <returns>the group</returns>
        public ElementGroup GetGroupFromGameObject(GameObject go)
        {
            foreach (var a in Elements)
            {
                if (a is ElementGroup)
                {
                    var res = (a as ElementGroup).GetGroupFromGameObject(go);
                    if (res != null) return res;
                }

                if (a.associated2DObject == go || a.associated3DObject == go) return this;
            }

            ;

            return null;
        }

        /// <summary>
        ///     Seek a group from an element
        /// </summary>
        /// <param name="go"></param>
        /// <returns>the group containing this element</returns>
        public ElementGroup GetGroupFromElement(Element e)
        {
            if (Elements.Contains(e))
                return this;
            foreach (var a in Elements)
                if (a is ElementGroup)
                {
                    var res = (a as ElementGroup).GetGroupFromElement(e);
                    if (res != null) return res;
                }

            return null;
        }

        /// <summary>
        ///     Get a textual description
        /// </summary>
        /// <returns>A string containing the data</returns>
        public override string GetDescription()
        {
            var clipboard = "=== Groupe contenant " + Elements.Count + " éléments ===\n";
            foreach (var e in Elements) clipboard += e.GetDescription() + "\n";
            return clipboard + "=== Fin du groupe ===\n";
        }
    }
}