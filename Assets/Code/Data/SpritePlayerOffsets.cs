using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu]
public class SpritePlayerOffsets : ScriptableObject {

    [Serializable]
    public class SpritePlayerOffsetPair
    {
        public string name;
        public List<Vector2> frames = new List<Vector2>();
    }

    public List<SpritePlayerOffsetPair> offsets = new List<SpritePlayerOffsetPair>();
}
