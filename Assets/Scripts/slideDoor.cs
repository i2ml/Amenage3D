using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class slideDoor : MonoBehaviour
{
    [SerializeField] private GameObject gm_door;
    void Update()
    {
        gm_door.GetComponent<SladingDoor>().isSladingDoor = true;
    }
}
