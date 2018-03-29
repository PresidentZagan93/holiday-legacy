using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI.Extensions;

public class MinimapManager : MonoBehaviour {
    
    public Sprite minimap;
    public int xOffset = -20;
    public int padding = 4;
    float lastScale;
    static MinimapManager singleton;
    public Transform prefab;
    public Sprite bossFocusSprite;

    Image mmImage;
    NicerOutline mmOutline;
    public List<MinimapPoint> pointers = new List<MinimapPoint>();

    [System.Serializable]
    public class MinimapPoint
    {
        public Vector3 position;
        public GameObject gameObject;
        public Image pointImage;
        public ObjectRoomChecker roomChecker;
        public bool hideInsideHiddenRoom;

        public MinimapPoint(GameObject gameObject)
        {
            roomChecker = gameObject.GetComponent<ObjectRoomChecker>();
            this.gameObject = gameObject;
            this.position = gameObject.transform.position;
        }
    }

    float Scale
    {
        get
        {
            return ConfigManager.singleton.configSettings.minimapScale;
        }
    }
    
    Rect size;

    void Awake()
    {
        singleton = this;
        mmImage = GetComponent<Image>();
        mmOutline = GetComponent<NicerOutline>();
        mmImage.enabled = false;
    }

    void GenerateMinimap()
    {
        //take data from the generator
        size = Generator.GetBounds();

        int width = Mathf.RoundToInt(size.width - size.x);
        int height = Mathf.RoundToInt(size.height - size.y);

        Texture2D newTex = new Texture2D(width + 1, height + 1, TextureFormat.RGBA32, false);

        Color[] allpixels = newTex.GetPixels();
        for (int i = 0; i < allpixels.Length; i++)
        {
            allpixels[i].a = 0;
        }

        newTex.SetPixels(allpixels);

        int x = 0;
        int y = 0;
        var blocks = Generator.GetBlocks();
        for (int i = 0; i < blocks.Count; i++)
        {
            x = blocks[i].x - Mathf.RoundToInt(size.x);
            y = blocks[i].y - Mathf.RoundToInt(size.y);
	        
	        bool hide = false;
            if(blocks[i].room)
            {
                hide = blocks[i].room.doorType == RoomDoorType.Hidden && !Character.Player.Inventory.Contains("Map");
            }
            hide |= x > width - 22;
	        if(!hide)
	        {
	        	if (blocks[i].type != GeneratorBlockType.Wall)
                {
                    Color color = Color.white;
                    if(blocks[i].room)
                    {
                        if (blocks[i].room.doorType == RoomDoorType.Hidden) color = Color.gray;
                        if (blocks[i].room is ObjectRoomShop) color = Color.green;
                        if (blocks[i].room is ObjectRoomBoss) color = Color.red;
                    }

                    if (!blocks[i].discovered) color *= 0.5f;
                    newTex.SetPixel(x, y, color);
	            }
	        }
        }

        TextureScale.Point(newTex, Mathf.RoundToInt(newTex.width * Scale * 2), Mathf.RoundToInt(newTex.height * Scale * 2));
        newTex.Apply(false);

        bool check = false;
        for (x = 0; x < newTex.width; x++)
        {
            check = !check;
            for (y = 0; y < newTex.height; y++)
            {
                check = !check;
                if (check)
                {
                    if (newTex.GetPixel(x, y).a != 0)
                    {
                        newTex.SetPixel(x, y, newTex.GetPixel(x, y) * 0.8f);
                    }
                }
            }
        }

        newTex.Apply(false);
        newTex.filterMode = FilterMode.Point;

        Sprite newSprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), Vector2.one * 0.5f, 1f, 0);

        minimap = newSprite;
        
        mmImage.sprite = minimap;
    }

    public static void Generate()
    {
        if (!singleton) singleton = FindObjectOfType<MinimapManager>();

        singleton.GenerateMinimap();
    }

    public static void Refresh()
    {
        if (!singleton) singleton = FindObjectOfType<MinimapManager>();
        if (!singleton) return;

        for (int i = 0; i < singleton.pointers.Count; i++)
        {
            Destroy(singleton.pointers[i].pointImage.gameObject);
        }

        singleton.pointers.Clear();

        List<GameObject> hiddenRoomObjects = new List<GameObject>();

        List<ObjectRoomChecker> roomCheckers = ObjectManager.GetAllOfType<ObjectRoomChecker>();
        if (roomCheckers == null) return;

        for (int i = 0; i < roomCheckers.Count;i++)
        {
            if(roomCheckers[i].hideInsideHiddenRoom)
            {
                hiddenRoomObjects.Add(roomCheckers[i].gameObject);
            }
        }

        List<ObjectMinimapIcon> icons = ObjectManager.GetAllOfType<ObjectMinimapIcon>();
        for (int i = 0; i < icons.Count; i++)
        {
            if (icons[i] && icons[i].show)
            {
                if(icons[i].character && icons[i].character.isDead)
                {
                    continue;
                }
                if(icons[i].pickup && icons[i].pickup is ObjectChest && icons[i].pickup.pickedUp)
                {
                    continue;
                }

                MinimapPoint point = new MinimapPoint(icons[i].transform.gameObject);
                Transform newPoint = Instantiate(singleton.prefab.gameObject).transform;
                newPoint.gameObject.SetActive(true);

                point.pointImage = newPoint.GetComponent<Image>();

                point.pointImage.sprite = icons[i].icon;
                if (icons[i].character)
                {
                    if (icons[i].character.isBoss)
                    {
                        if (EnemyManager.EnemiesAlive == 0) point.pointImage.sprite = singleton.bossFocusSprite;
                    }
                }
                singleton.pointers.Add(point);
            }
        }

        for (int i = 0; i < singleton.pointers.Count; i++)
        {
            singleton.pointers[i].hideInsideHiddenRoom = hiddenRoomObjects.Contains(singleton.pointers[i].gameObject);

            singleton.pointers[i].pointImage.enabled = singleton.mmImage.enabled;
            singleton.pointers[i].pointImage.transform.SetParent(singleton.transform);
            if (singleton.pointers[i].pointImage.sprite)
            {
                singleton.pointers[i].pointImage.rectTransform.sizeDelta = new Vector2(singleton.pointers[i].pointImage.sprite.rect.width, singleton.pointers[i].pointImage.sprite.rect.height);
            }
        }
    }

    int lastEnemies;
    Character lastPlayer;
    bool lastEnabled;
    Vector3 offset;
    Vector3 blockPos = Vector2.zero;
    float nextRoomCheck;

    void Update()
    {
        mmImage.enabled = !PickerItems.Active && !Console.Open;
        if (!Generator.singleton) return;

        singleton = this;
        if (lastEnemies != EnemyManager.EnemiesAlive || lastPlayer != Character.Player || lastEnabled != mmImage.enabled || lastScale != Scale)
        {
            if(lastScale != Scale)
            {
                lastScale = Scale;
                Generate();
            }

            lastEnemies = EnemyManager.EnemiesAlive;
            lastPlayer = Character.Player;
            lastEnabled = !lastEnabled;

            Refresh();
        }

        for(int i = 0; i < pointers.Count;i++)
        {
            if(!pointers[i].pointImage || !pointers[i].gameObject)
            {
                pointers.RemoveAt(i);
                break;
            }

            pointers[i].pointImage.enabled = mmImage.enabled;
            if (pointers[i].pointImage.enabled)
            {
                pointers[i].position = pointers[i].gameObject.transform.position;
                if (pointers[i].roomChecker && pointers[i].roomChecker.room)
                {
                    if (pointers[i].hideInsideHiddenRoom && pointers[i].roomChecker.room.doorType == RoomDoorType.Hidden)
                    {
                        pointers[i].pointImage.enabled = false;
                    }
                }
                if (pointers[i].pointImage.enabled)
                {
                    offset = new Vector2((size.x + size.width) / 2f + 0.5f, (size.y + size.height) / 2f + 0.5f) * 32;
                    blockPos = Vector2.zero;
                    blockPos = (pointers[i].position - offset) / GeneratorManager.TileDimension * Scale * 2f;

                    pointers[i].pointImage.rectTransform.anchoredPosition3D = new Vector3((int)blockPos.x, (int)blockPos.y, 0f) + Vector3.one * 0.5f;

                }
            }
        }
        
        if (minimap)
        {
            mmImage.sprite = minimap;
            
            mmImage.color = Generator.singleton.preset.effects.minimapColor;
            mmOutline.effectColor = Generator.singleton.preset.effects.minimapOutline;

            mmImage.rectTransform.sizeDelta = new Vector2(minimap.rect.width, minimap.rect.height);
            mmImage.rectTransform.anchoredPosition3D = new Vector3((GameManager.GameWidth / 2f - ((minimap.rect.width - xOffset * Scale) * 0.5f)).RoundToInt(), (GameManager.GameHeight / 2f - minimap.rect.height * 0.5f).RoundToInt(), 0f);
            mmImage.rectTransform.anchoredPosition3D -= new Vector3(padding, padding, 0);
        }
    }
}
