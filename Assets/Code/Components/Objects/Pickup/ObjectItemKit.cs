using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;

public enum CantPickupReason
{
    Price,
    Requirements,
    PriceAndRequirements,
    AlreadyOwns
}

public class ObjectItemKit : ObjectKit
{
    SpriteRenderer spriteRenderer;
    SpriteRenderer shadowRenderer;
    public bool destroyOnPickup = false;
    ObjectSoundEmitter sound;

    private void Awake()
    {
        sound = gameObject.AddComponent<ObjectSoundEmitter>();
        sound.CreateSource("Item", AudioManager.AudioType.Pickups);
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        shadowRenderer = transform.Find("Shadow").GetComponent<SpriteRenderer>();

        Initialize();
    }

    public void Initialize()
    {
        List<Item> items = ItemManager.singleton.items;
        List<Item> validItems = new List<Item>();
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].ready)
            {
                validItems.Add(items[i]);
            }
        }

        IItem item = validItems[Random.Range(0,validItems.Count)];
        Initialize(item);
    }

    private void Update()
    {
        if (isInRange)
        {
            ShowPreview();
        }
        if (Item != null && spriteRenderer.sprite != Item.GetIcon())
        {
            spriteRenderer.sprite = Item.GetIcon();
        }
        shadowRenderer.sprite = spriteRenderer.sprite;
        roundOffset = new Vector2(spriteRenderer.sprite.rect.width % 2 == 0f ? 0f : 0.5f, spriteRenderer.sprite.rect.width % 2 == 0f ? 0f : 0.5f);
        Outline(spriteRenderer, isInRange);
    }

    public override bool CanPickup(CharacterPickupMaster character)
    {
        if (!character.CanPickup) return false;
        if (!isInRange) return false;

        bool hasRequirements = character.Inventory.HasItemRequirements(Item.GetName());
        if (character.Character.Gold < Price && character.CanPickup && isInRange)
        {
            sound.PlaySound("Fart", "Item");

            if(!hasRequirements)
            {
                UnableToPickup(CantPickupReason.PriceAndRequirements);
            }
            else
            {
                UnableToPickup(CantPickupReason.Price);
            }

            return false;
        }

        if(!hasRequirements)
        {
            if(isInRange)
            {
                ShowRequirements();
                sound.PlaySound("Fart", "Item");
                Delay(2f);

                UnableToPickup(CantPickupReason.Requirements);
            }

            return false;
        }

        return true;
    }

    public override bool DoPickup(CharacterPickupMaster master)
    {
        bool alreadyHas = master.Inventory.Contains(Item.GetName());
        bool isFull = master.Inventory.Full(Item.GetName());

        if(!alreadyHas || (alreadyHas && !isFull))
        {
            PickedUp(master);
        }
        else
        {
            UnableToPickup(CantPickupReason.AlreadyOwns);

            return false;
        }

        return destroyOnPickup;
    }
}
