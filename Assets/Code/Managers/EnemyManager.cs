using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnemyProp
{
    public string enemyName = "Enemy";
    public GameObject enemyPrefab;
    public EnemyManager.EnemyType type = EnemyManager.EnemyType.Normal;
}

public class EnemyManager : MonoBehaviour
{
    public enum EnemyType
    {
        Normal,
        Boss
    }

    public List<Character> enemiesAlive = new List<Character>();
    public List<EnemyProp> enemies = new List<EnemyProp>();
    List<Character> deadEnemies = new List<Character>();

    ObjectSoundEmitter sound;

    bool waiting;
    bool ready;
    Vector3 lastEnemy;
    public static EnemyManager singleton;
    bool updateEnemies;

    private void Awake()
    {
        sound = GetComponent<ObjectSoundEmitter>();
        sound.CreateSource("Enemy Sounds", AudioManager.AudioType.Other);
    }

    public static void EnemyDied(Character enemy)
    {
        if (!singleton) singleton = FindObjectOfType<EnemyManager>();

        EnemyProp enemyProp = GetEnemy(enemy.name);

        if (enemyProp == null) return;
        if (singleton.deadEnemies.Contains(enemy)) return;

        singleton.deadEnemies.Add(enemy);
        singleton.lastEnemy = enemy.transform.position;
        singleton.enemiesAlive.Remove(enemy);
    }

    public static EnemyProp GetEnemy(string name)
    {
        if (!singleton) singleton = FindObjectOfType<EnemyManager>();

        for (int i = 0; i < singleton.enemies.Count; i++)
        {
            if (singleton.enemies[i].enemyName == name) return singleton.enemies[i];
        }

        return null;
    }

    public static int EnemiesAlive
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<EnemyManager>();

            int enemies = 0;
            for(int i = 0; i < singleton.enemiesAlive.Count;i++)
            {
                if (!singleton.enemiesAlive[i].isDead && !singleton.enemiesAlive[i].isBoss) enemies++;
            }
            return enemies;
        }
    }

    public static int BossesAlive
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<EnemyManager>();

            int bosses = 0;
            for (int i = 0; i < singleton.enemiesAlive.Count; i++)
            {
                if (!singleton.enemiesAlive[i].isDead && singleton.enemiesAlive[i].isBoss) bosses++;
            }
            return bosses;
        }
    }

    void Update()
    {
        singleton = this;
        if (Generator.singleton)
        {
            if (!ready)
            {
                if (EnemiesAlive > 0 || BossesAlive > 0)
                {
                    ready = true;
                }
            }
            else if (Generator.singleton.ready)
            {
                if (EnemiesAlive <= 0 && BossesAlive <= 0 && !waiting)
                {
                    GameManager.ClearedLevel(lastEnemy);
                    sound.PlaySound("DoorOpen", "Enemy Sounds");
                    waiting = true;
                    ready = false;
                }
            }
        }

        if(updateEnemies)
        {
            updateEnemies = false;
        }
    }

    public static void Clear(bool force = false)
    {
        if (!singleton) singleton = FindObjectOfType<EnemyManager>();

        var enemies = FindObjectsOfType<Character>();
        for (int i = 0; i < enemies.Length; i++)
        {
            if((enemies[i].Team == GameTeam.Evil || !Character.Player) && (enemies[i].isDead || force))
            {
                Destroy(enemies[i].gameObject);
            }
        }

        MinimapManager.Refresh();
        singleton.enemiesAlive.Clear();
    }

    public static void KillAll()
    {
        if (!singleton) singleton = FindObjectOfType<EnemyManager>();

        var enemies = ObjectManager.GetAllOfType<Character>();
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i].Team == GameTeam.Evil)
            {
                Health enemyHealth = enemies[i].GetComponent<Health>();
                if (enemyHealth && !enemies[i].isDead)
                {
                    enemyHealth.Damage(3000, Character.Player);
                }
            }
        }

        MinimapManager.Refresh();
        singleton.enemiesAlive.Clear();
    }

    public static List<string> Bosses
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<EnemyManager>();

            LevelManager.Level level = LevelManager.GetLevel(GeneratorManager.Stage);
            return level.bosses;
        }
    }

    public static bool HasBoss(int stage)
    {
        if (!singleton) singleton = FindObjectOfType<EnemyManager>();
        
        LevelManager.Level level = LevelManager.GetLevel(stage);
        return level.bosses.Count > 0;
    }

    static Character SpawnEnemy(EnemyProp enemyProp, Vector2 spawnPos)
    {
        if (!singleton) singleton = FindObjectOfType<EnemyManager>();

        GameObject enemySpawned = Instantiate(enemyProp.enemyPrefab);
        enemySpawned.transform.position = spawnPos;
        enemySpawned.name = enemyProp.enemyName;
        enemySpawned.transform.SetParent(GameObject.Find("Enemies").transform);

        Character enemy = enemySpawned.GetComponent<Character>();
        singleton.enemiesAlive.Add(enemy);

        SpriteRenderer[] spriteRenderers = enemySpawned.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].sortingLayerName = "Enemy";
        }

        return enemy;
    }

    public static void SpawnBosses()
    {
        if (!singleton) singleton = FindObjectOfType<EnemyManager>();

        List<ObjectRoomBoss> bossRooms = ObjectManager.GetAllOfType<ObjectRoomBoss>();
        for (int r = 0; r < bossRooms.Count; r++)
        {
            for (int f = 0; f < singleton.enemies.Count; f++)
            {
                if (singleton.enemies[f].enemyName == bossRooms[r].bossName)
                {
                    Vector2 spawnPos = bossRooms[r].blocks[Random.Range(0, bossRooms[r].blocks.Count)].transform.position + Vector3.one * GeneratorManager.TileDimension/2f;

                    Character boss = SpawnEnemy(singleton.enemies[f], spawnPos);
                    boss.isBoss = true;
                    bossRooms[r].boss = boss;
                    
                    break;
                }
            }
        }
    }

    public static void SpawnEnemies()
    {
        if (!singleton) singleton = FindObjectOfType<EnemyManager>();

        singleton.enemiesAlive.Clear();
        SpawnBosses();
            
        LevelManager.Level level = LevelManager.GetLevel(GeneratorManager.Stage);
            
        float enemyCountMp = Character.Player.Inventory.GetMultiplier("EnemyCount");

        singleton.waiting = false;

        int spawns = Mathf.RoundToInt(level.enemyAmount.Random() * enemyCountMp);

        for (int i = 0; i < spawns; i++)
        {
            Vector2 spawnPos = GeneratorManager.GetEnemySpawnpoint().transform.position;

            List<int> possibleEnemies = new List<int>();
            for (int e = 0; e < level.enemies.Count; e++)
            {
                for (int f = 0; f < singleton.enemies.Count; f++)
                {
                    if (singleton.enemies[f].enemyName == level.enemies[e].name)
                    {
                        int chance = level.enemies[e].chance;
                        if (chance == 100) chance = 99;

                        int minsFound = 0;
                        for (int ec = 0; ec < possibleEnemies.Count; ec++)
                        {
                            if (possibleEnemies[ec] == f) minsFound++;
                        }
                        if (level.enemies[e].min < minsFound % 100)
                        {
                            chance = 100;
                        }

                        for (int ec = 0; ec < chance; ec++)
                        {
                            possibleEnemies.Add(f);
                        }
                    }
                }
            }

            int enemyIndex = Random.Range(0, possibleEnemies.Count);

            EnemyProp enemyToSpawn = singleton.enemies[possibleEnemies[enemyIndex]];
            SpawnEnemy(enemyToSpawn, spawnPos);
        }

        singleton.updateEnemies = true;
        singleton.ready = true;
    }
}
