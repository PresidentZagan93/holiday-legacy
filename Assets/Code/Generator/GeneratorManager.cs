using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

[System.Serializable]
public class GeneratorTileset
{
    public string name;
    public Sprite[] sprites;
}

public class GeneratorManager : MonoBehaviour {

    [System.Serializable]
    public class GeneratorMovementModifier
    {
        [AnyTileset]
        public string tileset;
        public float speedMultiplier = 1f;
        public float accelerationMultiplier = 1f;
    }

    [System.Serializable]
    public class GeneratorRoomModifier
    {
        [Tileset]
        public string tileset;
        public Vector2Int sizeOffset = new Vector2Int(0, 0);
        public Vector2Int positionOffset = new Vector2Int(0, 0);
    }
    
    [System.Serializable]
    public class GeneratorFootstepsModifier
    {
        [AnyTileset]
        public string tileset;
        public AudioClip footstepSound;
    }

    public List<GeneratorFootstepsModifier> footstepMods = new List<GeneratorFootstepsModifier>();
    public List<GeneratorMovementModifier> movementMods = new List<GeneratorMovementModifier>();
    public List<GeneratorRoomModifier> roomMods = new List<GeneratorRoomModifier>();

    public List<GeneratorTileset> tilesets = new List<GeneratorTileset>();
    public List<GeneratorPreset> presets = new List<GeneratorPreset>();
    public List<GeneratorTile> art = new List<GeneratorTile>();
    public List<GeneratorTile> artFloorAlt = new List<GeneratorTile>();
    public GameObject[] ambientOcclusion;

    List<int> seedsGenerated = new List<int>();
    public GameObject generatorPrefab;
    public int tileDimension = 16;

    public int stage = 0;
    public int startStage = 0;
    public bool autoGenerate = false;

    public int seed;
    public Generator level;

    public static GeneratorManager singleton;

    void Awake()
    {
        singleton = this;
    }

    void Update()
    {
        singleton = this;
    }

    public static GeneratorTileset GetTileset(string tileset)
    {
        if (!singleton) singleton = FindObjectOfType<GeneratorManager>();

        for (int i = 0; i < singleton.footstepMods.Count; i++)
        {
            if (singleton.tilesets[i].name == tileset) return singleton.tilesets[i];
        }

        return null;
    }

    public static AudioClip GetFootstep(string tileset)
    {
        if (!singleton) singleton = FindObjectOfType<GeneratorManager>();

        for (int i = 0; i < singleton.footstepMods.Count;i++)
        {
            if(singleton.footstepMods[i].tileset == tileset)
            {
                return singleton.footstepMods[i].footstepSound;
            }
        }
        return null;
    }

    public static List<GeneratorMovementModifier> MovementMods
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<GeneratorManager>();

            return singleton.movementMods;
        }
    }

    public static List<GeneratorRoomModifier> RoomMods
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<GeneratorManager>();

            return singleton.roomMods;
        }
    }

    public static string Tileset
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<GeneratorManager>();

            if (singleton.level && singleton.level.ready) return singleton.level.preset.tileset;

            return "";
        }
    }

    public static bool Generating
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<GeneratorManager>();
            if (!singleton.level) return false;

            return !singleton.level.ready;
        }
    }

    public static int Stage
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<GeneratorManager>();
            return singleton.stage;
        }
        set
        {
            if (!singleton) singleton = FindObjectOfType<GeneratorManager>();
            singleton.stage = value;
        }
    }

    public static int TileDimension
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<GeneratorManager>();
            return singleton.tileDimension;
        }
    }

    public static int RandomSeed
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<GeneratorManager>();
            return singleton.r.Next(int.MaxValue);
        }
    }
    
    public static Generator Generator
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<GeneratorManager>();
            return singleton.level;
        }
    }

    public static void StartGame(int seed)
    {
        NewStage(true, seed);
    }

    public static void NewStage()
    {
        NewStage(false, singleton.seed);
    }

    static void NewStage(bool firstStart, int seed)
    {
        if (!singleton) singleton = FindObjectOfType<GeneratorManager>();

        if(firstStart)
        {
#if UNITY_EDITOR
            singleton.stage = singleton.startStage;
#else
            singleton.stage = 0;
#endif
        }

        //generate the main map,
        GameObject newGen = Instantiate(singleton.generatorPrefab);
        newGen.transform.parent = singleton.transform;
        newGen.transform.localPosition = Vector2.zero;
        Generator newGenerator = newGen.GetComponent<Generator>();
        singleton.level = newGenerator;

        singleton.seed = seed;

        LevelManager.Level level = LevelManager.Order[singleton.stage % LevelManager.Order.Count];

        //testing
        //seed = 0;

        newGenerator.Generate(seed, level);
        Visibility.Enabled = newGenerator.preset.effects.underground;
    }

    public static void CleanUp()
    {
        if (!singleton) singleton = FindObjectOfType<GeneratorManager>();

        var allVis = ObjectManager.GetAllOfType<Visibility>();
        for (int i = 0; i < allVis.Count; i++)
        {
            if (allVis[i].transform.root.name != "Managers" && !allVis[i].GetComponent<Character>())
            {
                Destroy(allVis[i].gameObject);
            }
        }
        var allPickups = ObjectManager.GetAllOfType<ObjectPickup>();
        for (int i = 0; i < allPickups.Count; i++)
        {
            Destroy(allPickups[i].gameObject);
        }

        Destroy(GameObject.Find("WallProps"));
        new GameObject("WallProps");

        EnemyManager.Clear();
    
        singleton.seedsGenerated.Clear();
        
        if (singleton.level) Destroy(singleton.level.gameObject);
    }
    
    public static void FinishStage()
    {
        if (!singleton) singleton = FindObjectOfType<GeneratorManager>();

        singleton.stage++;
        CleanUp();
    }

    public static Generator.GeneratorBlock GetSpawnpoint(bool onlyInsideRoom, bool ignoreWalls = true)
    {
        List<GeneratorBlockObject> blocks = ObjectManager.GetAllOfType<GeneratorBlockObject>();
        List<Generator.GeneratorBlock> positions = new List<Generator.GeneratorBlock>();
        for (int i = 0; i < blocks.Count;i++)
        {
            if ((blocks[i].block.type == GeneratorBlockType.Floor) || (!ignoreWalls && blocks[i].block.type == GeneratorBlockType.Wall))
            {
                if (blocks[i].block.room && blocks[i].block.room.doorType == RoomDoorType.Hidden) continue;

                if (onlyInsideRoom == blocks[i].block.room)
                {
                    positions.Add(blocks[i].block);
                }
            }
        }

        if (positions.Count == 0) return GetSpawnpoint();
        return positions[singleton.r.Next(positions.Count)];
    }

    public static Generator.GeneratorBlock GetPlayerSpawnpoint()
    {
        List<GeneratorBlockObject> blocks = ObjectManager.GetAllOfType<GeneratorBlockObject>().ToList();
        List<Generator.GeneratorBlock> positions = new List<Generator.GeneratorBlock>();
        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i].block.room && blocks[i].block.room is ObjectRoomPlayer && blocks[i].block.type == GeneratorBlockType.Floor)
            {
                if (blocks[i].block.room && blocks[i].block.room.doorType == RoomDoorType.Hidden) continue;

                positions.Add(blocks[i].block);
            }
        }
        if(positions.Count == 0)
        {
            Generator.GeneratorBlock blockFound = Generator.FindBlock(Settings.Temporary.spawn);
            if(blockFound == null) blockFound = blocks[singleton.r.Next(positions.Count)].block;

            positions.Add(blockFound);
        }

        return positions[singleton.r.Next(positions.Count)];
    }

    public static Generator.GeneratorBlock GetEnemySpawnpoint()
    {
        List<GeneratorBlockObject> blocks = ObjectManager.GetAllOfType<GeneratorBlockObject>().ToList();
        List<Generator.GeneratorBlock> positions = new List<Generator.GeneratorBlock>();
        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i].block.type == GeneratorBlockType.Floor)
            {
                if ((blocks[i].block.x > Settings.Temporary.finish.x - 2 || (blocks[i].block.x < Settings.Temporary.spawn.x + 5 && Generator.preset.turns != 0)) && !blocks[i].block.room)
                {
                    continue;
                }

                if (blocks[i].block.room && blocks[i].block.room.doorType == RoomDoorType.Hidden) continue;
                if (blocks[i].block.room && blocks[i].block.room is ObjectRoomChest) continue;
                if (blocks[i].block.room && blocks[i].block.room is ObjectRoomShop) continue;
                if (blocks[i].block.room && blocks[i].block.room is ObjectRoomPlayer) continue;
                if (blocks[i].block.room && blocks[i].block.room is ObjectRoomBoss) continue;
                if (blocks[i].block.room && blocks[i].block.room is ObjectRoomChestChallenge) continue;

                positions.Add(blocks[i].block);
            }
        }
        
        return positions[singleton.r.Next(positions.Count)];
    }

    public static Generator.GeneratorBlock GetSpawnpoint()
    {
        List<GeneratorBlockObject> blocks = ObjectManager.GetAllOfType<GeneratorBlockObject>().ToList();
        List<Generator.GeneratorBlock> positions = new List<Generator.GeneratorBlock>();
        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i].block.type == GeneratorBlockType.Floor)
            {
                positions.Add(blocks[i].block);
            }
        }
        return positions[singleton.r.Next(positions.Count)];
    }

    System.Random r = new System.Random(System.DateTime.Now.Millisecond);
}
