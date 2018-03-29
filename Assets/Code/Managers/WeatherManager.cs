using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour {
    
    [System.Serializable]
    public class WeatherBind
    {
        [Tileset]
        public string tileset;
        public Transform weatherRoot;

        [HideInInspector]
        public ParticleSystem[] particleSystems;
    }

    public List<WeatherBind> weathers = new List<WeatherBind>();
    string tileset;
    CameraManager cameraManager;

    private void Awake()
    {
        cameraManager = FindObjectOfType<CameraManager>();
        for (int i = 0; i < weathers.Count; i++)
        {
            weathers[i].particleSystems = weathers[i].weatherRoot.GetComponentsInChildren<ParticleSystem>();
            SetParticleSystem(i, false, true);
        }
    }

    private void Update()
    {
        if(cameraManager.target)
        {
            transform.position = cameraManager.target.transform.position;
        }

        if(tileset != GeneratorManager.Tileset)
        {
            tileset = GeneratorManager.Tileset;

            UpdateWeather();
        }
    }

    void SetParticleSystem(int index, bool state, bool force)
    {
        ParticleSystem.EmissionModule emitModule = new ParticleSystem.EmissionModule();
        for (int i = 0; i < weathers[index].particleSystems.Length; i++)
        {
            emitModule = weathers[index].particleSystems[i].emission;
            emitModule.enabled = state;

            if(force)
            {
                weathers[index].particleSystems[i].Clear(true);
            }
        }
    }

    void UpdateWeather()
    {
        for (int i = 0; i < weathers.Count; i++)
        {
            SetParticleSystem(i, weathers[i].tileset == tileset, false);
        }
    }
}
