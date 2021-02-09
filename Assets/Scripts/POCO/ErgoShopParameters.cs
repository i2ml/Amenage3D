using System.Collections.Generic;

namespace ErgoShop.POCO
{
    /// <summary>
    ///     Settings stored in a json file in appdata
    /// </summary>
    public class ErgoShopParameters
    {
        /// <summary>
        ///     Measure to the next obstacle or to the next wall only ?
        /// </summary>
        public bool TagOnlyWall { get; set; }

        /// <summary>
        ///     CameraSpeed to move and zoom
        /// </summary>
        public float CameraSpeed { get; set; }

        /// <summary>
        ///     Folder path used to store screenshots
        /// </summary>
        public string ScreenShotFolder { get; set; }

        /// <summary>
        ///     Show the 1meter/1meter grid in 2d view ?
        /// </summary>
        public bool ShowGrid { get; set; }

        /// <summary>
        ///     Custom Furnitures List
        /// </summary>
        public List<CustomFurniture> CustomFurnitures { get; set; }
    }
}