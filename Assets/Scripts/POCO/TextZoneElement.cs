using ErgoShop.Managers;
using ErgoShop.UI;
using ErgoShop.Utils;
using UnityEngine;

namespace ErgoShop.POCO
{
    /// <summary>
    ///     Comment zone in both 2d and 3d view
    /// </summary>
    public class TextZoneElement : HelperElement
    {
        /// <summary>
        ///     Text in the comment box
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        ///     Size of the text
        /// </summary>
        public float TextSize { get; set; }

        /// <summary>
        ///     text color
        /// </summary>
        public Color TextColor { get; set; }

        /// <summary>
        ///     Box bg color
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        ///     Instantiate an identical object
        /// </summary>
        /// <returns>The same Element</returns>
        public override Element GetCopy()
        {
            return new TextZoneElement
            {
                Id = Id,
                BackgroundColor = BackgroundColor,
                Text = Text,
                TextColor = TextColor,
                TextSize = TextSize,
                Size = Size,
            };
        }

        /// <summary>
        ///     Get a textual description
        /// </summary>
        /// <returns>A string containing the data</returns>
        public override string GetDescription()
        {
            return "Zone de Texte : \n" + Text + "\n";
        }

        /// <summary>
        ///     Rebuild 2D and 3D objects from data
        /// </summary>
        public override void RebuildSceneData()
        {
            HelpersCreator.Instance.RebuildTextZone(this);
        }

        /// <summary>
        ///     Set element size, updates 2d and 3d objects
        /// </summary>
        /// <param name="newSize">Wanted size</param>
        public override void SetSize(Vector3 newSize)
        {
            Size = new Vector3
            {
                x = Mathf.Abs(Size.x),
                y = Mathf.Abs(Size.y),
                z = Mathf.Abs(Size.z)
            };
            Size = newSize;
            associated2DObject.GetComponent<TextZoneScript>().bg.size = VectorFunctions.Switch3D2D(Size);
            associated3DObject.GetComponent<TextZoneScript>().bg.size = VectorFunctions.Switch3D2D(Size);
        }
    }
}