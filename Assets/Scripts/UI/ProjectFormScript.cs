using System;
using ErgoShop.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace ErgoShop.UI
{
    /// <summary>
    ///     UI for project properties like the person name, version of project etc...
    /// </summary>
    public class ProjectFormScript : MonoBehaviour
    {
        public InputField projectField,
            firstNameField,
            lastNameField,
            dateField,
            homeTypeField,
            versionField,
            commentField;

        public static ProjectFormScript Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            if (dateField.text == "") dateField.text = DateTime.Now.ToShortDateString();
        }

        /// <summary>
        ///     Load properties from project data
        /// </summary>
        public void LoadProject()
        {
            projectField.text = ProjectManager.Instance.Project.ProjectName;
            firstNameField.text = ProjectManager.Instance.Project.Person.FirstName;
            lastNameField.text = ProjectManager.Instance.Project.Person.LastName;
            dateField.text = ProjectManager.Instance.Project.Date.ToShortDateString();
            versionField.text = ProjectManager.Instance.Project.Version + "";
            homeTypeField.text = ProjectManager.Instance.Project.HomeType;
            commentField.text = ProjectManager.Instance.Project.Comment;


            FloorPropertiesScript.Instance.SetCurrentFloor(ProjectManager.Instance.Project.CurrentFloorIdx);
        }

        public void SetProjectName(string pn)
        {
            ProjectManager.Instance.Project.ProjectName = pn;
        }

        public void SetHomeType(string ht)
        {
            ProjectManager.Instance.Project.HomeType = ht;
        }

        public void SetFirstName(string firstname)
        {
            ProjectManager.Instance.Project.Person.FirstName = firstname;
        }

        public void SetLastName(string lastname)
        {
            ProjectManager.Instance.Project.Person.LastName = lastname;
        }

        public void SetDate(string date)
        {
            var dt = DateTime.Now;
            try
            {
                dt = DateTime.Parse(date);
            }
            catch (Exception e)
            {
                dt = DateTime.Now;
            }

            ProjectManager.Instance.Project.Date = dt;
            dateField.text = ProjectManager.Instance.Project.Date.ToShortDateString();
        }

        public void SetVersion(string version)
        {
            //if (ParsingFunctions.ParseFloatCommaDot(version, out float res))
            //{
            ProjectManager.Instance.Project.Version = version;
            //}
        }

        public void SetComment(string com)
        {
            ProjectManager.Instance.Project.Comment = com;
        }
    }
}