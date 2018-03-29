using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectButton : ObjectPickup
{
    public AudioClip buttonPressSound;
    public AudioClip resetSound;

    public bool allowMultiplePresses = true;
    public float deactivateTimer = 2f;
    public bool buttonState = false;
    float nextButton;
    bool lastState;
    public string message = "";
    int id;

    AudioSource source;
    SpritePlayer spritePlayer;
    Action method;
    public SpriteRenderer glow;

    private void Awake()
    {
        id = Helper.RandomID;
        spritePlayer = GetComponent<SpritePlayer>();
        source = GetComponent<AudioSource>();
        AudioManager.Assign(source, AudioManager.AudioType.Pickups);
    }

    public void Initialize(Action method)
    {
        this.method = method;
    }

    private void Update()
    {
        if(isInRange && !buttonState)
        {
            UIManager.DrawText(id + 1, transform.position + Vector3.up * 12f, message.ToUpper());
            UIManager.DrawKeybind(id + 2, transform.position + Vector3.up * 24, "E");
        }

        if (buttonState)
        {
            if (allowMultiplePresses)
            {
                if (Time.time > nextButton)
                {
                    source.clip = resetSound;
                    source.Play();

                    buttonState = false;
                    pickedUp = false;
                }
            }
        }

        if (lastState != buttonState)
        {
            lastState = buttonState;
            spritePlayer.Play(buttonState ? "ButtonOn" : "ButtonOff");
        }

        glow.color = buttonState ? Color.green : Color.red;
        Outline(spritePlayer.spriteRenderer, isInRange);
    }

    public override bool CanPickup(CharacterPickupMaster character)
    {
        return !pickedUp && character.CanPickup && !buttonState;
    }

    public override bool DoPickup(CharacterPickupMaster character)
    {
        if (!buttonState)
        {
            source.clip = buttonPressSound;
            source.Play();

            if (method != null)
            {
                method.Invoke();
            }

            buttonState = true;
            nextButton = Time.time + deactivateTimer;
        }

        return false;
    }
}