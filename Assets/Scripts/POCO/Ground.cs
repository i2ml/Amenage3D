using Newtonsoft.Json;
using UnityEngine;

namespace ErgoShop.POCO
{
    /// <summary>
    ///     Ground data is the color (saved) and its gameobject (regenerated)
    /// </summary>
    public class Ground
    {
        /// <summary>
        ///     its gameobject (regenerated)
        /// </summary>
        [JsonIgnore] public GameObject planeGenerated;

        public Color Color { get; set; }
    }
}