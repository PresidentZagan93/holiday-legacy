using UnityEngine;
using System.Collections;

public class SelfDestroy : MonoBehaviour {

    ParticleSystem part;
    
    void Awake()
    {
        part = GetComponent<ParticleSystem>();
    }
    void Update()
    {
        if(!part.isPlaying && PoolManager.singleton)
        {
            PoolManager.PoolDestroy(gameObject);
        }
    }
}
