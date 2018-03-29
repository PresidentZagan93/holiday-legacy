using UnityEngine;
using System;

namespace Data
{
    [CreateAssetMenu]
    public class PlayerBuild : ScriptableObject, IItem
    {
        [Serializable]
        public class PlayerBuildChance
        {
            public bool goForAmmo = true;
            [AmmoType]
            public string ammoType;
            public GunType gunType;
            [Range(0, 100)]
            public int chance = 100;
        }

        public bool editorOnly;
        public Sprite icon;
        [TextArea]
        public string description = "";
        public Gun[] startingWeapons;
        public Armor[] startingArmor;
        public PlayerBuildChance[] chances;

        public int maxHealth = 6;
        public int startingAmmo = 50;

        public string GetName()
        {
            return name;
        }

        public string GetDescription()
        {
            return description;
        }

        public int GetPrice()
        {
            return 0;
        }

        public Sprite GetIcon()
        {
            return icon;
        }

        public string GetItemType()
        {
            return "Build";
        }

        public int GetID()
        {
            return 0;
        }

        public AudioSet GetAudio()
        {
            throw new NotImplementedException();
        }
    }
}