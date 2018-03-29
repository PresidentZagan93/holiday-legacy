using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu]
    public class Effect : ScriptableObject
    {
        public float duration = 2f;
        public int ticks = 2;
        public float offset = 0f;

        public bool explode = false;
        public float explosionRange = 64f;

        public string effectVisual = "";
        public EffectType effect;

        public enum EffectType
        {
            Heal,
            Damage,
            Speed,
            Acceleration,
            FireRate,
            Freeze,
            Combust
        }
    }
}