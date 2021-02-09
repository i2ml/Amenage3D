using Newtonsoft.Json;
using UnityEngine;

namespace ErgoShop.POCO
{
    /// <summary>
    ///     Ceil data is the color (saved) and its gameobject (regenerated)
    /// </summary>
    public class Ceil
    {
        /// <summary>
        ///     its gameobject (regenerated)
        /// </summary>
        [JsonIgnore] public GameObject planeGenerated;

        public Color Color { get; set; }
    }
}