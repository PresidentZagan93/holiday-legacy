using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : GameBehaviour {

    public delegate void OnLevelUp();
    public OnLevelUp onlevelUp;

    [System.Serializable]
    public class CharacterInput
    {
        public bool left;
        public bool right;
        public bool up;
        public bool down;
        public bool shoot;

        public bool showInventory;
        public bool activateItem;
        public bool pickupItem;
        public bool changeGun;
        public bool dropArmor;
    }

    public enum CharacterAttackMode
    {
        Input,
        TargetVisibility,
        Constant
    }

    public enum CharacterInputMode
    {
        User,
        AI
    }

    [SerializeField]
    Character owner;
    public bool isDead = false;
    public bool isBoss = false;
    public bool isPlayer = false;
    public bool disableOffscreen = true;
    [HideInInspector]
    public ObjectRoom parentRoom;

    public int scoreOnDeath = 0;
    public CharacterInput input = new CharacterInput();

    public CharacterInputMode inputMode = CharacterInputMode.User;
    public CharacterAttackMode attackMode = CharacterAttackMode.Input;

    int gold = 0;
    int keys = 0;
    bool lastShoot;

    public AudioClip onEnemyHit;
    public AudioClip[] onHit;

    public static Character Player { get; set; }

    public bool Active
    {
        get
        {
            if (!disableOffscreen) return true;

            return CameraManager.Contains(transform.position);
        }
    }

    public Character Owner
    {
        get
        {
            return owner;
        }
        set
        {
            owner = value;
            if(owner == null)
            {
                //kill this
                Health.Damage(Health.hp, this);
            }
        }
    }
    
    public int Gold
    {
        get
        {
            return gold;
        }
    }

    public int Keys
    {
        get
        {
            return keys;
        }
    }

    public ObjectRoomBoss InsideBossRoom
    {
        get
        {
            if (!RoomChecker) return null;
            if (RoomChecker.room is ObjectRoomBoss) return RoomChecker.room as ObjectRoomBoss;

            return null;
        }
    }
    
    public void CharacterKilled(Health victim)
    {
        if(GunShooter && victim)
        {
            GunShooter.Killed(victim);
        }
        if (isPlayer)
        {
            Settings.Game.kills++;
            if(victim.Character)
            {
                GameManager.AddScore(victim.Character.scoreOnDeath);
                Settings.Temporary.kills++;
            }
        }
    }

    public void CharacterHit(Health victim)
    {
        Sound.PlaySound(onEnemyHit, "Character");

        if(GunShooter && victim)
        {
            GunShooter.Hit(victim);
        }
    }

    public override void Awake()
    {
        base.Awake();

        ObjectManager.Add(this);

        if (isPlayer)
        {
            Player = this;
            GameObject.Find("HealthRoot").GetComponent<InterfaceHealthBar>().Assign(Health);
        }

        Sound.CreateSource("Character", AudioManager.AudioType.Other);

        Health.onDied += Died;
        Health.onDamage += Damaged;

        if (PickupMaster) PickupMaster.onPickup += PickedUp;
    }

    private void Update()
    {
        if(isPlayer)
        {
            Settings.Temporary.gold = gold;
            Settings.Temporary.keys = keys;
            Settings.Game.time += Time.deltaTime;
            //TestingManager.DrawBox(transform, Vector2.one * 24, Color.blue);

            PlayerInput();
            UpdateTransition();

            float viewMp = 5000;
            if (Generator && Generator.preset.effects.underground)
            {
                viewMp = Inventory.GetMultiplier("Visibility");
            }

            Visibility.Multiplier = viewMp;
        }
        else
        {
            if(Team == GameTeam.Evil)
            {
                //TestingManager.DrawBox(transform, Vector2.one * 24, Color.red);
            }
            else
            {
                //TestingManager.DrawBox(transform, Vector2.one * 24, Color.green);
            }

            if(Look)
            {
                if(Look.target && nextReaction == 0f)
                {
                    nextReaction = Time.time + Look.reactionDelay;
                }
                else if(!Look.target && nextReaction != 0f)
                {
                    nextReaction = 0f;
                }
            }
            if(Time.time > nextReaction)
            {
                if (attackMode == CharacterAttackMode.Constant)
                {
                    if (GunShooter) GunShooter.attack = !GameManager.Paused && Process;
                }
                if (attackMode == CharacterAttackMode.TargetVisibility)
                {
                    if (GunShooter) GunShooter.attack = Look.target && !GameManager.Paused && Process;
                }
            }
        }
    }

    float nextReaction;

    void UpdateTransition()
    {
        if (transform.position.ToBlockPos().x > Settings.Temporary.finish.x + 6 && !GeneratorManager.Generating)
        {
            GeneratorManager.CleanUp();
            GeneratorManager.FinishStage();
            GameManager.FinishedLevel();
            GeneratorManager.NewStage();
        }
    }
    
    public bool UseKey()
    {
        if (keys == 0) return false;
        keys--;

        return true;
    }

    public void RemoveCoins(int amount)
    {
        if (amount <= 0) return;

        Sound.PlaySound("Purchase", "Character");
        gold -= amount;
    }

    void PickedUp(ObjectPickup pickup)
    {
        if (pickup is ObjectCurrency)
        {
            ObjectCurrency currency = (ObjectCurrency)pickup;
            if (currency.type == Currency.Gold) gold += 1;
            else if (currency.type == Currency.Key) keys += 1;
        }
    }

    void Damaged()
    {
        if (onHit.Length == 0) return;
        Sound.PlaySound(onHit[Random.Range(0, onHit.Length)], "Character");
    }
    
    private void PlayerInput()
    {
        if(inputMode == CharacterInputMode.User)
        {
            input.up = Input.GetKey(KeyCode.W);
            input.left = Input.GetKey(KeyCode.A);
            input.down = Input.GetKey(KeyCode.S);
            input.right = Input.GetKey(KeyCode.D);
            input.shoot = Input.GetKey(KeyCode.Mouse0);

            input.changeGun = Input.GetKeyDown(KeyCode.Q);
            input.activateItem = Input.GetKeyDown(KeyCode.Space);
            input.pickupItem = Input.GetKeyDown(KeyCode.E);
            input.dropArmor = Input.GetKeyDown(KeyCode.G);
            input.showInventory = Input.GetKeyDown(KeyCode.Tab);
            
            if (attackMode == CharacterAttackMode.Input)
            {
                bool firstShot = false;
                if (!lastShoot)
                {
                    lastShoot = true;
                    firstShot = true;
                }
                if (GunShooter)
                {
                    GunShooter.firstShot = firstShot;
                    GunShooter.attack = input.shoot;
                }
            }
        }
    }

    void OnDestroy()
    {
        if (Application.isPlaying)
        {
            ObjectManager.Remove(this);
        }
    }

    public virtual void OnDisable()
    {
        Health.onDied -= Died;
        Health.onDamage -= Damaged;

        if (PickupMaster) PickupMaster.onPickup -= PickedUp;
    }

    public virtual void OnEnable()
    {
        Health.onDied += Died;
        Health.onDamage += Damaged;

        if (PickupMaster) PickupMaster.onPickup += PickedUp;

        if (isPlayer)
        {
            Player = this;
            CameraManager.Snap();
        }
    }

    void Died()
    {
        ObjectManager.Remove(this);
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        isDead = true;

        //Disable comps
        if (Movement) Movement.Disable();
        if (GunShooter) GunShooter.enabled = false;
        if (Look) Look.enabled = false;
        if (RoomChecker) RoomChecker.enabled = false;
        if (Armor)
        {
            Armor.enabled = false;
            Armor.Dispose();
        }

        //Funny junk

        if (!isPlayer)
        {
            GameManager.AddScore(scoreOnDeath);
            if (Team == GameTeam.Evil) EnemyManager.EnemyDied(this);
            
            if (GunShooter)
            {
                if (transform.Find("Gun"))
                {
                    Destroy(transform.Find("Gun").gameObject);
                }
                Destroy(GunShooter);
            }

            if (owner)
            {
                //check if this is a companion item from a consumable

                owner.Inventory.CompanionDied(this);
            }

            //destroy completely if no animation for death
            if (Health.destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
        else
        {
            GameManager.GameEnd();
            if (Health.lastAttacker)
            {
                GameManager.SetDeathCause(Health.lastAttacker.deathCause);
            }

            Settings.Game.deaths++;

            Destroy(gameObject);

            return;
        }

        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].sortingLayerName = "GeneratorProps";
        }
    }

    public bool Process
    {
        get
        {
            if(Team == GameTeam.Evil)
            {
                Vector2 pos = Character.Player.transform.position;

                if (transform.position.x > pos.x + GameManager.GameWidth / 2f)
                {
                    return false;
                }
                else if (transform.position.x < pos.x - GameManager.GameWidth / 2f)
                {
                    return false;
                }
                else if (transform.position.y > pos.y + GameManager.GameHeight / 2f)
                {
                    return false;
                }
                else if (transform.position.y < pos.y - GameManager.GameHeight / 2f)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
