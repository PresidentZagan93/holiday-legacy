using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum RoomDoorType
{
    Hidden,
    Open
}

public class ObjectRoom : MonoBehaviour {

    public RoomDoorType doorType = RoomDoorType.Open;
    public bool open = true;
    public bool locked = false;
    public bool usesKey = false;
    
    public List<ObjectRoomChecker> checkersInside = new List<ObjectRoomChecker>();
    public List<Generator.GeneratorBlock> blocks = new List<Generator.GeneratorBlock>();

    bool lastOpen = true;
    public Vector2 doorPosition;
    public Rect rect = new Rect();
    SpriteRenderer hideSprite;
    ObjectMinimapIcon minimapIcon;
    public UnityEngine.AI.NavMeshObstacle door;
    float nextRectRefresh;

    private void Awake()
    {
        hideSprite = new GameObject("Hide").AddComponent<SpriteRenderer>();
        hideSprite.color = Color.black;
        hideSprite.sortingOrder = 20;
        hideSprite.sortingLayerName = "GeneratorAbove";
        hideSprite.sprite = ObjectManager.GetSprite("Square");
        hideSprite.transform.SetParent(transform);
        hideSprite.transform.localPosition = Vector2.zero;

        ObjectManager.Add(this);
    }

    private void OnDestroy()
    {
        ObjectManager.Remove(this);
    }
    
    public virtual void OnEnterRoom(ObjectRoomChecker checker)
    {

    }

    public virtual void OnExitRoom(ObjectRoomChecker checker)
    {

    }

    void OnDrawGizmosSelected()
    {
        for (int i = 0; i < blocks.Count;i++)
        {
            if(blocks[i].type != GeneratorBlockType.Wall && blocks[i].transform)
            {
                Gizmos.color = new Color(0f,0f,0f,0.5f);
                Gizmos.DrawCube(blocks[i].transform.position, Vector3.one * 32f);
            }
        }
    }

    public static ObjectRoom FindRoom(float x, float y, int scale = 32)
    {
        List<ObjectRoom> allRooms = ObjectManager.GetAllOfType<ObjectRoom>();
        for (int i = 0; i < allRooms.Count; i++)
        {
            if(allRooms[i].rect.Contains(new Vector2(x, y) * scale))
            {
                return allRooms[i];
            }
        }

        return null;
    }

    public static void Refresh()
    {
        List<ObjectRoom> allRooms = ObjectManager.GetAllOfType<ObjectRoom>();
        for (int i = 0; i < allRooms.Count; i++)
        {
            allRooms[i].FindBlocks();
            allRooms[i].RefreshRect();
            allRooms[i].InitializeRoom();
        }
    }

    void RefreshRect()
    {
        if (!this) return;

        if (blocks.Count == 0)
        {
            rect = new Rect(0f, 0f, 0f, 0f);
            return;
        }

        float maxX = float.MinValue;
        float maxY = float.MinValue;
        float minX = float.MaxValue;
        float minY = float.MaxValue;

        for (int i = 0; i < blocks.Count; i++)
        {
            bool isFloor = blocks[i].type == GeneratorBlockType.Floor || blocks[i].type == GeneratorBlockType.FloorAlt;
            if (isFloor)
            {
                if(blocks[i].transform)
                {
                    if (blocks[i].transform.position.x > maxX) maxX = blocks[i].transform.position.x;
                    if (blocks[i].transform.position.x < minX) minX = blocks[i].transform.position.x;
                    if (blocks[i].transform.position.y > maxY) maxY = blocks[i].transform.position.y;
                    if (blocks[i].transform.position.y < minY) minY = blocks[i].transform.position.y;
                }
            }
        }

        GeneratorManager.GeneratorRoomModifier mod = null;
        for (int i = 0; i < GeneratorManager.RoomMods.Count; i++)
        {
            if (GeneratorManager.RoomMods[i].tileset == blocks[0].tileset)
            {
                mod = GeneratorManager.RoomMods[i];
            }
        }

        rect.x = minX - (mod != null ? mod.positionOffset.x : 0);
        rect.width = Mathf.Abs(maxX - minX) + (mod != null ? mod.sizeOffset.x : 0);
        rect.y = minY - (mod != null ? mod.positionOffset.y : 0);
        rect.height = Mathf.Abs(maxY - minY) + (mod != null ? mod.sizeOffset.y : 0);

        transform.position = new Vector3(rect.x + rect.width / 2f, rect.y + rect.height / 2f, 0f) + Vector3.zero;
        hideSprite.transform.localScale = rect.size + Vector2.zero;
        hideSprite.color = Generator.singleton.preset.effects.backgroundColor;
    }

    void FindBlocks()
    {
        var blocks = Generator.singleton.blocks;
        for (int i = 0; i < blocks.Count; i++)
        {
            bool isFloor = blocks[i].type == GeneratorBlockType.Floor || blocks[i].type == GeneratorBlockType.FloorAlt;
            if (isFloor && blocks[i].room && blocks[i].room == this)
            {
                this.blocks.Add(blocks[i]);
            }
        }

        door = GeneratorNavmesh.GetDoor(doorPosition.x.RoundToInt(), doorPosition.y.RoundToInt());
    }

    public bool ShowIcon
    {
        get
        {
            if (!minimapIcon)
            {
                SetIcon(null);
            }
            return minimapIcon.show;
        }
        set
        {
            if(!minimapIcon)
            {
                SetIcon(null);
            }
            minimapIcon.show = value;
        }
    }

    public void SetIcon(Sprite sprite, bool show = true)
    {
        if(!minimapIcon)
        {
            minimapIcon = gameObject.AddComponent<ObjectMinimapIcon>();
        }

        minimapIcon.icon = sprite;
        minimapIcon.show = show;
    }
    
    public virtual void InitializeRoom()
    {

    }

    public void Initialize(Vector2 doorPosition)
    {
        this.doorPosition = doorPosition;
    }

    float nextDoor;

    public virtual void Discover()
    {
        if(doorType == RoomDoorType.Hidden)
        {
            Settings.Temporary.secretsFound++;
            doorType = RoomDoorType.Open;
            MinimapManager.Generate();
        }
    }

    public void Remove(ObjectRoomChecker checker)
    {
        OnExitRoom(checker);
        checkersInside.Remove(checker);

        Character character = checker.GetComponent<Character>();
        if (character && character.isPlayer)
        {
            CameraManager.Focus(Vector2.zero);
        }
    }

    public void Add(ObjectRoomChecker checker)
    {
        OnEnterRoom(checker);
        checkersInside.Add(checker);

        Character character = checker.GetComponent<Character>();
        if (character && character.isPlayer)
        {
            CameraManager.Focus(rect.center);
        }
    }

    void LateUpdate()
    {
        if(lastOpen != open)
        {
            lastOpen = open;
            if (open)
            {
                PoolManager.PoolInstantiate("RoomOpen", doorPosition * GeneratorManager.TileDimension + Vector2.one * 16f, Random.rotation);
                Generator.singleton.Modify(doorPosition.ToVector2Int(), GeneratorBlockType.Door);
            }
            else
            {
                Generator.singleton.Modify(doorPosition.ToVector2Int(), GeneratorBlockType.DoorClosed);
            }
        }

        if (door) door.enabled = !open;
        if (hideSprite) hideSprite.enabled = doorType == RoomDoorType.Hidden;
    }
}
