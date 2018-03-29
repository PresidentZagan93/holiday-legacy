using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;

public enum PickupType
{
    Health,
    Ammo,
    Item,
    Gun,
    Currency,
    Gem,
    Armor
}

public class ObjectKit : ObjectPickup
{
    public delegate void OnCantPickup(CantPickupReason reason);
    public delegate void OnPickup(CharacterPickupMaster master);

    public OnCantPickup onCantPickup;
    public OnPickup onPickup;

    IItem item;
    int price = 0;
    int id = 0;
    string itemCached = "";

    public void Initialize(IItem item, int price = 0)
    {
        id = Helper.RandomID;
        this.item = item;
        this.price = price;
        this.itemCached = item.GetType() + "|" + item.GetID();
    }

    public IItem Item
    {
        get
        {
            if(item == null && itemCached != "")
            {
                System.Type type = System.Type.GetType(itemCached.Split('|')[0]);
                IItem foundItem = ItemManager.GetItem(type, int.Parse(itemCached.Split('|')[1]));
                Initialize(foundItem, price);
            }
            return item;
        }
    }

    public int Price
    {
        get
        {
            return price;
        }
    }

    public void ShowPreview()
    {
        UIManager.DrawText(id + 0, new Vector2(0, -60), Item.GetName().ToUpper(), "arcade", false);
        UIManager.DrawKeybind(id + 1, new Vector2(0, -45), "E", "arcade", false);

        if (price != 0)
        {
            UIManager.DrawText(id + 2, new Vector2(0, -120), "<color=orange>" + Price + " GOLD</color>", "console", false);
        }

        string extra = "";
        if (item is Item || item is Consumable)
        {
            extra = item.GetDescription().ToUpper();
        }
        else if (item is Gun)
        {
            Gun gun = (Gun)item;
            GunShooter shooter = Character.Player.GunShooter;

            if(shooter.Gun == gun)
            {
                extra = "Same thing.";
            }
            else if(shooter.Gun == null)
            {
                if(gun.wieldMode == Gun.GunWieldMode.Dual && shooter.Armor.HasShield)
                {
                    extra = "<color=orange>CANT HAVE A SHIELD,\nAND A DUO GUN!.</color>";
                }
                else
                {
                    extra = "Please pick me up.";
                }
            }
            else
            {
                List<string> displayText = new List<string>();
                int gunDamage = gun.damage;
                for (int i = 0; i < gun.effects.Length; i++)
                {
                    if (gun.effects[i].quality.name == "EffectDamage")
                    {
                        gunDamage += gun.effects[i].quality.value.ToInt();
                    }
                }

                int damageDifference = shooter.Gun.damage - gunDamage;
                int bpsDifference = shooter.Gun.shots - gun.shots;
                float recoilDifference = shooter.Gun.recoil - gun.recoil;
                float fireRateDifference = shooter.Gun.fireRate - gun.fireRate;

                if (fireRateDifference > 0) displayText.Add("Attacks " + Mathf.Abs(fireRateDifference) + "s faster.");
                if (fireRateDifference < 0) displayText.Add("Attacks " + Mathf.Abs(fireRateDifference) + "s slower.");

                if (damageDifference < 0) displayText.Add("+" + Mathf.Abs(damageDifference) + " damage.");
                if (damageDifference > 0) displayText.Add("-" + Mathf.Abs(damageDifference) + " damage.");
                if (damageDifference == 0) displayText.Add("Equal damage.");

                if (bpsDifference > 0) displayText.Add(Mathf.Abs(bpsDifference) + " less pellets per shot.");
                if (bpsDifference < 0) displayText.Add(Mathf.Abs(bpsDifference) + " more pellets per shot.");

                if (recoilDifference > 0) displayText.Add("+" + Mathf.Abs(recoilDifference) + " accuracy.");
                if (recoilDifference < 0) displayText.Add("-" + Mathf.Abs(recoilDifference) + " accuracy.");

                if (gun.ammoCost == 0 || gun.ammoType == "None") displayText.Add("Doesnt use ammo.");
                else if (gun.ammoCost > 0) displayText.Add("Uses " + gun.ammoCost + " " + gun.ammoType.ToString() + " ammo.");
                else if (gun.ammoCost < 0) displayText.Add("Uses " + gun.ammoCost + " " + gun.ammoType.ToString() + " ammo.");

                if (gun.speed < 0f) displayText.Add("Shoots backwards.");
                if (gun.splash) displayText.Add("Deals " + gun.splashDamage + " splash damage.");

                for (int i = 0; i < gun.effects.Length; i++)
                {
                    displayText.Add(gun.effects[i].quality.FriendlyName);
                }

                if (gun.wieldMode == Gun.GunWieldMode.Dual && shooter.Armor.HasShield)
                {
                    displayText.Add("<color=orange>CANT HAVE A SHIELD,\nAND A DUO GUN!.</color>");
                }

                extra = string.Join("\n", displayText.ToArray());
            }
        }
        else if (item is Armor)
        {
            Armor armor = (Armor)item;
            CharacterArmor holder = Character.Player.Armor;
            List<string> displayText = new List<string>();
            for (int i = 0; i < armor.qualities.Count; i++)
            {
                displayText.Add(armor.qualities[i].FriendlyName);
            }

            if (Character.Player.GunShooter.HasDuoGun && armor.type == Armor.ArmorType.Shield)
            {
                extra = "<color=orange>CANT HAVE A SHIELD,\nAND A DUO GUN!.</color>";
            }

            extra = string.Join("\n", displayText.ToArray());
        }

        if(extra != "")
        {
            UIManager.DrawText(id + 3, new Vector2(0, -95), extra.ToUpper(), "console", false);
        }
    }

    public virtual void PickedUp(CharacterPickupMaster master)
    {
        if (onPickup != null) onPickup.Invoke(master);

        InterfaceAlmanacLayer.Found(item);

        GameManager.AddScore(100);

        Settings.Temporary.pickups++;

        master.Character.RemoveCoins(Price);

        if (item is Item) master.Inventory.Add(item as Item);
        else if (item is Consumable) master.Inventory.Add(item as Consumable);

        master.Collect(this);
    }

    public virtual void UnableToPickup(CantPickupReason reason)
    {
        if (onCantPickup != null) onCantPickup.Invoke(reason);
    }

    public virtual void ShowRequirements()
    {
        if (item is Item)
        {
            UIManager.DrawNotificationText(id, transform.position + Vector3.up * 12f, "<color=red>" + (Item as Item).Requirements.ToUpper() + "</color>", 2f);
        }
    }
}

public class ObjectPickup : MonoBehaviour {

    [System.Serializable]
    public class ObjectPickupMagnet
    {
        public bool enabled = true;
        public int range = 32;
        public float speed = 1f;
    }

    public Vector2 roundOffset = Vector2.zero;
    public int pickupRange = 4;
    public ObjectPickupMagnet magnet;
    
    public bool pickedUp = false;

    public bool dieOnTimer = false;
    public float duration = 5f;
    float nextTimer;
    float pickupDelay;
    bool canPickup;

    new Rigidbody2D rigidbody;
    Collider2D[] colliders;
    public bool isInRange;
    public Transform parent;
    public bool lerpToOrigin = false;
    Vector3 originalPosition;
    Character player;
    [HideInInspector]
    public Vector3 fadePoint;
    bool playerPicked;

    SpriteRenderer outline;

    public void Outline(SpriteRenderer source, bool state)
    {
        if(!outline)
        {
            if (transform.Find("Outline"))
            {
                outline = transform.Find("Outline").GetComponent<SpriteRenderer>();
                if (!outline)
                {
                    outline = transform.Find("Outline").gameObject.AddComponent<SpriteRenderer>();
                    outline.transform.SetParent(transform);
                    outline.transform.localPosition = Vector3.zero;
                    outline.sortingLayerName = "Weather";
                }
            }
            else
            {
                outline = new GameObject("Outline").AddComponent<SpriteRenderer>();
                outline.transform.SetParent(transform);
                outline.transform.localPosition = Vector3.zero;
                outline.sortingLayerName = "Weather";
            }
        }

        if (outline)
        {
            outline.sprite = ObjectManager.GetSpriteOutline(source.sprite.name);
            outline.transform.position = source.transform.position;
            outline.flipX = source.flipX;
            outline.flipY = source.flipY;
            outline.enabled = state;
        }
    }

    private void Start()
    {
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].sortingLayerName = "Pickups";
        }

        pickupDelay = Time.time + 0.5f;
        ObjectManager.Add(this);

        transform.SetParent(GameObject.Find("Pickups").transform);
        rigidbody = GetComponent<Rigidbody2D>();
        nextTimer = Time.time + duration;
        originalPosition = transform.position;
        colliders = transform.GetComponentsInChildren<Collider2D>();

        transform.FixSpriteRenderers();
        actualPosition = transform.position;
    }

    private void OnDestroy()
    {
        ObjectManager.Remove(this);
    }

    bool lastColliderState;
    bool hasDied;
    bool isBlinking;
    float nextBlink;

    void SetColliders(bool state)
    {
        if(lastColliderState != state)
        {
            lastColliderState = state;
            for(int i = 0; i < colliders.Length;i++)
            {
                colliders[i].enabled = state;
            }
        }
    }

    public void Delay(float amount)
    {
        pickupDelay = Time.time + amount;
    }

    private void LateUpdate()
    {
        if (GameManager.Paused || Console.Open)
        {
            nextTimer += Time.deltaTime;
            pickupDelay += Time.deltaTime;

            return;
        }

        if (Time.time > nextTimer && dieOnTimer)
        {
            if(!hasDied)
            {
                transform.localScale = Vector3.one * 2f;
                hasDied = true;
            }

            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * 2f);
            if(transform.localScale.magnitude < 0.1f)
            {
                Destroy(gameObject);
            }

            if(playerPicked)
            {
                Vector3 worldPos = CameraManager.ScreenToWorldPoint(fadePoint);
                transform.position = Vector3.Lerp(transform.position, worldPos, Time.deltaTime * 2f);
            }

            return;
        }

        if (Time.time > nextTimer - 2.5f && dieOnTimer)
        {
            if (Time.time > nextBlink)
            {
                if (Time.time > nextTimer - 1.5f)
                {
                    nextBlink = Time.time + 0.075f;
                }
                else
                {
                    nextBlink = Time.time + 0.15f;
                }
                isBlinking = !isBlinking;
            }

            transform.localScale = isBlinking ? Vector3.one : Vector3.zero;
        }

        player = Character.Player;
        if (Time.time > pickupDelay && player)
        {
            float dist = Vector2.Distance(player.transform.position, transform.position);
            isInRange = dist < magnet.range;
            canPickup = CanPickup(player.PickupMaster);

            if (dist < pickupRange && canPickup)
            {
                if (DoPickup(player.PickupMaster))
                {
                    Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
                    for(int i = 0; i < colliders.Length;i++)
                    {
                        colliders[i].enabled = false;
                    }

                    //Destroy(gameObject);
                    playerPicked = true;
                    nextTimer = 0f;
                    dieOnTimer = true;
                }

                pickedUp = true;
                pickupDelay = Time.time + 0.5f;
            }

            SetColliders(isInRange);
        }

        if (outline) outline.enabled = isInRange;
    }

    Vector3 actualPosition;
    private void FixedUpdate()
    {
        if (!rigidbody) return;
        if(Console.Open || GameManager.Paused)
        {
            rigidbody.velocity = Vector2.zero;
            return;
        }

        bool doFly = magnet.enabled && !lerpToOrigin && rigidbody.bodyType == RigidbodyType2D.Dynamic && isInRange && player && canPickup;
        if (doFly)
        {
            Vector2 flyDir = player.transform.position - transform.position;
            rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, flyDir.normalized * 300f * magnet.speed, Time.fixedDeltaTime * 5f);
            Round();
        }

        if(lerpToOrigin)
        {
            rigidbody.mass = 3;
            rigidbody.drag = 30;

            actualPosition = Vector3.Lerp(actualPosition, parent ? parent.position : originalPosition, Time.fixedDeltaTime * 5f);
            transform.position = new Vector3(actualPosition.x.RoundToInt(), actualPosition.y.RoundToInt(), transform.position.z) + (Vector3)roundOffset;
        }
        else
        {
            if(!doFly)
            {
                rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, Vector2.zero, Time.fixedDeltaTime * 10f);
                Round();
            }
        }
    }

    void Round()
    {
        transform.position = new Vector3(transform.position.x.RoundToInt(), transform.position.y.RoundToInt(), transform.position.z) + (Vector3)roundOffset;
    }

    public virtual bool CanPickup(CharacterPickupMaster character)
    {
        return true;
    }

    public virtual bool DoPickup(CharacterPickupMaster character)
    {
        return true;
    }
}
