using System.Collections.Generic;
using System.Linq;
using ErgoShop.POCO;
using UnityEngine;

namespace ErgoShop.Managers
{
    /// <summary>
    ///     Manager for groups, creation, suppression...
    /// </summary>
    public class GroupsManager : CreatorBehaviour
    {
        private List<ElementGroup> m_groups;

        public static GroupsManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        private void Start()
        {
            m_groups = new List<ElementGroup>();
        }

        // Update is called once per frame
        private void Update()
        {
            if (Mathf.RoundToInt(Time.time * 100) % 100 == 0) CheckGroups();
        }

        /// <summary>
        ///     Check for duplicates, and destroy them
        /// </summary>
        private void CheckGroups()
        {
            var kill = new List<ElementGroup>();
            foreach (var group in m_groups)
            foreach (var group2 in m_groups)
                if (@group != group2)
                    if (@group.Elements.SequenceEqual(group2.Elements))
                        if (!kill.Contains(@group) && !kill.Contains(group2))
                            kill.Add(@group);
            foreach (var k in kill) RemoveGroup(k);
        }

        /// <summary>
        ///     Create a group from list of elements
        /// </summary>
        /// <param name="elementsToAdd">Elements to group</param>
        /// <returns>The group</returns>
        public ElementGroup CreateGroup(List<Element> elementsToAdd)
        {
            var group = new ElementGroup
            {
                Elements = new List<Element>(),
                ElementsIds = new List<int>()
            };

            // ATTEMPT TO MAKE GROUP-CEPTION :

            //List<Element> finalElementsToAdd = new List<Element>();

            //// 1) Add elements groups if they are contained in any
            //foreach(var elem in elementsToAdd)
            //{
            //    ElementGroup potGroup = GetGroupFromElement(elem);
            //    if (potGroup != null)
            //    {
            //        if (!finalElementsToAdd.Contains(potGroup))
            //        {
            //            finalElementsToAdd.Add(potGroup);
            //        }
            //    }
            //    else
            //    {
            //        finalElementsToAdd.Add(elem);
            //    }
            //}


            group.Elements.AddRange(elementsToAdd);


            m_groups.Add(group);
            return group;
        }

        // ATTEMPT TO MAKE GROUP-CEPTION :
        //public ElementGroup GetGroupFromSubGroup(ElementGroup subgroup)
        //{
        //    foreach(var group in m_groups)
        //    {
        //        ElementGroup result = group.GetGroupFromElement(subgroup);
        //        if (result != null) return result;
        //    }
        //    return null;
        //}

        /// <summary>
        ///     Seek all associated objects in groups to find the ElementGroup object concerned
        /// </summary>
        /// <param name="go">Associated gameobject, can be 2D or 3D</param>
        /// <returns>The ElementGroup data or null if not found</returns>
        public ElementGroup GetGroupFromGameObject(GameObject go)
        {
            ElementGroup result = null;
            foreach (var group in m_groups)
            {
                var gr = group.GetGroupFromGameObject(go);
                if (gr != null)
                    if (result == null || result.Elements.Count > gr.Elements.Count)
                        result = @group.GetGroupFromGameObject(go);
            }

            return result;
        }

        /// <summary>
        ///     Seek all elements in groups to find the ElementGroup object concerned
        /// </summary>
        /// <param name="e">Element you want to find its group</param>
        /// <returns>The ElementGroup data or null if not found</returns>
        public ElementGroup GetGroupFromElement(Element e)
        {
            foreach (var group in m_groups)
            {
                var result = group.GetGroupFromElement(e);
                if (result != null) return result;
            }

            return null;
        }

        /// <summary>
        ///     Delete association without destroying elements
        /// </summary>
        /// <param name="groupToRemove"></param>
        public void RemoveGroup(ElementGroup groupToRemove)
        {
            m_groups.Remove(groupToRemove);
            groupToRemove.Elements.Clear();
            groupToRemove = null;
        }

        /// <summary>
        ///     Is the element in a group ?
        /// </summary>
        /// <param name="e">the element</param>
        /// <returns>true if is grouped</returns>
        public bool IsElementGrouped(Element e)
        {
            foreach (var group in m_groups)
                if (@group.Elements.Contains(e))
                    return true;
            return false;
        }

        /// <summary>
        ///     Get all groups in the current floor
        /// </summary>
        /// <returns></returns>
        public List<ElementGroup> GetGroups()
        {
            return m_groups;
        }

        /// <summary>
        ///     Load all groups in a given floor. Does not instantiate the elements but try to find them and update the references
        /// </summary>
        /// <param name="floor"></param>
        public void LoadGroupsFromFloor(Floor floor)
        {
            m_groups.Clear();
            foreach (var eg in floor.Groups)
            {
                var newEg = new ElementGroup();
                newEg.Elements = new List<Element>();
                foreach (var elemId in eg.ElementsIds)
                {
                    var goodRef = AllElementsManager.Instance.GetElementById(elemId);
                    newEg.Elements.Add(goodRef);
                    newEg.ElementsIds.Add(elemId);
                }

                m_groups.Add(newEg);
            }
        }

        /// <summary>
        ///     Disband all groups without deleting elements
        /// </summary>
        public override void DestroyEverything()
        {
            m_groups.Clear();
        }

        /// <summary>
        ///     Paste a copied Group by getting a copy and instantiate it, and rebuilding gameobjects
        /// </summary>
        /// <param name="m_copiedElement">Copied ElementGroup</param>
        /// <returns>The new ElementGroup, identical to the copied one</returns>
        public override Element CopyPaste(Element elem)
        {
            if (elem is ElementGroup)
            {
                var group = elem as ElementGroup;
                var newOne = group.GetCopy() as ElementGroup;
                //newOne.RebuildSceneData();
                m_groups.Add(newOne);
                return newOne;
            }

            return null;
        }
    }
}