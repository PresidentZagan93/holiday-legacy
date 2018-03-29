using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct Quality
{
    public string name;
    public string value;

    public int Int
    {
        get
        {
            return value.ToInt();
        }
    }

    public float Float
    {
        get
        {
            return value.ToFloat();
        }
    }

    public bool Bool
    {
        get
        {
            return value.ToLower() == "true";
        }
    }

    public bool Valid
    {
        get
        {
            return name != null && value != null;
        }
    }

    public static Quality Invalid
    {
        get
        {
            return new Quality(null, null);
        }
    }

    public Quality(string name, string value)
    {
        this.name = name;
        this.value = value;
    }

    public string FriendlyName
    {
        get
        {
            if (name == "PlayerSpeedResistance") return value == "1" ? "Resistance to all movement debuffs" : "";
            if (name == "ProjectileSpeed") return value.ToFloat() < 0f ? "Projectiles fire backwards" : "Projectiles fire faster by x" + value;
            if (name == "PlayerAcceleration") return "Player movement acceleration increase by x" + value;
            if (name == "PlayerSpeed")
            {
                float perc = value.ToFloat();
                return perc > 1f ? "Player movement speed increased by x" + value : "Player movement speed decreased by x" + value;
            }
            if (name == "ArmorBlock") return value+"% chance to block damage";
            if (name == "ArmorReflectDamage") return "Chance to reflect and deal damage is " + value + "%";
            if (name == "IgnoreCombust") return "Fire immunity";
            if (name == "IgnoreExplosion") return "Explosion immunity";
            if (name == "EffectCombust") return "Sets targets on fire";
            if (name == "EffectFreeze") return "Freezes targets";
            if (name == "EffectDamage") return "";

            if (name == "PlayerContactDamage" && value == "true") return "Player can deal damage";
            if (name == "ArmorBlockContact") return value != "100" ? value +"% chance to block contact damage" : "Blocks all contact damage";
            if (name == "ArmorBlockMelee") return value != "100" ? value + "% chance to block melee" : "Blocks all melee damage";

            return name + " " + value;
        }
    }

#if UNITY_EDITOR

    public static List<string> possibleQualities = new List<string>();

    public static void ShowInspector(ref List<Quality> qualities)
    {
        EditorGUILayout.LabelField("Qualities");
        int selected = -1;
        int newSelected = GUILayout.Toolbar(selected, new string[] { "Add", "Delete" });

        if(newSelected != -1)
        {
            if(newSelected == 0)
            {
                if (qualities.Count > 0)
                {
                    qualities.Add(qualities[qualities.Count - 1]);
                }
                else
                {
                    qualities.Add(new Quality("Name", "Value"));
                }
            }
            else
            {
                if (qualities.Count > 0)
                {
                    qualities.RemoveAt(qualities.Count - 1);
                }
            }
        }

        for (int i = 0; i < qualities.Count; i++)
        {
            Quality quality = qualities[i];
            
            int selectedIndex = 0;
            for (int m = 0; m < possibleQualities.Count; m++)
            {
                if (possibleQualities[m] == qualities[i].name) selectedIndex = m;
            }

            int newIndex = EditorGUILayout.Popup("    Quality", selectedIndex, possibleQualities.ToArray());
            quality.name = possibleQualities[newIndex];

            quality.value = EditorGUILayout.TextField("        Value", quality.value);

            qualities[i] = quality;
        }

        GUI.changed = true;
    }

#endif
}

namespace Data
{
    [CreateAssetMenu]
    public class Consumable : ScriptableObject, IItem
    {
        public enum ConsumableType
        {
            Powerup,
            Companion
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

        public bool ready = true;
        public int price = 50;
        public bool clearOnFinish = true;

        public ConsumableType type = ConsumableType.Powerup;
        [TextArea]
        public string description = "An item";
        public float duration = 4f;
        public bool forever = false;
        public bool extendOnKill;
        public float extendOnKillAmount = 2f;
        public Sprite icon;
        public AudioSet audio;

        public List<Quality> qualities = new List<Quality>();

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
            return description;
        }

        public Sprite GetIcon()
        {
            return icon;
        }

        public string GetItemType()
        {
            return "Consumable";
        }

        public AudioSet GetAudio()
        {
            return audio;
        }
    }
}