using System.Collections.Generic;

namespace ErgoShop.POCO
{
    /// <summary>
    ///     To have more computer resources, only one floor is loaded at a time.
    ///     Each floor contain all data to rebuild rooms, furnitures etc
    /// </summary>
    public class Floor
    {
        public Floor()
        {
            NewLists();
        }

        public string FloorName { get; set; }

        /// <summary>
        ///     wall height when creating a new wall
        /// </summary>
        public float WallHeight { get; set; }

        public List<Room> Rooms { get; set; }
        public List<Wall> Walls { get; set; }
        public List<Furniture> Furnitures { get; set; }
        public List<Stairs> Stairs { get; set; }
        public List<TextZoneElement> TextZoneElements { get; set; }
        public List<ElementGroup> Groups { get; set; }
        public List<CharacterElement> Characters { get; set; }

        public void ClearAll()
        {
            NewLists();
        }

        public void NewLists()
        {
            Rooms = new List<Room>();
            Walls = new List<Wall>();
            Furnitures = new List<Furniture>();
            Stairs = new List<Stairs>();
            TextZoneElements = new List<TextZoneElement>();
            Groups = new List<ElementGroup>();
            Characters = new List<CharacterElement>();
        }
    }
}