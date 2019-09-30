using ErgoShop.Utils;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoShop.POCO
{
    /// <summary>
    /// A furniture is a movableelement
    /// </summary>
    public class Furniture : MovableElement
    {
        /// <summary>
        /// Type to filter in list (assise, eau, cuisine...)
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Name diplayed
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// PrefabName for runtime loading
        /// </summary>
        public string PrefabName { get; set; }

        /// <summary>
        /// Scale for init size (usually = 1)
        /// </summary>
        public float ScaleModifier { get; set; }

        /// <summary>
        /// True if imported furniture
        /// </summary>
        public bool IsCustom { get; set; }
        /// <summary>
        /// If imported furniture, the obj path. If not, string.empty
        /// </summary>
        public string CustomPath { get; set; }

        // Unity scene data
        /// <summary>
        /// Rebuild 2D and 3D objects from data
        /// </summary>
        public override void RebuildSceneData()
        {
            if (IsCustom)
            {

            }
            else
            {
                string path = "Furniture/3D/" + Type + "/" + PrefabName;
                associated3DObject = (GameObject)Resources.Load(path, typeof(GameObject));
                string path2D = "Furniture/2D/Top/" + Type + "/" + PrefabName;

                associated2DObject = (GameObject)Resources.Load(path2D, typeof(GameObject));
            }
        }


        /// <summary>
        /// Set element size, updates 2d and 3d objects
        /// </summary>
        /// <param name="newSize">Wanted size</param>
        public override void SetSize(Vector3 newSize)
        {
            base.SetSize(newSize);

            //associated2DObject.transform.localScale = associated2DObject.transform.localScale / ScaleModifier;
        }

        /// <summary>
        /// Adjust sprite size according to its real size, the scaling, and the furniture size.
        /// The goal is to have the same x y dimensions as x z in 3d
        /// </summary>
        public void AdjustSpriteSize()
        {
            var spr = associated2DObject.GetComponent<SpriteRenderer>().sprite;

            float xscale = spr.rect.width / spr.pixelsPerUnit;
            float zscale = spr.rect.height / spr.pixelsPerUnit;

            xscale = MeshSize.x / xscale;
            zscale = MeshSize.z / zscale;

            associated2DObject.transform.localScale =
            new Vector3(
                associated2DObject.transform.localScale.x * xscale,
                associated2DObject.transform.localScale.y * zscale,
                1f);
        }

        /// <summary>
        /// Instantiate an identical object
        /// </summary>
        /// <returns>The same Element</returns>
        public override Element GetCopy()
        {
            return new Furniture
            {
                Id = this.Id,
                ScaleModifier = this.ScaleModifier,
                CanBePutOnFurniture = this.CanBePutOnFurniture,
                IsOnWall = this.IsOnWall,
                EulerAngles = this.associated3DObject.transform.eulerAngles,
                Rotation = this.Rotation,
                Position = this.associated3DObject.transform.position,
                IsLocked = this.IsLocked,
                Name = this.Name,
                PrefabName = this.PrefabName,
                Type = this.Type,
                Size = this.Size,
                IsCustom = this.IsCustom,
                CustomPath = this.CustomPath
            };
        }

        /// <summary>
        /// Get a textual description
        /// </summary>
        /// <returns>A string containing the data</returns>
        public override string GetDescription()
        {
            return  "Meuble " + Name + "\n"
            + "Dimensions : "
            + ParsingFunctions.ToStringCentimeters(Size.x)
            + "/" + ParsingFunctions.ToStringCentimeters(Size.y)
            + "/" + ParsingFunctions.ToStringCentimeters(Size.z)
            + "\n";
        }
    }
}