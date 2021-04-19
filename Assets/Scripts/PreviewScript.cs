using ErgoShop.POCO;
using ErgoShop.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PreviewScript : MonoBehaviour
{
    [SerializeField] private Camera previewCam = null;
    [SerializeField] private RawImage rawImage = null;
    [SerializeField] private GameObject gm_Preview = null;

    //private bool manyMeshFilter = false;

    private Vector3 posModel = Vector3.zero;

    private MeshFilter mesh_Gm_Preview = null;

    /// <summary>
    /// pas de furniture le model 3D
    /// </summary>
    /// <param name="_gm"></param>
    public void NewPreview(GameObject _gm)
    {
        if (_gm != null)
        {
            GameObject gm_tmp = Instantiate
            (
            _gm, posModel, Quaternion.identity, this.transform
            );

            //MeshFilter[] meshAll = gm_tmp.GetComponents<MeshFilter>();
            //if (meshAll.Length < 1)
            //{
            //    manyMeshFilter = false;
            //}
            //else
            //{
            //    manyMeshFilter = true;
            //}

            gm_Preview.GetComponent<MeshFilter>().mesh = gm_tmp.GetComponent<MeshFilter>().mesh;
            if (gm_Preview.GetComponent<MeshFilter>().mesh == null)
            {
                gm_Preview.GetComponent<MeshFilter>().mesh = gm_tmp.GetComponentInChildren<MeshFilter>().mesh;
            }

            gm_Preview.GetComponent<MeshRenderer>().material = gm_tmp.GetComponent<MeshRenderer>().material;
            if (gm_Preview.GetComponent<MeshRenderer>().material == null)
            {
                gm_Preview.GetComponent<MeshRenderer>().material = gm_tmp.GetComponentInChildren<MeshRenderer>().material;
            }


            Destroy(gm_tmp);

            mesh_Gm_Preview = gm_Preview.GetComponent<MeshFilter>();
        }
        else
        {
            Destroy(gm_Preview.gameObject);
        }
    }

    private void Start()
    {
        if (gm_Preview != null)
        {
            NewPreview(gm_Preview);
            posModel = gm_Preview.transform.position;
        }
    }

    private void Update()
    {
        if (mesh_Gm_Preview != null && !InputFunctions.IsMouseOutsideUI())
        {
            rawImage.gameObject.SetActive(true);

            gm_Preview.transform.RotateAround(transform.up, 1 * Time.deltaTime);
        }
        else
        {
            rawImage.gameObject.SetActive(false);
        }
    }
}
