using System;
using System.Collections.Generic;
using ErgoShop.POCO;

namespace ErgoShop.Managers
{
    /// <summary>
    ///     Currently NOT USED
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
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
        }

        public override void DestroyEverything()
        {
            throw new NotImplementedException();
        }

        public override Element CopyPaste(Element elem)
        {
            throw new NotImplementedException();
        }
    }
}