using ErgoShop.POCO;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ErgoShop.Utils
{
    /// <summary>
    /// MonoBehaviour to make prefabs for furniture
    /// </summary>
    public class FurnitureScript : MonoBehaviour
    {
        /// <summary>
        /// Sprite for 2D View
        /// </summary>
        public GameObject furniture2DTop;

        /// <summary>
        /// Mesh
        /// </summary>
        public GameObject furniture3D;

        /// <summary>
        /// Put on a wall (like a painting...)
        /// </summary>
        public bool isOnWall;

        /// <summary>
        /// Can be put on another furniture (like a microwave on a table)
        /// </summary>
        public bool canBePutOnFurniture;

        /// <summary>
        /// Displayed name
        /// </summary>
        public string furnitureName;

        /// <summary>
        /// Type (assise, eau, cuisine, ...)
        /// </summary>
        public string furnitureType;

        /// <summary>
        /// Resource name for runtime loading
        /// </summary>
        public string resourceName;

        /// <summary>
        /// Runtime scaling for big meshs
        /// </summary>
        public float scaleRatio = 1;
    }
}