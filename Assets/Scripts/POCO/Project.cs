using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoShop.POCO
{
    /// <summary>
    /// Class containing the project data. Its used to load and save data with json files
    /// </summary>
    public class Project
    {
        public string ProjectName { get; set; }
        public DateTime Date { get; set; }
        public Person Person { get; set; }
        public string HomeType { get; set; }
        public string Version { get;set; }
        public string Comment { get; set; }

        // Home
        /// <summary>
        /// All scene data is splitted into the floors. One floor will be loaded at a time
        /// </summary>
        public List<Floor> Floors { get; set; }
        public int CurrentFloorIdx { get; set; }

        // Settings
        public float WallThickness { get; set; }
        public float WallHeight { get; set; }

        public Floor GetCurrentFloor()
        {
            return Floors[CurrentFloorIdx];
        }
    }
}
