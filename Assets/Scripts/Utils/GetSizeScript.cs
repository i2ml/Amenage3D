using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoShop.Utils
{
    /// <summary>
    /// Script to use in editor only. Its like meshsize but in editor
    /// </summary>
    [ExecuteInEditMode]
    public class GetSizeScript : MonoBehaviour
    {
        public Vector3 size;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
#if (UNITY_EDITOR)
            if (size == Vector3.zero)
            {
                if (GetComponent<MeshFilter>())
                {
                    size = GetComponent<MeshFilter>().sharedMesh.bounds.size;
                }
                else
                {
                    size = GetComponent<SkinnedMeshRenderer>().sharedMesh.bounds.size;
                }
            }
#endif
        }
    }
}