using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;

public class ObjectRoomGunShop : ObjectRoomShop {

    public override void CreateShop()
    {
        var allGuns = ItemManager.AllGuns;
        List<Gun> guns = new List<Gun>();
        for (int i = 0; i < allGuns.Count; i++)
        {
            if (allGuns[i].ready && !allGuns[i].enemyOnly)
            {
                guns.Add(allGuns[i]);
            }
        }

        guns = guns.Randomize();
        List<IItem> toSpawn = new List<IItem>();

        for (int i = 0; i < shop.childCount; i++)
        {
            if (shop.GetChild(i).name == "ShopSlot")
            {
                toSpawn.Add(guns[i]);
            }
        }

        PlaceItems(toSpawn);
    }
}
