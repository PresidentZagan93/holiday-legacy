using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDFadeOut : MonoBehaviour {

    public float delay = 1.5f;
    public float timeToFade = 1f;
    Image image;
    public Gradient gradient;

    float nextFade;
    float nextTimeToFade;

    private void Awake()
    {
        image = GetComponent<Image>();
        nextFade = Time.time + delay;
        nextTimeToFade = Time.time + delay + timeToFade;
    }

    void Update()
    {
        if(!image.enabled)
        {
            nextFade += Time.deltaTime;
            nextTimeToFade += Time.deltaTime;
            return;
        }
        if(Time.time > nextFade)
        {
            float t = (Time.time - nextTimeToFade) / timeToFade;
            image.color = gradient.Evaluate(t);
        }
    }
}
