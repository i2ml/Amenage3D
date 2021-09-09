using ErgoShop.Managers;
using ErgoShop.POCO;
using ErgoShop.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UI;

public class IsInteractibleContent : MonoBehaviour
{
    [SerializeField] GameObject gm;
    FurnitureScript furniture;
    Button btn;

    private WallsCreator Sc_WallsCreator;

    private void Start()
    {
        btn = gameObject.GetComponent<Button>();
        furniture = gm.GetComponent<FurnitureScript>();
        btn.interactable = !furniture.isOnWall;

        Sc_WallsCreator = FindObjectOfType<WallsCreator>();
        if (Sc_WallsCreator == null)
        {
            Debug.LogWarning("IsInteractibleContent 'Sc_WallCreator' is null");
        }
    }

    private void Update()
    {
        if (furniture.isOnWall)
        {
            btn.interactable = Sc_WallsCreator.isOneWall;
        }
    }
}