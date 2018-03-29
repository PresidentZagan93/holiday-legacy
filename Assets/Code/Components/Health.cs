using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Data;

public interface IKillable
{
    void Died();
}

public class Health : GameBehaviour {

    public delegate void OnDied();
    public OnDied onDied;

    public delegate void OnDamage();
    public OnDamage onDamage;

    public delegate void OnHeal();
    public OnHeal onHeal;

    public GameTeam team = GameTeam.Evil;
    public bool selfDamage = true;

    public int screenshake = 4;
    public int hp = 100;
    public int maxHp = 10;
    public bool invincible = false;
    public bool destroyOnDeath = false;
    public bool takeContactDamage = true;

    public Health lastAttacker;
    public GameObject dieEffect;
    public AudioClip hitSound;

    public Vector2 effectsOffset = Vector2.zero;
    public Loot propsToDrop;
    
    GeneratorBlockObject blockObject;
    public DeathCause deathCause;

    public bool IsDead
    {
        get
        {
            return hp <= 0;
        }
    }

    private void Start()
    {
        blockObject = GetComponent<GeneratorBlockObject>();
    }

    public override void Awake()
    {
        base.Awake();

        ObjectManager.Add(this);

        AwakeEffects();

        if(Sound) Sound.CreateSource("Health", AudioManager.AudioType.Health);

        maxHp = hp;
    }

    private void OnEnable()
    {
        ObjectManager.Add(this);
    }

    void OnDestroy()
    {
        if(Application.isPlaying)
        {
            ObjectManager.Remove(this);
        }
    }
    
    void Update()
    {
        UpdateEffects();
    }

    void Died()
    {
        if (Animations) Animations.Freeze(false);
        ClearAllEffects();
        if(!Character)
        {
            //must be a propr
            if(SpritePlayer) SpritePlayer.Play("Dead", true);
            Collider2D col = GetComponent<Collider2D>();
            if(col)
            {
                col.enabled = false;
            }
        }

        if (lastAttacker != null)
        {
            if(propsToDrop)
            {
                ObjectManager.DropHealthProps(propsToDrop, lastAttacker.Character, transform.position);
            }
            lastAttacker.HealthKilled(this);
        }

        if (blockObject)
        {
            PoolManager.PoolInstantiate(blockObject.wallBreakPrefab, transform.position, Random.rotation);
            Generator.singleton.Modify(blockObject.block.x, blockObject.block.y, GeneratorBlockType.Floor);
        }
        else
        {
            PoolManager.PoolInstantiate(dieEffect, transform.position, Random.rotation);
            ObjectManager.Remove(this);
        }
    }

    void HealthHit(Health victim)
    {
        Character.CharacterHit(victim);
    }

    void HealthKilled(Health victim)
    {
        if (IsDead) return;

        Character.CharacterKilled(victim);
    }

    public void Heal(int amount)
    {
        if (IsDead) return;

        if (amount < 0f) amount = 0;

        int hpToAdd = (int)Mathf.RoundToInt(amount);

        if (hp + hpToAdd > maxHp)
        {
            hp = maxHp;
        }
        else
        {
            hp += hpToAdd;
        }

        if(onHeal != null)
        {
            onHeal();
        }
    }

    void SubHealthInternal(float amount)
    {
        if (IsDead) return;

        if (invincible) return;

        if (amount < 0f) return;

        if (Inventory && Random.Range(0, 100) < Inventory.GetMultiplier("IgnoreDamage")) return;

        int hpToRemove = Mathf.RoundToInt(amount);

        if (hp <= hpToRemove)
        {
            hp = 0;
            Died();
            if (Armor) Armor.RemoveAllArmor();
            if (onDied != null) onDied();
        }
        else
        {
            hp -= hpToRemove;
            if (onDamage != null) onDamage();
        }

        if (team == GameTeam.Good)
        {
            //the players got hit
            if(Character.isPlayer)
            {
                HUD.BloodyHit(hpToRemove * 0.5f);
            }
            HUD.Shake(hpToRemove * screenshake);
        }
        if (team == GameTeam.Evil)
        {
            //the evil team is hit
            GameManager.AddScore(5);
        }

        if (lastAttacker) lastAttacker.HealthHit(this);
        if (Sound && hitSound)
        {
            Sound.GetSource("Health").pitch = Random.Range(0.9f, 1.1f);
            Sound.GetSource("Health").clip = hitSound;
            Sound.GetSource("Health").Play();
        }
        if (blockObject) blockObject.Hurt();
        if (Animations) Animations.Hurt();
    }

    public void Damage(int amount, Character attacker)
    {
        if(attacker.Owner)
        {
            Damage(amount, attacker.Owner.GetComponent<Health>());
        }
        else
        {
            Damage(amount, attacker.GetComponent<Health>());
        }
    }

    public void Damage(int amount, Health attacker)
    {
        if (IsDead) return;

        if (invincible) return;

        float damageMp = 1f;
        if (attacker)
        {
            if (attacker.team == team && attacker != this) return;
            if (!selfDamage && attacker == this) return;
            
            lastAttacker = attacker;

            if (Inventory) damageMp = Inventory.GetMultiplier("DamageRecieveMultiplier");
            if (Movement)
            {
                Vector2 dir = transform.position - attacker.transform.position;
                Movement.Knockback(dir, amount * 5f);
            }
        }
        
        amount = Mathf.RoundToInt(amount * damageMp);
        SubHealthInternal(amount);
    }

    [System.Serializable]
    public class HealthEffect
    {
        public Quality quality;
        public float duration;
        public float tickRate;

        [System.NonSerialized]
        public float expireTime;
        [System.NonSerialized]
        public Health caller;
        [System.NonSerialized]
        public float nextTick;

        public HealthEffect(Quality quality, float expireTime, float tickRate, Health caller)
        {
            this.quality = quality;
            this.expireTime = expireTime;
            this.tickRate = tickRate;
            this.caller = caller;
        }
    }

    [System.Serializable]
    public class EffectObject
    {
        public string name;
        public Transform transform;
        public ParticleSystem[] particleSystems;
    }
    
    List<EffectObject> effects = new List<EffectObject>();
    public List<HealthEffect> effectsApplied = new List<HealthEffect>();

    void AwakeEffects()
    {
        Transform effectProp = Instantiate(ObjectManager.GetPrefab("HealthEffects")).transform;
        effectProp.SetParent(transform);
        effectProp.localPosition = effectsOffset;

        for (int i = 0; i < effectProp.childCount; i++)
        {
            Transform child = effectProp.GetChild(i);
            if (child.parent == effectProp)
            {
                try
                {
                    EffectObject newObject = new EffectObject()
                    {
                        particleSystems = child.GetComponentsInChildren<ParticleSystem>(),
                        name = child.name,
                        transform = child
                    };
                    effects.Add(newObject);
                }
                catch { }
            }
        }
    }

    public bool HasEffect(string name)
    {
        for (int i = 0; i < effectsApplied.Count; i++)
        {
            if (effectsApplied[i].quality.name == name) return true;
        }

        return false;
    }

    public void AddEffects(HealthEffect[] effects, Health caller)
    {
        for (int i = 0; i < effects.Length; i++)
        {
            AddEffect(effects[i], caller);
        }
    }

    public void AddEffect(HealthEffect effect, Health caller)
    {
        HealthEffect newEffect = new HealthEffect(effect.quality, Time.time + effect.duration, effect.tickRate, caller);
        int indexAt = -1;
        for (int i = 0; i < effectsApplied.Count; i++)
        {
            if (effectsApplied[i].quality.name == effect.quality.name)
            {
                float diff = Mathf.Abs(effectsApplied[i].expireTime - (Time.time + effect.duration));
                if (diff < Time.deltaTime)
                {
                    return;
                }
                indexAt = i;
            }
        }

        float ignoreCombust = Inventory ? Inventory.GetMultiplier("IgnoreCombust") : 0f;
        float ignoreFreeze = Inventory ? Inventory.GetMultiplier("IgnoreFreeze") : 0f;
        float ignoreSpeed = Inventory ? Inventory.GetMultiplier("IgnoreSpeed") : 0f;
        float ignoreAcceleration = Inventory ? Inventory.GetMultiplier("IgnoreAcceleration") : 0f;

        if (effect.quality.name == "EffectCombust" && Random.Range(0, 100) < ignoreCombust) return;
        if (effect.quality.name == "EffectFreeze" && Random.Range(0, 100) < ignoreFreeze) return;
        if (effect.quality.name == "PlayerSpeed" && Random.Range(0, 100) < ignoreSpeed) return;
        if (effect.quality.name == "PlayerAcceleration" && Random.Range(0, 100) < ignoreAcceleration) return;

        if (indexAt != -1)
        {
            newEffect = effectsApplied[indexAt];

            newEffect.expireTime += effect.duration;
            newEffect.tickRate = effect.tickRate;
            newEffect.caller = caller;
        }
        else
        {
            effectsApplied.Add(newEffect);
        }

        EffectApplied(newEffect);
    }

    public void ClearAllEffects()
    {
        for (int i = 0; i < effectsApplied.Count; i++)
        {
            EffectRemoved(effectsApplied[i]);
            effectsApplied.RemoveAt(i);
        }
    }

    void UpdateEffects()
    {
        for (int i = 0; i < effects.Count; i++)
        {
            bool has = HasEffect(effects[i].name);
            if (hp == 0) has = false;

            if (has)
            {
                if(!effects[i].transform.gameObject.activeSelf)
                {
                    effects[i].transform.gameObject.SetActive(has);
                }
            }
            else
            {
                if (effects[i].transform.gameObject.activeSelf)
                {
                    //effects[i].transform.gameObject.SetActive(has);
                }
            }


            for (int p = 0; p < effects[i].particleSystems.Length; p++)
            {
                ParticleSystem.EmissionModule emit = effects[i].particleSystems[p].emission;
                emit.enabled = has;
            }
        }

        for (int i = 0; i < effectsApplied.Count; i++)
        {
            if (Time.time > effectsApplied[i].expireTime)
            {
                EffectRemoved(effectsApplied[i]);
                effectsApplied.RemoveAt(i);
                break;
            }
            else if (Time.time > effectsApplied[i].nextTick)
            {
                effectsApplied[i].nextTick = Time.time + effectsApplied[i].tickRate;
                EffectTick(effectsApplied[i]);
            }
        }
    }

    void EffectApplied(HealthEffect effect)
    {
        if (effect.quality.name == "EffectFreeze")
        {
            if (SpritePlayer) SpritePlayer.Freeze(true);
            if (GunShooter) GunShooter.Freeze(true);
            if (Animations) Animations.Freeze(true);
        }
    }

    void EffectRemoved(HealthEffect effect)
    {
        if (effect.quality.name == "EffectFreeze")
        {
            if (SpritePlayer) SpritePlayer.Freeze(false);
            if (GunShooter) GunShooter.Freeze(false);
            if (Animations) Animations.Freeze(false);
        }
    }

    void EffectTick(HealthEffect effect)
    {
        if (effect.quality.name == "EffectDamage")
        {
            Damage(effect.quality.Int, effect.caller);
        }
    }
}
