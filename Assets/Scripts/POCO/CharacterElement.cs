using ErgoShop.Managers;
using ErgoShop.Utils;
using ErgoShop.Utils.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ErgoShop.POCO
{
    /// <summary>
    /// Character is used to check environnemental constraints. Can be a dummy or a wheelchair (or both)
    /// </summary>
    public class CharacterElement : MovableElement
    {
        public CharacterType Type { get; set; }
        /// <summary>
        /// Spread arms (T-Pose) or let them along the body
        /// </summary>
        public bool SpreadArms { get; set; }

        /// <summary>
        /// Instantiate an identical object
        /// </summary>
        /// <returns>The same Element</returns>
        public override Element GetCopy()
        {
            return new CharacterElement
            {
                Type = this.Type,
                SpreadArms = this.SpreadArms,
                Position = this.Position,
                Size = this.Size,
                CanBePutOnFurniture = true,
                Id = this.Id,
                IsLocked = this.IsLocked,
                EulerAngles = this.EulerAngles,
                Rotation = this.Rotation
            };
        }

        /// <summary>
        /// Get a textual description
        /// </summary>
        /// <returns>A string containing the data</returns>
        public override string GetDescription()
        {
            return "Personnage " + (Type == CharacterType.StandUp ? " debout" : (Type == CharacterType.Sitting ? " assis" : " allongé"))
                + (SpreadArms ? " bras écartés\n" : "\n")
                + "Dimensions : "
                + ParsingFunctions.ToStringCentimeters(Size.x)
                + "/" + ParsingFunctions.ToStringCentimeters(Size.y)
                + "/" + ParsingFunctions.ToStringCentimeters(Size.z)
                +"\n";
        }

        /// <summary>
        /// Rebuild 2D and 3D objects from data
        /// </summary>
        public override void RebuildSceneData()
        {
            CharactersCreator.Instance.DestroyGameObject(associated2DObject);
            CharactersCreator.Instance.DestroyGameObject(associated3DObject);
            associated2DObject = CharactersCreator.Instance.CreateCharacter2D(Type, SpreadArms);
            associated3DObject = CharactersCreator.Instance.CreateCharacter3D(Type, SpreadArms);
            associated2DObject.transform.position = VectorFunctions.Switch3D2D(Position);
            associated3DObject.transform.position = Position;
            associated2DObject.transform.localEulerAngles = Vector3.forward * Rotation * -1f;
            associated3DObject.transform.localEulerAngles = Vector3.up * Rotation;

            //if(Type == CharacterType.WheelChairEmpty)
            //{
            //    associated3DObject.transform.localEulerAngles -= Vector3.right * 90;
            //}

            associated2DObject.tag = "Character";
            associated3DObject.tag = "Character";
            associated2DObject.gameObject.SetLayerRecursively((int)ErgoLayers.Top);
            associated3DObject.gameObject.SetLayerRecursively((int)ErgoLayers.ThreeD);

            Vector3 s = Size;
            Vector3 ms = MeshSize;

            associated2DObject.transform.localScale = VectorFunctions.Switch3D2D(
                new Vector3(s.x / ms.x, s.y / ms.y, s.z / ms.z));
            associated3DObject.transform.localScale =
                new Vector3(s.x / ms.x, s.y / ms.y, s.z / ms.z);

        }

    }
}