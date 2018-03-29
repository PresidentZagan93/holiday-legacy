using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;

public class ObjectPresent : ObjectPickup
{
    int id = 0;
    IItem item;
    ObjectSoundEmitter sound;
    SpritePlayer spritePlayer;

    private void Awake()
    {
        id = Helper.RandomID;

        spritePlayer = GetComponent<SpritePlayer>();
        sound = gameObject.AddComponent<ObjectSoundEmitter>();
        sound.CreateSource("Gift", AudioManager.AudioType.Pickups);
    }

    public void Initialize(IItem item)
    {
        this.item = item;
    }

    public IItem Present
    {
        get
        {
            if (item != null) return item;
            
            List<Item> items = ItemManager.AllItems;
            List<Gun> guns = ItemManager.AllGuns;
            List<Armor> armor = ItemManager.AllArmor;
            List<Consumable> consumables = ItemManager.AllConsumables;

            List<IItem> validItems = new List<IItem>();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].ready && Character.Player.Inventory.HasItemRequirements(items[i].name)) validItems.Add(items[i]);
            }
            for (int i = 0; i < guns.Count; i++)
            {
                if (guns[i].ready && !guns[i].enemyOnly) validItems.Add(guns[i]);
            }
            for (int i = 0; i < armor.Count; i++)
            {
                if (armor[i].ready) validItems.Add(armor[i]);
            }
            for (int i = 0; i < consumables.Count; i++)
            {
                if (consumables[i].ready) validItems.Add(consumables[i]);
            }

            return validItems[Random.Range(0, validItems.Count)];
        }
    }

    private void Update()
    {
        if (isInRange && !pickedUp)
        {
            UIManager.DrawText(id + 1, new Vector2(0, -140), "PRESENT!", "arcade", false);
            UIManager.DrawKeybind(id + 2, new Vector2(0, -120), "E", "arcade", false);
        }
    }

    public override bool CanPickup(CharacterPickupMaster character)
    {
        return character.CanPickup && !pickedUp;
    }

    public override bool DoPickup(CharacterPickupMaster character)
    {
        ObjectKit kit = ItemManager.DropItem(transform.position + Vector3.down, Present);

        kit.parent = transform;
        kit.lerpToOrigin = true;

        SpriteRenderer[] spriteRenderers = kit.transform.GetComponentsInChildren<SpriteRenderer>();
        for(int i = 0; i < spriteRenderers.Length;i++)
        {
            spriteRenderers[i].sortingLayerName = spritePlayer.spriteRenderer.sortingLayerName;
            spriteRenderers[i].sortingOrder = spritePlayer.spriteRenderer.sortingOrder + 1;
        }

        GameManager.AddScore(100);
        sound.PlaySound("Gift", "Gift");
        spritePlayer.Play("GiftOpen");

        GetComponentInChildren<Collider2D>().enabled = false;

        return false;
    }
}
