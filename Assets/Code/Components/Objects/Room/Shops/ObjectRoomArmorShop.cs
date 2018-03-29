using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRoomArmorShop : ObjectRoomShop {

    public override void CreateShop()
    {
        var armor = ItemManager.AllArmor.Randomize();
        for (int i = 0; i < armor.Count; i++)
        {
            if (!armor[i].ready)
            {
                armor.RemoveAt(i);
            }
        }

        List<IItem> toSpawn = new List<IItem>();

        for (int i = 0; i < shop.childCount; i++)
        {
            if (shop.GetChild(i).name == "ShopSlot")
            {
                toSpawn.Add(armor[i]);
            }
        }

        PlaceItems(toSpawn);
    }
}
