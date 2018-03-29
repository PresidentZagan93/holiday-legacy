using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu]
    public class AudioSet : ScriptableObject
    {

        [System.Serializable]
        public struct AudioClipPair
        {
            public string name;
            public AudioClip clip;

            public AudioClipPair(string name)
            {
                this.name = name;
                this.clip = null;
            }
        }

        public List<AudioClipPair> audioClips = new List<AudioClipPair>();

        public AudioClip GetClip(string name)
        {
            for (int i = 0; i < audioClips.Count; i++)
            {
                if (audioClips[i].name == name) return audioClips[i].clip;
            }
            return null;
        }
    }
}