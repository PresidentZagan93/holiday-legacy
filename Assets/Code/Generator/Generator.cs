using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum GeneratorBlockType
{
    Floor,
    FloorAlt,
    Wall,
    Door,
    DoorClosed
}

public enum OutlineType
{
    Inside,
    Outside
}

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Generator : MonoBehaviour
{
    [System.Serializable]
    public class Direction
    {
        Vector2Int dir;

        public int X
        {
            get
            {
                return dir.x;
            }
        }
        public int Y
        {
            get
            {
                return dir.y;
            }
        }

        public static Direction Random
        {
            get
            {
                Vector3[] dirs = new Vector3[] { Vector3.left, Vector3.right, Vector3.up, Vector3.down };
                return new Direction(dirs[UnityEngine.Random.Range(0, dirs.Length)]);
            }
        }

        public Direction(Vector3 dir)
        {
            this.dir.x = dir.x.RoundToInt();
            this.dir.y = dir.y.RoundToInt();
        }
    }

    [System.Serializable]
    public class GeneratorWallProp
    {
        public GameObject horizontal;
        public int horizontalOffset;
        public GameObject vertical;
        public GameObject verticalClipped;
        public int mod = 5;
    }

    [System.Serializable]
    public class GeneratorBlock
    {
        public int x;
        public int y;
        public int id;
        public int height;
        public bool canDestroy;
        public ObjectRoom room;
        public int sprite;
        public string tileset;
        public Transform transform;
	    public GeneratorBlockType type;
	    public float speedMultiplier = 1f;
	    public float accelerationMultiplier = 1f;
        public bool discovered = false;

        public GeneratorBlock(int x, int y, int height)
        {
            this.height = height;
            this.x = x;
            this.y = y;
        }

        public GeneratorBlock(int x, int y, int height, ObjectRoom room)
        {
            this.height = height;
            this.x = x;
            this.y = y;

            this.room = room;
        }

        public GeneratorBlock(int x, int y, int height, int id, GeneratorBlockType type, string tileset, ObjectRoom room, bool canDestroy = false)
        {
            this.height = height;
            this.room = room;
            this.tileset = tileset;
            this.x = x;
            this.y = y;
            this.id = id;
            this.type = type;

            this.canDestroy = canDestroy;
        }
    }

    public static Generator singleton;
    public int seed = 0;
    //public bool active = true;
    public bool ready = false;
    bool updatingGrid;
    public Rect size;
    int x, y = 0;
    Direction dir;
    bool generating;
    public int speed = 1;
    public int turn = 0;
    public int maxTurns = 0;
    string otherProgress;
    GeneratorManager generatorManager;
    AudioSource sound;

    [HideInInspector]
    public GeneratorPreset preset;

    [HideInInspector]
    public List<GeneratorBlock> blocks = new List<GeneratorBlock>();
    
    void Awake()
    {
        sound = gameObject.AddComponent<AudioSource>();
        AudioManager.Assign(sound, AudioManager.AudioType.Other);

        Settings.Temporary.generatorProgress = "";
        generatorManager = FindObjectOfType<GeneratorManager>();
        singleton = this;
    }

    bool placingAlt;
    int streakOn;
    int streakAmount;

    void Update()
    {
        singleton = this;
        if (!ready && generating)
        {
            Settings.Temporary.generatorProgress = otherProgress;
        }
        else
        {
            Settings.Temporary.generatorProgress = "";
            for (int i = 0; i < aoRenderers.Count;i++)
            {
                if(aoRenderers[i])
                {
                    aoRenderers[i].color = preset.effects.ambientShadingColor;
                }
            }
        }
        if (generating)
        {
            if (turn < maxTurns)
            {
                otherProgress = "CREATING LAYOUT " + ((float)turn / (float)maxTurns * 100f) + "%";
                for (int i = 0; i < speed; i++)
                {
                    if(streakOn >= streakAmount)
                    {
                        placingAlt = false;
                    }

                    if(Random.Range(0,100) < preset.altStreakChance && !placingAlt)
                    {
                        placingAlt = true;
                        streakAmount = preset.altStreakAmount;
                        streakOn = 0;
                    }

                    if (Random.Range(0, 100) < preset.turnChance)
                    {
                        dir = Direction.Random;
                    }
                    x += dir.X;
                    y += dir.Y;

                    if(Random.Range(0,100) < preset.bloatChance)
                    {
                        for (int extraX = 0; extraX < preset.bloatSize; extraX++)
                        {
                            for (int extraY = 0; extraY < preset.bloatSize; extraY++)
                            {
                                if(placingAlt)
                                {
                                    streakOn++;
                                }

                                AddBlock(x + extraX, y + extraY, placingAlt ? GeneratorBlockType.FloorAlt : GeneratorBlockType.Floor, null, preset.tileset, false);
                            }
                        }
                    }
                    else
                    {
                        if (placingAlt)
                        {
                            streakOn++;
                        }

                        AddBlock(x, y, placingAlt ? GeneratorBlockType.FloorAlt : GeneratorBlockType.Floor, null, preset.tileset, false);
                    }

                    turn++;
                }
            }
            else
            {
                if(!updatingGrid)
                {
                    updatingGrid = true;
                    turn = maxTurns;
                    
                    StartCoroutine(UpdateGrid());
                }
            }
        }
    }

    public static bool DiscoverArea(GeneratorBlock origin, int radius = 4)
    {
        //discover surrounding area

        int centerX = origin.x;
        int centerY = origin.y;
        bool changed = false;

        for (int x = centerX - radius; x < centerX + radius; x++)
        {
            for (int y = centerY - radius; y < centerY + radius; y++)
            {
                //using pythagoras
                //(x - center_x)^2 + (y - center_y)^2 < radius^2.

                if (Mathf.Pow(x - centerX, 2) + Mathf.Pow(y - centerY, 2) <= radius * radius)
                {
                    GeneratorBlock block = FindBlock(x, y);
                    if (block != null)
                    {
                        if (!block.discovered) changed = true;
                        block.discovered = true;
                    }
                }
            }
        }

        return changed;
    }

    public static GeneratorBlock FindBlock(Vector2Int v)
    {
        return FindBlock(v.x, v.y);
    }

    public static GeneratorBlock FindBlock(Vector3 v)
    {
        return FindBlock(Mathf.RoundToInt(v.x), Mathf.RoundToInt(v.y));
    }

    public static GeneratorBlock FindBlock(int x, int y)
    {
        if (!singleton) singleton = FindObjectOfType<Generator>();

	    for (int i = 0; i < singleton.blocks.Count; i++)
        {
	        if (singleton.blocks[i].x == x && singleton.blocks[i].y == y)
            {
		        return singleton.blocks[i];
            }
        }
        return null;
    }

    public static GeneratorBlock FindBlock(GeneratorBlockType type)
    {
        List<int> validPositions = new List<int>();
        for (int i = 0; i < singleton.blocks.Count; i++)
        {
            if (singleton.blocks[i].type == type)
            {
                validPositions.Add(i);
            }
        }
        if (validPositions.Count == 0)
        {
            return null;
        }
        return singleton.blocks[new System.Random(System.DateTime.Now.Millisecond).Next(validPositions.Count)];
    }

    public static List<GeneratorBlock> GetBlocks()
    {
        return singleton.blocks;
    }

    public static Rect GetBounds()
    {
        if(!singleton)
        {
            singleton = FindObjectOfType<Generator>();
        }

        int x = int.MaxValue;
        int y = int.MaxValue;
        int width = int.MinValue;
        int height = int.MinValue;

        for(int i = 0; i < singleton.blocks.Count;i++)
        {
            if(singleton.blocks[i].x > width)
            {
                width = singleton.blocks[i].x;
            }
            if(singleton.blocks[i].x < x)
            {
                x = singleton.blocks[i].x;
            }
            if (singleton.blocks[i].y > height)
            {
                height = singleton.blocks[i].y;
            }
            if (singleton.blocks[i].y < y)
            {
                y = singleton.blocks[i].y;
            }
        }

        return new Rect(x, y, width, height);
    }

    GeneratorPreset FindPreset(string name)
    {
        for (int i = 0; i < generatorManager.presets.Count; i++)
        {
            if (generatorManager.presets[i].name == name)
            {
                return generatorManager.presets[i];
            }
        }
        return null;
    }

    ObjectRoomPlayer playerRoom;
    public void Generate(int seed, LevelManager.Level level)
    {
        HUD.ChangeLayer("Generating");
        this.seed = seed;
        Random.InitState(seed);

        ready = false;

        preset = FindPreset(level.tileset);

        Settings.Temporary.generatorBanner = preset.banner;
        Settings.Temporary.generatorTip = GameManager.GetRandomHint();
        Settings.Temporary.generatorLevel = level.levelName;

        turn = 0;
        
        maxTurns = preset.turns;
        blocks.Clear();
        
        if (maxTurns == 0)
        {
            Vector2 playerRoomSize = new Vector2(4, 4);
            playerRoom = new GameObject("ObjectRoomPlayer").AddComponent<ObjectRoomPlayer>();
            playerRoom.transform.SetParent(GameObject.Find("Rooms").transform);
            playerRoom.gameObject.SetLayer("Room");
            playerRoom.transform.localPosition = new Vector3(0,0);

            level.tileset = preset.GetRoom(playerRoom.GetType()).tileset;
        
            for (int x = 0; x < playerRoomSize.x; x++)
            {
                for (int y = 0; y < playerRoomSize.y; y++)
                {
                    AddBlock(x, y, GeneratorBlockType.Floor, playerRoom, level.tileset, false);
                }
            }
        }

        dir = Direction.Random;
        x = 0;
        y = 0;

        otherProgress = "CREATING LAYOUT";

        generating = true;
        updatingGrid = false;

        sound.loop = true;
        sound.clip = preset.effects.ambience;
        sound.Play();
    }

    GameObject GetSprite(int tileType)
    {
        for(int i = 0; i < generatorManager.art.Count;i++)
        {
            if(generatorManager.art[i].id == tileType)
            {
                return GeneratorTile.Random(generatorManager.art[i].tiles.ToArray()).tile;
            }
        }
        return null;
    }

    GameObject GetAltSprite(int tileType)
    {
        for (int i = 0; i < generatorManager.artFloorAlt.Count; i++)
        {
            if (generatorManager.artFloorAlt[i].id == tileType)
            {
                return GeneratorTile.Random(generatorManager.artFloorAlt[i].tiles.ToArray()).tile;
            }
        }
        return null;
    }

    void PlaceSprite(int x, int y, GeneratorBlockType t, int id)
    {
        int tileType = 0;

        //More jonathan code
        #region Jonathan code

        if (t == GeneratorBlockType.FloorAlt) t = GeneratorBlockType.Floor;
        GeneratorBlockType blockAbove = FindBlockType(blocks[id].x, blocks[id].y + 1);
        GeneratorBlockType blockBelow = FindBlockType(blocks[id].x, blocks[id].y - 1);
        GeneratorBlockType blockLeft = FindBlockType(blocks[id].x - 1, blocks[id].y);
        GeneratorBlockType blockRight = FindBlockType(blocks[id].x + 1, blocks[id].y);
        GeneratorBlockType blockAboveLeft = FindBlockType(blocks[id].x - 1, blocks[id].y + 1);
        GeneratorBlockType blockAboveRight = FindBlockType(blocks[id].x + 1, blocks[id].y + 1);
        GeneratorBlockType blockBelowLeft = FindBlockType(blocks[id].x - 1, blocks[id].y - 1);
        GeneratorBlockType blockBelowRight = FindBlockType(blocks[id].x + 1, blocks[id].y - 1);

        if (blockAbove.ToString().Contains("Door") || blockAbove == GeneratorBlockType.FloorAlt) blockAbove = GeneratorBlockType.Floor;
        if (blockBelow.ToString().Contains("Door") || blockBelow == GeneratorBlockType.FloorAlt) blockBelow = GeneratorBlockType.Floor;
        if (blockLeft.ToString().Contains("Door") || blockLeft == GeneratorBlockType.FloorAlt) blockLeft = GeneratorBlockType.Floor;
        if (blockRight.ToString().Contains("Door") || blockRight == GeneratorBlockType.FloorAlt) blockRight = GeneratorBlockType.Floor;
        if (blockAboveLeft.ToString().Contains("Door") || blockAboveLeft == GeneratorBlockType.FloorAlt) blockAboveLeft = GeneratorBlockType.Floor;
        if (blockAboveRight.ToString().Contains("Door") || blockAboveRight == GeneratorBlockType.FloorAlt) blockAboveRight = GeneratorBlockType.Floor;
        if (blockBelowLeft.ToString().Contains("Door") || blockBelowLeft == GeneratorBlockType.FloorAlt) blockBelowLeft = GeneratorBlockType.Floor;
        if (blockBelowRight.ToString().Contains("Door") || blockBelowRight == GeneratorBlockType.FloorAlt) blockBelowRight = GeneratorBlockType.Floor;

        string currentTileset = blocks[id].tileset;

        if (t == GeneratorBlockType.Floor)
        {
            #region floor
            //is floor
            if (blockAbove == GeneratorBlockType.Floor && blockBelow == GeneratorBlockType.Floor && blockLeft == GeneratorBlockType.Floor && blockRight == GeneratorBlockType.Floor)
            {
                tileType = 17;
            }
            else
            {
                if(blockAbove == GeneratorBlockType.Wall)
                {
                    if(blockAboveLeft == GeneratorBlockType.Floor && blockAboveRight == GeneratorBlockType.Floor)
                    {
                        tileType = 18;
                    }
                    else if (blockAboveLeft == GeneratorBlockType.Wall && blockAboveRight == GeneratorBlockType.Wall)
                    {
                        tileType = 11;
                    }
                    else
                    {
                        if(blockAboveLeft == GeneratorBlockType.Floor)
                        {
                            tileType = 10;
                        }
                        else if(blockAboveRight == GeneratorBlockType.Floor)
                        {
                            tileType = 12;
                        }
                    }
                }
                else
                {
                    tileType = 17;
                }
            }
            #endregion
        }
        else if(t == GeneratorBlockType.Wall)
        {
            #region wall
            //is wall
            if (blockAbove == GeneratorBlockType.Wall && blockBelow == GeneratorBlockType.Wall && blockLeft == GeneratorBlockType.Wall && blockRight == GeneratorBlockType.Wall && blockAboveRight == GeneratorBlockType.Wall && blockAboveLeft == GeneratorBlockType.Wall && blockBelowRight == GeneratorBlockType.Wall && blockBelowLeft == GeneratorBlockType.Wall)
            {
                tileType = 5;
            }
            if (blockAbove == GeneratorBlockType.Wall && blockBelow == GeneratorBlockType.Wall && blockLeft == GeneratorBlockType.Wall && blockRight == GeneratorBlockType.Wall)
            {
                if (blockAboveRight == GeneratorBlockType.Floor && blockAboveLeft == GeneratorBlockType.Floor)
                {
                    if(blockBelowLeft == GeneratorBlockType.Floor && blockBelowRight == GeneratorBlockType.Floor)
                    {
                        tileType = 44;
                    }
                    else if(blockBelowRight == GeneratorBlockType.Floor)
                    {
                        tileType = 29;
                    }
                    else if (blockBelowLeft == GeneratorBlockType.Floor)
                    {
                        tileType = 28;
                    }
                    else
                    {
                        tileType = 32;
                    }
                }
                else if(blockBelowRight == GeneratorBlockType.Floor && blockBelowLeft == GeneratorBlockType.Floor)
                {
                    tileType = 33;
                }
                else if(blockAboveRight == GeneratorBlockType.Floor && blockBelowRight == GeneratorBlockType.Floor)
                {
                    tileType = 36;
                }
                else if (blockAboveLeft == GeneratorBlockType.Floor && blockBelowLeft == GeneratorBlockType.Floor)
                {
                    tileType = 37;
                }
                else if (blockAboveRight == GeneratorBlockType.Floor)
                {
                    if (blockBelowLeft == GeneratorBlockType.Floor)
                    {
                        tileType = 51;
                    }
                    else
                    {
                        tileType = 13;
                    }
                }
                else if (blockAboveLeft == GeneratorBlockType.Floor)
                {
                    if(blockBelowRight == GeneratorBlockType.Floor)
                    {
                        tileType = 50;
                    }
                    else
                    {
                        tileType = 14;
                    }
                }
                else if (blockBelowRight == GeneratorBlockType.Floor)
                {
                    tileType = 15;
                }
                else if (blockBelowLeft == GeneratorBlockType.Floor)
                {
                    tileType = 16;
                }
                else
                {
                    tileType = 5;
                }
            }
            else if (blockAbove == GeneratorBlockType.Floor && blockBelow == GeneratorBlockType.Floor && blockLeft == GeneratorBlockType.Floor && blockRight == GeneratorBlockType.Floor)
            {
                tileType = 23;
            }
            else
            {
                if (blockLeft == GeneratorBlockType.Floor && blockRight == GeneratorBlockType.Wall && blockBelow == GeneratorBlockType.Floor && blockAbove == GeneratorBlockType.Wall)
                {
                    if (blockAboveRight == GeneratorBlockType.Floor)
                    {
                        tileType = 49;
                    }
                    else
                    {
                        tileType = 7;
                    }
                }
                else if(blockLeft == GeneratorBlockType.Wall && blockRight == GeneratorBlockType.Floor && blockBelow == GeneratorBlockType.Floor && blockAbove == GeneratorBlockType.Wall)
                {
                    if (blockAboveLeft == GeneratorBlockType.Floor)
                    {
                        tileType = 48;
                    }
                    else
                    {
                        tileType = 9;
                    }
                }
                else if(blockLeft == GeneratorBlockType.Floor && blockRight == GeneratorBlockType.Wall && blockBelow == GeneratorBlockType.Wall && blockAbove == GeneratorBlockType.Floor)
                {
                    if (blockBelowRight == GeneratorBlockType.Floor)
                    {
                        tileType = 43;
                    }
                    else
                    {
                        tileType = 1;
                    }
                }
                else if(blockLeft == GeneratorBlockType.Wall && blockRight == GeneratorBlockType.Floor && blockBelow == GeneratorBlockType.Wall && blockAbove == GeneratorBlockType.Floor)
                {
                    if(blockBelowLeft == GeneratorBlockType.Floor)
                    {
                        tileType = 42;
                    }
                    else
                    {
                        tileType = 3;
                    }
                }
                else if (blockLeft == GeneratorBlockType.Wall && blockRight == GeneratorBlockType.Wall)
                {
                    if(blockAbove == GeneratorBlockType.Floor && blockBelow == GeneratorBlockType.Floor)
                    {
                        tileType = 24;
                    }
                    else if (blockAbove == GeneratorBlockType.Floor)
                    {
                        if (blockBelowLeft == GeneratorBlockType.Floor && blockBelowRight == GeneratorBlockType.Floor)
                        {
                            tileType = 45;
                        }
                        else if (blockBelowLeft == GeneratorBlockType.Floor)
                        {
                            tileType = 38;
                        }
                        else if (blockBelowRight == GeneratorBlockType.Floor)
                        {
                            tileType = 39;
                        }
                        else
                        {
                            tileType = 2;
                        }
                    }
                    else if (blockBelow == GeneratorBlockType.Floor)
                    {
                        if(blockAboveLeft == GeneratorBlockType.Floor)
                        {
                            tileType = 26;
                        }
                        else if(blockAboveRight == GeneratorBlockType.Floor)
                        {
                            tileType = 27;
                        }
                        else
                        {
                            tileType = 8;
                        }
                    }
                }
                else if(blockAbove == GeneratorBlockType.Wall && blockBelow == GeneratorBlockType.Wall)
                {
                    if (blockRight == GeneratorBlockType.Floor && blockLeft == GeneratorBlockType.Floor)
                    {
                        tileType = 25;
                    }
                    else if (blockRight == GeneratorBlockType.Floor)
                    {
                        if (blockAboveLeft == GeneratorBlockType.Floor)
                        {
                            if(blockBelowLeft == GeneratorBlockType.Floor)
                            {
                                tileType = 41;
                            }
                            else
                            {
                                tileType = 35;
                            }
                        }
                        else
                        {
                            if (blockBelowLeft == GeneratorBlockType.Floor)
                            {
                                tileType = 47;
                            }
                            else
                            {
                                tileType = 6;
                            }
                        }
                    }
                    else if(blockLeft == GeneratorBlockType.Floor)
                    {
                        if(blockAboveRight == GeneratorBlockType.Floor)
                        {
                            if (blockBelowRight == GeneratorBlockType.Floor)
                            {
                                tileType = 40;
                            }
                            else
                            {
                                tileType = 34;
                            }
                        }
                        else
                        {
                            if(blockBelowRight == GeneratorBlockType.Floor)
                            {
                                tileType = 46;
                            }
                            else
                            {
                                tileType = 4;
                            }
                        }
                    }
                }
                else if (blockAbove == GeneratorBlockType.Floor && blockBelow == GeneratorBlockType.Floor)
                {
                    if (blockRight == GeneratorBlockType.Floor && blockLeft == GeneratorBlockType.Wall)
                    {
                        tileType = 20;
                    }
                    else if (blockLeft == GeneratorBlockType.Floor && blockRight == GeneratorBlockType.Wall)
                    {
                        tileType = 21;
                    }
                }
                else if (blockRight == GeneratorBlockType.Floor && blockLeft == GeneratorBlockType.Floor)
                {
                    if(blockAbove == GeneratorBlockType.Floor && blockBelow == GeneratorBlockType.Floor)
                    {
                        tileType = 25;
                    }
                    else if (blockAbove == GeneratorBlockType.Floor && blockBelow == GeneratorBlockType.Wall)
                    {
                        tileType = 19;
                    }
                    else if (blockBelow == GeneratorBlockType.Floor && blockAbove == GeneratorBlockType.Wall)
                    {
                        tileType = 22;
                    }
                }
                else if (blockAbove == GeneratorBlockType.Floor && blockRight == GeneratorBlockType.Floor && blockBelow == GeneratorBlockType.Wall)
                {
                    tileType = 3;
                }
            }
            #endregion
        }
        else if(t.ToString().Contains("Door"))
        {
            #region door
            if(blockAbove == GeneratorBlockType.Wall || blockBelow == GeneratorBlockType.Wall)
            {
                if(t == GeneratorBlockType.Door)
                {
                    tileType = 101;
                }
                else
                {
                    tileType = 102;
                }
            }
            else if(blockLeft == GeneratorBlockType.Wall || blockRight == GeneratorBlockType.Wall)
            {
                if (t == GeneratorBlockType.Door)
                {
                    tileType = 100;
                }
                else
                {
                    tileType = 103;
                }
            }
            else
            {
                if (t == GeneratorBlockType.Door)
                {
                    tileType = 101;
                }
                else
                {
                    tileType = 102;
                }
            }
            #endregion
        }
        #endregion block

        if (tileType != 0)
        {
            if(blocks[id].transform)
            {
                Destroy(blocks[id].transform.gameObject);
            }

            GameObject newTile = Instantiate(GetSprite(tileType));

            if (blocks[id].canDestroy && blocks[id].type == GeneratorBlockType.Wall)
            {
                Health tileHealth = newTile.AddComponent<ObjectSoundEmitter>().gameObject.AddComponent<Health>();
                tileHealth.hitSound = ObjectManager.GetAudioClip("MetalHit");
                tileHealth.maxHp = 6;
                tileHealth.hp = 6;
                tileHealth.takeContactDamage = false;
                tileHealth.team = GameTeam.Both;
            }

            blocks[id].transform = newTile.transform;
            GeneratorBlockObject blockObject = newTile.GetComponent<GeneratorBlockObject>();
            blockObject.block = blocks[id];
            blockObject.block.sprite = tileType;

            if (blocks[id].type == GeneratorBlockType.Door || blocks[id].type == GeneratorBlockType.DoorClosed)
            {
                ObjectMinimapIcon minimapIcon = newTile.AddComponent<ObjectMinimapIcon>();
                minimapIcon.icon = ObjectManager.GetSprite("MinimapIcons_9");
                minimapIcon.show = blocks[id].type == GeneratorBlockType.DoorClosed;
            }

            newTile.transform.parent = transform;//
            newTile.name = tileType + "." + t.ToString();
            if (blocks[id].room)
            {
                newTile.name += "Room";
            }

            SpriteRenderer[] rends = newTile.transform.GetComponentsInChildren<SpriteRenderer>();
            for (int s = 0; s < rends.Length; s++)
            {
                int spriteNumber = int.Parse(rends[s].sprite.name.Replace("Tiles_", ""));
                if(spriteNumber == 3 || spriteNumber == 17 || spriteNumber == 0 || spriteNumber == 27 || spriteNumber == 28 || spriteNumber == 7 ||
                   spriteNumber == 6 || spriteNumber == 5 || spriteNumber == 4 || spriteNumber == 25)
                {
                    rends[s].sortingLayerName = "GeneratorAbove";
                }
                else
                {
                    if(spriteNumber == 12 || spriteNumber == 14 || spriteNumber == 11)
                    {
                        rends[s].sortingOrder = -10;
                    }
                    else if(spriteNumber == 13 || spriteNumber == 26)
                    {
                        rends[s].sortingOrder = -19;
                    }
                    else if(spriteNumber == 25)
                    {
                        rends[s].sortingOrder = -20;
                    }
                    else
                    {
                        rends[s].sortingOrder = 10;
                    }
                    rends[s].sortingLayerName = "GeneratorGround";
                }

                GeneratorTileset tilesetUsing = new GeneratorTileset();
                for (int ts = 0, len = generatorManager.tilesets.Count; ts < len; ts++) 
                {
                    if (generatorManager.tilesets[ts].name == currentTileset)
                    {
                        tilesetUsing = generatorManager.tilesets[ts];
                    }
                }

                for (int f = 0; f < tilesetUsing.sprites.Length; f++)
                {
                    if(rends[s].sprite.name == tilesetUsing.sprites[f].name)
                    {
                        rends[s].sprite = tilesetUsing.sprites[f];
                        break;
                    }
                }
            }

            float scale = generatorManager.tileDimension;
            newTile.transform.localPosition = new Vector2(x + 0.5f, y + 0.5f) * scale;
            newTile.SetLayer("Generator");
        }
        else
        {
            Debug.LogError("coulnt place a block for " + blocks[id].type);
        }
    }

    public void Modify(Vector2Int v, GeneratorBlockType type)
    {
        Modify(v.x, v.y, type);
    }

    public void Modify(float xPos, float yPos, GeneratorBlockType type)
    {
        int x = (int)xPos;
        int y = (int)yPos;
        bool changed = false;
        
        for(int i = 0; i < blocks.Count;i++)
        {
            if(blocks[i].x == x && blocks[i].y == y)
            {
                blocks[i].type = type;
                changed = true;
                break;
            }
        }

        if(!changed)
        {
            GeneratorBlock newBlock = new GeneratorBlock(x, y, 0, blocks.Count, type, preset.tileset, null, false);
            blocks.Add(newBlock);
        }
        
        GeneratorBlock block = FindBlock(x - 1, y - 1);
        int blockBelowLeft = block.id;
        if (block != null) PlaceSprite(x - 1, y - 1, block.type, block.id);

        block = FindBlock(x - 1, y);
        int blockLeft = block.id;
        if (block != null) PlaceSprite(x - 1, y, block.type, block.id);

        block = FindBlock(x - 1, y + 1);
        int blockAboveLeft = block.id;
        if (block != null) PlaceSprite(x - 1, y + 1, block.type, block.id);

        block = FindBlock(x, y - 1);
        int blockBelow = block.id;
        if (block != null) PlaceSprite(x, y - 1, block.type, block.id);

        block = FindBlock(x, y);
        int blockMiddle = block.id;
        if (block != null) PlaceSprite(x, y, block.type, block.id);

        block = FindBlock(x, y + 1);
        int blockAbove = block.id;
        if (block != null) PlaceSprite(x, y + 1, block.type, block.id);

        block = FindBlock(x + 1, y - 1);
        int blockBelowRight = block.id;
        if (block != null) PlaceSprite(x + 1, y - 1, block.type, block.id);

        block = FindBlock(x + 1, y);
        int blockRight = block.id;
        if (block != null) PlaceSprite(x + 1, y, block.type, block.id);

        block = FindBlock(x + 1, y + 1);
        int blockAboveRight = block.id;
        if (block != null) PlaceSprite(x + 1, y + 1, block.type, block.id);

        UpdateAmbientOcclusionTile(blockBelowLeft);
        UpdateAmbientOcclusionTile(blockLeft);
        UpdateAmbientOcclusionTile(blockAboveLeft);

        UpdateAmbientOcclusionTile(blockBelow);
        UpdateAmbientOcclusionTile(blockMiddle);
        UpdateAmbientOcclusionTile(blockAbove);

        UpdateAmbientOcclusionTile(blockBelowRight);
        UpdateAmbientOcclusionTile(blockRight);
        UpdateAmbientOcclusionTile(blockAboveRight);

        GeneratorNavmesh.RefreshObstacles();
    }

    IEnumerator PlaceProps()
    {
        List<string> tilesetsFound = new List<string>();
        List<List<GeneratorBlock>> tilesetBlocks = new List<List<GeneratorBlock>>();

        Transform wallPropRoot = GameObject.Find("WallProps").transform;
        int wallPropsCount = 0;
        for(int i = 0; i < blocks.Count;i++)
        {
            if(blocks[i].type == GeneratorBlockType.Wall)
            {
                for (int w = 0; w < preset.propsWall.Count; w++)
                {
                    wallPropsCount++;
                    if (wallPropsCount % preset.propsWall[w].mod == 0)
                    {
                        try
                        {
                            bool floorBelow = FindBlock(blocks[i].x, blocks[i].y - 1).type.ToString().StartsWith("Floor");
                            bool floorRight = FindBlock(blocks[i].x + 1, blocks[i].y).type.ToString().StartsWith("Floor");
                            bool floorAbove = FindBlock(blocks[i].x, blocks[i].y + 1).type.ToString().StartsWith("Floor");
                            bool floorLeft = FindBlock(blocks[i].x - 1, blocks[i].y).type.ToString().StartsWith("Floor");

                            if(floorAbove)
                            {
                                GameObject newTorch = Instantiate(preset.propsWall[w].verticalClipped, wallPropRoot);
                                newTorch.transform.position = blocks[i].transform.position;
                            }
                            else if(floorBelow)
                            {
                                GameObject newTorch = Instantiate(preset.propsWall[w].vertical, wallPropRoot);
                                newTorch.transform.position = blocks[i].transform.position;
                            }
                            else if(floorLeft != floorRight)
                            {
                                GameObject newTorch = Instantiate(preset.propsWall[w].horizontal, wallPropRoot);
                                SpriteRenderer[] torchSprites = newTorch.transform.GetComponentsInChildren<SpriteRenderer>();
                                for (int ts = 0; ts < torchSprites.Length; ts++)
                                {
                                    torchSprites[ts].flipX = floorLeft;
                                }
                                newTorch.transform.position = blocks[i].transform.position - (!floorLeft ? new Vector3(-GeneratorManager.TileDimension - preset.propsWall[w].horizontalOffset, 0) : new Vector3(preset.propsWall[w].horizontalOffset, 0));
                                HelperExt.FixSpriteRenderers(torchSprites);
                            }

                            break;
                        }
                        catch { }
                    }
                }
            }
            else
            {
                if(!tilesetsFound.Contains(blocks[i].tileset))
                {
                    tilesetBlocks.Add(new List<GeneratorBlock>());
                    tilesetsFound.Add(blocks[i].tileset);
                }
                for (int t = 0; t < tilesetsFound.Count;t++)
                {
                    if(tilesetsFound[t] == blocks[i].tileset) tilesetBlocks[t].Add(blocks[i]);
                }
            }
        }
        
        List<Vector2Int> propsPlaced = new List<Vector2Int>();

        int run = 0;

        for (int t = 0; t < tilesetsFound.Count; t++)
        {
            List<GeneratorProp> props = new List<GeneratorProp>(FindPreset(tilesetsFound[t]).props);
            props.AddRange(preset.pickups);

            for (int i = 0; i < props.Count; i++)
            {
                int amount = props[i].amount.Random();

                bool isPickup = props[i].prop.GetComponent<ObjectPickup>();

                for (int a = 0; a < amount; a++)
                {
                    if (run % 10 == 0) yield return new WaitForEndOfFrame();

                    GameObject newProp = Instantiate(props[i].prop, isPickup ? null : transform);
                    newProp.name = newProp.name.Replace("(Clone)", string.Empty);

                    GeneratorBlock blockAt = null;
                    for (int b = 0; b < tilesetBlocks[t].Count; b++)
                    {
                        //Get a random block for this tileset
                        blockAt = tilesetBlocks[t][Random.Range(0, tilesetBlocks[t].Count)];

                        bool valid = true;
                        if (isPickup)
                        {
                            //If its a pickup, make sure its not a room thats hidden or a boss room
                            //And make sure its under the x threshold.

                            if (blockAt.room && blockAt.room is ObjectRoomBoss) valid = false; //room is a boss room
                            if (blockAt.room && blockAt.room.doorType != RoomDoorType.Open) valid = false; //room is hidden, not valid
                            else if (blockAt.x >= Settings.Temporary.finish.x) valid = false; //past threshold, not valid
                        }

                        if (blockAt.room && blockAt.room is ObjectRoomShop) valid = false; //dont spawn anything in the shop rooms

                        if (valid) break;
                    }

                    if(blockAt != null && blockAt.transform)
                    {
                        newProp.transform.localPosition = blockAt.transform.position;
                        if (!props[i].ignoreContains) propsPlaced.Add(new Vector2Int(blockAt.x, blockAt.y));

                        newProp.transform.localPosition += new Vector3(0f, 0f, -3f);

                        bool flipX = Random.value > 0.5f;
                        SpriteRenderer[] sprits = newProp.transform.GetComponentsInChildren<SpriteRenderer>();
                        for (int s = 0; s < sprits.Length; s++)
                        {
                            sprits[s].sortingLayerName = "GeneratorGround";
                            if (props[i].randomOrient)
                            {
                                sprits[s].flipX = flipX;
                                HelperExt.FixSpriteRenderers(sprits);
                            }
                        }

                        if(!props[i].ignoreContains)
                        {
                            tilesetBlocks[t].Remove(blockAt);
                        }
                    }

                    run++;
                }
            }
        }
    }

    void PlaceAltFloor()
    {
        GeneratorTileset tilesetUsing = null;
        for (int ts = 0, len = generatorManager.tilesets.Count; ts < len; ts++)
        {
            if (generatorManager.tilesets[ts].name == preset.alternateTileset)
            {
                tilesetUsing = generatorManager.tilesets[ts];
            }
        }

        if (tilesetUsing == null) return;

        for (int i = 0; i < blocks.Count;i++)
        {
            if(blocks[i].type == GeneratorBlockType.FloorAlt)
            {
                #region Jonathan code
                GeneratorBlockType blockAbove = FindBlockType(blocks[i].x, blocks[i].y + 1);
                GeneratorBlockType blockBelow = FindBlockType(blocks[i].x, blocks[i].y - 1);
                GeneratorBlockType blockLeft = FindBlockType(blocks[i].x - 1, blocks[i].y);
                GeneratorBlockType blockRight = FindBlockType(blocks[i].x + 1, blocks[i].y);

                int tileType = 0;
                if(blockAbove != GeneratorBlockType.FloorAlt && blockLeft != GeneratorBlockType.FloorAlt && blockRight != GeneratorBlockType.FloorAlt && blockBelow != GeneratorBlockType.FloorAlt)
                {
                    tileType = 10;
                }
                else
                {
                    if (blockAbove == GeneratorBlockType.FloorAlt && blockBelow == GeneratorBlockType.FloorAlt && blockLeft != GeneratorBlockType.FloorAlt && blockRight != GeneratorBlockType.FloorAlt)
                    {
                        tileType = 11;
                    }
                    else if(blockAbove != GeneratorBlockType.FloorAlt && blockBelow != GeneratorBlockType.FloorAlt && blockLeft == GeneratorBlockType.FloorAlt && blockRight == GeneratorBlockType.FloorAlt)
                    {
                        tileType = 12;
                    }
                    else
                    {
                        if (blockAbove == GeneratorBlockType.FloorAlt && blockRight == GeneratorBlockType.FloorAlt)
                        {
                            tileType = 7;
                        }
                        else if (blockBelow == GeneratorBlockType.FloorAlt && blockRight == GeneratorBlockType.FloorAlt)
                        {
                            tileType = 1;
                        }
                        else if (blockAbove == GeneratorBlockType.FloorAlt && blockLeft == GeneratorBlockType.FloorAlt)
                        {
                            tileType = 9;
                        }
                        else if (blockBelow == GeneratorBlockType.FloorAlt && blockLeft == GeneratorBlockType.FloorAlt)
                        {
                            tileType = 3;
                        }
                        else if (blockAbove == GeneratorBlockType.FloorAlt && blockBelow == GeneratorBlockType.FloorAlt)
                        {
                            if (blockLeft == GeneratorBlockType.FloorAlt)
                            {
                                tileType = 6;
                            }
                            else
                            {
                                tileType = 4;
                            }
                        }
                        else if (blockRight == GeneratorBlockType.FloorAlt && blockLeft == GeneratorBlockType.FloorAlt)
                        {
                            if (blockBelow == GeneratorBlockType.FloorAlt)
                            {
                                tileType = 2;
                            }
                            else
                            {
                                
                                tileType = 8;
                            }
                        }
                    }
                }

                #endregion

                if (tileType != 0)
                {
                    blocks[i].height = 3;
                    blocks[i].tileset = preset.alternateTileset;
                    var mods = GeneratorManager.MovementMods;
                    for (int m = 0; m < mods.Count; m++)
                    {
                        if (preset.alternateTileset == mods[m].tileset)
                        {
                            blocks[i].speedMultiplier = mods[m].speedMultiplier;
                            blocks[i].accelerationMultiplier = mods[m].accelerationMultiplier;
                            break;
                        }
                    }

                    GameObject newTile = Instantiate(GetAltSprite(tileType));
                    newTile.transform.parent = transform;//
                    newTile.name = tileType + "." + blocks[i].type.ToString();
                    newTile.SetLayer("Generator");

                    SpriteRenderer[] rends = newTile.transform.GetComponentsInChildren<SpriteRenderer>();
                    for (int s = 0; s < rends.Length; s++)
                    {
                        rends[s].sortingLayerName = "GeneratorGround";
                        rends[s].sortingOrder = -9;

                        for (int f = 0; f < tilesetUsing.sprites.Length; f++)
                        {
                            if (rends[s].sprite.name == tilesetUsing.sprites[f].name)
                            {
                                rends[s].sprite = tilesetUsing.sprites[f];
                                break;
                            }
                        }
                    }
                    float scale = generatorManager.tileDimension;
                    newTile.transform.localPosition = new Vector2(blocks[i].x + 0.5f, blocks[i].y + 0.5f) * scale;
                }
            }
        }
    }

    GeneratorBlockType FindBlockType(int x, int y)
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            if (blocks[i].x == x && blocks[i].y == y)
            {
                return blocks[i].type;
            }
        }
        return GeneratorBlockType.Wall;
    }

    bool AddBlock(int x, int y, GeneratorBlockType type, ObjectRoom room, string tileset, bool canDestroy)
    {
        int height = type.ToString().Contains("Alt") ? 0 : 0;
        int indexAt = -1;
        int blocksCount = blocks.Count;
        for(int b = 0; b < blocksCount; b++)
        {
            if(blocks[b].x == x && blocks[b].y == y)
            {
                indexAt = b;
                break;
            }
        }
        if(indexAt == -1)
        {
            //block doesnt exist, add it
            blocks.Add(new GeneratorBlock(x, y, height, blocksCount, type, tileset, room, canDestroy));
        }
        else
        {
            //block already exists, replace it
            blocks[indexAt] = new GeneratorBlock(x, y, height, indexAt, type, tileset, room, canDestroy);
        }

        return indexAt == -1;
    }

    List<GeneratorBlock> roomBlocks = new List<GeneratorBlock>();

    enum GeneratorDirection
    {
        Left,
        Right,
        Down,
        Up
    }

    IEnumerator PlaceRooms(int roomsToMake, GeneratorPreset.GeneratorRoom roomPreset, string bossName)
    {
        roomBlocks.Clear();

        int tries = 0;
        int padding = 2;
        int roomsMade = 0;

        List<int> outline = GetOutlineInside();
        System.Type roomType = roomPreset.type.Type;
        
        while (roomsMade < roomsToMake && tries < outline.Count)
        {
            if(tries % 10 == 0)
            {
                yield return new WaitForEndOfFrame();
            }

            GeneratorPreset.GeneratorRoom room = preset.GetRoom(roomType);
            Vector2Int roomSize = new Vector2Int(room.dimension.Random(), room.dimension.Random());

            if(roomPreset.type.Type == typeof(ObjectRoomShop) || roomPreset.type.Type.IsSubclassOf(typeof(ObjectRoomShop)))
            {
                roomSize = new Vector2Int(5, 5);
            }

            GeneratorBlock randomDoor = blocks[outline[Random.Range(0, outline.Count)]];

            bool invalidChain = false;
            if(randomDoor.room)
            {
                if (randomDoor.room is ObjectRoomBoss)
                {
                    invalidChain = true;
                }
                else if(randomDoor.room is ObjectRoomShop)
                {
                    invalidChain = true;
                }
            }

            if (randomDoor.x >= Settings.Temporary.finish.x || (randomDoor.x <= Settings.Temporary.spawn.x + 4 && preset.turns != 0) || invalidChain || randomDoor.type == GeneratorBlockType.Door)
            {
                if(preset.turns != 0)
                {
                    tries++;
                    continue;
                }
            }

            int paddingHorizontal = 0;
            int paddingVertical = 0;

            int type = Random.Range(1, 5);
            if(type == 1)
            {
                //up
                paddingHorizontal = -padding;
                paddingVertical = padding;
            }
            else if(type == 2)
            {
                //down
                paddingHorizontal = -padding;
                paddingVertical = (roomSize.y * -1) - padding + 1;
            }
            else if (type == 3)
            {
                //right
                paddingHorizontal = padding;
                paddingVertical = -padding;
            }
            else if (type == 4)
            {
                //left
                paddingHorizontal = (roomSize.x * -1) - padding + 1;
                paddingVertical = -padding;
            }

            bool overlap = false;

            for (int x = -1; x < roomSize.x + 1; x++)
            {
                for (int y = -1; y < roomSize.y + 1; y++)
                {
                    GeneratorBlock blockFound = FindBlock(randomDoor.x + x + paddingHorizontal, randomDoor.y + y + paddingVertical);
                    bool overLimit = false;
                    if (preset.turns != 0) overLimit = randomDoor.x >= Settings.Temporary.finish.x;

                    if (blockFound != null || overLimit)
                    {
                        overlap = true;
                        break;
                    }
                }
            }
            
            if(!overlap)
            {
                int doorX = 0;
                int doorY = 0;
                if (type == 1)
                {
                    doorX = randomDoor.x;
                    doorY = randomDoor.y + 1;
                }
                else if (type == 2)
                {
                    doorX = randomDoor.x;
                    doorY = randomDoor.y - 1;
                }
                else if (type == 3)
                {
                    doorX= randomDoor.x + 1;
                    doorY = randomDoor.y;
                }
                else if (type == 4)
                {
                    doorX = randomDoor.x - 1;
                    doorY = randomDoor.y;
                }

                //Create room
                ObjectRoom newRoom = new GameObject(roomType.Name).AddComponent(roomType) as ObjectRoom;
                newRoom.doorType = roomPreset.doorType;
                newRoom.usesKey = roomPreset.usesKey;
                newRoom.transform.SetParent(GameObject.Find("Rooms").transform);

                newRoom.gameObject.SetLayer("Room");
                if(newRoom is ObjectRoomBoss)
                {
                    (newRoom as ObjectRoomBoss).bossName = bossName;
                }
                if(GeneratorManager.Stage == 0)
                {
                    //only place these signs on the first level.
                    if (newRoom is ObjectRoomChestChallenge)
                    {
                        PlaceSign(randomDoor.x + 0.5f, randomDoor.y + 0.5f, "THIS IS A CHALLENGE ROOM, UPON PRESSING THE BUTTON, ENEMIES SPAWN, IF YOU KILL ALL ENEMIES, YOU GET LOOT.", true);
                    }
                }

                //Add door
                GeneratorBlock doorPos = new GeneratorBlock(doorX, doorY, 0);
                newRoom.Initialize(new Vector2(doorPos.x, doorPos.y));

                if (newRoom.doorType == RoomDoorType.Hidden)
                {
                    AddBlock(doorPos.x, doorPos.y, GeneratorBlockType.Wall, newRoom, room.tileset, true);
                }
                else
                {
                    AddBlock(doorPos.x, doorPos.y, GeneratorBlockType.Door, newRoom, room.tileset, false);
                }

                //Create blocks for room
                for (int x = 0; x < roomSize.x; x++)
                {
                    for (int y = 0; y < roomSize.y; y++)
                    {
                        roomBlocks.Add(new GeneratorBlock(randomDoor.x + x + paddingHorizontal - 1, randomDoor.y + y + paddingVertical - 1, 0, -1, GeneratorBlockType.Floor, room.tileset, newRoom));
                        roomBlocks.Add(new GeneratorBlock(randomDoor.x + x + paddingHorizontal + 1, randomDoor.y + y + paddingVertical - 1, 0, -1, GeneratorBlockType.Floor, room.tileset, newRoom));
                        roomBlocks.Add(new GeneratorBlock(randomDoor.x + x + paddingHorizontal - 1, randomDoor.y + y + paddingVertical + 1, 0, -1, GeneratorBlockType.Floor, room.tileset, newRoom));
                        roomBlocks.Add(new GeneratorBlock(randomDoor.x + x + paddingHorizontal + 1, randomDoor.y + y + paddingVertical + 1, 0, -1, GeneratorBlockType.Floor, room.tileset, newRoom));

                        roomBlocks.Add(new GeneratorBlock(randomDoor.x + x + paddingHorizontal - 1, randomDoor.y + y + paddingVertical, 0, -1, GeneratorBlockType.Floor, room.tileset, newRoom));
                        roomBlocks.Add(new GeneratorBlock(randomDoor.x + x + paddingHorizontal, randomDoor.y + y + paddingVertical - 1, 0, -1, GeneratorBlockType.Floor, room.tileset, newRoom));
                        roomBlocks.Add(new GeneratorBlock(randomDoor.x + x + paddingHorizontal + 1, randomDoor.y + y + paddingVertical, 0, -1, GeneratorBlockType.Floor, room.tileset, newRoom));
                        roomBlocks.Add(new GeneratorBlock(randomDoor.x + x + paddingHorizontal, randomDoor.y + y + paddingVertical + 1, 0, -1, GeneratorBlockType.Floor, room.tileset, newRoom));

                        bool contains = false;
                        for (int rb = 0; rb < roomBlocks.Count; rb++)
                        {
                            if (roomBlocks[rb].x == randomDoor.x + x + paddingHorizontal && roomBlocks[rb].y == randomDoor.y + y + paddingVertical)
                            {
                                contains = true;
                                break;
                            }
                        }

                        if (!contains)
                        {
                            GeneratorBlock newMask = new GeneratorBlock(randomDoor.x + x + paddingHorizontal, randomDoor.y + y + paddingVertical, 0, -1, GeneratorBlockType.Floor, room.tileset, newRoom, false);
                            roomBlocks.Add(newMask);
                        }

                        AddBlock(randomDoor.x + x + paddingHorizontal, randomDoor.y + y + paddingVertical, GeneratorBlockType.Floor, newRoom, room.tileset, false);
                    }
                }

                AddPillars(randomDoor.x + paddingHorizontal, randomDoor.y + paddingVertical, (int)roomSize.x, (int)roomSize.y, newRoom, room.tileset, room.blocks);
                roomsMade++;
                outline = GetOutlineInside();
            }

            tries++;
        }
    }

    void AddPillars(int x, int y, int width, int height, ObjectRoom room, string tileset, int blocks)
    {
        if (blocks == 0) return;

        height -= 1;
        width -= 1;

        Helper.DrawSquare(new Vector2(x, y), 1f, 30f);
        Helper.DrawSquare(new Vector2(x + width, y + height), 1f, 30f);

        Direction pillarDir = Direction.Random;
        int newX = Random.Range(x + 1, x + width - 1);
        int newY = Random.Range(y + 1, y + height - 1);
        //Helper.DrawCrosshair(new Vector2(newX, newY), 1f, 30f);

        List<Vector2> positions = new List<Vector2>();

        for (int i = 0; i < blocks; i++)
        {
            if (!positions.Contains(new Vector2(newX, newY)))
            {
                AddBlock(newX, newY, GeneratorBlockType.Wall, room, tileset, false);
                positions.Add(new Vector2(newX, newY));
            }

            pillarDir = Direction.Random;
            newX += pillarDir.X;
            newY += pillarDir.Y;

            if(newX == x)
            {
                i--;
                newX++;
            }
            if(newY == y)
            {
                i--;
                newY++;
            }
            if(newX == x + width)
            {
                i--;
                newX--;
            }
            if(newY == y + height)
            {
                i--;
                newY--;
            }
        }
    }

    IEnumerator PlaceFinish()
    {
        int doorPos = 2;
        int xPoint = int.MinValue;
        int yPoint = 0;
        
    	for (int i = 0; i < blocks.Count;i++)
        {
            if(!blocks[i].room || (preset.turns == 0 && blocks[i].room && blocks[i].room.doorType != RoomDoorType.Hidden))
            {
                if (blocks[i].room && blocks[i].room is ObjectRoomChestChallenge) continue;

                yield return new WaitForEndOfFrame();

                bool clear = true;
                for (int x = 1; x < 19; x++)
                {
                    GeneratorBlock blockTop = FindBlock(blocks[i].x + x, blocks[i].y);
                    GeneratorBlock blockBot = FindBlock(blocks[i].x + x, blocks[i].y + 1);
                    if (blockTop != null || blockBot != null)
                    {
                        clear = false;
                        break;
                    }
                }
                if (blocks[i].x > xPoint && clear)
                {
                    yPoint = blocks[i].y;
                    xPoint = blocks[i].x;
                }
            }
        }

        Settings.Temporary.finish = new Vector2Int(xPoint + doorPos, yPoint);
        //PlayerPrefsExtra.SetVector2("finishDoor2", new Vector2(xPoint + doorPos, yPoint+1));

        for (int i = 1; i < 30;i++)
        {
            if(i == doorPos)
            {
                AddBlock(xPoint + i, yPoint, GeneratorBlockType.DoorClosed, null, preset.tileset, false);
                AddBlock(xPoint + i, yPoint + 1, GeneratorBlockType.Wall, null, preset.tileset, false);
            }
            else if(i == doorPos + 1)
            {
                AddBlock(xPoint + i, yPoint, GeneratorBlockType.Floor, null, preset.tileset, false);
                AddBlock(xPoint + i, yPoint + 1, GeneratorBlockType.Wall, null, preset.tileset, false);
            }
            else
            {
                AddBlock(xPoint + i, yPoint, GeneratorBlockType.Floor, null, preset.tileset, false);
                AddBlock(xPoint + i, yPoint + 1, GeneratorBlockType.Floor, null, preset.tileset, false);
            }
        }

        ObjectSign sign = Instantiate(ObjectManager.GetPrefab("SignExit")).GetComponent<ObjectSign>();
        sign.transform.position = new Vector2(xPoint + doorPos - 1, yPoint + 0.5f).ToWorldPos();
        sign.inputRequried = false;
        sign.message = "";
    }

    void PlaceSign(float x, float y, string text, bool inputRequired = false)
    {
        ObjectSign sign = Instantiate(ObjectManager.GetPrefab("Sign")).GetComponent<ObjectSign>();
        sign.transform.position = new Vector2(x, y).ToWorldPos();
        sign.inputRequried = inputRequired;
        sign.message = text;
    }

    IEnumerator PlaceStart()
    {
        int xPoint = int.MaxValue;
        int yPoint = 0;

        for (int i = 0; i < blocks.Count;i++)
        {
            if (blocks[i].x < xPoint && !blocks[i].room)
            {
                yield return new WaitForEndOfFrame();
                xPoint = blocks[i].x;
                yPoint = blocks[i].y;
            }
        }

        Vector2 playerRoomSize = new Vector2(4, 3);
        for (int x = 0; x < playerRoomSize.x; x++)
        {
            for (int y = 0; y < playerRoomSize.y; y++)
            {
                AddBlock(xPoint - 2 - x, yPoint - y, GeneratorBlockType.Floor, null, preset.tileset, false);
            }
        }

        AddBlock(xPoint - 1, yPoint - 1, GeneratorBlockType.Door, null, preset.tileset, false);
        AddBlock(xPoint, yPoint - 1, GeneratorBlockType.Floor, null, preset.tileset, false);

        string text = "<color=orange>Q</color> TO SWITCH WEAPONS.\n";
        text += "<color=orange>E</color> TO INTERACT.\n";
        text += "<color=orange>SPACE</color> TO ACTIVATE POWERUPS.\n";
        text += "<color=orange>G</color> TO DROP ARMOR.\n";

        PlaceSign(xPoint - 4.5f, yPoint - 0.5f, text);

        Settings.Temporary.spawn = new Vector2Int(xPoint - 4, yPoint - 1);
    }

    List<int> GetAmbientOcclusionTiles(int blockIndex)
    {
        int x = blocks[blockIndex].x;
        int y = blocks[blockIndex].y;
        GeneratorBlockType solidType = GeneratorBlockType.DoorClosed | GeneratorBlockType.Wall;
        if (blocks[blockIndex].type == solidType) return new List<int>();

        GeneratorBlock block = null;
        block = FindBlock(x, y + 1);
        GeneratorBlockType blockAbove = block != null ? block.type : GeneratorBlockType.Wall;
        block = FindBlock(x, y - 1);
        GeneratorBlockType? blockBelow = block != null ? block.type : GeneratorBlockType.Wall;
        block = FindBlock(x - 1, y);
        GeneratorBlockType? blockLeft = block != null ? block.type : GeneratorBlockType.Wall;
        block = FindBlock(x + 1, y);
        GeneratorBlockType? blockRight = block != null ? block.type : GeneratorBlockType.Wall;
        block = FindBlock(x + 1, y - 1);
        GeneratorBlockType? blockBelowRight = block != null ? block.type : GeneratorBlockType.Wall;
        block = FindBlock(x + 1, y + 1);
        GeneratorBlockType? blockAboveRight = block != null ? block.type : GeneratorBlockType.Wall;
        block = FindBlock(x - 1, y - 1);
        GeneratorBlockType? blockBelowLeft = block != null ? block.type : GeneratorBlockType.Wall;
        block = FindBlock(x - 1, y + 1);
        GeneratorBlockType? blockAboveLeft = block != null ? block.type : GeneratorBlockType.Wall;

        List<int> tileTypes = new List<int>();
        
        if (blockAbove == solidType)
        {
            tileTypes.Add(2);
        }
        if (blockBelow == solidType)
        {
            tileTypes.Add(8);
        }
        if (blockRight == solidType)
        {
            tileTypes.Add(4);
        }
        if (blockLeft == solidType)
        {
            tileTypes.Add(6);
        }

        if (blockBelowRight == solidType)
        {
            if ((blockBelow == solidType && blockRight == solidType))
            {
                if(blockAbove != solidType && blockLeft != solidType)
                {
                    tileTypes.Add(10);
                }
            }
        }
        if (blockBelowLeft == solidType)
        {
            if (blockBelow == solidType && blockLeft == solidType)
            {
                if (blockAbove != solidType && blockRight != solidType)
                {
                    tileTypes.Add(12);
                }
            }
        }
        if (blockAboveRight == solidType)
        {
            if (blockAbove == solidType && blockRight == solidType)
            {
                if (blockBelow != solidType && blockLeft != solidType)
                {
                    tileTypes.Add(13);
                }
            }
        }
        if (blockAboveLeft == solidType)
        {
            if (blockAbove == solidType && blockLeft == solidType)
            {
                if (blockBelow != solidType && blockRight != solidType)
                {
                    tileTypes.Add(11);
                }
            }
        }

        return tileTypes;
    }

    List<SpriteRenderer> aoRenderers = new List<SpriteRenderer>();

    void UpdateAmbientOcclusionTile(int blockIndex)
    {
        int x = blocks[blockIndex].x;
        int y = blocks[blockIndex].y;

        Transform ambientOcclusionTile = transform.Find(x + "+" + y + "+.Ambient Occlusion");
        if(ambientOcclusionTile)
        {
            Destroy(ambientOcclusionTile.gameObject);
        }

        List<int> tileTypes = GetAmbientOcclusionTiles(blockIndex);

        for (int t = 0; t < tileTypes.Count; t++)
        {
            int type = tileTypes[t] - 1;
            GameObject newAmbientOcclusion = Instantiate(GeneratorManager.singleton.ambientOcclusion[type]);
            newAmbientOcclusion.transform.parent = transform;//
            newAmbientOcclusion.name = x + "+" + y + "+.Ambient Occlusion";
            newAmbientOcclusion.SetLayer("Generator");
            newAmbientOcclusion.transform.localPosition = new Vector2(x + 0.5f, y + 0.5f) * generatorManager.tileDimension;

            SpriteRenderer[] rends = newAmbientOcclusion.GetComponentsInChildren<SpriteRenderer>();
            for(int i = 0; i < rends.Length;i++)
            {
                rends[i].sortingLayerName = "AmbientOcclusion";
            }

            aoRenderers.AddRange(rends);
        }
    }

    IEnumerator AddAmbientOcclusion()
    {
        aoRenderers.Clear();
        for (int i = 0; i < blocks.Count; i++)
        {
            if(i % 10 == 0)
            {
                yield return new WaitForEndOfFrame();
            }

            UpdateAmbientOcclusionTile(i);
        }
    }
	
	IEnumerator AddCurtains()
	{
        yield return new WaitForEndOfFrame();
		
		List<Vector2> curtainPoints = new List<Vector2>();
        for (int i = 0; i < blocks.Count; i++)
        {
            if (i % 10 == 0)
            {
                yield return new WaitForEndOfFrame();
            }
			
            if (blocks[i].type != GeneratorBlockType.Wall)
            {
                //check for surrounding blocks
                GeneratorBlock blockMiddle = blocks[i];
                GeneratorBlock blockAbove = FindBlock(blocks[i].x, blocks[i].y + 1);
                GeneratorBlock blockAboveLeft = FindBlock(blocks[i].x - 1, blocks[i].y + 1);
                GeneratorBlock blockAboveRight = FindBlock(blocks[i].x + 1, blocks[i].y + 1);

                GeneratorBlock blockLeft = FindBlock(blocks[i].x - 1, blocks[i].y);
                GeneratorBlock blockRight = FindBlock(blocks[i].x + 1, blocks[i].y);
				
				if(blockAbove != null && blockLeft != null && blockRight != null)
				{
					if(blockAboveLeft != null && blockAboveRight != null)
					{
						if(blockAbove.type == GeneratorBlockType.Wall)
						{
							Vector2 middle = blockMiddle.transform.position;
							if(blockLeft.type != GeneratorBlockType.Wall && blockAboveLeft.type == GeneratorBlockType.Wall &&
								blockRight.type != GeneratorBlockType.Wall && blockAboveRight.type == GeneratorBlockType.Wall)
							{
                                //both left, right and middle are valid places
                                Vector2 leftPoint = blockMiddle.transform.position + Vector3.left * 16;
                                Vector2 rightPoint = blockMiddle.transform.position + Vector3.right * 16;
								if(!curtainPoints.Contains(leftPoint)) curtainPoints.Add(leftPoint);
								if(!curtainPoints.Contains(rightPoint)) curtainPoints.Add(rightPoint);
							}
							else if(blockLeft.type != GeneratorBlockType.Wall && blockAboveLeft.type == GeneratorBlockType.Wall)
							{
                                //left and middle are valid
                                Vector2 extraLeftPoint = blockMiddle.transform.position + Vector3.left * 20;
                                Vector2 leftPoint = blockMiddle.transform.position + Vector3.left * 16;
                                Vector2 rightPoint = blockMiddle.transform.position + Vector3.right * 16;
								if(!curtainPoints.Contains(leftPoint)) curtainPoints.Add(leftPoint);
								if(!curtainPoints.Contains(extraLeftPoint)) curtainPoints.Add(extraLeftPoint);
								if(!curtainPoints.Contains(rightPoint)) curtainPoints.Add(rightPoint);
							}
							else if(blockRight.type != GeneratorBlockType.Wall && blockAboveRight.type == GeneratorBlockType.Wall)
							{
                                //right and middle are valid
                                Vector2 leftPoint = blockMiddle.transform.position + Vector3.left * 16;
                                Vector2 rightPoint = blockMiddle.transform.position + Vector3.right * 16;
								Vector2 extraRightPoint = blockMiddle.transform.position + Vector3.right * 20;
								if(!curtainPoints.Contains(leftPoint)) curtainPoints.Add(leftPoint);
								if(!curtainPoints.Contains(rightPoint)) curtainPoints.Add(rightPoint);
								if(!curtainPoints.Contains(extraRightPoint)) curtainPoints.Add(extraRightPoint);
							}
							else
							{
								Vector2 leftPoint = blockMiddle.transform.position + Vector3.left * 20;
                                Vector2 rightPoint = blockMiddle.transform.position + Vector3.right * 20;
								if(!curtainPoints.Contains(leftPoint)) curtainPoints.Add(leftPoint);
								if(!curtainPoints.Contains(rightPoint)) curtainPoints.Add(rightPoint);
							}
						}
					}
				}
            }
		}
		
		//place sprites
		
	}

    IEnumerator PlaceDestroyableWalls()
    {
        for (int i = 0; i < blocks.Count; i++)
        {
            if (i % 10 == 0)
            {
                yield return new WaitForEndOfFrame();
            }

            if(blocks[i].type == GeneratorBlockType.Floor || blocks[i].type == GeneratorBlockType.FloorAlt)
            {
                //check for surrounding blocks
                GeneratorBlock blockAbove = FindBlock(blocks[i].x, blocks[i].y + 2);
                GeneratorBlock blockAboveBetween = FindBlock(blocks[i].x, blocks[i].y + 1);

                GeneratorBlock blockDown = FindBlock(blocks[i].x, blocks[i].y - 2);
                GeneratorBlock blockDownBetween = FindBlock(blocks[i].x, blocks[i].y - 1);

                GeneratorBlock blockRight = FindBlock(blocks[i].x + 2, blocks[i].y);
                GeneratorBlock blockRightBetween = FindBlock(blocks[i].x + 1, blocks[i].y);

                GeneratorBlock blockLeft = FindBlock(blocks[i].x - 2, blocks[i].y);
                GeneratorBlock blockLeftBetween = FindBlock(blocks[i].x - 1, blocks[i].y);

                if (blockAbove != null && blockAboveBetween != null && blockAboveBetween.type == GeneratorBlockType.Wall)
                {
                    if (blockAbove.type == GeneratorBlockType.Floor || blockAbove.type == GeneratorBlockType.FloorAlt)
                    {
                        blockAboveBetween.canDestroy = true;
                    }
                }

                if (blockDown != null && blockDownBetween != null && blockDownBetween.type == GeneratorBlockType.Wall)
                {
                    if (blockDown.type == GeneratorBlockType.Floor || blockDown.type == GeneratorBlockType.FloorAlt)
                    {
                        blockDownBetween.canDestroy = true;
                    }
                }

                if (blockRight != null && blockRightBetween != null && blockRightBetween.type == GeneratorBlockType.Wall)
                {
                    if (blockRight.type == GeneratorBlockType.Floor || blockRight.type == GeneratorBlockType.FloorAlt)
                    {
                        blockRightBetween.canDestroy = true;
                    }
                }

                if (blockLeft != null && blockLeftBetween != null && blockLeftBetween.type == GeneratorBlockType.Wall)
                {
                    if (blockLeft.type == GeneratorBlockType.Floor || blockLeft.type == GeneratorBlockType.FloorAlt)
                    {
                        blockLeftBetween.canDestroy = true;
                    }
                }
            }
        }
    }

    IEnumerator UpdateGrid()
    {
        if (maxTurns != 0)
        {
            otherProgress = "GENERATING START AND FINISH";
            yield return new WaitForEndOfFrame();

            yield return StartCoroutine(PlaceStart());
            yield return StartCoroutine(PlaceFinish());
        }

        otherProgress = "GENERATING ROOMS";
        yield return new WaitForEndOfFrame();

        List<int> challengeRooms = new List<int>();

        for (int r = 0; r < preset.rooms.Count; r++)
        {
            System.Type type = preset.rooms[r].type.Type;
            if (type == typeof(ObjectRoomChestChallenge))
            {
                challengeRooms.Add(r);
                continue;
            }
            if (type != typeof(ObjectRoomBoss) && preset.rooms[r].doorType != RoomDoorType.Hidden)
            {
                if (type == typeof(ObjectRoomPlayer))
                {
                    if (preset.turns != 0)
                    {
                        yield return StartCoroutine(PlaceRooms(preset.rooms[r].amount.Random(), preset.rooms[r], ""));
                    }
                }
                else
                {
                    yield return StartCoroutine(PlaceRooms(preset.rooms[r].amount.Random(), preset.rooms[r], ""));
                }
            }
        }

        yield return new WaitForEndOfFrame();
        otherProgress = "SPAWNING BOSSES";
        yield return new WaitForEndOfFrame();

        List<string> bossesToMake = EnemyManager.Bosses;
        for (int b = 0; b < bossesToMake.Count; b++)
        {
            GeneratorPreset.GeneratorRoom room = new GeneratorPreset.GeneratorRoom();
            room.type.typeClassName = "ObjectRoomBoss";
            room.doorType = RoomDoorType.Open;

            yield return StartCoroutine(PlaceRooms(1, room, bossesToMake[b]));
        }

        for (int r = 0; r < challengeRooms.Count; r++)
        {
            yield return StartCoroutine(PlaceRooms(preset.rooms[challengeRooms[r]].amount.Random(), preset.rooms[challengeRooms[r]], ""));
        }

        for (int r = 0; r < preset.rooms.Count; r++)
        {
            if (preset.rooms[r].doorType == RoomDoorType.Hidden)
            {
                yield return StartCoroutine(PlaceRooms(preset.rooms[r].amount.Random(), preset.rooms[r], ""));
            }
        }

        if (playerRoom)
        {
            Settings.Temporary.spawn = Vector2Int.Zero;
            yield return StartCoroutine(PlaceFinish());
            playerRoom.Discover();
        }

        yield return new WaitForEndOfFrame();
        otherProgress = "ADDING AMBIENT OCCLUSION";
        yield return new WaitForEndOfFrame();
        yield return StartCoroutine(AddAmbientOcclusion());

        yield return new WaitForEndOfFrame();
        otherProgress = "ADDING OUTLINE";
        yield return new WaitForEndOfFrame();
        yield return StartCoroutine(AddOutline(GeneratorBlockType.Wall));

        /*
        yield return new WaitForEndOfFrame();
        otherProgress = "ADDING DESTROYABLE WALLS";
        yield return new WaitForEndOfFrame();
        yield return StartCoroutine(PlaceDestroyableWalls());
        */

        yield return new WaitForEndOfFrame();
        otherProgress = "BUILDING MESH";
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < blocks.Count; i++)
        {
            int x = blocks[i].x;
            int y = blocks[i].y;
            GeneratorBlockType t = blocks[i].type;

            for (int rb = 0; rb < roomBlocks.Count; rb++)
            {
                if (roomBlocks[rb].x == x && roomBlocks[rb].y == y)
                {
                    blocks[i].room = roomBlocks[rb].room;
                    blocks[i].tileset = roomBlocks[rb].tileset;
                }
            }

            PlaceSprite(x, y, t, i);
        }

        yield return new WaitForEndOfFrame();
        otherProgress = "ADDING CURTAINS";
        yield return new WaitForEndOfFrame();
        yield return StartCoroutine(AddCurtains());
		
        yield return new WaitForEndOfFrame();
        otherProgress = "CREATING OVERLAY";
        yield return new WaitForEndOfFrame();
        
        size = GetBounds();

        yield return new WaitForEndOfFrame();
        otherProgress = "CREATING NAVMESH";
        yield return new WaitForEndOfFrame();
        GeneratorNavmesh.Generate();

        yield return new WaitForEndOfFrame();
        otherProgress = "PLACING PROPS";
        yield return new WaitForEndOfFrame();
        yield return StartCoroutine(PlaceProps());

        size = GetBounds();

        yield return new WaitForEndOfFrame();
        otherProgress = "SPAWNING ENEMIES";
        yield return new WaitForEndOfFrame();

        HUD.ChangeLayer("InGame");
        GameManager.StartedLevel();
        ObjectRoom.Refresh();
        EnemyManager.SpawnEnemies();

        yield return new WaitForEndOfFrame();
        otherProgress = "GENERATING MINIMAP";
        yield return new WaitForEndOfFrame();
        PlaceAltFloor();
        MinimapManager.Generate();
        InterfaceInGameLayer.Refresh();

        SpriteRenderer[] overlays = GameObject.Find("OverlayRoot").GetComponentsInChildren<SpriteRenderer>();
        for(int i = 0; i < overlays.Length;i++)
        {
            overlays[i].sprite = preset.effects.overlay;
        }

        generating = false;
        ready = true;

        Settings.Temporary.generatorTip = "";
        Settings.Temporary.generatorLevel = "";
        Settings.Temporary.generatorBanner = null;
    }

    List<int> GetOutlineInside()
    {
        List<int> outline = new List<int>();
        int blocksCount = blocks.Count;
        
        for(int i = 0; i < blocksCount; i++)
        {
            bool add = false;
            GeneratorBlock block = FindBlock(blocks[i].x, blocks[i].y + 1);
            if (block == null)
            {
                add = true;
            }
            block = FindBlock(blocks[i].x, blocks[i].y - 1);
            if (block == null)
            {
                add = true;
            }
            block = FindBlock(blocks[i].x + 1, blocks[i].y);
            if (block == null)
            {
                add = true;
            }
            block = FindBlock(blocks[i].x - 1, blocks[i].y);
            if (block == null)
            {
                add = true;
            }
            block = FindBlock(blocks[i].x - 1, blocks[i].y + 1);
            if (block == null)
            {
                add = true;
            }
            block = FindBlock(blocks[i].x - 1, blocks[i].y - 1);
            if (block == null)
            {
                add = true;
            }
            block = FindBlock(blocks[i].x + 1, blocks[i].y + 1);
            if (block == null)
            {
                add = true;
            }
            block = FindBlock(blocks[i].x + 1, blocks[i].y - 1);
            if (block == null)
            {
                add = true;
            }
            if (add && !outline.Contains(i))
            {
                outline.Add(i);
            }
        }

        return outline;
    }

    IEnumerator AddOutline(GeneratorBlockType type)
    {
        List<Vector2> outline = new List<Vector2>();
        yield return null;

        for (int i = 0; i < blocks.Count; i++)
        {
            int x = blocks[i].x;
            int y = blocks[i].y;

            GeneratorBlock above = FindBlock(x, y + 1);
            GeneratorBlock below = FindBlock(x, y - 1);
            GeneratorBlock left = FindBlock(x - 1, y);
            GeneratorBlock right = FindBlock(x + 1, y);

            GeneratorBlock aboveRight = FindBlock(x + 1, y + 1);
            GeneratorBlock aboveLeft = FindBlock(x - 1, y + 1);
            GeneratorBlock belowRight = FindBlock(x + 1, y - 1);
            GeneratorBlock belowLeft = FindBlock(x - 1, y - 1);

            if (above == null)
            {
                Vector2 vector = new Vector2(x, y + 1);
                if (!outline.Contains(vector))
                {
                    outline.Add(vector);
                }
            }
            if (below == null)
            {
                Vector2 vector = new Vector2(x, y - 1);
                if (!outline.Contains(vector))
                {
                    outline.Add(vector);
                }
            }
            if (right == null)
            {
                Vector2 vector = new Vector2(x + 1, y);
                if (!outline.Contains(vector))
                {
                    outline.Add(vector);
                }
            }
            if (left == null)
            {
                Vector2 vector = new Vector2(x - 1, y);
                if (!outline.Contains(vector))
                {
                    outline.Add(vector);
                }
            }
            if (aboveRight == null)
            {
                Vector2 vector = new Vector2(x + 1, y + 1);
                if (!outline.Contains(vector))
                {
                    outline.Add(vector);
                }
            }
            if (aboveLeft == null)
            {
                Vector2 vector = new Vector2(x - 1, y + 1);
                if (!outline.Contains(vector))
                {
                    outline.Add(vector);
                }
            }
            if (belowRight == null)
            {
                Vector2 vector = new Vector2(x + 1, y - 1);
                if (!outline.Contains(vector))
                {
                    outline.Add(vector);
                }
            }
            if (belowLeft == null)
            {
                Vector2 vector = new Vector2(x - 1, y - 1);
                if (!outline.Contains(vector))
                {
                    outline.Add(vector);
                }
            }
        }

        for (int i = 0; i < outline.Count; i++)
        {
            AddBlock((int)outline[i].x, (int)outline[i].y, type, null, preset.tileset, false);
        }
    }
}
