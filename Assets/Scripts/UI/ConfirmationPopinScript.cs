using ErgoShop.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoShop.UI
{
    /// <summary>
    /// Confirmation popin logic when save/load project
    /// </summary>
    public class ConfirmationPopinScript : MonoBehaviour
    {
        public bool wantToQuit, wantToSave, goQuit;

        // Start is called before the first frame update
        void Start()
        {
            goQuit = false;
            this.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void QuitAndSave()
        {
            wantToQuit = true;
            wantToSave = true;
            if (ProjectManager.Instance.SaveProject())
            {
                goQuit = true;
                Application.Quit();
            }
            else
            {
                CancelQuitting();
            }
        }

        public void QuitWithoutSave()
        {
            wantToQuit = true;
            wantToSave = false;
            goQuit = true;
            Application.Quit();
        }

        public void CancelQuitting()
        {
            wantToQuit = false;
            wantToSave = false;
            goQuit = false;
            this.gameObject.SetActive(false);
        }
    }
}