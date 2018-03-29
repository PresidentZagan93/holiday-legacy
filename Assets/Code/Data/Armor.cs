using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Data
{
    [CreateAssetMenu]
    public class Armor : ScriptableObject, IItem
    {
        public enum ArmorOrder
        {
            Front,
            Back
        }

        public enum ArmorType
        {
            Head,
            Chest,
            Feet,
            Arms,
            Shield
        }

        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                if (!lockId)
                {
                    id = value;
                }
            }
        }

        public int id;
        public bool lockId = false;
        
        public bool ready;
        public int price = 50;
        public Sprite icon;
        public Sprite shieldDisabledIcon;
        public string description;
        public ArmorType type;
        public ArmorOrder order;
        public bool visible = true;
        public Vector2 position;
        public Vector2 shieldIdlePosition;
        public int rotation;
        public AudioSet audio;
        public List<Quality> qualities = new List<Quality>();

        [Serializable]
        public struct Chance
        {
            public string name;
            [Range(0, 100)]
            public int chance;

            public Chance(string name, int chance)
            {
                this.name = name;
                this.chance = chance;
            }
        }

        public Quality GetQuality(string name)
        {
            for (int i = 0; i < qualities.Count; i++)
            {
                if (qualities[i].name == name) return qualities[i];
            }

            return Quality.Invalid;
        }

        public int GetID()
        {
            return id;
        }
        
        public string StatDescription
        {
            get
            {
                string txt = "";
                for (int i = 0; i < qualities.Count; i++)
                {
                    string friendlyName = qualities[i].FriendlyName;
                    if (i == qualities.Count - 1)
                    {
                        txt += friendlyName;
                    }
                    else
                    {
                        txt += friendlyName + "\n";
                    }
                }
                if (txt != "") txt = "\n" + txt;

                return txt;
            }
        }

        public string GetDescription()
        {
            return description + "\n" + StatDescription;
        }

        public Sprite GetIcon()
        {
            return icon;
        }

        public int GetPrice()
        {
            return price;
        }

        public string GetItemType()
        {
            return type.ToString();
        }

        public string GetName()
        {
            return name;
        }

        public AudioSet GetAudio()
        {
            return audio;
        }
    }
}