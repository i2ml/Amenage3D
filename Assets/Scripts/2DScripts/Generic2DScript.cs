using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoShop.TwoDScripts
{
    /// <summary>
    /// Adapt text transform to the sprite
    /// </summary>
    public class Generic2DScript : MonoBehaviour
    {
        private Transform text;
        // Start is called before the first frame update
        void Start()
        {
            text = transform.GetChild(0);
        }

        // Update is called once per frame
        void Update()
        {
            text.parent = transform.parent;
            text.position = transform.position;
            text.rotation = Quaternion.identity;
            text.localScale = Vector3.one * 0.05f;

            //var s = transform.localScale;
            //if (s.x == 0) s.x = 1;
            //if (s.y == 0) s.y = 1;
            //if (s.z == 0) s.z = 1;

            //transform.GetChild(0).localScale = new Vector3(1f / s.x, 1f / s.y, 1f / s.z) * 0.05f;
        }
    }
}