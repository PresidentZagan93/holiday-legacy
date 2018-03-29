using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ObjectSign : ObjectPickup
{
    public bool isOpen = false;
    public bool inputRequried = false;
    bool lastOpen;
    public string message = "";
    int id;
    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        id = Helper.RandomID;
        spriteRenderer = transform.Find("Sign").GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!inputRequried)
        {
            isOpen = isInRange;
        }
        else
        {
            if(isInRange && !isOpen)
            {
                UIManager.DrawText(id + 1, Vector3.up * 32f, "INTERACT", "arcade", false);
                UIManager.DrawKeybind(id + 2, Vector3.up * 64f - Vector3.up * 12f, "E", "arcade", false);
            }
            else if(!isInRange && isOpen)
            {
                isOpen = false;
                pickedUp = false;
            }
        }

        if (lastOpen != isOpen)
        {
            lastOpen = isOpen;
        }

        if(isOpen)
        {
            UIManager.DrawText(id, Vector3.up * 64f, message, "arcade", false);
        }
        Outline(spriteRenderer, isInRange);
    }

    public override bool CanPickup(CharacterPickupMaster character)
    {
        if(inputRequried)
        {
            return !pickedUp && character.CanPickup && !isOpen;
        }
        else
        {
            return false;
        }
    }

    public override bool DoPickup(CharacterPickupMaster character)
    {
        if(inputRequried)
        {
            isOpen = !isOpen;
        }

        return false;
    }
}