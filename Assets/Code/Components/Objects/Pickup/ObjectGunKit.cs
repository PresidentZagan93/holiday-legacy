using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;

public class ObjectGunKit : ObjectKit {
    
    string lastGun;
    new SpriteRenderer renderer;
    SpriteRenderer shadowRenderer;
    
    private void Awake()
    {
        fadePoint = new Vector3(GameManager.GameWidth * 1.5f, -120f);
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

        shadowRenderer.transform.position = transform.position + Vector3.down * 4;
        shadowRenderer.sprite = renderer.sprite;
        shadowRenderer.color = new Color(0f, 0f, 0f, 0.5f);
        if (Item == null)
        {
            renderer.sprite = null;
            return;
        }
        else
        {
            if(lastGun != Item.GetName())
            {
                lastGun = Item.GetName();
                renderer.sprite = Item.GetIcon();
            }
        }

        roundOffset = new Vector2(renderer.sprite.rect.width % 2 == 0f ? 0f : 0.5f, renderer.sprite.rect.width % 2 == 0f ? 0f : 0.5f);
        Outline(renderer, isInRange);
    }

    public override bool CanPickup(CharacterPickupMaster master)
    {
        if(master.Armor)
        {
            if(master.Armor.HasShield)
            {
                if (master.GunShooter)
                {
                    Gun gun = (Gun)Item;
                    if (gun.wieldMode == Gun.GunWieldMode.Dual)
                    {
                        return false;
                    }
                }
            }
        }
        return master.CanPickup && master.Character.Gold >= Price;
    }

    public override bool DoPickup(CharacterPickupMaster master)
    {
        PickedUp(master);
        return true;
    }
}
