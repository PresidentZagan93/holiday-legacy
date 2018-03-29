using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Data;

public class ObjectManager : MonoBehaviour {
    
    [System.Serializable]
    public class ObjectPrefab
    {
        public string name;
        public GameObject prefab;
    }

    [System.Serializable]
    public class ObjectLayerMask
    {
        public string name;
        public LayerMask mask;
    }

    public Material defaultMaterial;
    public List<Sprite> sprites = new List<Sprite>();
    public List<Sprite> spriteOutlines = new List<Sprite>();
    public List<Material> materials = new List<Material>();
    public List<ObjectPrefab> prefabs = new List<ObjectPrefab>();
    public List<AudioClip> clips = new List<AudioClip>();
    public List<ObjectLayerMask> layerMasks = new List<ObjectLayerMask>();
    public List<MonoBehaviour> objects = new List<MonoBehaviour>();
    
    public static ObjectManager singleton;

    public static List<ObjectPrefab> AllPrefabs
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<ObjectManager>();

            return singleton.prefabs;
        }
    }

    public static List<AudioClip> AllAudioClips
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<ObjectManager>();

            return singleton.clips;
        }
    }

    public static List<T> GetAllOfType<T>() where T : MonoBehaviour
    {
        List<T> toReturn = new List<T>();
        if (!singleton) singleton = FindObjectOfType<ObjectManager>();
        if (!singleton) return toReturn;

        for (int i = 0; i < singleton.objects.Count;i++)
        {
            if(singleton.objects[i] is T)
            {
                toReturn.Add((T)singleton.objects[i]);
            }
        }
        return toReturn;
    }

    void Awake()
    {
        singleton = this;
    }

    public static AudioClip GetAudioClip(string name)
    {
        if (!singleton) singleton = FindObjectOfType<ObjectManager>();

        for (int i = 0; i < singleton.clips.Count; i++)
        {
            if (singleton.clips[i] && singleton.clips[i].name == name)
            {
                return singleton.clips[i];
            }
        }

        return null;
    }

    public static Sprite GetSprite(string name)
    {
        if (!singleton) singleton = FindObjectOfType<ObjectManager>();

        for (int i = 0; i < singleton.sprites.Count; i++)
        {
            if (singleton.sprites[i].name == name)
            {
                return singleton.sprites[i];
            }
        }
        return null;
    }

    public static Sprite GetSpriteOutline(string name)
    {
        if (!singleton) singleton = FindObjectOfType<ObjectManager>();

        for (int i = 0; i < singleton.spriteOutlines.Count; i++)
        {
            if (singleton.spriteOutlines[i].name == name)
            {
                return singleton.spriteOutlines[i];
            }
        }
        return null;
    }

    public static Material GetMaterial(string name)
    {
        if (!singleton) singleton = FindObjectOfType<ObjectManager>();

        for (int i = 0; i < singleton.materials.Count; i++)
        {
            if (singleton.materials[i].name == name)
            {
                return singleton.materials[i];
            }
        }
        return null;
    }

    public static LayerMask GetLayerMask(string name)
    {
        if (!singleton) singleton = FindObjectOfType<ObjectManager>();

        for (int i = 0; i < singleton.layerMasks.Count;i++)
        {
            if(singleton.layerMasks[i].name == name)
            {
                return singleton.layerMasks[i].mask;
            }
        }
        return new LayerMask();
    }

    public static GameObject GetPrefab(string name)
    {
        if (!singleton) singleton = FindObjectOfType<ObjectManager>();

        for (int i = 0; i < singleton.prefabs.Count; i++)
        {
            if (singleton.prefabs[i].name == name)
            {
                return singleton.prefabs[i].prefab;
            }
        }
        return null;
    }
    
    public static void DestroyAll()
    {
        if (!singleton) singleton = FindObjectOfType<ObjectManager>();

        for (int i = 0; i < singleton.objects.Count; i++)
        {
            if(singleton.objects[i] != null)
            {
                Destroy((singleton.objects[i] as MonoBehaviour).gameObject);
            }
        }

        singleton.objects.Clear();
    }

    List<DropHealthPropsQueue> healthDropQueue = new List<DropHealthPropsQueue>();
    
    public class DropHealthPropsQueue
    {
        public bool executing;
        public List<Loot.LootProp> props = new List<Loot.LootProp>();
        public Vector3 position;
        public Inventory inventory;

        public DropHealthPropsQueue(Loot loot, Character character, Vector3 position)
        {
            this.position = position;
            this.inventory = character.Inventory ? character.Inventory : character.Owner ? character.Owner.Inventory : null;

            this.props.AddRange(loot.loot);
            for(int i = 0;i < loot.extra.Count;i++)
            {
                this.props.AddRange(loot.extra[i].loot);
            }
        }
    }

    public static void DropHealthProps(Loot loot, Character character, Vector3 position)
    {
        if (!singleton) singleton = FindObjectOfType<ObjectManager>();

        singleton.healthDropQueue.Add(new DropHealthPropsQueue(loot, character, position));
    }

    IEnumerator HealthDropsCoroutine(DropHealthPropsQueue queue)
    {
        Character player = Character.Player;
        if (queue.inventory)
        {
            float chanceMultiplier = queue.inventory.GetMultiplier("Luckiness");

            float dropsMp = queue.inventory.GetMultiplier("PickupDropAmount");
            for (int i = 0; i < queue.props.Count; i++)
            {
                if (queue.props[i].prop)
                {
                    bool isHealth = queue.props[i].prop.name == "Pickup_HealthKit";
                    bool isAmmo = queue.props[i].prop.name.StartsWith("Pickup_Ammo");
                    bool isCurrency = queue.props[i].prop.name.StartsWith("Pickup_Coin");
                    bool isGun = queue.props[i].prop.name == "Pickup_Gun";
                    bool isItem = queue.props[i].prop.name == "Pickup_ItemKit";
                    bool isArmor = queue.props[i].prop.name == "Pickup_Armor";

                    bool drop = Random.Range(0, 100) < queue.props[i].chance * chanceMultiplier;

                    if (isHealth)
                    {
                        float healthChance = queue.inventory.GetMultiplier("PickupHealthDropChance");
                        drop = Random.Range(0, 100) < queue.props[i].chance * chanceMultiplier * healthChance;
                    }
                    if (isAmmo)
                    {
                        float ammoChance = queue.inventory.GetMultiplier("PickupAmmoDropChance");
                        ObjectAmmoBox ammoKit = queue.props[i].prop.GetComponent<ObjectAmmoBox>();
                        if (!ammoKit.genericAmmo)
                        {
                            drop = false;
                            for (int g = 0; g < player.GunShooter.guns.Count; g++)
                            {
                                if (player.GunShooter.guns[g].gun.ammoType == ammoKit.ammoType)
                                {
                                    var ammo = player.GunShooter.GetAmmo(player.GunShooter.guns[g].gun);
                                    float ammoPercentage = (float)ammo.ammo / (float)ammo.max;
                                    ammoPercentage = ammoPercentage.Remap(0f, 1f, queue.props[i].chance, 0f);

                                    if (Random.Range(0, 100) < ammoPercentage * chanceMultiplier * ammoChance)
                                    {
                                        drop = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            drop = Random.Range(0, 100) < queue.props[i].chance * chanceMultiplier * ammoChance;
                        }
                    }
                    if (isCurrency)
                    {
                        float currencyChance = queue.inventory.GetMultiplier("PickupCurrencyDropChance");
                        drop = Random.Range(0, 100) < queue.props[i].chance * chanceMultiplier * currencyChance;
                    }

                    if (drop)
                    {
                        float extraDropsMp = 1f;
                        if (isAmmo) extraDropsMp = queue.inventory.GetMultiplier("PickupAmmoDropAmount");
                        if (isHealth) extraDropsMp = queue.inventory.GetMultiplier("PickupHealthDropAmount");
                        if (isCurrency) extraDropsMp = queue.inventory.GetMultiplier("PickupCurrencyDropAmount");

                        int amount = Mathf.RoundToInt(queue.props[i].amount.Random() * dropsMp * extraDropsMp);
                        for (int a = 0; a < amount; a++)
                        {
                            //yield return new WaitForSeconds(0.015f);

                            GameObject newPropMade = null;
                            if (isGun && Character.Player)
                            {
                                newPropMade = ItemManager.DropGun(queue.position, ObjectChest.GetGun().name).gameObject;
                            }
                            else if (isItem)
                            {
                                newPropMade = ItemManager.DropItem(queue.position, ItemManager.RandomItem().name).gameObject;
                            }
                            else if (isArmor)
                            {
                                newPropMade = ItemManager.DropArmor(queue.position, ItemManager.RandomArmor().name).gameObject;
                            }
                            else
                            {
                                newPropMade = Instantiate(queue.props[i].prop, queue.position, Quaternion.identity) as GameObject;
                            }

                            if(newPropMade)
                            {
                                Rigidbody2D newPropRigidbody = newPropMade.GetComponent<Rigidbody2D>();
                                if (newPropRigidbody && newPropRigidbody.bodyType != RigidbodyType2D.Static)
                                {
                                    newPropRigidbody.velocity = Random.insideUnitCircle * Random.Range(20 + amount, 26 + amount * 1.5f) * 24f;
                                }
                            }
                        }
                    }
                }
            }
        }

        healthDropQueue.Remove(queue);
        yield return new WaitForSeconds(0.015f);
    }

    void Update()
    {
        singleton = this;
        for(int i = 0; i < healthDropQueue.Count;i++)
        {
            if(!healthDropQueue[i].executing)
            {
                healthDropQueue[i].executing = true;
                StartCoroutine(HealthDropsCoroutine(healthDropQueue[i]));
                break;
            }
        }
        for(int i = 0; i < objects.Count;i++)
        {
            if(objects[i] == null)
            {
                objects.RemoveAt(i);
            }
        }
    }

    public static void Remove(MonoBehaviour thing)
    {
        if (thing == null) return;
        if (!singleton) singleton = FindObjectOfType<ObjectManager>();
        if (!singleton) return;

        if (!singleton.objects.Contains(thing)) return;

        singleton.objects.Remove(thing);
    }

    public static void Add(MonoBehaviour thing)
    {
        if (thing == null) return;
        if (!singleton) singleton = FindObjectOfType<ObjectManager>();
        if (!singleton) return;

        if (singleton.objects.Contains(thing)) return;

        singleton.objects.Add(thing);
    }
}
