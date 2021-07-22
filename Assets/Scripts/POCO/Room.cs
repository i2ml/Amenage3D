using System;
using System.Collections.Generic;
using ErgoShop.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace ErgoShop.POCO
{
    /// <summary>
    ///     Room data. A room is made of at least 3 walls
    ///     Will all those data we can generate a ceiling and a ground floor
    /// </summary>
    public class Room : MovableElement
    {
        // Unity data (Scene)
        //[JsonIgnore]
        //public GameObject selectableText;
        [JsonIgnore] public bool ShowCotations = true;

        public string Name { get; set; }

        /// <summary>
        ///     Not used for now
        /// </summary>
        public string Type { get; set; }

        public List<Wall> Walls { get; set; }
        public float Height { get; set; }
        public Ceil Ceil { get; set; }
        public Ground Ground { get; set; }
        public bool LockAngles { get; set; }

        public Vector3 GetCenter()
        {
            if (Walls.Count == 0) return VectorFunctions.Switch3D2D(Position);
            var positions = new List<Vector3>();
            foreach (var w in Walls)
            {
                if (!positions.Contains(w.P1)) positions.Add(w.P1);
                if (!positions.Contains(w.P2)) positions.Add(w.P2);
            }

            var center = Vector3.zero;
            foreach (var p in positions) center = center + p;
            return new Vector3(center.x / positions.Count, center.y / positions.Count, center.z / positions.Count);
        }

        /// <summary>
        ///     Not used since the room is not an element like the others...
        /// </summary>
        public override void RebuildSceneData()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///     Instantiate an identical object
        ///     Copies also the walls
        /// </summary>
        /// <returns>The same Element</returns>
        public override Element GetCopy()
        {
            var copy = new Room
            {
                Id = Id,
                Height = Height,
                IsLocked = IsLocked,
                LockAngles = LockAngles,
                Size = Size,
                CanBePutOnFurniture = CanBePutOnFurniture,
                IsOnWall = IsOnWall,
                Position = GetCenter(),
                Name = Name,
                Type = Type,
                Ceil = new Ceil {Color = Ceil.Color},
                Ground = new Ground {Color = Ground.Color},
                Walls = new List<Wall>()
            };

            foreach (var w in Walls) copy.Walls.Add(w.GetCopy() as Wall);
            return copy;
        }

        /// <summary>
        ///     Get a textual description
        /// </summary>
        /// <returns>A string containing the data</returns>
        public override string GetDescription()
        {
            var desc = Name + " : " + Walls.Count + " murs\n";
            foreach (var w in Walls) desc += w.GetDescription();
            return desc;
        }

        /// <summary>
        ///     since we move the walls, this override does nothing in particular
        /// </summary>
        /// <param name="startingPos"></param>
        public override void Move(Vector3 startingPos)
        {
            base.Move(startingPos);
        }
    }
}