using ErgoShop.Managers;
using ErgoShop.Utils;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoShop.POCO
{
    /// <summary>
    /// WallOpening : Can be door or window. Several types
    /// </summary>
    public class WallOpening : MovableElement
    {        
        /// <summary>
        /// Percentage position from Wall P1 (extremity)
        /// </summary>
        public float PercentagePosition { get; set; }

        /// <summary>
        /// Pull or Push ?
        /// </summary>
        public bool IsPull { get; set; }
        /// <summary>
        /// handle to open is left
        /// </summary>
        public bool IsLeft { get; set; }

        // Window part
        public bool IsWindow { get; set; }
        public float WindowHeight { get; set; }
        public bool IsDouble { get; set; }

        // Unity data
        [JsonIgnore]
        public Wall Wall { get; set; }
        [JsonIgnore]
        public float DistanceLeft { get; set; }
        [JsonIgnore]
        public float DistanceRight { get; set; }
        //[JsonIgnore]
        //public List<GameObject> WallParts { get; set; }

        public WallOpening()
        {
            //WallParts = new List<GameObject>();
        }

        /// <summary>
        /// Update opening global position from its position according to the first wall extremity (p1)
        /// </summary>
        public void SetPositionFromPercentagePosition()
        {
            Position = Wall.P1 + Wall.Direction * PercentagePosition * Wall.Length;
        }

        /// <summary>
        /// Update the position according to wall P1 (in percentage)
        /// </summary>
        public void SetPercentagePositionFromPosition()
        {
            float dist = Vector3.Distance(Position, Wall.P1);
            float wSize = Wall.Length;
            PercentagePosition = dist / wSize;
        }

        /// <summary>
        /// Update Global Position. The position is 2D !!
        /// </summary>
        /// <param name="v">Position 2D</param>
        public override void SetPosition(Vector3 v)
        {
            Position = v;
            SetPercentagePositionFromPosition();
        }

        /// <summary>
        /// Computes where the opposite of the handle is.
        /// </summary>
        /// <returns>Opposite side of the handle. Returns the two extremities if its a double window/door</returns>
        public Vector3[] GetCorners()
        {
            if (IsDouble)
            {
                return new Vector3[]
                {
                    Position + Wall.Direction * Size.x/2f,
                    Position - Wall.Direction * Size.x/2f
                };
            }
            return new Vector3[] { Position + Wall.Direction * (IsLeft ? 1 : -1f) * Size.x / 2f };
        }

        /// <summary>
        /// Move the opening along the wall, according to mouse.
        /// </summary>
        /// <param name="offset">The starting position when the users clicks</param>
        public override void Move(Vector3 offset)
        {
            WallsCreator.Instance.UpdateWallOpeningPosition(this);
        }

        /// <summary>
        /// Rebuild 2D and 3D objects from data
        /// </summary>
        public override void RebuildSceneData()
        {
            WallsCreator.Instance.InstantiateWallOpening(this);
        }

        /// <summary>
        /// Instantiate an identical object
        /// </summary>
        /// <returns>The same Element</returns>
        public override Element GetCopy()
        {
            return new WallOpening
            {
                Size = this.Size,
                Position = this.Position,
                PercentagePosition = this.PercentagePosition,
                IsLeft = this.IsLeft,
                IsPull = this.IsPull,
                IsWindow = this.IsWindow,
                IsDouble = this.IsDouble,
                WindowHeight = this.WindowHeight,
                Wall = this.Wall
            };
        }

        /// <summary>
        /// Get a textual description
        /// </summary>
        /// <returns>A string containing the data</returns>
        public override string GetDescription()
        {
            string dble = IsDouble ? " double" : " simple";
            return "Ouverture\n"
            + (IsWindow ? "Fenetre" + dble + "\nHauteur : " + ParsingFunctions.ToStringCentimeters(WindowHeight) : "Porte\n")
            + "Longueur : " + ParsingFunctions.ToStringCentimeters(Size.x) + "\n";
        }
    }
}