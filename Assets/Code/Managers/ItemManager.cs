using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Data;

[System.Serializable]
public class ItemTypeOffset
{
    public ItemType type = ItemType.None;
    public float offset = 1f;
}

public class ItemManager : MonoBehaviour {

    public List<Armor> armor = new List<Armor>();
    public List<Item> items = new List<Item>();
    public List<Consumable> consumables = new List<Consumable>();
    public List<Gun> guns = new List<Gun>();
    public List<AmmoType> ammo = new List<AmmoType>();
    public List<PlayerBuild> builds = new List<PlayerBuild>();

    public List<Quality> qualityOffsets = new List<Quality>();
    public static ItemManager singleton;

    void Awake()
    {
        singleton = this;
    }

    void OnEnable()
    {
        singleton = this;
    }

    public static List<PlayerBuild> AllPlayerBuilds
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<ItemManager>();
            return singleton.builds;
        }
    }

    public static List<Gun> AllGuns
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<ItemManager>();
            return singleton.guns;
        }
    }

    public static List<Item> AllItems
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<ItemManager>();
            return singleton.items;
        }
    }

    public static List<Consumable> AllConsumables
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<ItemManager>();
            return singleton.consumables;
        }
    }

    public static List<Armor> AllArmor
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<ItemManager>();
            return singleton.armor;
        }
    }

    public static Item RandomItem()
    {
        if(!singleton) singleton = FindObjectOfType<ItemManager>();

        List<Item> validItems = new List<Item>();
        for(int i = 0; i < singleton.items.Count;i++)
        {
            if(singleton.items[i].ready)
            {
                validItems.Add(singleton.items[i]);
            }
        }

        return validItems[Random.Range(0, validItems.Count)];
    }

    public static Armor RandomArmor()
    {
        if (!singleton) singleton = FindObjectOfType<ItemManager>();

        return singleton.armor[Random.Range(0, singleton.armor.Count)];
    }

    public static AmmoType GetAmmoType(string name)
    {
        if (!singleton) singleton = FindObjectOfType<ItemManager>();

        for (int i = 0; i < singleton.ammo.Count; i++)
        {
            if (singleton.ammo[i].name == name)
            {
                return singleton.ammo[i];
            }
        }

        return null;
    }

    public static ObjectKit DropItem(Vector2 position, IItem item)
    {
        string name = item.GetName();
        if (item is Item) return DropItem(position, name);
        if (item is Armor) return DropArmor(position, name);
        if (item is Gun) return DropGun(position, name);
        if (item is Consumable) return DropConsumable(position, name);

        return null;
    }

    public static ObjectItemKit DropItem(Vector2 position, string name)
    {
        ObjectItemKit pickup = Instantiate(ObjectManager.GetPrefab("ItemPickup")).GetComponent<ObjectItemKit>();

        Item item = GetItem(name);
        pickup.Initialize(item);
        pickup.transform.position = position;

        return pickup;
    }

    public static ObjectAmmoBox DropAmmo(Vector2 position)
    {
        ObjectAmmoBox pickup = Instantiate(ObjectManager.GetPrefab("AmmoPickup")).GetComponent<ObjectAmmoBox>();
        pickup.transform.position = position;

        return pickup;
    }

    public static ObjectConsumableKit DropConsumable(Vector2 position, string name)
    {
        ObjectConsumableKit pickup = Instantiate(ObjectManager.GetPrefab("ConsumablePickup")).GetComponent<ObjectConsumableKit>();

        Consumable item = GetConsumable(name);
        pickup.Initialize(item);
        pickup.transform.position = position;

        return pickup;
    }

    public static ObjectArmorKit DropArmor(Vector2 position, string name)
    {
        ObjectArmorKit pickup = Instantiate(ObjectManager.GetPrefab("ArmorPickup")).GetComponent<ObjectArmorKit>();

        Armor armor = GetArmor(name);
        pickup.Initialize(armor);
        pickup.transform.position = position;

        return pickup;
    }

    public static ObjectGunKit DropGun(Vector2 position, string gun)
    {
        ObjectGunKit pickup = Instantiate(ObjectManager.GetPrefab("GunPickup")).GetComponent<ObjectGunKit>();
        Gun gunToAdd = GetGun(gun);
        pickup.Initialize(gunToAdd);
        pickup.transform.position = position;

        return pickup;
    }

    public static Gun RandomGun()
    {
        if (!singleton) singleton = FindObjectOfType<ItemManager>();

        return singleton.guns[Random.Range(0, singleton.guns.Count)];
    }

    public static Consumable GetConsumable(string name)
    {
        if (!singleton) singleton = FindObjectOfType<ItemManager>();

        for (int i = 0; i < singleton.consumables.Count; i++)
        {
            if (singleton.consumables[i].name == name)
            {
                return singleton.consumables[i];
            }
        }

        return null;
    }

    public static Consumable GetConsumable(int id)
    {
        if (!singleton) singleton = FindObjectOfType<ItemManager>();

        for (int i = 0; i < singleton.consumables.Count; i++)
        {
            if (singleton.consumables[i].ID == id)
            {
                return singleton.consumables[i];
            }
        }

        return null;
    }

    public static Gun GetGun(int id)
    {
        if (!singleton) singleton = FindObjectOfType<ItemManager>();

        for (int i = 0; i < singleton.guns.Count; i++)
        {
            if (singleton.guns[i].ID == id)
            {
                return singleton.guns[i];
            }
        }

        return null;
    }

    public static Gun GetGun(string name)
    {
        if (!singleton) singleton = FindObjectOfType<ItemManager>();

        for (int i = 0; i < singleton.guns.Count; i++)
        {
            if (singleton.guns[i].name == name)
            {
                return singleton.guns[i];
            }
        }

        return null;
    }

    public static PlayerBuild RandomBuild()
    {
        if (!singleton) singleton = FindObjectOfType<ItemManager>();

        return singleton.builds[Random.Range(0, singleton.builds.Count)];
    }

    public static PlayerBuild GetBuild(string name)
    {
        if (!singleton) singleton = FindObjectOfType<ItemManager>();

        for (int i = 0; i < singleton.guns.Count; i++)
        {
            if (singleton.builds[i].name == name)
            {
                return singleton.builds[i];
            }
        }

        return null;
    }

    public static bool QualityExists(string name)
    {
        if (!singleton) singleton = FindObjectOfType<ItemManager>();

        for (int i = 0; i < singleton.items.Count; i++)
        {
            for (int m = 0; m < singleton.items[i].qualities.Count; m++)
            {
                if (singleton.items[i].qualities[m].name == name)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static float GetQualityOffset(string name)
    {
        if (!singleton) singleton = FindObjectOfType<ItemManager>();

        for (int i = 0; i < singleton.qualityOffsets.Count; i++)
        {
            if (singleton.qualityOffsets[i].name == name)
            {
                return singleton.qualityOffsets[i].Float;
            }
        }

        return 0f;
    }

    public static List<Item> RandomItems(int amount)
    {
        Character player = Character.Player;
        List<Item> validItems = new List<Item>();
        //Remove all items, if theres no player
        if (player)
        {
            for (int i = 0; i < singleton.items.Count; i++)
            {
                bool alreadyHas = player.Inventory.Contains(singleton.items[i].name);
                if (!alreadyHas && singleton.items[i].ready)
                {
                    //add if we dont have and if its ready
                    validItems.Add(singleton.items[i]);
                }
            }
        }
        if (amount > validItems.Count)
        {
            //not enough items to show, add an empty item
            int more = amount - validItems.Count;
            for (int i = 0; i < more; i++)
            {
                validItems.Add(new Item());
            }
        }

        validItems = validItems.Randomize();
        List<Item> newItems = new List<Item>();
        //Debug.Break();

        for (int i = 0; i < validItems.Count; i++)
        {
            Item toAdd = validItems[i];
            if (player)
            {
                if (!player.Inventory.HasItemRequirements(toAdd.name)) continue;
            }
            newItems.Add(toAdd);
        }
        return newItems;
    }

    public static IItem GetItem(System.Type type, int id)
    {
        if (!singleton) singleton = FindObjectOfType<ItemManager>();

        if(type == typeof(Item)) return GetItem(id);
        if(type == typeof(Consumable)) return GetConsumable(id);
        if(type == typeof(Armor)) return GetArmor(id);
        if(type == typeof(Gun)) return GetGun(id);

        return null;
    }

    public static Item GetItem(string name)
    {
        if (!singleton) singleton = FindObjectOfType<ItemManager>();

        for (int i = 0; i < singleton.items.Count; i++)
        {
            if (singleton.items[i].name == name) return singleton.items[i];
        }

        return null;
    }

    public static Item GetItem(int id)
    {
        if (!singleton) singleton = FindObjectOfType<ItemManager>();

        for (int i = 0; i < singleton.items.Count; i++)
        {
            if (singleton.items[i].ID == id) return singleton.items[i];
        }
        return null;
    }

    public static Armor GetArmor(string name)
    {
        if (!singleton) singleton = FindObjectOfType<ItemManager>();

        for (int i = 0; i < singleton.armor.Count; i++)
        {
            if (singleton.armor[i].name == name) return singleton.armor[i];
        }
        return null;
    }

    public static Armor GetArmor(int id)
    {
        if (!singleton) singleton = FindObjectOfType<ItemManager>();

        for (int i = 0; i < singleton.armor.Count; i++)
        {
            if (singleton.armor[i].ID == id) return singleton.armor[i];
        }
        return null;
    }
}
