using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Linq;

namespace Data
{
    [System.Serializable]
    [CreateAssetMenu]
    public class Tip : ScriptableObject
    {

        [System.Serializable]
        public class TipTypeColor
        {
            public TipReference.TipReferenceType type = TipReference.TipReferenceType.Item;
            public Color color = Color.white;
        }

        [System.Serializable]
        public class TipReference
        {
            public enum TipReferenceType
            {
                Armor,
                Build,
                Tileset,
                Enemy,
                Gun,
                Item,
                Consumable,
                Setting
            }

            public TipReferenceType type = TipReferenceType.Item;

            public Gun gunValue;
            public Armor armorValue;
            public Item itemValue;
            public Consumable consumableValue;
            public PlayerBuild buildValue;
            [ConfigSetting]
            public string settingValue;
            [Enemy]
            public string enemyValue;
            [Tileset]
            public string tilesetValue;

            public string Value
            {
                get
                {
                    GameManager gameManager = FindObjectOfType<GameManager>();

                    string newColor = "pink";
                    for (int i = 0; i < gameManager.tipTypeColors.Count; i++)
                    {
                        if (gameManager.tipTypeColors[i].type == type) newColor = gameManager.tipTypeColors[i].color.ToHex();
                    }
                    string toReturn = "<color=" + newColor + ">";

                    if (type == TipReferenceType.Armor && armorValue) toReturn += armorValue.name;
                    if (type == TipReferenceType.Build && buildValue) toReturn += buildValue.name;
                    if (type == TipReferenceType.Tileset) toReturn += tilesetValue;
                    if (type == TipReferenceType.Enemy) toReturn += enemyValue;
                    if (type == TipReferenceType.Gun && gunValue) toReturn += gunValue.name;
                    if (type == TipReferenceType.Item && itemValue) toReturn += itemValue.name;
                    if (type == TipReferenceType.Consumable && consumableValue) toReturn += consumableValue.name;
                    if (type == TipReferenceType.Setting)
                    {
                        if (!settingValue.Contains('+')) settingValue = "+";

                        object val = ConfigManager.GetValue(settingValue.Split('+')[0], settingValue.Split('+')[1]);
                        toReturn += val ?? "null";
                    }

                    return toReturn + "</color>";
                }
            }
        }

        public string tip;
        public List<TipReference> references = new List<TipReference>();

        public List<Armor> specificArmor = new List<Armor>();
        public List<Item> specificItems = new List<Item>();
        public List<Consumable> specificConsumables = new List<Consumable>();
        public List<Gun> specificGuns = new List<Gun>();
        [Level]
        public List<string> specificLevels = new List<string>();
        [Tileset]
        public List<string> specificTilesets = new List<string>();

        public string TipText
        {
            get
            {
                string toReturn = tip;
                ICollection<string> matches = Regex.Matches(toReturn.Replace(System.Environment.NewLine, ""), @"\[([^]]*)\]").Cast<Match>().Select(x => x.Groups[1].Value).ToList();

                foreach (string match in matches)
                {
                    int index = int.Parse(match);
                    if (references.Count > index)
                    {
                        string val = references[index].Value;
                        toReturn = toReturn.Replace("[" + match + "]", val);
                    }
                }

                return toReturn;
            }
        }
    }
}