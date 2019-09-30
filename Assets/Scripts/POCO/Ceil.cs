using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ErgoShop.POCO
{
    /// <summary>
    /// Ceil data is the color (saved) and its gameobject (regenerated)
    /// </summary>
    public class Ceil
    {
        public Color Color { get; set; }
        /// <summary>
        /// its gameobject (regenerated)
        /// </summary>
        [JsonIgnore]
        public GameObject planeGenerated;
    }
}