using System.Collections.Generic;
using Dynagon;
using ErgoShop.Managers;
using ErgoShop.UI;
using ErgoShop.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace ErgoShop.POCO
{
    /// <summary>
    ///     Wall data. A wall is constitued of two extremities : P1 and P2
    ///     The wall shape is a trapeze according to other walls, thickness, and those 2 points.
    /// </summary>
    public class Wall : MovableElement
    {
        /// <summary>
        ///     measure on one side if ShowDetailedCotations is false
        /// </summary>
        [JsonIgnore] public CotationsScript cotOne;

        /// <summary>
        ///     measure on one side if ShowDetailedCotations is false
        /// </summary>
        [JsonIgnore] public CotationsScript cotTwo;

        /// <summary>
        ///     detailed mesures if ShowDetailedCotations is true
        /// </summary>
        [JsonIgnore] public List<CotationsScript> detailedCotations = new List<CotationsScript>();

        /// <summary>
        ///     detailed mesures if ShowDetailedCotations is true
        /// </summary>
        [JsonIgnore] public List<CotationsScript> detailedCotations2 = new List<CotationsScript>();

        // Unity data (Scene)

        /// <summary>
        ///     Linked walls to P1
        /// </summary>
        [JsonIgnore] public List<Wall> linkedP1 = new List<Wall>();

        /// <summary>
        ///     Linked walls to P2
        /// </summary>
        [JsonIgnore] public List<Wall> linkedP2 = new List<Wall>();

        /// <summary>
        ///     Mesh data
        /// </summary>
        [JsonIgnore] public List<Vector3> vertices2D = new List<Vector3>();

        /// <summary>
        ///     2D objects to make the wall
        /// </summary>
        [JsonIgnore] public List<Polygon> walls2D = new List<Polygon>();

        /// <summary>
        ///     3D objects to make the wall
        /// </summary>
        [JsonIgnore] public List<Polygon> walls3D = new List<Polygon>();

        /// <summary>
        ///     Constructor. Initialize associated gameobject for Unity
        /// </summary>
        public Wall()
        {
            Openings = new List<WallOpening>();

            associated2DObject = new GameObject("Walls2D");
            associated2DObject.layer = (int) ErgoLayers.Top;
            associated2DObject.tag = "Associated";

            associated3DObject = new GameObject("Walls3D");
            associated3DObject.layer = (int) ErgoLayers.ThreeD;
            associated3DObject.tag = "Associated";
            Color = new Color(200f / 255f, 200f / 255f, 200f / 255f);
        }

        /// <summary>
        ///     Thickness in meters
        /// </summary>
        public float Thickness { get; set; }

        /// <summary>
        ///     Height in meters
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        ///     Currently not used
        /// </summary>
        public bool IsBearingWall { get; set; }

        /// <summary>
        ///     Point 1, 2D
        /// </summary>
        public Vector3 P1 { get; set; }

        /// <summary>
        ///     Point 2, 2D
        /// </summary>
        public Vector3 P2 { get; set; }

        /// <summary>
        ///     Wall color, defined by a color picker with RGB
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        ///     Wall index
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        ///     Doors and windows contained in the wall
        /// </summary>
        public List<WallOpening> Openings { get; set; }

        // COTATIONS
        /// <summary>
        ///     Show measures between each part of wall and openings
        /// </summary>
        [JsonIgnore]
        public bool ShowDetailedCotations { get; set; }

        // METHODS AND COMPUTED PROPS
        /// <summary>
        ///     Distance between the two points
        /// </summary>
        public float Length => Vector3.Distance(P1, P2);

        /// <summary>
        ///     Get the length outside the room
        /// </summary>
        public float OutsideLength
        {
            get
            {
                var length = Mathf.Floor(Length * 100f) / 100f;
                if (!cotOne || !cotTwo) return length;
                if (cotOne.Length > 0 && cotTwo.Length == 0)
                    length = cotOne.Length;
                else if (cotTwo.Length > 0 && cotOne.Length == 0)
                    length = cotTwo.Length;
                else if (cotTwo.Length > 0 && cotOne.Length > 0) length = Mathf.Max(cotOne.Length, cotTwo.Length);

                length = Mathf.Floor(length * 100f) / 100f;

                return length;
            }
        }

        /// <summary>
        ///     Get the length inside the room
        /// </summary>
        public float InsideLength
        {
            get
            {
                var length = Mathf.Floor(Length * 100f) / 100f;

                if (!cotOne || !cotTwo) return length;

                if (cotOne.Length > 0 && cotTwo.Length == 0)
                    length = cotOne.Length;
                else if (cotTwo.Length > 0 && cotOne.Length == 0)
                    length = cotTwo.Length;
                else if (cotTwo.Length > 0 && cotOne.Length > 0) length = Mathf.Min(cotOne.Length, cotTwo.Length);

                length = Mathf.Floor(length * 100f) / 100f;

                return length;
            }
        }

        /// <summary>
        ///     Get direction between the two points, normalized (2D)
        /// </summary>
        public Vector3 Direction => (P2 - P1).normalized;

        /// <summary>
        ///     Get the perpendicular vector according to the Direction, (2D)
        /// </summary>
        public Vector3 Perpendicular => new Vector3(Direction.y, -Direction.x, 0);

        /// <summary>
        ///     Get wall center (2D)
        /// </summary>
        public Vector3 Center => (P1 + P2) / 2;

        /// <summary>
        ///     Get perpendicular to adjust furniture on the wall
        /// </summary>
        public Vector3 GetPerpendicularFromPos(Vector3 pos)
        {
            var pos2D = VectorFunctions.Switch3D2D(pos);

            if (Vector3.Dot(Center - pos2D, Perpendicular) < 0)
            {
                Debug.Log("Perpendicular");
                return VectorFunctions.Switch2D3D(Perpendicular);
            }

            Debug.Log("Pas Perpendicular");
            return VectorFunctions.Switch2D3D(-Perpendicular);
        }

        /// <summary>
        ///     Get Normal pointing room interior
        /// </summary>
        /// <param name="r">The room</param>
        /// <returns>the normal</returns>
        public Vector3 GetInteriorSide(Room r)
        {
            if (r != null)
                return r.GetCenter() - (P1 + P2) / 2;
            return Vector3.positiveInfinity;
        }

        /// <summary>
        ///     Set wall position. Cost lots of resources since it may update linkedwalls and each point
        /// </summary>
        /// <param name="newPosition">Wanted position</param>
        /// <param name="updateLinked">true to update linked walls. Be careful If false the wall might be separated from the others</param>
        public void SetPosition(Vector3 newPosition, bool updateLinked = true)
        {
            var newP1 = newPosition - Length * Direction / 2f;
            var newP2 = newPosition + Length * Direction / 2f;
            if (updateLinked)
            {
                WallsCreator.Instance.UpdateBothWallPoints(this, newP1, newP2);
            }
            else
            {
                P1 = newP1;
                P2 = newP2;
                Position = Center;
            }
        }

        /// <summary>
        ///     Rebuild associated gameobjects for unity
        /// </summary>
        public override void RebuildSceneData()
        {
            if (!associated2DObject) associated2DObject = new GameObject("Walls2D");
            associated2DObject.layer = (int) ErgoLayers.Top;
            associated2DObject.tag = "Associated";

            if (!associated3DObject) associated3DObject = new GameObject("Walls3D");
            associated3DObject.layer = (int) ErgoLayers.ThreeD;
            associated3DObject.tag = "Associated";
        }

        /// <summary>
        ///     Move the opening along the wall, according to mouse.
        /// </summary>
        /// <param name="offset">The starting position when the users clicks</param>
        public override void Move(Vector3 offset)
        {
            var cam = GlobalManager.Instance.GetActiveCamera();

            switch (cam.gameObject.layer)
            {
                case (int) ErgoLayers.Top:
                    Debug.Log("Moving Wall !!");
                    var pos2D = InputFunctions.GetWorldPoint2D(cam);
                    var r = SelectedObjectManager.Instance.currentRoomData;
                    var roomMod = r == null ? 1f : r.LockAngles ? 1f / 4f : 1f / 2f;
                    SetPosition(VectorFunctions.Switch3D2D(Position) + (pos2D - offset) * roomMod);
                    break;
            }
        }

        /// <summary>
        ///     Instantiate an identical object
        /// </summary>
        /// <returns>The same Element</returns>
        public override Element GetCopy()
        {
            var w =
                new Wall
                {
                    Id = Id,
                    Height = Height,
                    IsLocked = IsLocked,
                    Size = Size,
                    CanBePutOnFurniture = CanBePutOnFurniture,
                    IsOnWall = IsOnWall,
                    Rotation = Rotation,
                    Position = Center,
                    Index = Index,
                    P1 = P1,
                    P2 = P2,
                    Thickness = Thickness,
                    Color = Color,
                    Openings = new List<WallOpening>()
                };
            foreach (var wo in Openings) w.Openings.Add(wo.GetCopy() as WallOpening);
            return w;
        }

        /// <summary>
        ///     Get a textual description
        /// </summary>
        /// <returns>A string containing the data</returns>
        public override string GetDescription()
        {
            var clipboard = "Mur\n";
            clipboard += "Epaisseur : " + ParsingFunctions.ToStringCentimeters(Thickness) + "\n";
            clipboard += "Longueur : " + ParsingFunctions.ToStringCentimeters(InsideLength) + "\n";
            return clipboard;
        }
    }
}