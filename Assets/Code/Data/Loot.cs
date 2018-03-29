using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Data
{
    [CreateAssetMenu]
    public class Loot : ScriptableObject
    {
        [Serializable]
        public class LootProp
        {
            public IntRange amount = new IntRange(1, 1);
            public int chance = 8;
            public GameObject prop;
        }

        public List<LootProp> loot = new List<LootProp>();
        public List<Loot> extra = new List<Loot>();
    }
}