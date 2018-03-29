using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;

public class ObjectArmorKit : ObjectKit {
    
    string lastArmor;
    new SpriteRenderer renderer;
    SpriteRenderer shadowRenderer;

    private void Awake()
    {
        fadePoint = new Vector3(GameManager.GameWidth * 1f, 120f);
        renderer = GetComponent<SpriteRenderer>();
        shadowRenderer = new GameObject("Shadow").AddComponent<SpriteRenderer>();
        shadowRenderer.transform.SetParent(transform);

        renderer.sprite = null;
    }

    private void Update()
    {
        if(isInRange && Item != null)
        {
            ShowPreview();
        }

        shadowRenderer.transform.position = transform.position + Vector3.down * 2;
        shadowRenderer.sprite = renderer.sprite;
        shadowRenderer.color = new Color(0f, 0f, 0f, 0.5f);
        if (Item == null)
        {
            renderer.sprite = null;
            return;
        }
        else
        {
            if(lastArmor != Item.GetName())
            {
                lastArmor = Item.GetName();
                renderer.sprite = Item.GetIcon();
            }
        }
        roundOffset = new Vector2(renderer.sprite.rect.width % 2 == 0f ? 0f : 0.5f, renderer.sprite.rect.width % 2 == 0f ? 0f : 0.5f);
        Outline(renderer, isInRange);
    }

    public override bool CanPickup(CharacterPickupMaster character)
    {
        return character.CanPickup && character.Armor.CanPickup((Armor)Item) && character.Character.Gold >= Price;
    }

    public override bool DoPickup(CharacterPickupMaster master)
    {
        Armor armor = (Armor)Item;
        if(armor.type == Armor.ArmorType.Shield)
        {
            if(master.GunShooter.HasDuoGun)
            {
                master.GunShooter.DropDuoGun();
            }
        }
        PickedUp(master);

        return true;
    }
}
