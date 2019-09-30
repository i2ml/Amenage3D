using ErgoShop.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.TwoDScripts
{
    /// <summary>
    /// Scale for the 2D View. Show the size of a square, in meters, according to camera zoom
    /// </summary>
    public class ScaleSpriteScript : MonoBehaviour
    {
        /// <summary>
        /// Camera
        /// </summary>
        public Transform cam2DTransform;
        /// <summary>
        /// Text to show the scaling
        /// </summary>
        public Text myText;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            float cm = Mathf.Round((cam2DTransform.position.z / -10f) * 100f);
            myText.text = (cm / 100f) + (cm < 200 ? " mètre" : " mètres");

            bool enab = GlobalManager.Instance.GetActiveCamera() == GlobalManager.Instance.cam2DTop.GetComponent<Camera>();

            GetComponent<Image>().enabled = enab;
            myText.enabled = enab;
        }
    }
}