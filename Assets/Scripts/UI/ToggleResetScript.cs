using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    public class ToggleResetScript : MonoBehaviour
    {
        // Start is called before the first frame update
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
        }

        private void OnEnable()
        {
            GetComponent<Toggle>().isOn = false;
        }
    }
}