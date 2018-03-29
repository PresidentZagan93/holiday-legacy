using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Data;

public enum GameTeam
{
    Good,
    Evil,
    Both
}

public class GameManager : MonoBehaviour {

    public List<DeathCause> deathCauses = new List<DeathCause>();

    public GameObject buildList;

    public int version;
    public List<Tip> tips = new List<Tip>();
    public List<Tip.TipTypeColor> tipTypeColors = new List<Tip.TipTypeColor>();

    public int gameHeight;
    public int gameWidth;

    public GameObject playerPrefab;
    bool firstStart;

    public GameObject transitionLevel;

    static GameManager singleton;

    public static GameObject PlayerPrefab
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<GameManager>();

            return singleton.playerPrefab;
        }
    }

    public static PlayerBuild Build
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<GameManager>();

            return ItemManager.AllPlayerBuilds[Settings.Game.lastBuild];
        }
    }

    public static bool Paused
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<GameManager>();

            return singleton.paused;
        }
        set
        {
            if (!singleton) singleton = FindObjectOfType<GameManager>();

            singleton.paused = value;
        }
    }

    public static int GameWidth
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<GameManager>();

            return singleton.gameWidth;
        }
    }

    public static int GameHeight
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<GameManager>();

            return singleton.gameHeight;
        }
    }

    public static string GetRandomHint()
    {
        if (!singleton) singleton = FindObjectOfType<GameManager>();
        
        string currentLevelName = LevelManager.Order[GeneratorManager.Stage % LevelManager.Order.Count].levelName;
        string currentTileset = LevelManager.Order[GeneratorManager.Stage % LevelManager.Order.Count].tileset;
        Character player = Character.Player;

        var list = singleton.tips.Randomize();

        for(int t = 0; t < list.Count;t++)
        {
            Tip tip = list[t];
            
            if(tip.specificTilesets.Count > 0 && !tip.specificTilesets.Contains(currentTileset))
            {
                continue;
            }
            if (tip.specificLevels.Count > 0 && !tip.specificLevels.Contains(currentLevelName))
            {
                continue;
            }
            if (tip.specificItems.Count > 0)
            {
                for (int i = 0; i < player.Inventory.items.Count; i++)
                {
                    if (!tip.specificItems.Contains(player.Inventory.items[i])) continue;
                }
                if(player.Inventory.HasConsumable)
                {
                    if (!tip.specificConsumables.Contains(player.Inventory.GetConsumable().item)) continue;
                }
            }
            if (tip.specificGuns.Count > 0 && player)
            {
                for (int i = 0; i < player.GunShooter.guns.Count; i++)
                {
                    if (!tip.specificGuns.Contains(player.GunShooter.guns[i].gun)) continue;
                }
            }
            if (tip.specificArmor.Count > 0 && player)
            {
                for (int i = 0; i < player.Armor.armor.Count; i++)
                {
                    if (!tip.specificArmor.Contains(player.Armor.armor[i].armor)) continue;
                }
            }

            return tip.TipText;
        }

        return "No tip";
    }

    public static void AddScore(int score)
    {
        if (!singleton) singleton = FindObjectOfType<GameManager>();

        Settings.Temporary.score += score;
    }

    public static void Reset()
    {
        if (!singleton) singleton = FindObjectOfType<GameManager>();
        
        singleton.firstStart = false;
        GeneratorManager.Stage = GeneratorManager.singleton.startStage;
        ResetLevel();

        Settings.Temporary.time = 0;
        Settings.Temporary.gold = 0;
        Settings.Temporary.kills = 0;
        Settings.Temporary.pickups = 0;
        Settings.Temporary.score = 0;
    }

    public static void StartedLevel()
    {
        if (!singleton) singleton = FindObjectOfType<GameManager>();

        Vector2 playerPosition = GeneratorManager.GetPlayerSpawnpoint().transform.position + (Vector3)Random.insideUnitCircle * 12f;

        Character player = Character.Player;

        if (!player)
        {
            player = Instantiate(singleton.playerPrefab).GetComponent<Character>();
            Character.Player = player;
            player.transform.position = playerPosition;
        }
        
        player.gameObject.transform.position = playerPosition;

        if (!singleton.firstStart)
        {
            Paused = true;
            singleton.firstStart = true;

            PickerItems.CreatePicker(PickerItems.PickerType.Builds, OnBuildChosen);
        }

        var companions = ObjectManager.GetAllOfType<Character>();
        for(int i = 0; i < companions.Count;i++)
        {
            if(companions[i].Owner)
            {
                companions[i].transform.position = companions[i].Owner.transform.position + (Vector3)Random.insideUnitCircle * 16f;
                Debug.Log("Moved " + companions[i].transform.name);
            }
        }
    }

    private static void OnBuildChosen(IItem item)
    {
        if (!singleton) singleton = FindObjectOfType<GameManager>();

        PlayerBuild build = item as PlayerBuild;
        Character player = Character.Player;

        player.Health.hp = build.maxHealth;
        player.Health.maxHp = build.maxHealth;
        player.GunShooter.Ammo = build.startingAmmo;

        for (int a = 0; a < build.startingWeapons.Length; a++)
        {
            player.GunShooter.Equip(build.startingWeapons[a].name);
        }
        for (int a = 0; a < build.startingArmor.Length; a++)
        {
            player.Armor.Equip(build.startingArmor[a].name);
        }

        Paused = false;
        InterfaceInGameLayer.Refresh();
    }

    public static void ClearedLevel(Vector3 lastEnemyPosition)
    {
        //ran once all enemies are killed

        Generator g = Generator.singleton;
        g.Modify(Settings.Temporary.finish, GeneratorBlockType.Door);
    }
    
    public static void ResetLevel()
    {
        EnemyManager.Clear(true);

        List<ObjectRoom> allRooms = ObjectManager.GetAllOfType<ObjectRoom>();
        for (int i = 0; i < allRooms.Count; i++)
        {
            if(allRooms[i])
            {
                Destroy(allRooms[i].gameObject);
            }
        }
        List<ObjectPickup> allPickups = ObjectManager.GetAllOfType<ObjectPickup>();
        for (int i = 0; i < allPickups.Count; i++)
        {
            Destroy(allPickups[i].gameObject);
        }
    }

    public static void SetDeathCause(DeathCause deathCause)
    {
        var data = new Dictionary<string, object>
        {
            { "Cause", deathCause.friendlyName },
        };
        for(int i = 0; i < Character.Player.GunShooter.guns.Count;i++)
        {
            var ammo = Character.Player.GunShooter.GetAmmo(Character.Player.GunShooter.guns[i].gun);
            data.Add(ammo.type + "." + Character.Player.GunShooter.guns[i].gun.name, Character.Player.GunShooter.guns[i].gun.name + ", " + ammo.ammo + "/" + ammo.max);
        }

        if (!singleton) singleton = FindObjectOfType<GameManager>();

        Image causeIcon = GameObject.Find("CauseIcon").GetComponent<Image>();
        
        causeIcon.color = Color.white;

        Settings.Temporary.deathCause = deathCause.friendlyName;
        Settings.Temporary.deathTip = deathCause.GetTip;

        causeIcon.sprite = deathCause.icon;
        causeIcon.color = deathCause.color;

        causeIcon.rectTransform.sizeDelta = new Vector2(causeIcon.sprite.rect.width, causeIcon.sprite.rect.height);
    }
    
    public static void GameEnd()
    {
        if (!singleton) singleton = FindObjectOfType<GameManager>();

        HUD.ChangeLayer("Dead");
        ResetLevel();
    }

    public static void FinishedLevel()
    {
        ResetLevel();
    }

    int GetXP(int level)
    {
        float y = level * (Mathf.Pow(2.5f, -0.4f)) * 22f;
        y = 5f + y;
        return Mathf.RoundToInt(y);
    }

    void Awake()
    {
        Settings.Game.version = version;

        singleton = this;
        screenDelay = Time.time + 1f;
    }
    
    float lastScreenMagnitude;
    float nextScreenRefresh;
    float screenDelay;
    public void Refresh()
    {
        if(Time.time < screenDelay)
        {
            return;
        }

        float currentScreenMagnitude = new Vector2(Screen.height, Screen.width).magnitude;
        if (currentScreenMagnitude != lastScreenMagnitude)
        {
            lastScreenMagnitude = currentScreenMagnitude;
            nextScreenRefresh = Time.time + 0.25f;
        }
        if(Time.time < nextScreenRefresh)
        {
            //Canvas.ForceUpdateCanvases();
        }
    }

    public bool paused;
    bool lastPaused;

    void PauseToggle()
    {
        PauseForce(!paused);
    }

    bool CanPause()
    {
        return Character.Player;
    }

    public void PauseForce(bool pause)
    {
        paused = pause;
        if (CanPause())
        {
            HUD.ChangeLayer(paused ? "InGame-Menu" : "InGame");
        }
    }

    void OnActive()
    {
        HUD.ChangeLayer(paused ? "InGame-Menu" : "InGame");

        List<PausableObject> allPausables = ObjectManager.GetAllOfType<PausableObject>();
        for (int i = 0; i < allPausables.Count; i++)
        {
            allPausables[i].Refresh();
        }
    }
    
    void UpdateParticleSystems()
    {
        ParticleSystem[] systems = FindObjectsOfType<ParticleSystem>();
        for (int i = 0; i < systems.Length; i++)
        {
            if (paused) systems[i].Pause();
            else systems[i].Play();
        }
    }
    
    void Update()
    {
        singleton = this;
        AudioListener.volume = Settings.Setting.volumeMaster / 100f;

        if(Input.GetKeyDown(KeyCode.Pause))
        {
            Debug.Break();
        }
        if(paused != lastPaused)
        {
            lastPaused = paused;
            UpdateParticleSystems();
        }
        if (Character.Player)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetButtonDown("Pause"))
            {
                PauseToggle();
            }
            if (!paused && Character.Player)
            {
                Settings.Game.time += Time.deltaTime;
                Settings.Temporary.time += Time.deltaTime;
            }
        }

        Refresh();
    }
}
