using UnityEngine;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public enum GunType
{
    Laser,
    Projectiles,
    Sprays,
    Electricity,
    Melee,
    ConstantLaser
}

public enum MeleeType
{
    Poke,
    Swing
}

public enum ProjectileRotationMode
{
    Off,
    Random,
    Direction,
    Velocity
}

public interface IItem
{
    string GetName();
    string GetDescription();
    Sprite GetIcon();
    string GetItemType();
    int GetPrice();
    int GetID();
    Data.AudioSet GetAudio();
}

namespace Data
{
    [CreateAssetMenu]
    public class Gun : ScriptableObject, IItem
    {
        public int ID
        {
            get
            {
                return id;
            }
            set
            {
                if (!lockId)
                {
                    id = value;
                }
            }
        }

        public int id;
        public bool lockId = false;

        public int level = 1;
        public int price = 50;
        public bool ready = true;
        public bool enemyOnly = false;

        public GunType type = GunType.Projectiles;
        [AmmoType]
        public string ammoType = "Bullets";
        public Sprite icon;
        public string description = "";

        public bool colorRandom = false;
        public Color color = Color.white;

        public bool selfHarm = true;
        public float fireRate = 0.2f;
        public float fireDelay = 0f;
        public float recoil = 0f;
        public int ammoCost = 1;
        public bool evenRecoil = false;
        public bool automatic = false;

        public int bursts = 1;
        public float burstRate = 0.1f;
        public float shooterKnockback = 2f;

        [Range(1, 100)]
        public int shots = 1;
        public string projectile = "";
        public ProjectileRotationMode projectileRotation = ProjectileRotationMode.Off;
        public bool projectileArcs = false;
        public float projectileArcGravity = 5f;
        public float projectileArcUpwardsKick = 50f;
        public bool canCharge = false;
        public float chargeDuration = 2f;
        public GunMultipliers chargeMultiplier;
        public float slowdown = 1f;
        public int maxBounces = 3;
        public float lifeTime = 5f;
        public float speed = 30f;
        public float successChance = 100f;
        public int damage = 1;
        public float scale = 1f;
        public AnimationCurve sizeOverLifetime = new AnimationCurve();
        public GameObject hitEffect;
        public bool splash;
        public float splashRange = 32f;
        public int splashDamage = 1;
        public float meleeSwingSpeed = 3f;
        public bool meleeBreaks = true;
        public bool meleeReflects = false;
        public MeleeType meleeType = MeleeType.Swing;
        public float rotationOffset = 0f;
        public Vector2 holdPosition = new Vector2(7, 0);
        public GunAttack attack;
        public GunRecursion recursion;
        public Health.HealthEffect[] effects;
        public List<Quality> qualities = new List<Quality>();
        public Texture laserTexture;
        public bool shellEjection;
        public string shellName;
        public AudioClip shellSound;
        public GunHandling handle;
        public GunWieldMode wieldMode = GunWieldMode.Single;
        public GunKickbackStyle kickbackMode = GunKickbackStyle.Kickback;

        public enum GunWieldMode
        {
            Single,
            Dual
        }

        public enum GunKickbackStyle
        {
            Blowback,
            Kickback
        }

        [Serializable]
        public class GunHandling
        {
            public enum GunHandleType
            {
                SingleHanded,
                SingleHandedMelee,
                DoubleHandedBarell,
                DoubleHandedGrip,
                DoubleHandedMinigun,
                DoubleHandedMelee,
                None
            }

            public GunHandleType type = GunHandleType.SingleHanded;
            public Vector2 hand1Position = new Vector2(-6f, -3f);
            public Vector2 hand2Position = new Vector2(6f, -3f);
        }
        
        public enum SoundType
        {
            Loop,
            Single
        }

        [System.Serializable]
        public class GunAttack
        {
            public SoundType soundType = SoundType.Single;
            public float screenshake = 0f;
            public AudioClip equipSound;
            public AudioClip loopSound;
            public AudioClip startSound;
            public AudioClip endSound;
            public AudioClip idleSound;
            public AudioClip dryFireSound;
            public AudioClip successSound;
            public GameObject shootEffect;
            public Vector2 shootOffset = new Vector2(6, 0);
        }

        [System.Serializable]
        public class GunRecursion
        {
            public bool enabled = false;
            public int maxRecursions = 2;
            public int instancesPerRecursion = 3;
        }

        public Color Color
        {
            get
            {
                if (colorRandom)
                {
                    return new Color(Random.value, Random.value, Random.value);
                }
                else
                {
                    return color;
                }
            }
        }

        public int GetID()
        {
            return id;
        }

        public string GetProjectile()
        {
            if (type == GunType.Electricity) return "Projectile_Spark";
            if (type == GunType.Laser || type == GunType.ConstantLaser)
            {
                return "Projectile_Laser";
            }
            if (type == GunType.Melee)
            {
                if (meleeType == MeleeType.Swing)
                {
                    return "Projectile_Melee";
                }
                else
                {
                    return "Projectile_MeleeKnife";
                }
            }
            if (type == GunType.Projectiles)
            {
                return projectile;
            }
            if (type == GunType.Sprays) return projectile;

            return "";
        }

        public Sprite GetAmmoIcon()
        {
            if (ammoCost == 0 || ammoType == "None" || (this.type == GunType.Melee && ammoCost == 0))
            {
                return icon;
            }
            AmmoType type = ItemManager.GetAmmoType(ammoType);
            return type.sprite ? type.sprite : icon;
        }

        public Sprite GetIcon()
        {
            return icon;
        }

        public int GetPrice()
        {
            return price;
        }

        public string GetItemType()
        {
            return type.ToString();
        }

        public string GetName()
        {
            return name;
        }

        public string GetDescription()
        {
            string toRet = description;

            if ((type != GunType.Melee && ammoCost != 0) && ammoType.ToLower() != "none")
            {
                if (toRet == "")
                {
                    toRet = "Uses " + ammoType;
                }
                else
                {
                    toRet += "\n\nUses " + ammoType;
                }
                return toRet;
            }

            if (toRet == "") return "";
            return toRet;
        }

        public AudioSet GetAudio()
        {
            throw new NotImplementedException();
        }

        public bool GetState(string name)
        {
            for (int m = 0; m < qualities.Count; m++)
            {
                if (qualities[m].name == name)
                {
                    return qualities[m].Bool;
                }
            }

            return false;
        }
    }
}