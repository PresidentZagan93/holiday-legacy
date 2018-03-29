using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu]
    public class AmmoType : ScriptableObject
    {

        public Sprite sprite;
        public int maxAmmo = 200;
    }
}
