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

    private bool isActif = false;

    private Vector3 posModel = Vector3.zero;

    private MeshFilter mesh_Gm_Preview = null;

    public bool IsActif { get => isActif; set => isActif = value; }

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

            MeshFilter[] meshAll = gm_tmp.GetComponentsInChildren<MeshFilter>();

            if (meshAll != null && meshAll.Length != 0)
            {
                if (meshAll.Length <= 1)
                {
                    gm_Preview.GetComponent<MeshFilter>().mesh = meshAll[0].mesh;
                    if (gm_Preview.GetComponent<MeshFilter>().mesh == null)
                    {
                        gm_Preview.GetComponent<MeshFilter>().mesh = meshAll[0].mesh;
                    }

                    gm_Preview.GetComponent<MeshRenderer>().material = meshAll[0].gameObject.GetComponent<MeshRenderer>().material;
                    if (gm_Preview.GetComponent<MeshRenderer>().material == null)
                    {
                        gm_Preview.GetComponent<MeshRenderer>().material = meshAll[0].gameObject.GetComponentInChildren<MeshRenderer>().material;
                    }


                    Destroy(gm_tmp);

                    mesh_Gm_Preview = gm_Preview.GetComponent<MeshFilter>();
                }
                else
                {
                    //Debug.Log("Many MeshFilter " + meshAll.Length + " " + gm_tmp.name);

                    float VertexSort = 0;
                    int itemCount = 0, NumMesh = 0;
                    foreach (var item in meshAll)
                    {
                        //SORT
                        if (item.mesh.vertexCount > VertexSort)
                        {
                            VertexSort = item.mesh.vertexCount;

                            //sort in Mesh filter List
                            MeshFilter tmp_filter = meshAll[0];
                            meshAll[0] = meshAll[itemCount];
                            meshAll[itemCount] = tmp_filter;
                            NumMesh = itemCount; // num mesh hight Vertex
                        }
                        itemCount++;
                    }

                    //Debug.Log(itemCount + " " + NumMesh);

                    gm_Preview.GetComponent<MeshFilter>().mesh = meshAll[NumMesh].mesh;
                    gm_Preview.GetComponent<MeshRenderer>().material = meshAll[NumMesh].gameObject.GetComponentInChildren<MeshRenderer>().material;



                    //destroy MultiMesh
                    for (int i = 0; i < meshAll.Length - 1; i++)
                    {
                        try
                        {
                            Destroy(gm_tmp.transform.GetChild(i).gameObject);
                        }
                        catch
                        {

                        }
                    }
                    Destroy(gm_tmp);
                }
            }
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
        if (mesh_Gm_Preview != null && gm_Preview != null && IsActif && !InputFunctions.IsMouseOutsideUI())
        {
            rawImage.gameObject.SetActive(true);

            gm_Preview.transform.RotateAround(transform.up, 1 * Time.deltaTime);

            //rawImage.gameObject.transform.position = Input.mousePosition + (Vector3.right * 100) + (Vector3.up * 100);
        }
        else
        {
            rawImage.gameObject.SetActive(false);
        }
    }
}
