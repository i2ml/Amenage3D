using ErgoShop.POCO;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

namespace ErgoShop.ConstraintsScripts
{
    /// <summary>
    ///     Bake the accessible area for a 45cm radius disk
    ///     Find the tools at github at https://github.com/Unity-Technologies/NavMeshComponents
    /// </summary>
    public class NavigationBaker : MonoBehaviour
    {
        private bool m_goBake;
        private NavMeshSurface m_surface;

        // Start is called before the first frame update
        private void Start()
        {
            m_surface = GetComponent<NavMeshSurface>();
        }


        // Update 1 time per second (only for debug, costs a lot of resources)
        // UNCOMMENT TO TEST STUFF
        //void Update()
        //{
        //    int dixiemes = Mathf.RoundToInt(Time.time * 10) % 10;
        //    if (dixiemes == 0 && m_goBake)
        //    {
        //        m_goBake = false;
        //        Debug.Log("BAKE");
        //        BakeNavMeshSurface();
        //    }
        //    else if (dixiemes == 5 && !m_goBake) m_goBake = true;
        //}

        /// <summary>
        ///     Function called by the "Calculer le passage" button
        /// </summary>
        public void BakeNavMeshSurface()
        {
            //desactivate all opening for calculBaking
            GameObject[] wallOpening = GameObject.FindGameObjectsWithTag("WallOpening");
            foreach (GameObject item in wallOpening)
            {
                item.SetActive(false);
            }

            //Bake
            GetComponent<MeshFilter>().mesh = null;
            m_surface.BuildNavMesh();
            var triangles = NavMesh.CalculateTriangulation();
            var mesh = new Mesh();
            mesh.vertices = triangles.vertices;
            mesh.triangles = triangles.indices;
            GetComponent<MeshFilter>().mesh = mesh;

            //Reactivate for visual all opening
            foreach (GameObject item in wallOpening)
            {
                item.SetActive(true);
            }
        }

        public void EraseNavMeshSurface()
        {
            GetComponent<MeshFilter>().mesh = null;
        }

        public int GetAgenTypeIDByName(string agentTypeName)
        {
            int count = NavMesh.GetSettingsCount();
            string[] agentTypeNames = new string[count + 2];
            for (var i = 0; i < count; i++)
            {
                Debug.Log(i);
                int id = NavMesh.GetSettingsByIndex(i).agentTypeID;
                string name = NavMesh.GetSettingsNameFromID(id);
                if (name == agentTypeName)
                {
                    return id;
                }
            }
            return -1;
        }

        public void ChangeAgent(GameObject _dropDown)
        {
            int indexID = _dropDown.GetComponent<TMP_Dropdown>().value;
            GetComponent<NavMeshSurface>().agentTypeID = NavMesh.GetSettingsByIndex(indexID).agentTypeID;
        }
    }
}