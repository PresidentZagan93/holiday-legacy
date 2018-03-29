using UnityEngine;
using System.Collections.Generic;
using System;

namespace Data
{
    [CreateAssetMenu]
    public class Item : ScriptableObject, IItem
    {
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

        public bool ready = true;
        public int price = 50;
        public int maxStacks = 1;
        [TextArea]
        public string description = "An item";
        public Sprite icon;
        public Item[] requirements;
        public AudioSet audio;
        public Consumable.ConsumableType type = Consumable.ConsumableType.Powerup;

        public List<Quality> qualities = new List<Quality>();

        public string Requirements
        {
            get
            {
                if (requirements.Length == 0) return "";

                string req = "Requires ";

                for (int i = 0; i < requirements.Length; i++)
                {
                    if (i == requirements.Length - 2)
                    {
                        req += requirements[i].name + " and ";
                    }
                    else if (i == requirements.Length - 1)
                    {
                        req += requirements[i].name;
                    }
                    else
                    {
                        req += requirements[i].name + ", ";
                    }
                }

                return req;
            }
        }

        public int GetID()
        {
            return id;
        }

        public string GetName()
        {
            return name;
        }

        public int GetPrice()
        {
            return price;
        }

        public string GetDescription()
        {
            string grants = "";

            for (int i = 0; i < ItemManager.AllItems.Count; i++)
            {
                for (int r = 0; r < ItemManager.AllItems[i].requirements.Length; r++)
                {
                    if (ItemManager.AllItems[i].requirements[r] == this)
                    {
                        if (grants == "")
                        {
                            grants = "\n\nGRANTS ";
                        }
                        grants += ItemManager.AllItems[i].name + ", ";
                    }
                }
            }

            if (grants.EndsWith(", "))
            {
                grants = grants.Substring(0, grants.Length - 2);
            }

            if (Requirements == "" && grants == "") return description;
            if (Requirements == "")
            {
                return description + "<color=orange>" + Requirements + "</color><color=lime>" + grants + "</color>";
            }
            else
            {
                return description + "\n\n<color=orange>" + Requirements + "</color><color=lime>" + grants + "</color>";
            }
        }

        public Sprite GetIcon()
        {
            return icon;
        }

        public string GetItemType()
        {
            return "Item";
        }

        public AudioSet GetAudio()
        {
            return audio;
        }
    }
}
