using ErgoShop.Utils;
using System.Collections;
using System.Collections.Generic;
using ErgoShop.Utils.Extensions;

using UnityEngine;
using ErgoShop.Managers;

namespace ErgoShop.POCO
{
    public class Stairs : MovableElement
    {
        // Generic Stair
        public int NbSteps { get; set; }
        public bool BuildSides { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }

        // Curved
        public float Curvature { get; set; }
        public float InnerRadius { get; set; }
        public bool ToTheLeft { get; set; }

        // Not Curved
        public float Depth { get; set; }

        // Unity scene data
        /// <summary>
        /// Rebuild 2D and 3D objects from data
        /// </summary>
        public override void RebuildSceneData()
        {
            EulerAngles = new Vector3(0, Rotation, 0);
            StairsCreator.Instance.DestroyPreviousStairs(associated3DObject);
            StairsCreator.Instance.DestroyPreviousStairs(associated2DObject);
            if (Curvature > 0)
            {
                associated3DObject = StairsCreator.Instance.GenerateCurvedStairs(
                    InnerRadius, Curvature, NbSteps, Width, Height, BuildSides, ToTheLeft
                );
            }
            else
            {
                associated3DObject = StairsCreator.Instance.GenerateStairs(NbSteps, Width, Height, Depth, BuildSides);
            }

            associated3DObject.transform.position = Position;
            associated3DObject.transform.localEulerAngles = EulerAngles;
            associated3DObject.tag = "Stairs";
            associated3DObject.layer = (int)ErgoLayers.ThreeD;
            associated3DObject.AddComponent<MeshCollider>().convex = true;

            var rb = associated3DObject.AddComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
            rb.useGravity = true;


            associated2DObject = StairsCreator.Instance.Generate2DStairs(this);
            

            associated2DObject.transform.position = VectorFunctions.Switch3D2D(Position + Depth/2f * associated2DObject.transform.forward);
            associated2DObject.transform.localEulerAngles = new Vector3(0, 0, -Rotation);
            associated2DObject.name = "Stairs2D";
            associated2DObject.tag = "Stairs";
            associated2DObject.SetLayerRecursively((int)ErgoLayers.Top);
        }

        /// <summary>
        /// Move the opening along the wall, according to mouse.
        /// </summary>
        /// <param name="offset2d">The starting position when the users clicks</param>
        public override void Move(Vector3 offset2d)
        {
            base.Move(offset2d);
        }

        /// <summary>
        /// Instantiate an identical object
        /// </summary>
        /// <returns>The same Element</returns>
        public override Element GetCopy()
        {
            return new Stairs
            {
                Id = this.Id,
                BuildSides = this.BuildSides,
                CanBePutOnFurniture = this.CanBePutOnFurniture,
                Curvature = this.Curvature,
                Depth = this.Depth,
                EulerAngles = this.EulerAngles,
                InnerRadius = this.InnerRadius,
                IsLocked = this.IsLocked,
                NbSteps = this.NbSteps,
                Position = this.Position,
                Rotation = this.Rotation,
                ToTheLeft = this.ToTheLeft,
                Height = this.Height,
                Width = this.Width
            };
        }

        /// <summary>
        /// Get a textual description
        /// </summary>
        /// <returns>A string containing the data</returns>
        public override string GetDescription()
        {
            return "Escaliers Modulables\n"            
            + "Dimensions : "
            + ParsingFunctions.ToStringCentimeters(Width)
            + "/" + ParsingFunctions.ToStringCentimeters(Height)
            + "/" + ParsingFunctions.ToStringCentimeters(Depth) + "\n"
            + "Nombre de marches : " + NbSteps;
        }
    }
}
