using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu]
public class Localization : ScriptableObject {

    [Serializable]
    public class CStringPair
    {
        public string name;
        public string text;
    }

    public Language language = Language.English;

    public List<CStringPair> dict = new List<CStringPair>();
}
