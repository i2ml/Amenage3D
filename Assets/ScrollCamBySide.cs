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


        if (Input.GetKey(KeyCode.UpArrow))
        {
            offesteFinal += Vector3.up * speed * Time.deltaTime;
        }
        else
        {
            if (Input.GetKey(KeyCode.DownArrow))
            {
                offesteFinal += Vector3.down * speed * Time.deltaTime;
            }
            else
            {
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    offesteFinal += Vector3.left * speed * Time.deltaTime;
                }
                else
                {
                    if (Input.GetKey(KeyCode.RightArrow))
                    {
                        offesteFinal += Vector3.right * speed * Time.deltaTime;
                    }
                    else
                    {
                        switch (currantePos)
                        {
                            case SIDE.NONE:
                                offesteFinal = Vector3.zero;
                                break;
                            case SIDE.UP:
                                offesteFinal += Vector3.up * speed * Time.deltaTime;
                                break;
                            case SIDE.DOWN:
                                offesteFinal += Vector3.down * speed * Time.deltaTime;
                                break;
                            case SIDE.RIGHT:
                                offesteFinal += Vector3.right * speed * Time.deltaTime;
                                break;
                            case SIDE.LEFT:
                                offesteFinal += Vector3.left * speed * Time.deltaTime;
                                break;
                            default:
                                offesteFinal = Vector3.zero;
                                break;
                        }
                    }
                }
            }
        }
        cam.SetForcedpos(offesteFinal);
    }
}
