using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;

public class ObjectConsumableKit : ObjectKit {
    
    SpriteRenderer spriteRenderer;
    SpriteRenderer shadowRenderer;
    public bool destroyOnPickup = false;
    ObjectSoundEmitter sound;

    private void Awake()
    {
        fadePoint = new Vector3(0f, -480f);
        sound = gameObject.AddComponent<ObjectSoundEmitter>();
        sound.CreateSource("Item", AudioManager.AudioType.Pickups);
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        shadowRenderer = transform.Find("Shadow").GetComponent<SpriteRenderer>();
        Initialize();
    }

    public void Initialize()
    {
        List<Consumable> items = ItemManager.singleton.consumables;
        List<Consumable> validItems = new List<Consumable>();
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].ready)
            {
                validItems.Add(items[i]);
            }
        }

        Consumable consumable = validItems[Random.Range(0, validItems.Count)];
        Initialize(consumable);
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

    public override bool CanPickup(CharacterPickupMaster master)
    {
        if (!master.CanPickup) return false;
        if (!isInRange) return false;

        bool alreadyHas = master.Inventory.Contains(Item.GetName());
        if(!alreadyHas && master.Inventory.HasConsumable && !master.Inventory.GetConsumable().Active)
        {
            return true;
        }
        if(alreadyHas || master.Inventory.HasConsumable)
        {
            UnableToPickup(CantPickupReason.AlreadyOwns);
            return false;
        }
        if (master.Character.Gold < Price && master.CanPickup && isInRange)
        {
            sound.PlaySound("Fart", "Item");
            UnableToPickup(CantPickupReason.Price);
            return false;
        }

        return true;
    }

    public override bool DoPickup(CharacterPickupMaster master)
    {
        bool alreadyHas = master.Inventory.Contains(Item.GetName());

        if(!alreadyHas)
        {
            if(master.Inventory.HasConsumable)
            {
                ItemManager.DropConsumable(master.transform.position, master.Inventory.GetConsumable().item.name);
                master.Inventory.Remove((Consumable)Item);
            }
            PickedUp(master);
        }
        else
        {
            return false;
        }

        return destroyOnPickup;
    }
}
