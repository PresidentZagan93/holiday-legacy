using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IRoomable
{
    bool InRoom();
}

public class ObjectRoomChecker : MonoBehaviour {
    
    public ObjectRoom room;
    public Generator.GeneratorBlock block;

    public bool hideInsideHiddenRoom;
    Vector3 lastBlockPos;

    [HideInInspector]
    public Character character;

    void Awake()
    {
        ObjectManager.Add(this);
        character = GetComponent<Character>();
        Refresh();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawCube(transform.position.ToBlockPos() + Vector3.back, Vector3.one);
    }

    public void Refresh()
    {
        lastBlockPos = Vector3.zero;
    }

    private void Update()
    {
        if(lastBlockPos != transform.position.ToBlockPos())
        {
            lastBlockPos = transform.position.ToBlockPos();

            if (!Generator.singleton) return;
            
            block = Generator.FindBlock(transform.position.ToBlockPos());
            if (block != null && block.type != GeneratorBlockType.Wall)
            {
                if(character && character.isPlayer)
                {
                    //discoverd blocks
                    bool changed = Generator.DiscoverArea(block);
                    if(changed) MinimapManager.Generate();
                }

                if (room != block.room)
                {
                    if(block.type != GeneratorBlockType.Door && ((block.room && block.room.doorType != RoomDoorType.Hidden) || !block.room))
                    {
                        if (room)
                        {
                            room.Remove(this);
                        }
                        if (block.room)
                        {
                            block.room.Add(this);
                        }
                        room = block.room;
                    }
                    else
                    {
                        if (character && character.Team == GameTeam.Good)
                        {
                            if(block.room)
                            {
                                block.room.Discover();
                            }
                        }
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        ObjectManager.Remove(this);
    }
}
