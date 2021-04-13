using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SladingDoor : MonoBehaviour
{
    [SerializeField] private GameObject gm_door = null;
    [SerializeField] private GameObject gm_wheelLeft = null;
    [SerializeField] private GameObject gm_wheelRight = null;
    [SerializeField] private GameObject gm_rail = null;

    private Vector3 StartPos = Vector3.zero;
    [SerializeField] private Vector3 EndPos = Vector3.zero;

    [SerializeField] private Vector3 offsetWheelLeft = Vector3.zero;
    [SerializeField] private Vector3 offsetWheelRight = Vector3.zero;

    public bool isSladingDoor = false;
    public bool isOpen = false;

    private void Start()
    {
        StartPos = gm_door.transform.localPosition;

        offsetWheelLeft = gm_door.transform.localPosition - gm_wheelLeft.transform.localPosition;
        offsetWheelRight = gm_door.transform.localPosition - gm_wheelRight.transform.localPosition;
    }

    void Update()
    {
        if (isOpen)
        {
            gm_door.transform.localPosition = EndPos;
        }
        else
        {
            gm_door.transform.localPosition = StartPos;
        }

        if (isSladingDoor)
        {
            gm_door.SetActive(true);
            gm_rail.SetActive(true);
            gm_wheelLeft.SetActive(true);
            gm_wheelRight.SetActive(true);
        }
        else
        {
            gm_door.SetActive(false);
            gm_rail.SetActive(false);
            gm_wheelLeft.SetActive(false);
            gm_wheelRight.SetActive(false);
        }
    }
}
