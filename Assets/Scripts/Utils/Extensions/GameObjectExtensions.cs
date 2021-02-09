using System;
using System.Collections.Generic;
using ErgoShop.POCO;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ErgoShop.Utils.Extensions
{
    /// <summary>
    ///     Extensions methods for gameobjects
    /// </summary>
    public static class GameObjectExtensions
    {
        /// <summary>
        ///     Get boundaries size
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        public static Vector3 MeshSize(this GameObject go)
        {
            if (!go) return Vector3.positiveInfinity;
            if (go.GetComponent<MeshFilter>() != null)
                return go.GetComponent<MeshFilter>().sharedMesh.bounds.size;
            return go.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh.bounds.size;
        }

        /// <summary>
        ///     Set layer to gameobject and its children
        /// </summary>
        /// <param name="go"></param>
        /// <param name="layer"></param>
        public static void SetLayerRecursively(this GameObject go, int layer)
        {
            go.layer = layer;
            for (var i = 0; i < go.transform.childCount; i++)
                go.transform.GetChild(i).gameObject.SetLayerRecursively(layer);
        }

        /// <summary>
        ///     Get a random color
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static Color RandomColor(this Color c)
        {
            return new Color(
                Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        }

        /// <summary>
        ///     Get walls center
        /// </summary>
        /// <param name="walls"></param>
        /// <returns></returns>
        public static Vector3 GetCenter(this List<Wall> walls)
        {
            var v = Vector3.zero;
            foreach (var w in walls) v += w.Center;
            return new Vector3(v.x / walls.Count, walls[0].Height / 2, v.y / walls.Count);
        }

        public static Vector3 GetCenter(this List<Furniture> furnitures)
        {
            var v = Vector3.zero;
            foreach (var f in furnitures) v += f.associated3DObject.transform.position;
            return new Vector3(v.x / furnitures.Count, v.y / furnitures.Count, v.z / furnitures.Count);
        }

        public static Bounds GetSpriteBounds(this GameObject go)
        {
            Bounds b;
            if (go.GetComponent<SpriteRenderer>() != null)
                b = go.GetComponent<SpriteRenderer>().bounds;
            else if (go.transform.childCount > 0)
                b = go.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().bounds;
            else
                b = go.GetComponent<BoxCollider2D>().bounds;
            return b;
        }

        public static Bounds GetMeshBounds(this GameObject go)
        {
            Bounds b;
            if (go.GetComponent<BoxCollider>() != null)
            {
                b = go.GetComponent<BoxCollider>().bounds;
            }
            else if (go.GetComponent<MeshCollider>() != null)
            {
                b = go.GetComponent<MeshCollider>().bounds;
            }
            else if (go.transform.childCount > 0)
            {
                if (go.transform.GetChild(0).GetComponent<BoxCollider>() != null)
                    b = go.transform.GetChild(0).GetComponent<BoxCollider>().bounds;
                else
                    b = go.transform.GetChild(0).GetComponent<MeshCollider>().bounds;
            }
            else
            {
                throw new Exception("BOUNDS ?");
            }

            return b;
        }
    }
}