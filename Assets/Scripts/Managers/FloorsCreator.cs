using ErgoShop.POCO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoShop.Managers
{
    /// <summary>
    /// Currently NOT USED
    /// </summary>
    public class FloorsCreator : CreatorBehaviour
    {
        private List<Floor> m_floorsData;
        public Floor CurrentFloor { get; set; }
        public static FloorsCreator Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public override void DestroyEverything()
        {
            throw new System.NotImplementedException();
        }

        public override Element CopyPaste(Element elem)
        {
            throw new System.NotImplementedException();
        }
    }
}