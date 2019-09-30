using ErgoShop.Utils;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ErgoShop.POCO
{
    /// <summary>
    /// Room data. A room is made of at least 3 walls
    /// Will all those data we can generate a ceiling and a ground floor
    /// </summary>
    public class Room : MovableElement
    {
        public string Name { get; set; }
        /// <summary>
        /// Not used for now
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
            List<Vector3> positions = new List<Vector3>();
            foreach (var w in Walls)
            {
                if (!positions.Contains(w.P1))
                {
                    positions.Add(w.P1);
                }
                if (!positions.Contains(w.P2))
                {
                    positions.Add(w.P2);
                }
            }
            Vector3 center = Vector3.zero;
            foreach (var p in positions)
            {
                center = center + p;
            }
            return new Vector3(center.x / positions.Count, center.y / positions.Count, center.z / positions.Count);
        }

        /// <summary>
        /// Not used since the room is not an element like the others...
        /// </summary>
        public override void RebuildSceneData()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Instantiate an identical object
        /// Copies also the walls
        /// </summary>
        /// <returns>The same Element</returns>
        public override Element GetCopy()
        {
            Room copy = new Room
            {
                Id = this.Id,
                Height = this.Height,
                IsLocked = this.IsLocked,
                LockAngles = this.LockAngles,
                Size = this.Size,
                CanBePutOnFurniture = this.CanBePutOnFurniture,
                IsOnWall = this.IsOnWall,
                Position = GetCenter(),
                Name = this.Name,
                Type = this.Type,
                Ceil = new Ceil { Color = this.Ceil.Color },
                Ground = new Ground { Color = this.Ground.Color },
                Walls = new List<Wall>()
            };

            foreach(var w in Walls)
            {
                copy.Walls.Add(w.GetCopy() as Wall);
            }
            return copy;
        }

        /// <summary>
        /// Get a textual description
        /// </summary>
        /// <returns>A string containing the data</returns>
        public override string GetDescription()
        {
            string desc = this.Name + " : " + this.Walls.Count + " murs\n";
            foreach (var w in this.Walls)
            {
                desc += w.GetDescription();
            }
            return desc;
        }

        /// <summary>
        /// since we move the walls, this override does nothing in particular
        /// </summary>
        /// <param name="startingPos"></param>
        public override void Move(Vector3 startingPos)
        {

        }

        // Unity data (Scene)
        //[JsonIgnore]
        //public GameObject selectableText;
        [JsonIgnore]
        public bool ShowCotations = true;
    }
}