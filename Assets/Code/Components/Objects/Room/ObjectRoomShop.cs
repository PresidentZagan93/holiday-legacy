using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;

public class ObjectRoomShop : ObjectRoom
{
    [HideInInspector]
    public Transform shop;
    Speaker speaker;
    List<ObjectKit> kits = new List<ObjectKit>();

    public override void InitializeRoom()
    {
        base.InitializeRoom();
        
        shop = Instantiate(ObjectManager.GetPrefab("Shop"), transform).transform;
        shop.transform.position = rect.center + Vector2.zero;

        CreateShop();

        SpriteRenderer topShop = shop.Find("Top").GetComponent<SpriteRenderer>();
        SpriteRenderer bottomShop = shop.Find("Bottom").GetComponent<SpriteRenderer>();

        SetIcon(ObjectManager.GetSprite("MinimapIcons_11"));
        bool isTop = doorPosition.ToWorldPos().y < transform.position.y;

        if(isTop)
        {
            speaker = topShop.GetComponent<Speaker>();
            Destroy(bottomShop.gameObject);
        }
        else
        {
            speaker = bottomShop.GetComponent<Speaker>();
            Destroy(topShop.gameObject);
        }
    }

    public virtual void CreateShop()
    {
        var items = ItemManager.AllItems.Randomize();
        var consumables = ItemManager.AllConsumables.Randomize();
        for (int i = 0; i < items.Count; i++)
        {
            if (!items[i].ready || !Character.Player.Inventory.HasItemRequirements(items[i].name))
            {
                items.RemoveAt(i);
            }
        }
        for (int i = 0; i < consumables.Count; i++)
        {
            if (!consumables[i].ready)
            {
                consumables.RemoveAt(i);
            }
        }

        List<IItem> toSpawn = new List<IItem>();

        for (int i = 0; i < shop.childCount; i++)
        {
            if (shop.GetChild(i).name == "ShopSlot")
            {
                bool isItem = Random.value > ((float)consumables.Count / (float)items.Count) * 1.5f;
                
                if (isItem)
                {
                    toSpawn.Add(items[i]);
                }
                else
                {
                    toSpawn.Add(consumables[i]);
                }

            }
        }

        PlaceItems(toSpawn);
    }

    public void PlaceItems(List<IItem> items)
    {
        int itemsSpawned = 0;
        for (int i = 0; i < shop.childCount; i++)
        {
            if (shop.GetChild(i).name == "ShopSlot")
            {
                ObjectKit kit = null;
                IItem item = items[itemsSpawned];

                if (item is Item) kit = ItemManager.DropItem(shop.GetChild(i).position, item.GetName());
                else if (item is Gun) kit = ItemManager.DropGun(shop.GetChild(i).position, item.GetName());
                else if (item is Armor) kit = ItemManager.DropArmor(shop.GetChild(i).position, item.GetName());
                else if (item is Consumable) kit = ItemManager.DropConsumable(shop.GetChild(i).position, item.GetName());

                kit.onCantPickup += OnCantPickup;
                kit.onPickup += OnPickup;

                kit.lerpToOrigin = true;
                kit.Initialize(item, item.GetPrice());

                ObjectMinimapIcon minimapIcon = kit.GetComponent<ObjectMinimapIcon>();
                if(minimapIcon)
                {
                    minimapIcon.show = doorType != RoomDoorType.Hidden;
                }

                kits.Add(kit);
                itemsSpawned++;
            }
        }
    }

    public override void Discover()
    {
        base.Discover();

        for (int i = 0; i < kits.Count; i++)
        {
            if (kits[i])
            {
                ObjectMinimapIcon minimapIcon = kits[i].GetComponent<ObjectMinimapIcon>();
                if (minimapIcon)
                {
                    minimapIcon.show = doorType != RoomDoorType.Hidden;
                }
            }
        }
    }

    private void OnDisable()
    {
        for (int i = 0; i < kits.Count; i++)
        {
            if(kits[i])
            {
                kits[i].onCantPickup -= OnCantPickup;
                kits[i].onPickup -= OnPickup;
            }
        }
    }

    private void OnEnable()
    {
        for(int i = 0; i < kits.Count;i++)
        {
            if (kits[i])
            {
                kits[i].onCantPickup += OnCantPickup;
                kits[i].onPickup += OnPickup;
            }
        }
    }

    void OnCantPickup(CantPickupReason reason)
    {
        string text = "";
        if (reason == CantPickupReason.AlreadyOwns) text = "Greedy? You already have this!";
        if (reason == CantPickupReason.Price) text = "Oi! Come back when you have your money!";
        if (reason == CantPickupReason.Requirements) text = "No cant do, you are missing some stuff.";
        if (reason == CantPickupReason.PriceAndRequirements) text = "Come back with the money, and when you get your requirements.";

        if(text != "")
        {
            speaker.Say(text.ToUpper());
        }
    }

    void OnPickup(CharacterPickupMaster master)
    {
        speaker.Say("Enjoy");
    }

    private void Update()
    {
        if (GameManager.Paused) return;

        ShowIcon = doorType != RoomDoorType.Hidden;
    }

    private void OnDestroy()
    {
        if(shop)
        {
            Destroy(shop.gameObject);
        }
    }
}
