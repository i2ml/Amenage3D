using ErgoShop.Cameras;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollCamBySide : MonoBehaviour
{
    enum SIDE
    {
        NONE, UP, DOWN, RIGHT, LEFT,
    }

    SIDE currantePos = SIDE.NONE;

    [SerializeField] Camera2DMove cam = null;
    [SerializeField] float speed = 10F;
    [SerializeField] Vector3 offesteFinal;


    public void SetSide(int _side)
    {
        currantePos = (SIDE)_side;
    }

    private void Update()
    {
        Debug.Log(currantePos);
        switch (currantePos)
        {
            case SIDE.NONE:
                cam.isScrollByUI = false;
                offesteFinal = Vector3.zero;
                break;
            case SIDE.UP:
                cam.isScrollByUI = true;
                offesteFinal += Vector3.up * speed * Time.deltaTime;
                break;
            case SIDE.DOWN:
                cam.isScrollByUI = true;
                offesteFinal += Vector3.down * speed * Time.deltaTime;
                break;
            case SIDE.RIGHT:
                cam.isScrollByUI = true;
                offesteFinal += Vector3.right * speed * Time.deltaTime;
                break;
            case SIDE.LEFT:
                cam.isScrollByUI = true;
                offesteFinal += Vector3.left * speed * Time.deltaTime;
                break;
            default:
                cam.isScrollByUI = false;
                offesteFinal = Vector3.zero;
                break;
        }

        cam.SetForcedpos(offesteFinal);
    }
}
