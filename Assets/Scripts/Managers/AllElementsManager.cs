using ErgoShop.POCO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ErgoShop.Managers
{
    /// <summary>
    /// A script computing all current instantiated Elements
    /// It is used mostly for saving and loading data from floors and projects
    /// </summary>
    public class AllElementsManager : MonoBehaviour
    {
        /// <summary>
        /// Instance
        /// </summary>
        public static AllElementsManager Instance { get; private set; }

        private List<Element> m_allElements;

        /// <summary>
        /// Getting the elements implies finding all elements (without updating ids)
        /// </summary>
        public List<Element> AllElements
        {
            get
            {
                UpdateAllElements(false);
                return m_allElements;
            }
        }

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
            m_allElements = new List<Element>();
        }

        /// <summary>
        /// Update ids by setting id equal to position in the list (arbitrary)
        /// </summary>
        public void UpdateElementsIds()
        {
            int cpt = 0;
            foreach (var elem in m_allElements)
            {
                elem.Id = cpt;
                cpt++;
            }

            foreach (var group in GroupsManager.Instance.GetGroups())
            {
                group.ElementsIds.Clear();
                group.ElementsIds.AddRange(group.Elements.Select(elems => elems.Id));
            }
        }

        /// <summary>
        /// GET ALL ELEMENTS AND UPDATE THEIR IDS
        /// </summary>
        /// <param name="updateIds">if true, update ids</param>
        public void UpdateAllElements(bool updateIds)
        {
            m_allElements.Clear();
            m_allElements.AddRange(WallsCreator.Instance.GetWalls());
            m_allElements.AddRange(WallsCreator.Instance.GetWallOpenings());
            m_allElements.AddRange(FurnitureCreator.Instance.GetFurnitures());
            m_allElements.AddRange(StairsCreator.Instance.GetStairs());
            m_allElements.AddRange(HelpersCreator.Instance.GetHelpers());
            m_allElements.AddRange(GroupsManager.Instance.GetGroups());
            //m_allElements.AddRange(CharactersCreator.Instance.GetCharacters());

            UpdateElementsIds();
        }

        /// <summary>
        /// Return all element of type Furniture
        /// </summary>
        /// <returns>a list containing all furnitures</returns>
        public List<Furniture> GetFurnitures()
        {
            List<Furniture> furnitures = new List<Furniture>();
            foreach (var elem in m_allElements)
            {
                if (elem is Furniture)
                {
                    furnitures.Add(elem as Furniture);
                }
            }
            return furnitures;
        }

        /// <summary>
        /// Finds the element with corresponding id
        /// </summary>
        /// <param name="id">the id</param>
        /// <returns>the element</returns>
        public Element GetElementById(int id)
        {
            UpdateAllElements(false);
            var elem = m_allElements.FirstOrDefault(e => e.Id == id);
            return elem;
        }

        /// <summary>
        /// Get all elements descriptions
        /// </summary>
        /// <returns>a big string containing all descriptions separated by =======</returns>
        public string GetDescription()
        {
            string res = "=== ELEMENTS : " + AllElements.Count + " ===\n";
            foreach (var elem in AllElements)
            {
                res += elem.GetDescription() + "\n";
            }
            return res + "=====================";
        }
    }
}