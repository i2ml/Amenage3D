using ErgoShop.Cameras;
using ErgoShop.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollCamBySide : MonoBehaviour
{
    enum SIDE
    {
        NONE, UP, DOWN, RIGHT, LEFT, DOWNRIGHT, UPRIGHT, UPLEFT, DOWNLEFT
    }

    SIDE currantePos = SIDE.NONE;

    [SerializeField] Camera2DMove cam = null;
    [SerializeField] Camera3DMove cam3D = null;
    [SerializeField] float speed = 10F;
    [SerializeField] Vector3 offesteFinal;

    [SerializeField] List<GameObject> listeButton;

    private GlobalManager globalManager = null;


    private void Start()
    {
        globalManager = GlobalManager.Instance;

        foreach (var item in listeButton)
        {
            item.SetActive(false);
        }
    }

    public void SetSide(int _side)
    {
        currantePos = (SIDE)_side;
    }

    private void ActiveBareDefil(bool _isActif)
    {
        foreach (var item in listeButton)
        {
            item.SetActive(_isActif);
        }
    }

    private void Update()
    {
        if (Input.GetAxis("DeplacementSouris") != 0)
        {
            foreach (var item in listeButton)
            {
                item.SetActive(true);
            }
            switch (globalManager.GetCurrentMode())
            {
                case ViewMode.Top:
                    ActiveBareDefil(true);

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
                                        case SIDE.DOWNRIGHT:
                                            offesteFinal += Vector3.down * speed * Time.deltaTime;
                                            offesteFinal += Vector3.right * speed * Time.deltaTime;
                                            break;
                                        case SIDE.DOWNLEFT:
                                            offesteFinal += Vector3.down * speed * Time.deltaTime;
                                            offesteFinal += Vector3.left * speed * Time.deltaTime;
                                            break;
                                        case SIDE.UPRIGHT:
                                            offesteFinal += Vector3.up * speed * Time.deltaTime;
                                            offesteFinal += Vector3.right * speed * Time.deltaTime;
                                            break;
                                        case SIDE.UPLEFT:
                                            offesteFinal += Vector3.up * speed * Time.deltaTime;
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
                    break;
                case ViewMode.ThreeD:
                    ActiveBareDefil(false);

                    //switch (currantePos)
                    //{
                    //    case SIDE.NONE:
                    //        offesteFinal = Vector3.zero;
                    //        break;
                    //    case SIDE.UP:
                    //        offesteFinal += Vector3.up * speed * Time.deltaTime;
                    //        break;
                    //    case SIDE.DOWN:
                    //        offesteFinal += Vector3.down * speed * Time.deltaTime;
                    //        break;
                    //    case SIDE.RIGHT:
                    //        offesteFinal += Vector3.right * speed * Time.deltaTime;
                    //        break;
                    //    case SIDE.LEFT:
                    //        offesteFinal += Vector3.left * speed * Time.deltaTime;
                    //        break;
                    //    default:
                    //        offesteFinal = Vector3.zero;
                    //        break;
                    //}

                    //cam3D.SetForcedpos3D(offesteFinal);
                    break;
                default:

                    break;
            }


            int Limite_Horizontale = 40;
            int Limite_Verticale = 70;

            if (offesteFinal.x > Limite_Horizontale)
            {
                offesteFinal = new Vector3(Limite_Horizontale, offesteFinal.y, offesteFinal.z);
            }
            else if (offesteFinal.x < -Limite_Horizontale)
            {
                offesteFinal = new Vector3(-Limite_Horizontale, offesteFinal.y, offesteFinal.z);
            }

            if (offesteFinal.y > Limite_Verticale)
            {
                offesteFinal = new Vector3(offesteFinal.x, Limite_Verticale, offesteFinal.z);
            }
            else if (offesteFinal.y < -Limite_Verticale)
            {
                offesteFinal = new Vector3(offesteFinal.x, -Limite_Verticale, offesteFinal.z);
            }

        }
        else
        {
            foreach (var item in listeButton)
            {
                item.SetActive(false);
            }
        }


    }
}
