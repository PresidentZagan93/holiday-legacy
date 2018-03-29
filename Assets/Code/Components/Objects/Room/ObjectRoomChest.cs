using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRoomChest : ObjectRoom
{
    public override void InitializeRoom()
    {
        base.InitializeRoom();
        
        Transform chest = Instantiate(ObjectManager.GetPrefab("Chest")).transform;

        chest.GetComponent<ObjectRoomChecker>().room = this;
        chest.transform.position = rect.center + Vector2.zero;
    }
}
