using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoShop.Utils.Extensions
{
    public static class CollectionsExtensions
    {
        /// <summary>
        /// Copy a list content
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<Object> Copy(this List<Object> list)
        {
            List<Object> resList = new List<Object>();
            foreach(var o in list)
            {
                resList.Add(o);
            }
            return resList;
        }
    }
}
