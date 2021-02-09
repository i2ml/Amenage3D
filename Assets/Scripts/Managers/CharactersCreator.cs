using System.Collections.Generic;
using System.Linq;
using ErgoShop.Operations;
using ErgoShop.POCO;
using ErgoShop.Utils;
using UnityEngine;

namespace ErgoShop.Managers
{
    /// <summary>
    ///     Manager to create characters (with or without wheelchair)
    /// </summary>
    public class CharactersCreator : CreatorBehaviour
    {
        // 3D
        public GameObject standingPrefab,
            standingSpreadPrefab,
            sittingPrefab,
            sittingSpreadPrefab,
            lyingDownPrefab,
            lyingDownSpreadPrefab;

        public GameObject wheelChair3DPrefab, wheelChairWithPerson3DPrefab;

        // 2D
        public GameObject standing2DPrefab,
            standingSpread2DPrefab,
            sitting2DPrefab,
            sittingSpread2DPrefab,
            lyingDown2DPrefab,
            lyingDownSpread2DPrefab;

        public GameObject wheelChair2DPrefab, wheelChairWithPerson2DPrefab;

        private List<CharacterElement> m_characters;

        /// <summary>
        ///     Instance
        /// </summary>
        public static CharactersCreator Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        private void Start()
        {
            m_characters = new List<CharacterElement>();
        }

        // Update is called once per frame
        private void Update()
        {
            foreach (var c in m_characters)
                c.associated2DObject.transform.position = new Vector3
                (
                    c.associated2DObject.transform.position.x,
                    c.associated2DObject.transform.position.y,
                    -0.05f
                );
        }

        /// <summary>
        ///     Destroy every element (gameobjects and data)
        /// </summary>
        public override void DestroyEverything()
        {
            while (m_characters.Count > 0) DestroyCharacter(m_characters.First());
            m_characters = new List<CharacterElement>();
        }

        public List<CharacterElement> GetCharacters()
        {
            return m_characters;
        }

        /// <summary>
        ///     Load CharacterElement from floor data
        /// </summary>
        /// <param name="floor">Floor</param>
        public void LoadCharactersFromFloor(Floor floor)
        {
            while (m_characters.Count > 0) DestroyCharacter(m_characters.First());
            m_characters = new List<CharacterElement>();
            foreach (var ch in floor.Characters) m_characters.Add(ch);
            foreach (var ch in m_characters) ch.RebuildSceneData();
        }

        /// <summary>
        ///     Create new character and add it to scene
        /// </summary>
        public void CreateNewCharacter()
        {
            var charac = new CharacterElement
            {
                Type = CharacterType.StandUp,
                SpreadArms = true,
                Size = new Vector3(1.76f, 1.77f, 0.32f)
            };
            m_characters.Add(charac);
            charac.RebuildSceneData();
            SelectedObjectManager.Instance.Select(charac);
            OperationsBufferScript.Instance.AddAutoSave("Création personnage");
        }

        /// <summary>
        ///     Instantiate the good prefab mesh according to CharacterType
        /// </summary>
        /// <param name="type">Enum containing 5 types of characters, with or without wheelchair and differents positions</param>
        /// <param name="spread"></param>
        /// <returns></returns>
        public GameObject CreateCharacter3D(CharacterType type, bool spread)
        {
            switch (type)
            {
                case CharacterType.StandUp:
                    return Instantiate(spread ? standingSpreadPrefab : standingPrefab);
                case CharacterType.Sitting:
                    return Instantiate(spread ? sittingSpreadPrefab : sittingPrefab);
                case CharacterType.LyingDown:
                    return Instantiate(spread ? lyingDownSpreadPrefab : lyingDownPrefab);
                case CharacterType.WheelChairEmpty:
                    return Instantiate(wheelChair3DPrefab);
                case CharacterType.OnWheelChair:
                    return Instantiate(wheelChairWithPerson3DPrefab);
            }

            return null;
        }

        /// <summary>
        ///     Destroy an associated gameobject 2d 3d
        /// </summary>
        /// <param name="go">any</param>
        public void DestroyGameObject(GameObject go)
        {
            Destroy(go);
        }

        /// <summary>
        ///     Create the sprite according to the type
        /// </summary>
        /// <param name="type">char type</param>
        /// <param name="spread">spread arms or not</param>
        /// <returns>gameobject containing sprite for 2d view</returns>
        public GameObject CreateCharacter2D(CharacterType type, bool spread)
        {
            switch (type)
            {
                case CharacterType.StandUp:
                    return Instantiate(spread ? standingSpread2DPrefab : standing2DPrefab);
                case CharacterType.Sitting:
                    return Instantiate(spread ? sittingSpread2DPrefab : sitting2DPrefab);
                case CharacterType.LyingDown:
                    return Instantiate(spread ? lyingDownSpread2DPrefab : lyingDown2DPrefab);
                case CharacterType.WheelChairEmpty:
                    return Instantiate(wheelChair2DPrefab);
                case CharacterType.OnWheelChair:
                    return Instantiate(wheelChairWithPerson2DPrefab);
            }

            return null;
        }

        /// <summary>
        ///     Seek all associated objects in characeters to find the CharacterElement object concerned
        /// </summary>
        /// <param name="go">Associated gameobject, can be 2D or 3D</param>
        /// <returns>The CharacterElement data or null if not found</returns>
        public CharacterElement GetCharacterFromGameObject(GameObject go)
        {
            foreach (var ch in m_characters)
                if (ch.associated2DObject == go || ch.associated3DObject == go)
                    return ch;
            return null;
        }

        /// <summary>
        ///     Destroy data and gamobjects from a character
        /// </summary>
        /// <param name="h">CharacterElement data</param>
        public void DestroyCharacter(CharacterElement h)
        {
            m_characters.Remove(h);
            Destroy(h.associated2DObject);
            Destroy(h.associated3DObject);
            h = null;
        }

        /// <summary>
        ///     Paste a copied CharacterElement by getting a copy and instantiate it, and rebuilding gameobjects
        /// </summary>
        /// <param name="m_copiedElement">Copied CharacterElement</param>
        /// <returns>The new CharacterElement, identical to the copied one</returns>
        public override Element CopyPaste(Element elem)
        {
            var ch = elem as CharacterElement;
            var newCh = ch.GetCopy() as CharacterElement;
            m_characters.Add(newCh);
            newCh.RebuildSceneData();
            return newCh;
        }
    }
}