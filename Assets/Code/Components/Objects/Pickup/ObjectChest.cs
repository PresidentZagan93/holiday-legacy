using UnityEngine;
using System.Collections.Generic;
using Data;
using System.Linq;

public class ObjectChest : ObjectPickup
{
    public IntRange ammo = new IntRange(20, 40);
    public AudioClip chestOpenSound;
    AudioSource source;
    SpritePlayer spritePlayer;

    private void Awake()
    {
        spritePlayer = GetComponent<SpritePlayer>();
        source = GetComponent<AudioSource>();
        AudioManager.Assign(source, AudioManager.AudioType.Pickups);
    }

    public override bool CanPickup(CharacterPickupMaster character)
    {
        return !pickedUp;
    }

    public static Gun GetGun()
    {
        int level = (GeneratorManager.Stage % LevelManager.Order.Count) + 1;
        List<Gun> guns = ItemManager.singleton.guns;
        List<Gun> validGuns = new List<Gun>();
        for (int i = 0; i < guns.Count; i++)
        {
            if (!guns[i].enemyOnly && guns[i].ready && guns[i].level == level)
            {
                //add this a hunded times, to represent 100 percent chance
                for (int a = 0; a < 100; a++)
                {
                    validGuns.Add(guns[i]);
                }
            }
        }

        //go through player build
        for(int i = 0; i < GameManager.Build.chances.Length;i++)
        {
            List<Gun> ofType = new List<Gun>();
            if(GameManager.Build.chances[i].goForAmmo)
            {
                ofType = guns.Where(g => g.ammoType == GameManager.Build.chances[i].ammoType).ToList();
            }
            else
            {
                ofType = guns.Where(g => g.type == GameManager.Build.chances[i].gunType).ToList();
            }

            for (int g = 0; g < ofType.Count; g++)
            {
                if(!ofType[g].enemyOnly && ofType[g].ready && (ofType[g].level == level || ofType[g].level == level - 1))
                {
                    for (int a = 0; a < GameManager.Build.chances[i].chance; a++)
                    {
                        validGuns.Add(ofType[g]);
                    }
                }
            }
        }

        if (validGuns.Count == 0) throw new System.Exception("No guns left for level " + level);

        return validGuns[Random.Range(0, validGuns.Count)];
    }

    public override bool DoPickup(CharacterPickupMaster character)
    {
        Gun gun = GetGun();

        ObjectGunKit pickup = ItemManager.DropGun(transform.position + Vector3.down, gun.name);
        pickup.lerpToOrigin = true;

        SpriteRenderer[] spriteRenderers = pickup.transform.GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].sortingLayerName = spritePlayer.spriteRenderer.sortingLayerName;
            spriteRenderers[i].sortingOrder = spritePlayer.spriteRenderer.sortingOrder + 1;
        }

        int toAdd = ammo.Random();
        character.GunShooter.Ammo += toAdd;

        UIManager.DrawNotificationText(Helper.RandomID, transform.position, "+" + toAdd + " AMMO");

        spritePlayer.Play("ChestOpen");
        source.clip = chestOpenSound;
        source.Play();

        GameManager.AddScore(50);
        MinimapManager.Refresh();

        return false;
    }
}
