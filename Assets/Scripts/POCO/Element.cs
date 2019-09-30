using ErgoShop.UI;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoShop.POCO
{
    /// <summary>
    /// Element is base class for almost any object in scene / project.
    /// It contain all common data for walls, furnitures, and so on
    /// </summary>
    public abstract class Element : IElement
    {
        /// <summary>
        /// USED IN SAVE/LOAD
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 2D Object in 2D view. Mostly a sprite 
        /// </summary>
        [JsonIgnore]
        public GameObject associated2DObject;
        /// <summary>
        /// 3D Object in 3D View. Either a generated mesh, or a, imported mesh with collider and rigidbody
        /// </summary>
        [JsonIgnore]
        public GameObject associated3DObject;
        /// <summary>
        /// Text mesh 2d to put in front of the associated2DObject with the element name
        /// </summary>
        [JsonIgnore]
        public TextMesh text2D;
        /// <summary>
        /// measure shown in 2D
        /// </summary>
        [JsonIgnore]
        public CotationsScript widthCotation;
        /// <summary>
        /// measure shown in 2D
        /// </summary>
        [JsonIgnore]
        public CotationsScript heightCotation;

        /// <summary>
        /// Compute mesh size from mesh boundaries (if no 3D Object returns positiveInfinity)
        /// </summary>
        [JsonIgnore]
        public Vector3 MeshSize
        {
            get
            {
                // raw data for character
                if (this is CharacterElement)
                {
                    if ((this as CharacterElement).Type != Utils.CharacterType.OnWheelChair
                        && (this as CharacterElement).Type != Utils.CharacterType.WheelChairEmpty)
                    {
                        return new Vector3(1.76f, 1.77f, 0.32f);
                    }
                    // wheelchair
                    else
                    {
                        return associated3DObject.GetComponent<MeshFilter>().sharedMesh.bounds.size;
                    }
                }
                if (!associated3DObject) return Vector3.positiveInfinity;
                if (associated3DObject.GetComponent<MeshFilter>() != null)
                {
                    return associated3DObject.GetComponent<MeshFilter>().sharedMesh.bounds.size;
                }
                else if (associated3DObject.transform.childCount > 0)
                {
                    if (associated3DObject.transform.GetChild(0).GetComponent<MeshFilter>() != null)
                    {
                        return associated3DObject.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh.bounds.size;
                    }
                    else
                    {
                        return associated3DObject.transform.GetChild(0).GetChild(0).GetComponent<MeshFilter>().sharedMesh.bounds.size;
                    }
                }
                else return Vector3.positiveInfinity;
            }
        }
        /// <summary>
        /// Rebuild 2D and 3D objects from data
        /// </summary>
        public abstract void RebuildSceneData();
        /// <summary>
        /// Instantiate an identical object
        /// </summary>
        /// <returns>The same Element</returns>
        public abstract Element GetCopy();
        /// <summary>
        /// Get a textual description
        /// </summary>
        /// <returns>A string containing the data</returns>
        public abstract string GetDescription();
    }
}