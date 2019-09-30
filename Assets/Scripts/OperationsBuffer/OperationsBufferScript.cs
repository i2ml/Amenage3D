using ErgoShop.Managers;
using ErgoShop.POCO;
using ErgoShop.UI;
using ErgoShop.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Could be in Managers namespace ?
namespace ErgoShop.Operations
{
    /// <summary>
    /// Autosave poco    
    /// </summary>
    public class AutoSave
    {
        /// <summary>
        /// When
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// path to json file
        /// </summary>
        public string ProjectPath { get; set; }
        /// <summary>
        /// What was the last operation ?
        /// </summary>
        public string OperationName { get; set; }
    }

    /// <summary>
    /// Cancel / Redo system
    /// </summary>
    public class OperationsBufferScript : MonoBehaviour
    {
        /// <summary>
        /// How many operations do we store
        /// </summary>
        private const int CANCEL_LIMIT_COUNT = 50;

        public Button cancelButton, redoButton;

        /// <summary>
        /// auto save + navigation between projects
        /// </summary>
        private List<AutoSave> m_projects;
        
        /// <summary>
        /// Index for current autosave state
        /// </summary>
        private int m_idx;

        private bool goAuto;

        public static OperationsBufferScript Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Get all autosave data
        /// </summary>
        /// <returns></returns>
        public List<AutoSave> GetOperations()
        {
            return m_projects;
        }

        // Start is called before the first frame update
        void Start()
        {
            //operations = new List<Operation>();
            m_projects = new List<AutoSave>();
            goAuto = true;
        }

        // Update is called once per frame
        void Update()
        {
            if(Time.time > 1 && goAuto)
            {
                goAuto = false;
                AddAutoSave("<Début>");
            }
            cancelButton.interactable = m_projects.Count > 0 == m_idx > 0;
            redoButton.interactable = m_projects.Count > 0 && m_idx < m_projects.Count - 1;
            if (InputFunctions.CTRL())
            {
                // CANCEL
                if (Input.GetKeyDown(KeyCode.Z))
                {
                    Cancel();
                }
                // REDO
                else if(Input.GetKeyDown(KeyCode.Y))
                {
                    Redo();
                }
                //if(operations.Count > 0)
                //{
                //    Operation currentOperation = operations.Last();
                //    currentOperation.Cancel();
                //    operations.Remove(currentOperation);
                //}
            }
        }

        /// <summary>
        ///  Cancel last action by loading the previous autosave
        /// </summary>
        public void Cancel()
        {
            if (m_idx > 0)
            {
                ProjectManager.Instance.LoadAutoSave(m_projects[m_idx]);
                m_idx--;
            }
            OperationsScrollScript.Instance.SetCurrentColor(m_idx);
        }

        /// <summary>
        /// Redo last action by loading the next autosave
        /// </summary>
        public void Redo()
        {
            if (m_idx < m_projects.Count - 1)
            {
                m_idx++;
                ProjectManager.Instance.LoadAutoSave(m_projects[m_idx]);
            }
            OperationsScrollScript.Instance.SetCurrentColor(m_idx);
        }

        /// <summary>
        /// Go to specific autosave
        /// </summary>
        /// <param name="auto"></param>
        public void GoToOperation(AutoSave auto)
        {
            m_idx = m_projects.IndexOf(auto);
            ProjectManager.Instance.LoadAutoSave(auto);
        }

        /// <summary>
        /// Clear list
        /// </summary>
        public void ClearAutoSave()
        {
            m_projects.Clear();
            AddAutoSave("<Vide>");
        }

        /// <summary>
        /// Get json file content
        /// </summary>
        /// <returns></returns>
        public string GetLastAutoSaveContent()
        {
            if (m_projects.Count > 0)
            {
                return File.ReadAllText(Path.Combine(ProjectManager.Instance.GetAutoSaveFolderPath(), m_projects.Last().ProjectPath));
            }
            return "";
        }

        /// <summary>
        /// Create new autosave
        /// This method is called by lots of scripts in the software
        /// All actions modifying the project will call this method after completing their function
        /// </summary>
        /// <param name="operationName">What is the operation the user just did ? (create wall, room, update a propertie etc)</param>
        public void AddAutoSave(string operationName)
        {
            DeleteAllAutoSavesAfterIdx();
            AutoSave auto = ProjectManager.Instance.SaveCurrentFileAsAutoSave();
            if (auto != null)
            {
                auto.OperationName = operationName;
                string lastJsonString = "";
                string autoJsonString = "";
                if (m_projects.Count > 0) {
                    lastJsonString = File.ReadAllText(Path.Combine(ProjectManager.Instance.GetAutoSaveFolderPath(), m_projects.Last().ProjectPath));
                }
                if (!string.IsNullOrEmpty(lastJsonString))
                {
                    autoJsonString = File.ReadAllText(Path.Combine(ProjectManager.Instance.GetAutoSaveFolderPath(), auto.ProjectPath));
                }

                if (autoJsonString != lastJsonString || lastJsonString == "")
                {
                    m_projects.Add(auto);
                    m_idx = m_projects.Count - 1;

                    if (m_projects.Count > CANCEL_LIMIT_COUNT)
                    {
                        m_projects.Remove(m_projects[0]);
                    }
                }                
            }

            OperationsScrollScript.Instance.UpdateList();
        }  

        /// <summary>
        /// Clean all "redo states" (because a new operation is being saved)
        /// </summary>
        private void DeleteAllAutoSavesAfterIdx()
        {
            List<AutoSave> newList = new List<AutoSave>();
            for (int i = 0; i <= Mathf.Min(m_projects.Count-1, m_idx); i++)
            {                
                newList.Add(m_projects[i]);
            }
            m_projects.Clear();
            m_projects.AddRange(newList);
        }
    }
}