using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    /// <summary>
    ///     Filter furnitures by type in the Add Furniture window.
    ///     Seeks gameobject names starting with two underscores
    ///     Those are the categories (assise, cuisine...)
    ///     This script sets those categories to be toggling and showing / hiding furnitures in them
    /// </summary>
    public class FurnitureListContentScript : MonoBehaviour
    {
        private bool hasBeenEnabled;

        // Start is called before the first frame update
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            var nObjects = 0;
            for (var i = 0; i < transform.childCount; i++)
                if (transform.GetChild(i).gameObject.activeInHierarchy)
                    nObjects++;
            GetComponent<RectTransform>().sizeDelta = new Vector2(0, 35 * nObjects);
        }

        private void OnEnable()
        {
            if (!hasBeenEnabled)
            {
                Toggle currentToggle = null;
                for (var i = 0; i < transform.childCount; i++)
                {
                    var go = transform.GetChild(i).gameObject;
                    // Categorie
                    if (go.name.StartsWith("__"))
                    {
                        currentToggle = go.AddComponent<Toggle>();
                    }
                    else
                    {
                        currentToggle.onValueChanged.AddListener(b => { go.SetActive(b); });
                        go.SetActive(false);
                    }
                }

                hasBeenEnabled = true;
            }
        }
    }
}