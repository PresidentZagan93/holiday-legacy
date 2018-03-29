using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSoundEmitter : MonoBehaviour {

    [System.Serializable]
    public class ObjectSoundEmitterSource
    {
        public string name;
        public AudioSource source;

        public ObjectSoundEmitterSource(string name, AudioSource source)
        {
            this.name = name;
            this.source = source;
        }
    }

    public List<ObjectSoundEmitterSource> emitters = new List<ObjectSoundEmitterSource>();
    public Transform audioRoot;
    int frame;

    private void Awake()
    {
        CheckForRoot();
    }

    private void FixedUpdate()
    {
        frame++;

        if(frame % 5 == 0)
        {
            for (int i = 0; i < emitters.Count; i++)
            {
                if(emitters[i].source.transform.parent != audioRoot)
                {
                    emitters[i].source.transform.SetParent(audioRoot);
                    emitters[i].source.transform.localPosition = Vector3.zero;
                }
            }
        }
    }

    void CheckForRoot()
    {
        if (transform.Find("Audio"))
        {
            audioRoot = transform.Find("Audio");
        }
        if(!audioRoot)
        {
            audioRoot = new GameObject("Audio").transform;
            audioRoot.SetParent(transform);
        }

        audioRoot.localPosition = Vector3.zero;
    }

    public void CreateSource(string emitter, AudioManager.AudioType type)
    {
        for(int i = 0; i < emitters.Count;i++)
        {
            if (emitter == emitters[i].name) return;
        }

        AudioSource source = new GameObject(emitter).AddComponent<AudioSource>();
        source.transform.SetParent(audioRoot);
        source.transform.localPosition = new Vector3(0, 0, CameraManager.Position.z);
        source.dopplerLevel = 0f;
        source.spatialBlend = 0f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.maxDistance = 5000;

        AudioManager.Assign(source, type);

        ObjectSoundEmitterSource newEmitter = new ObjectSoundEmitterSource(emitter, source);
        emitters.Add(newEmitter);

        CheckForRoot();
    }

    public AudioSource GetSource(string emitter)
    {
        if (emitter == "Default" && emitters.Count > 0)
        {
            return emitters[0].source;
        }
        for (int i = 0; i < emitters.Count; i++)
        {
            if (emitter == emitters[i].name) return emitters[i].source;
        }

        return null;
    }

    public void PlaySound(string sound, string emitter)
    {
        PlaySound(ObjectManager.GetAudioClip(sound), emitter);
    }

    public void PlaySoundDelayed(string sound, int delay, string emitter)
    {
        PlaySoundDelayed(ObjectManager.GetAudioClip(sound), delay, emitter);
    }

    public void PlaySound(AudioClip sound, string emitter)
    {
        if (emitter == "Default" && emitters.Count > 0)
        {
            emitters[0].source.PlayOneShot(sound);
            return;
        }
        for (int i = 0; i < emitters.Count; i++)
        {
            if (emitters[i].name == emitter)
            {
                emitters[i].source.PlayOneShot(sound);
            }
        }
    }

    public void PlaySoundDelayed(AudioClip sound, int delay, string emitter)
    {
        if (emitter == "Default" && emitters.Count > 0)
        {
            emitters[0].source.clip = sound;
            emitters[0].source.PlayDelayed(delay);
            return;
        }
        for (int i = 0; i < emitters.Count; i++)
        {
            if (emitters[i].name == emitter)
            {
                emitters[i].source.clip = sound;
                emitters[i].source.PlayDelayed(delay);
            }
        }
    }
}
