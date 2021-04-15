using ErgoShop.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayCameraSettings : MonoBehaviour
{
    GlobalManager globalManager = null;

    [SerializeField] private GameObject ViewPanel = null;

    private void Start()
    {
        globalManager = GlobalManager.Instance;
    }

    private void Update()
    {
        switch (globalManager.GetCurrentMode())
        {
            case ViewMode.Top:
                ViewPanel.SetActive(false);
                break;
            case ViewMode.ThreeD:
                ViewPanel.SetActive(true);
                break;
            default:
                break;
        }
    }
}
