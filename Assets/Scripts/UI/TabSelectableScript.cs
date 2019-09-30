using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    /// <summary>
    /// Script to catch any Tabulation to move between UI Elements
    /// </summary>
    public class TabSelectableScript : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    if (EventSystem.current.currentSelectedGameObject != null)
                    {
                        Selectable selectable = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnUp();
                        if (selectable != null)
                            selectable.Select();
                    }
                }
                else
                {
                    if (EventSystem.current.currentSelectedGameObject != null)
                    {
                        Selectable selectable = EventSystem.current.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
                        if (selectable != null)
                            selectable.Select();
                    }
                }
            }
        }
    }
}