using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoShop.POCO
{
    /// <summary>
    /// Interface for Element
    /// </summary>
    public interface IElement
    {
        /// <summary>
        /// Rebuild 2D and 3D objects from data
        /// </summary>
        void RebuildSceneData();
        /// <summary>
        /// Instantiate an identical object
        /// </summary>
        /// <returns>The same Element</returns>
        Element GetCopy();
        /// <summary>
        /// Get a textual description
        /// </summary>
        /// <returns>A string containing the data</returns>
        string GetDescription();
    }
}
