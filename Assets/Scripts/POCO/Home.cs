using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ErgoShop.POCO
{
    /// <summary>
    /// Stores address and type.    
    /// </summary>
    public class Home
    {
        public string Address { get; set; }
        /// <summary>
        /// Can be anything, the users sets it
        /// </summary>
        public string Type { get; set; }
    }
}