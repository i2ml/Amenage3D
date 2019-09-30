using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoShop.Utils
{
    /// <summary>
    /// Parse float
    /// </summary>
    public static class ParsingFunctions
    {
        public static bool ParseFloatCommaDot(string s, out float res)
        {
            if(float.TryParse(s, out res))
            {
                return true;
            }
            s=s.Replace(".", ",");
            res = 0;
            return float.TryParse(s, out res);
        }

        public static string ToStringCentimeters(float f)
        {
            return Mathf.RoundToInt(f * 100f) + " cm";
        }
    }
}