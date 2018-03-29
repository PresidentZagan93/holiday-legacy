using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Data;

[System.Serializable]
public class GunMultipliers
{
    public float fireRateMultiplier = 1f;
    public float sizeMultiplier = 1f;
    public float damageMultiplier = 1f;
    public float speedMultiplier = 1f;
    public float recoilMultiplier = 1f;

    public GunMultipliers()
    {
        fireRateMultiplier = 1f;
        sizeMultiplier = 1f;
        damageMultiplier = 1f;
        speedMultiplier = 1f;
        recoilMultiplier = 1f;
    }
}

public class GunShooter : GameBehaviour {

    [System.Serializable]
    public class GunSlot
    {
        public Gun gun;
        public float nextShot;

        public GunSlot(Gun gun)
        {
            this.gun = gun;
        }
    }
    public List<GunSlot> guns = new List<GunSlot>();
    public int index = 0;

    [System.Serializable]
    public class GunAmmo
    {
        [AmmoType]
        public string type = "";
        public int ammo = 0;
        public int max = 0;

        public GunAmmo(string type, int ammo, int max)
        {
            this.type = type;
            this.ammo = ammo;
            this.max = max;
        }
    }

    public List<GunAmmo> ammo = new List<GunAmmo>();
    public Vector2Int offset = new Vector2Int();
    public GunMultipliers multipliers;
    public AudioClip switchSound;
    public AudioClip breakSound;
    public Vector2 direction;
    public bool infiniteAmmo;
    
    SpriteRenderer gunSprite;
    SpriteRenderer gunDualSprite;

    SpriteRenderer gunSpriteOutline;
    SpriteRenderer gunDualSpriteOutline;

    Gun lastGun;
    float nextBurst;
    int burst = int.MaxValue;
    GameObject oldEffect;
    float swingAmount;
    float swingToAmount;
    SpriteRenderer gunShadow;
    SpriteRenderer otherGun;
    SpriteRenderer otherGunOutline;

    SpriteRenderer hand1;
    SpriteRenderer hand2;
    SpriteRenderer hand3;
    SpriteRenderer hand4;
    SpriteRenderer hand5;
    Transform hands;
    public Transform root;

    public Gun Gun
    {
        get
        {
            if (guns.Count == 0) return null;
            return guns[index].gun;
        }
        set
        {
            guns[index].gun = value;
        }
    }

    public bool HasDuoGun
    {
        get
        {
            if (Gun && OtherGun) return Gun.wieldMode == Gun.GunWieldMode.Dual || OtherGun.wieldMode == Gun.GunWieldMode.Dual;
            else if (Gun) return Gun.wieldMode == Gun.GunWieldMode.Dual;
            else if (OtherGun) return OtherGun.wieldMode == Gun.GunWieldMode.Dual;

            return false;
        }
    }

    public Gun OtherGun
    {
        get
        {
            if (guns.Count != 2) return null;

            if (index == 0) return guns[1].gun;
            if (index == 1) return guns[0].gun;

            return null;
        }
        set
        {
            if (guns.Count != 2) return;

            if (index == 0) guns[1].gun = value;
            if (index == 1) guns[0].gun = value;
        }
    }

    public override void Awake()
    {
        base.Awake();

        if (Character.isPlayer)
        {
            HUDAmmo.Assign(this);
        }

        Sound.CreateSource("Gun Shots", AudioManager.AudioType.Gun);
        Sound.CreateSource("Gun Idle", AudioManager.AudioType.Gun);
        Sound.CreateSource("Gun Shells", AudioManager.AudioType.Gun);

        if (!root) root = transform.Find("Gun");
        if(root)
        {
            if (root.Find("Gun other"))
            {
                otherGun = root.Find("Gun other/Gun other").GetComponent<SpriteRenderer>();
                otherGun.transform.parent.parent = transform;
            }
            if (root.Find("Gun sprite"))
            {
                gunSprite = root.Find("Gun sprite").GetComponent<SpriteRenderer>();
                gunSprite.sortingOrder = 14;
            }
            if (!root.Find("Gun shadow"))
            {
                gunShadow = new GameObject("Gun shadow").AddComponent<SpriteRenderer>();
                gunShadow.transform.SetParent(root);
            }
            else
            {
                gunShadow = root.Find("Gun shadow").GetComponent<SpriteRenderer>();
            }
            if (root.Find("Gun dual sprite"))
            {
                gunDualSprite = root.Find("Gun dual sprite").GetComponent<SpriteRenderer>();
            }
        }
        
        if(root.Find("Gun sprite outline"))
        {
            gunSpriteOutline = root.Find("Gun sprite outline").GetComponent<SpriteRenderer>();
        }
        else
        {
            gunSpriteOutline = new GameObject("Gun sprite outline").AddComponent<SpriteRenderer>();
            gunSpriteOutline.transform.SetParent(root);
        }

        if (root.Find("Gun dual sprite outline"))
        {
            gunDualSpriteOutline = root.Find("Gun dual sprite outline").GetComponent<SpriteRenderer>();
        }
        else
        {
            gunDualSpriteOutline = new GameObject("Gun dual sprite outline").AddComponent<SpriteRenderer>();
            gunDualSpriteOutline.transform.SetParent(root);
        }

        if (transform.Find("Gun other/Gun other outline"))
        {
            otherGunOutline = transform.Find("Gun other/Gun other outline").GetComponent<SpriteRenderer>();
        }
        else
        {
            if(transform.Find("Gun other"))
            {
                otherGunOutline = new GameObject("Gun other outline").AddComponent<SpriteRenderer>();
                otherGunOutline.transform.SetParent(transform.Find("Gun other"));
            }
        }

        if (!gunDualSprite && gunSprite)
        {
            gunDualSprite = Instantiate(gunSprite).GetComponent<SpriteRenderer>();
            gunDualSprite.name = "Gun dual sprite";
            gunDualSprite.transform.SetParent(root);
        }

        if (gunDualSprite)
        {
            gunDualSprite.enabled = false;
        }

        hand1 = new GameObject("Hand1").AddComponent<SpriteRenderer>();
        hand1.sprite = ObjectManager.GetSprite("Hands_4");
        hand1.transform.SetParent(root);
        hand1.sortingOrder = gunSprite.sortingOrder + 1;
        hand1.sortingLayerName = "Player";
        hand1.enabled = false;

        hand2 = new GameObject("Hand2").AddComponent<SpriteRenderer>();
        hand2.sprite = ObjectManager.GetSprite("Hands_0");
        hand2.transform.SetParent(root);
        hand2.sortingOrder = gunSprite.sortingOrder + 1;
        hand2.sortingLayerName = "Player";
        hand2.enabled = false;

        hand3 = new GameObject("Hand3").AddComponent<SpriteRenderer>();
        hand3.sprite = ObjectManager.GetSprite("Hands_2");
        hand3.transform.SetParent(root);
        hand3.sortingOrder = gunSprite.sortingOrder + 1;
        hand3.sortingLayerName = "Player";
        hand3.enabled = false;

        hand4 = new GameObject("Hand4").AddComponent<SpriteRenderer>();
        hand4.sprite = ObjectManager.GetSprite("Hands_3");
        hand4.transform.SetParent(root);
        hand4.sortingOrder = gunSprite.sortingOrder + 1;
        hand4.sortingLayerName = "Player";
        hand4.enabled = false;

        hand5 = new GameObject("Hand5").AddComponent<SpriteRenderer>();
        hand5.sprite = ObjectManager.GetSprite("Hands_1");
        hand5.transform.SetParent(root);
        hand5.sortingOrder = gunSprite.sortingOrder + 1;
        hand5.sortingLayerName = "Player";
        hand5.enabled = false;

        CreateAmmo();
    }

    public bool FullAmmo(string type)
    {
        for (int i = 0; i < ammo.Count; i++)
        {
            if (ammo[i].type == type)
            {
                return ammo[i].max == ammo[i].ammo;
            }
        }

        return false;
    }

    public void AddAmmo(string type, int amount)
    {
        for (int i = 0; i < ammo.Count; i++)
        {
            if (ammo[i].type == type)
            {
                ammo[i].ammo += amount;
                if(ammo[i].ammo > ammo[i].max)
                {
                    ammo[i].ammo = ammo[i].max;
                }
            }
        }
    }

    public GunAmmo GetAmmo(Gun gun)
    {
        for (int i = 0; i < ammo.Count; i++)
        {
            if (ammo[i].type == gun.ammoType) return ammo[i];
        }

        return null;
    }

    void CreateAmmo()
    {
        for(int i = 0; i < ItemManager.singleton.ammo.Count;i++)
        {
            ammo.Add(new GunAmmo(ItemManager.singleton.ammo[i].name, 0, ItemManager.singleton.ammo[i].maxAmmo));
        }
    }

    public bool CanEquip
    {
        get
        {
            if(Armor.HasShield)
            {
                return false;
            }
            return guns.Count < 2;
        }
    }

    public void DropDuoGun()
    {
        if(Gun && Gun.wieldMode == Gun.GunWieldMode.Dual)
        {
            DropGun();
        }
        else if(OtherGun && OtherGun.wieldMode == Gun.GunWieldMode.Dual)
        {
            DropOtherGun();
        }
    }

    public void DropGun()
    {
        if (guns.Count == 0) return;
        
        ItemManager.DropGun(transform.position, guns[index].gun.name);

        //Gun = OtherGun;
        //GunHealth = OtherGunHealth;

        guns.RemoveAt(index);
        index = 0;
        Sound.PlaySound(switchSound, "Gun Shots");
        UpdateSprite();
    }

    public void DropOtherGun()
    {
        if (!OtherGun) return;

        int otherIndex = index == 0 ? 1 : 0;
        
        ItemManager.DropGun(transform.position, guns[otherIndex].gun.name);

        //Gun = OtherGun;
        //GunHealth = OtherGunHealth;

        guns.RemoveAt(otherIndex);
        index = 0;
        Sound.PlaySound(switchSound, "Gun Shots");
        UpdateSprite();
    }

    public void Equip(string gunName)
    {
        Gun gunToAdd = ItemManager.GetGun(gunName);
        if (guns.Count == 2)
        {
            Gun = gunToAdd;
        }
        else if(guns.Count == 1)
        {
            if(Armor.HasShield)
            {
                Gun = gunToAdd;
            }
            else
            {
                index = 1;
                guns.Add(new GunSlot(gunToAdd));
            }
        }
        else
        {
            guns.Add(new GunSlot(gunToAdd));
        }

        lastGun = null;
        GunChanged();
    }

    void UpdateSprite()
    {
        if (guns.Count == 0)
        {
            gunSprite.sprite = null;
            otherGun.sprite = null;
            gunDualSprite.sprite = null;
            gunShadow.sprite = null;

            gunDualSpriteOutline.sprite = null;
            gunSpriteOutline.sprite = null;
            otherGunOutline.sprite = null;

            return;
        }

        if(gunSprite)
        {
            gunSprite.sprite = Gun.icon;
            gunShadow.sprite = gunSprite.sprite;
            gunShadow.color = new Color(0f, 0f, 0f, 0.25f);

            if(Gun.icon)
            {
                gunSpriteOutline.sprite = ObjectManager.GetSpriteOutline(Gun.icon.name);
            }

            gunDualSprite.enabled = Gun.wieldMode != Gun.GunWieldMode.Single;
            if(gunDualSprite.enabled)
            {
                gunDualSpriteOutline.sprite = gunSpriteOutline.sprite;
            }
            else
            {
                gunDualSpriteOutline.sprite = null;
            }
        }

        if(otherGun)
        {
            if(OtherGun)
            {
                otherGun.sprite = OtherGun.icon;
                otherGun.color = new Color(0.5f, 0.5f, 0.5f, 1f);

                otherGunOutline.sprite = ObjectManager.GetSpriteOutline(OtherGun.icon.name);
                otherGunOutline.flipX = otherGun.flipX;
                otherGunOutline.flipY = otherGun.flipY;
            }
            else
            {
                otherGun.sprite = null;
                otherGunOutline.sprite = null;
            }
        }
    }

    public int Ammo
    {
        get
        {
            if (!Gun) return -1;

            for(int i = 0; i < ammo.Count;i++)
            {
                if(ammo[i].type == Gun.ammoType)
                {
                    return ammo[i].ammo;
                }
            }
            return -1;
        }
        set
        {
            for (int i = 0; i < ammo.Count; i++)
            {
                if ((Gun && ammo[i].type == Gun.ammoType) || !Gun)
                {
                    ammo[i].ammo = value;
                }
            }
        }
    }

    public int OtherAmmo
    {
        get
        {
            if (!OtherGun) return -1;

            for (int i = 0; i < ammo.Count; i++)
            {
                if (ammo[i].type == OtherGun.ammoType)
                {
                    return ammo[i].ammo;
                }
            }
            return -1;
        }
        set
        {
            for (int i = 0; i < ammo.Count; i++)
            {
                if ((OtherGun && ammo[i].type == OtherGun.ammoType) || !OtherGun)
                {
                    ammo[i].ammo = value;
                }
            }
        }
    }

    public int OtherMaxAmmo
    {
        get
        {
            if (!OtherGun) return -1;

            for (int i = 0; i < ammo.Count; i++)
            {
                if (ammo[i].type == OtherGun.ammoType)
                {
                    return ammo[i].max;
                }
            }
            return -1;
        }
        set
        {
            for (int i = 0; i < ammo.Count; i++)
            {
                if ((OtherGun && ammo[i].type == OtherGun.ammoType) || !OtherGun)
                {
                    ammo[i].max = value;
                }
            }
        }
    }

    public int MaxAmmo
    {
        get
        {
            if (!Gun) return -1;

            for (int i = 0; i < ammo.Count; i++)
            {
                if (ammo[i].type == Gun.ammoType)
                {
                    return ammo[i].max;
                }
            }
            return -1;
        }
        set
        {
            for (int i = 0; i < ammo.Count; i++)
            {
                if ((Gun && ammo[i].type == Gun.ammoType) || !Gun)
                {
                    ammo[i].max = value;
                }
            }
        }
    }

    void GunHandling()
    {
        Gun.GunHandling.GunHandleType mode = Gun.GunHandling.GunHandleType.None;
        if (Gun)
        {
            gunDualSprite.enabled = Gun.wieldMode != Gun.GunWieldMode.Single;
            mode = Gun.handle.type;
        }

        if (!gunSprite.sprite)
        {
            mode = Gun.GunHandling.GunHandleType.None;
        }

        if (gunDualSprite.enabled)
        {
            if (mode == Gun.GunHandling.GunHandleType.DoubleHandedGrip || mode == Gun.GunHandling.GunHandleType.DoubleHandedBarell || mode == Gun.GunHandling.GunHandleType.DoubleHandedMinigun)
            {
                mode = Gun.GunHandling.GunHandleType.SingleHanded;
            }
            else if(mode == Gun.GunHandling.GunHandleType.DoubleHandedMelee)
            {
                mode = Gun.GunHandling.GunHandleType.SingleHandedMelee;
            }
        }

        bool lookingLeft = direction.x < 0f;

        hand1.flipY = gunSprite.flipY;
        hand1.flipX = gunSprite.flipX;

        hand2.flipY = gunSprite.flipY;
        hand2.flipX = gunSprite.flipX;

        hand3.flipY = gunSprite.flipY;
        hand3.flipX = gunSprite.flipX;

        hand4.flipY = gunSprite.flipY;
        hand4.flipX = gunSprite.flipX;

        hand5.flipY = gunSprite.flipY;
        hand5.flipX = gunSprite.flipX;

        if (mode == Gun.GunHandling.GunHandleType.SingleHanded)
        {
            hand1.enabled = false;
            hand2.enabled = true;
            hand3.enabled = false;
            hand4.enabled = false;
            hand5.enabled = false;
        }
        else if (mode == Gun.GunHandling.GunHandleType.SingleHandedMelee)
        {
            hand1.enabled = true;
            hand2.enabled = false;
            hand3.enabled = false;
            hand4.enabled = false;
            hand5.enabled = false;
        }
        else if (mode == Gun.GunHandling.GunHandleType.DoubleHandedBarell)
        {
            hand1.enabled = true;
            hand2.enabled = true;
            hand3.enabled = false;
            hand4.enabled = false;
            hand5.enabled = false;
        }
        else if (mode == Gun.GunHandling.GunHandleType.DoubleHandedGrip)
        {
            hand1.enabled = false;
            hand2.enabled = true;
            hand3.enabled = true;
            hand4.enabled = false;
            hand5.enabled = false;
        }
        else if (mode == Gun.GunHandling.GunHandleType.DoubleHandedMinigun)
        {
            hand1.enabled = false;
            hand2.enabled = false;
            hand3.enabled = true;
            hand4.enabled = true;
            hand5.enabled = false;
        }
        else if (mode == Gun.GunHandling.GunHandleType.DoubleHandedMelee)
        {
            hand1.enabled = false;
            hand2.enabled = true;
            hand3.enabled = false;
            hand4.enabled = false;
            hand5.enabled = true;
        }
        else if (mode == Gun.GunHandling.GunHandleType.None)
        {
            hand1.enabled = false;
            hand2.enabled = false;
            hand3.enabled = false;
            hand4.enabled = false;
            hand5.enabled = false;
        }

        if (mode != Gun.GunHandling.GunHandleType.None)
        {
            Vector2 extra = new Vector2(gunSprite.sprite.rect.width % 2 == 0 ? 0f : 0.5f, gunSprite.sprite.rect.height % 2 == 0 ? 0f : 0.5f);

            hand2.transform.position = gunSprite.transform.TransformPoint(Gun.handle.hand1Position.x.RoundToInt() + extra.x, (Gun.handle.hand1Position.y.RoundToInt() + extra.y) * (lookingLeft ? -1f : 1f), 0f);

            hand3.transform.position = gunSprite.transform.TransformPoint(Gun.handle.hand2Position.x.RoundToInt() + extra.x, (Gun.handle.hand2Position.y.RoundToInt() + extra.y) * (lookingLeft ? -1f : 1f), 0f);
            hand4.transform.position = gunSprite.transform.TransformPoint(Gun.handle.hand1Position.x.RoundToInt() + extra.x, (Gun.handle.hand1Position.y.RoundToInt() + extra.y) * (lookingLeft ? -1f : 1f), 0f);
            hand1.transform.position = gunSprite.transform.TransformPoint(Gun.handle.hand2Position.x.RoundToInt() + extra.x, (Gun.handle.hand2Position.y.RoundToInt() + extra.y) * (lookingLeft ? -1f : 1f), 0f);

            if (mode == Gun.GunHandling.GunHandleType.DoubleHandedMelee)
            {
                if (Gun.meleeType == MeleeType.Poke)
                {
                    hand5.flipX = lookingLeft;
                    hand5.transform.position = gunSprite.transform.TransformPoint((Gun.handle.hand2Position.y.RoundToInt() + extra.x) * (lookingLeft ? 1f : -1f), (Gun.handle.hand2Position.x.RoundToInt() + extra.y), 0f);

                    hand2.flipY = lookingLeft;
                    hand2.flipX = true;
                    hand2.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                    hand2.transform.position = gunSprite.transform.TransformPoint((Gun.handle.hand1Position.y.RoundToInt() + extra.x) * (lookingLeft ? 1f : -1f), (Gun.handle.hand1Position.x.RoundToInt() + extra.y) * (lookingLeft ? 1f : 1f), 0f);
                }
                else
                {
                    hand5.transform.position = gunSprite.transform.TransformPoint(Gun.handle.hand2Position.x.RoundToInt() + extra.x, (Gun.handle.hand2Position.y.RoundToInt() + extra.y) * (lookingLeft ? -1f : 1f), 0f);
                    hand1.flipY = lookingLeft;
                    hand1.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                    hand1.transform.position = gunSprite.transform.TransformPoint((Gun.handle.hand1Position.y.RoundToInt() + extra.x) * (lookingLeft ? 1f : -1f), (Gun.handle.hand1Position.x.RoundToInt() + extra.y) * (lookingLeft ? 1f : 1f), 0f);
                }
            }
            else if (mode == Gun.GunHandling.GunHandleType.SingleHandedMelee)
            {
                if (Gun.meleeType == MeleeType.Poke)
                {
                    hand1.flipY = lookingLeft;
                    hand1.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                    hand1.transform.position = gunSprite.transform.TransformPoint((Gun.handle.hand1Position.y.RoundToInt() + extra.x) * (lookingLeft ? 1f : -1f), (Gun.handle.hand1Position.x.RoundToInt() + extra.y) * (lookingLeft ? 1f : 1f), 0f);
                }
                else
                {
                    hand1.flipX = lookingLeft;
                    hand1.flipY = true;

                    hand1.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
                    hand1.transform.position = gunSprite.transform.TransformPoint((Gun.handle.hand1Position.x.RoundToInt() + extra.x) * (lookingLeft ? 1f : 1f), (Gun.handle.hand1Position.y.RoundToInt() + extra.y) * (lookingLeft ? -1f : 1f), 0f);
                }
            }
            else
            {
                hand2.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
                hand1.transform.localEulerAngles = new Vector3(0f, 0f, 0f);

                hand5.transform.position = gunSprite.transform.TransformPoint(Gun.handle.hand2Position.x.RoundToInt() + extra.x, (Gun.handle.hand2Position.y.RoundToInt() + extra.y) * (lookingLeft ? -1f : 1f), 0f);
                hand1.flipY = lookingLeft;
                hand1.transform.position = gunSprite.transform.TransformPoint(Gun.handle.hand2Position.x.RoundToInt() + extra.x, (Gun.handle.hand2Position.y.RoundToInt() + extra.y) * (lookingLeft ? -1f : 1f), 0f);
            }
        }
    }

    void Kickback()
    {
        if (guns.Count == 0) return;
        float amount = Gun.shooterKnockback;

        if (Character.isPlayer)
        {
            HUD.Shake(Gun.attack.screenshake);
        }

        if(Movement) Movement.Knockback(Look.lookDirection * -1f, amount);

        if (gunSprite)
        {
            if (Gun.type == GunType.Melee && Gun.meleeType == MeleeType.Poke)
            {
                if (Gun.wieldMode == Gun.GunWieldMode.Dual)
                {
                    if (alternateShot)
                    {
                        gunDualSprite.transform.localPosition += Vector3.up * amount * 3f;
                    }
                    else
                    {
                        gunSprite.transform.localPosition += Vector3.up * amount * 3f;
                    }
                }
                else
                {
                    gunSprite.transform.localPosition += Vector3.up * amount * 3f;
                }
            }
            else if(Gun.type != GunType.Melee)
            {
                if (Gun.wieldMode == Gun.GunWieldMode.Dual)
                {
                    if (alternateShot)
                    {
                        if (Gun.kickbackMode == Gun.GunKickbackStyle.Blowback) swingAmount = 45f * (direction.x < 0f ? -1f : 1f);
                        if (Gun.kickbackMode == Gun.GunKickbackStyle.Kickback) gunDualSprite.transform.localPosition -= Vector3.right * amount;
                    }
                    else
                    {
                        if (Gun.kickbackMode == Gun.GunKickbackStyle.Blowback) swingAmount = 45f * (direction.x < 0f ? -1f : 1f);
                        if (Gun.kickbackMode == Gun.GunKickbackStyle.Kickback) gunSprite.transform.localPosition -= Vector3.right * amount;
                    }
                }
                else
                {
                    if (Gun.kickbackMode == Gun.GunKickbackStyle.Blowback) swingAmount = 45f * (direction.x < 0f ? -1f : 1f);
                    if (Gun.kickbackMode == Gun.GunKickbackStyle.Kickback) gunSprite.transform.localPosition -= Vector3.right * amount;
                }
            }
        }
        if (Gun.type == GunType.Melee && Gun.meleeType == MeleeType.Swing)
        {
            if(Mathf.Abs(swingToAmount) == 120f)
            {
                swingToAmount = 0f;
            }
            else
            {
                swingToAmount = -120f * (gunSprite.flipY ? -1f : 1f);
            }
        }
    }

    void UpdateMultipliers()
    {
        if(Inventory)
        {
            multipliers.fireRateMultiplier = Inventory.GetMultiplier("GunFireRate");
            multipliers.damageMultiplier = Inventory.GetMultiplier("GunDamage");
            multipliers.recoilMultiplier = Inventory.GetMultiplier("GunRecoil");
            multipliers.sizeMultiplier = Inventory.GetMultiplier("ProjectileSize");
            multipliers.speedMultiplier = Inventory.GetMultiplier("ProjectileSpeed");

            if (Gun.type == GunType.Laser || Gun.type == GunType.Electricity || Gun.type == GunType.ConstantLaser)
            {
                multipliers.sizeMultiplier = Inventory.GetMultiplier("LaserSize");
                multipliers.damageMultiplier = Inventory.GetMultiplier("LaserDamage");
                multipliers.fireRateMultiplier = Inventory.GetMultiplier("LaserFireRate");
            }

            if (Inventory.HasQuality("StressFireRate"))
            {
                float ratio = (float)Health.hp / (float)Health.maxHp;
                multipliers.fireRateMultiplier *= ratio.Remap(0f, 1f, Inventory.GetMultiplier("StressFireRate"), 1f);
            }
        }
    }

    void FireShot()
    {
        if (guns.Count == 0) return;

        int shotsToFire = Gun.shots;

        UpdateMultipliers();

        if (Inventory)
        {
            shotsToFire = Mathf.RoundToInt(shotsToFire * Inventory.GetMultiplier("GunShots"));
            if (Gun.ammoType != "None" && !infiniteAmmo) Ammo -= Mathf.RoundToInt(Gun.ammoCost * Inventory.GetMultiplier("GunAmmoCost"));
        }
        else
        {
            Ammo -= Gun.ammoCost;
        }

        alternateShot = !alternateShot;

        Kickback();

        Vector3 extraOffset = alternateShot && Gun.wieldMode == Gun.GunWieldMode.Dual ? Vector2.up * 5f : Vector2.zero;
        
        Vector3 spawnPos = transform.position;
        if (gunSprite && Gun.type != GunType.Melee) spawnPos = gunSprite.transform.position;
        Vector3 gunSpawn = transform.position;
        if (gunSprite && Gun.type != GunType.Melee) gunSpawn = transform.position;
        
        int recursionRotation = Random.Range(0, 360);
        if(Gun.type == GunType.Electricity)
        {
            shotsToFire = 1;
        }

        if (Inventory && Inventory.GetState("Criticals"))
        {
            int critChance = Inventory.GetMultiplier("CiriticalChance").RoundToInt();
            if (Random.Range(0, 100) < critChance)
            {
                int critDamage = Inventory.GetMultiplier("CriticalDamage").RoundToInt();

                if (Gun.damage == 0)
                {
                    multipliers.damageMultiplier *= (critDamage + Gun.damage);
                }
                else
                {
                    multipliers.damageMultiplier *= (critDamage + Gun.damage) / Gun.damage;
                }

                Sound.PlaySound("Pistol4", "Gun Shots");
            }
        }

        for (int i = 0; i < shotsToFire; i++)
        {
            GameObject newBullet = PoolManager.PoolInstantiate(Gun.GetProjectile(), gunSpawn + (Vector3)direction.normalized * 6f, Quaternion.identity);
            GunProjectile gunProjectile = newBullet.GetComponent<GunProjectile>();
            gunProjectile.SetRecursion(0, recursionRotation);
            gunProjectile.Initialize(this, i);

            if (Gun.attack.shootEffect && Gun.type != GunType.Melee)
            {
                oldEffect = PoolManager.PoolInstantiate(Gun.attack.shootEffect, spawnPos + extraOffset + (Vector3)direction.normalized * 6f, Quaternion.identity);
                if (oldEffect)
                {
                    ParticleSystem part = oldEffect.GetComponent<ParticleSystem>();
                    ParticleSystem.MainModule main = part.main;
                    main.startColor = Gun.color;

                    ParticleSystem.VelocityOverLifetimeModule vel = part.velocityOverLifetime;
                    vel.x = new ParticleSystem.MinMaxCurve(direction.normalized.x * 100, direction.normalized.x * 400);
                    vel.y = new ParticleSystem.MinMaxCurve(direction.normalized.y * 100, direction.normalized.y * 400);

                    oldEffect.transform.SetParent(gunSprite ? gunSprite.transform : transform);
                    
                    oldEffect.transform.localPosition = Gun.attack.shootOffset;
                }
            }
        }

        if(Gun.shellEjection)
        {
            GameObject shellObject = PoolManager.PoolInstantiate(Gun.shellName, transform.position, transform.rotation);
            if (shellObject)
            {
                Rigidbody2D rigidbody = shellObject.GetComponent<Rigidbody2D>();
                if (rigidbody)
                {
                    rigidbody.velocity = (Random.insideUnitCircle - direction.normalized * 4f) * 32f;
                }
            }
        }

        if(Character.isPlayer)
        {
            Settings.Game.shots += shotsToFire;
        }

        if (Ammo < 0)
        {
            Ammo = 0;
        }
    }

    bool lastSwitch;

    public void Hit(Health victim)
    {
        Sound.PlaySound(Gun.attack.successSound, "Gun Shots");
    }

    public void Killed(Health victim)
    {
        if(Inventory) Inventory.UpdateKilled(victim);
    }

    bool alternateShot;

    void SetGunPos(Vector3 pos, bool lerp = true)
    {
        float lerpAmount = Gun.wieldMode == Gun.GunWieldMode.Dual ? 6f : 3f;
        float dualSpriteX = 0f;
        if(direction.y < 0f)
        {
            dualSpriteX = direction.normalized.y * (direction.x < 0f ? 6f : -6f);
        }
        if(lerp)
        {
            gunSprite.transform.localPosition = Vector3.Lerp(gunSprite.transform.localPosition, pos, Time.deltaTime * lerpAmount * 2f);
            gunDualSprite.transform.position = Vector3.Lerp(gunDualSprite.transform.position, gunSprite.transform.position + Vector3.up * 5f + Vector3.right * dualSpriteX, Time.deltaTime * lerpAmount * 4f);
        }
        else
        {
            gunSprite.transform.localPosition = pos;
            gunDualSprite.transform.position = gunSprite.transform.position + Vector3.up * 5f + Vector3.right * dualSpriteX;
        }
    }

    void GunChanged()
    {
        if (!Gun) return;

        Sound.PlaySound(switchSound, "Gun Shots");
        Sound.GetSource("Gun Idle").clip = Gun.attack.equipSound;
        Sound.GetSource("Gun Idle").loop = false;
        Sound.GetSource("Gun Idle").Play();
        UpdateSprite();

        nextBurst = 0f;
        UpdateMultipliers();

        burst = Gun.bursts;
        InterfaceAlmanacLayer.Found(Gun);
    }

    bool frozen;

    public void Freeze(bool freeze)
    {
        frozen = freeze;
    }

    Vector3 lookPos;

    void Update()
    {
        if (frozen) return;

        if (attack)
        {
            if (Gun.canCharge)
            {
                charge += Time.deltaTime + Gun.chargeDuration;
            }
            else
            {
                Attack();
            }
        }
        else
        {
            if (charge != 0f)
            {
                burst = 0;
            }
            charge = 0f;
        }

        direction = Look.lookDirection;
        if(Character && Character.input.changeGun)
        {
            if(guns.Count > 1)
            {
                index = index == 0 ? 1 : 0;
            }
        }

        lookPos = transform.position;
        lookPos += (Vector3)direction.normalized * 12f;

        if (GeneratorManager.Generating) return;

        if (Sound.GetSource("Gun Shots"))
        {
            Sound.GetSource("Gun Shots").pitch = Random.Range(0.95f, 1.05f);
        }

        if(gunSprite && gunSpriteOutline)
        {
            gunSpriteOutline.transform.localPosition = gunSprite.transform.localPosition;
            gunSpriteOutline.transform.localRotation = gunSprite.transform.localRotation;
            gunSpriteOutline.color = Color.black;
            gunSpriteOutline.sortingOrder = gunSprite.sortingOrder + 1;
            gunSpriteOutline.sortingLayerName = gunSprite.sortingLayerName;
            gunSpriteOutline.flipX = gunSprite.flipX;
            gunSpriteOutline.flipY = gunSprite.flipY;
        }
        if (gunDualSprite && gunDualSpriteOutline)
        {
            gunDualSpriteOutline.transform.localPosition = gunDualSprite.transform.localPosition;
            gunDualSpriteOutline.transform.localRotation = gunDualSprite.transform.localRotation;
            gunDualSpriteOutline.color = Color.black;
            gunDualSpriteOutline.sortingOrder = gunDualSprite.sortingOrder + 1;
            gunDualSpriteOutline.sortingLayerName = gunDualSprite.sortingLayerName;
            gunDualSpriteOutline.flipX = gunDualSprite.flipX;
            gunDualSpriteOutline.flipY = gunDualSprite.flipY;
        }

        if (gunSprite && gunShadow)
        {
            gunShadow.flipY = gunSprite.flipY;
            gunShadow.transform.position = gunSprite.transform.position + Vector3.down * 3;
            gunShadow.sortingOrder = gunSprite.sortingOrder - 1;
            gunShadow.transform.rotation = gunSprite.transform.rotation;

            gunDualSprite.flipX = gunSprite.flipX;
            gunDualSprite.flipY = gunSprite.flipY;

            gunDualSprite.sprite = gunSprite.sprite;
            gunDualSprite.color = Color.gray;
            gunDualSprite.sortingOrder = 0;

            gunDualSprite.transform.rotation = gunSprite.transform.rotation;

            if (otherGun)
            {
                if (OtherGun)
                {
                    otherGun.flipY = !gunShadow.flipY;
                    otherGun.flipX = !gunShadow.flipY;
                    otherGun.sortingOrder = -2;

                    if (OtherGun.type == GunType.Melee)
                    {
                        otherGun.transform.localEulerAngles = new Vector3(0, 0, 170 * (otherGun.flipY ? -1 : 1));
                        otherGun.transform.localPosition = new Vector3(!otherGun.flipY ? otherGun.sprite.rect.width / 2f : -otherGun.sprite.rect.width / 2f, otherGun.sprite.rect.height / 2f, 0);
                    }
                    else
                    {
                        otherGun.transform.localEulerAngles = new Vector3(0, 0, 80 * (otherGun.flipY ? -1 : 1));
                        otherGun.transform.localPosition = new Vector3(!otherGun.flipY ? otherGun.sprite.rect.height / 2f : -otherGun.sprite.rect.height / 2f, otherGun.sprite.rect.width / 2f, 0);
                    }
                    otherGun.transform.localScale = new Vector3(1, (otherGun.flipY ? 1 : -1), 1);
                }
            }

            if (otherGun && otherGunOutline)
            {
                otherGunOutline.transform.localPosition = otherGun.transform.localPosition;
                otherGunOutline.transform.localRotation = otherGun.transform.localRotation;
                otherGunOutline.color = Color.black;
                otherGunOutline.sortingOrder = otherGun.sortingOrder + 1;
                otherGunOutline.sortingLayerName = otherGun.sortingLayerName;
                otherGunOutline.flipX = otherGun.flipX;
                otherGunOutline.flipY = otherGun.flipY;
                otherGunOutline.transform.localScale = otherGun.transform.localScale;
            }
        }

        if (Gun && Sound.GetSource("Gun Idle").clip != Gun.attack.idleSound && !Sound.GetSource("Gun Idle").isPlaying)
        {
            Sound.GetSource("Gun Idle").clip = Gun.attack.idleSound;
            Sound.GetSource("Gun Idle").loop = true;
            Sound.GetSource("Gun Idle").Play();
        }

        GunHandling();

        if (Gun && Gun.type == GunType.Melee)
        {
            if(Gun.meleeType == MeleeType.Swing)
            {
                swingAmount = Mathf.Lerp(swingAmount, swingToAmount, Time.deltaTime * 8f * Gun.meleeSwingSpeed);
            }
            else
            {
                swingAmount = 0f;
                //gunSprite.flipX = true;
            }
        }
        else
        {
            swingAmount = Mathf.Lerp(swingAmount, 0f, Time.deltaTime * 8f);
            if (gunSprite) gunSprite.flipX = false;
        }

        if(Gun && Gun.ammoCost > 0 && Ammo > MaxAmmo)
        {
            Ammo = MaxAmmo;
        }
        
        if(lastGun != Gun)
        {
            lastGun = Gun;
            GunChanged();
        }

        if (gunSprite && Gun)
        {
            if (Gun.type == GunType.Melee)
            {
                if (Gun.meleeType == MeleeType.Swing)
                {
                    gunSprite.flipY = direction.x < 0f;
                    gunSprite.transform.parent.LookAt2D(lookPos, (gunSprite.flipY ? 45f : 135f) + swingAmount);

                    if (direction.x > 0f != lastSwitch)
                    {
                        lastSwitch = !lastSwitch;
                        if (lastSwitch)
                        {
                            if (swingToAmount == 120)
                            {
                                swingToAmount = 0;
                                swingAmount = 0;
                            }
                            SetGunPos(Gun.holdPosition, false);
                        }
                        else
                        {
                            if (swingToAmount == -120)
                            {
                                swingToAmount = 0;
                                swingAmount = 0;
                            }
                            SetGunPos(new Vector2(Gun.holdPosition.x, Gun.holdPosition.y * -1f), false);
                        }
                    }

                    if (direction.x > 0f)
                    {
                        SetGunPos(Gun.holdPosition);
                    }
                    else
                    {
                        SetGunPos(new Vector2(Gun.holdPosition.x, Gun.holdPosition.y * -1f));
                    }
                }
                else
                {
                    gunSprite.flipX = direction.x > 0f;
                    gunSprite.flipY = false;
                    gunSprite.transform.parent.LookAt2D(lookPos, 0f);

                    if (direction.x > 0f)
                    {
                        SetGunPos(new Vector2(Gun.holdPosition.y * -1f, Gun.holdPosition.x));
                    }
                    else
                    {
                        SetGunPos(new Vector2(Gun.holdPosition.y, Gun.holdPosition.x));
                    }
                }
            }
            else
            {
                if (direction.x > 0f != lastSwitch)
                {
                    lastSwitch = !lastSwitch;
                    if (lastSwitch)
                    {
                        SetGunPos(Gun.holdPosition, false);
                    }
                    else
                    {
                        SetGunPos(new Vector2(Gun.holdPosition.x, Gun.holdPosition.y * -1f), false);
                    }
                }
                if (direction.x > 0f)
                {
                    SetGunPos(Gun.holdPosition);
                }
                else
                {
                    SetGunPos(new Vector2(Gun.holdPosition.x, Gun.holdPosition.y * -1f));
                }

                gunSprite.flipY = direction.x < 0f;
                gunSprite.transform.parent.LookAt2D(lookPos, 90f + swingAmount);
            }
        }

        
        if(Time.time > nextBurst && Gun)
        {
            if (burst < Gun.bursts)
            {
                nextNotShooting = Time.time + Gun.fireRate + Time.deltaTime * 5f;
                shooting = true;

                nextBurst = Time.time + Gun.burstRate * multipliers.fireRateMultiplier;
                guns[index].nextShot = Time.time + Gun.fireRate * multipliers.fireRateMultiplier;
                
                FireShot();
                
                burst++;

                if (!Gun) return;

                if(Gun.shellEjection)
                {
                    Sound.GetSource("Gun Shells").pitch = Random.Range(0.9f, 1.1f);
                    Sound.PlaySound(Gun.shellSound, "Gun Shells");
                }

                if((firstShot || Gun.type != GunType.ConstantLaser) && Gun.attack.soundType == Gun.SoundType.Single)
                {
                    PlayShootSound();
                    firstShot = false;
                }
                else
                {
                    if(Sound.GetSource("Gun Shots").clip != Gun.attack.startSound && !Sound.GetSource("Gun Shots").loop)
                    {
                        PlayShootSound();
                    }
                }
            }
        }

        if(Gun && Gun.attack.soundType == Gun.SoundType.Loop)
        {
            if (shooting)
            {
                if(!Sound.GetSource("Gun Shots").loop)
                {
                    Sound.GetSource("Gun Shots").loop = true;
                    Sound.GetSource("Gun Shots").clip = Gun.attack.loopSound;
                    Sound.GetSource("Gun Shots").Play();
                }
            }
            else
            {
                if(Sound.GetSource("Gun Shots").clip != Gun.attack.endSound)
                {
                    Sound.GetSource("Gun Shots").clip = Gun.attack.endSound;
                    Sound.GetSource("Gun Shots").loop = false;
                    Sound.GetSource("Gun Shots").Play();
                }
            }
        }

        if(Gun && !Gun.automatic && !Inventory.GetState("GunForceAuto") && Gun.type != GunType.ConstantLaser)
        {
            if(Time.time > nextUnshoot)
            {
                semiAutoShoot = true;
            }
        }
        if(Time.time > nextNotShooting)
        {
            shooting = false;
        }
    }

    public void PlayShootSound()
    {
        if (!Sound.GetSource("Gun Shots")) return;

        Sound.GetSource("Gun Shots").clip = Gun.attack.startSound;
        Sound.GetSource("Gun Shots").loop = false;
        Sound.GetSource("Gun Shots").PlayOneShot(Gun.attack.startSound);
    }

    bool semiAutoShoot;
    float nextUnshoot;
    bool shooting = false;
    float nextNotShooting;
    public bool attack;
    public bool firstShot;
    float charge;

    void Attack()
    {
        if (GeneratorManager.Generating) return;
        if (guns.Count == 0) return;
        if (guns[index] == null) return;
        if (Armor && Armor.HasShield && Armor.ShieldEnabled) return;

        bool forceAuto = Inventory && Inventory.GetState("GunForceAuto");
        bool shootNow = false;
        if (!Gun.automatic && !forceAuto && Gun.type != GunType.ConstantLaser)
        {
            shootNow = semiAutoShoot;
            nextUnshoot = Time.time + Time.deltaTime;
        }
        else
        {
            shootNow = true;
        }

        bool ignoreAmmo = Gun.ammoType == "None" || Gun.ammoCost == 0 || infiniteAmmo;
        if (Time.time > guns[index].nextShot)
        {
            if(((Ammo > 0 && !ignoreAmmo) || ignoreAmmo) && shootNow)
            {
                burst = 0;

                if (!Gun.automatic && !forceAuto)
                {
                    semiAutoShoot = false;
                }
            }
            else if(Ammo == 0 && !ignoreAmmo)
            {
                guns[index].nextShot = Time.time + 0.25f;

                Sound.GetSource("Gun Shots").pitch = Random.Range(0.95f, 1.05f);
                Sound.GetSource("Gun Shots").PlayOneShot(Gun.attack.dryFireSound);
                
                UIManager.DrawNotificationText(Helper.RandomID, transform.position, "<color=red>EMPTY!</color>", 0.25f, "console");
            }
        }
    }
}
