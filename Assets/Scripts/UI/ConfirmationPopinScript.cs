using ErgoShop.Managers;
using UnityEngine;

namespace ErgoShop.UI
{
    /// <summary>
    ///     Confirmation popin logic when save/load project
    /// </summary>
    public class ConfirmationPopinScript : MonoBehaviour
    {
        public bool wantToQuit, wantToSave, goQuit;

        // Start is called before the first frame update
        private void Start()
        {
            goQuit = false;
            gameObject.SetActive(false);
        }

        // Update is called once per frame
        private void Update()
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

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void CancelQuitting()
        {
            wantToQuit = false;
            wantToSave = false;
            goQuit = false;
            gameObject.SetActive(false);
        }
    }
}