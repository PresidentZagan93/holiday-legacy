using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRoomAmmoHeaven : ObjectRoom {

    public override void InitializeRoom()
    {
        base.InitializeRoom();

        for(int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2 position = new Vector2(x, y).ToWorldPos() + rect.center;
                ItemManager.DropAmmo(position);
            }
        }
    }
}
