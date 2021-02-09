using System.Collections.Generic;
using ErgoShop.POCO;
using UnityEngine;

namespace ErgoShop.UI
{
    /// <summary>
    ///     Script to generate 2D sprites to match the stairs appearence in 3D
    /// </summary>
    public class RoundedStairsScript : MonoBehaviour
    {
        public GameObject stairTilePrefab;

        public Stairs associatedStairs;

        // Start is called before the first frame update
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
        }

        /// <summary>
        ///     Draw !
        /// </summary>
        public void Draw()
        {
            // CLEAN
            var children = new List<GameObject>();
            foreach (Transform child in transform) children.Add(child.gameObject);
            children.ForEach(child => Destroy(child));
            //
            if (associatedStairs.Curvature > 0)
                for (var i = 0; i <= associatedStairs.NbSteps; i++)
                {
                    var go = Instantiate(stairTilePrefab, transform);
                    var stepSize = associatedStairs.Curvature / associatedStairs.NbSteps;
                    var curAngle = (associatedStairs.ToTheLeft ? 1 : -1f) *
                                   (i * stepSize + (associatedStairs.ToTheLeft ? 0 : 180f));
                    var center = Quaternion.Euler(Vector3.forward * curAngle) *
                                 (associatedStairs.InnerRadius * Vector3.right);
                    go.transform.localPosition =
                        new Vector3(Mathf.Cos(curAngle * Mathf.Deg2Rad), Mathf.Sin(curAngle * Mathf.Deg2Rad)) + center;
                    go.transform.localEulerAngles = Vector3.forward * curAngle;
                    go.transform.localScale = new Vector3(associatedStairs.Width, stepSize / 10f, 1);
                }
            else
                for (var i = 0; i < associatedStairs.NbSteps; i++)
                {
                    var go = Instantiate(stairTilePrefab, transform);
                    var stepSize = associatedStairs.Depth / associatedStairs.NbSteps;
                    go.transform.localPosition = Vector3.up * i * stepSize;
                    go.transform.localScale = new Vector3(associatedStairs.Width, stepSize * 10f, 1);
                }
        }
    }
}