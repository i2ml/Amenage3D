using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ErgoShop.POCO
{
    /// <summary>
    /// Ground data is the color (saved) and its gameobject (regenerated)
    /// </summary>
    public class Ground
    {
        public Color Color { get; set; }

        /// <summary>
        /// its gameobject (regenerated)
        /// </summary>
        [JsonIgnore]
        public GameObject planeGenerated;
    }
}