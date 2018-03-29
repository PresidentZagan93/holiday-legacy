using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRubberBand : MonoBehaviour {

    ParticleSystem trail;
    Color rubberBandColor;
    SpritePlayer spritePlayer;
    public Color[] availableColors;
    
    private void OnEnable()
    {
        rubberBandColor = availableColors[Random.Range(0, availableColors.Length)];
        ParticleSystem.MainModule main = trail.main;
        main.startColor = rubberBandColor;
        spritePlayer.Play("RubberBand");
        if(spritePlayer.spriteRenderer) spritePlayer.spriteRenderer.color = rubberBandColor;
    }

    private void Awake()
    {
        spritePlayer = GetComponentInChildren<SpritePlayer>();
        trail = transform.Find("Trail").GetComponent<ParticleSystem>();
    }

}
