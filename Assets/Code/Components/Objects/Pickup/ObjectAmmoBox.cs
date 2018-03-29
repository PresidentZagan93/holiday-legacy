using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectAmmoBox : ObjectPickup {
    
    public IntRange ammo = new IntRange(20, 40);
    public bool genericAmmo = false;
    int ammoToGive;

    [AmmoType]
    public string ammoType;

    private void Awake()
    {
        fadePoint = new Vector3(GameManager.GameWidth * 1.5f, -120f);
        ammoToGive = ammo.Random() / 2;
    }

    public override bool CanPickup(CharacterPickupMaster character)
    {
        bool isFull = true;
        for(int i = 0; i < character.GunShooter.guns.Count;i++)
        {
            if (character.GunShooter.guns[i].gun.ammoType != "None")
            {
                var ammo = character.GunShooter.GetAmmo(character.GunShooter.guns[i].gun);
                if (ammo.ammo != ammo.max)
                {
                    isFull = false;
                    break;
                }
            }
        }

        return !isFull;
    }

    public override bool DoPickup(CharacterPickupMaster character)
    {
        if(genericAmmo)
        {
            if (character.GunShooter.Ammo < character.GunShooter.MaxAmmo)
            {
                character.GunShooter.Ammo += ammoToGive/2;
                if (character.GunShooter.Ammo > character.GunShooter.MaxAmmo)
                {
                    character.GunShooter.Ammo = character.GunShooter.MaxAmmo;
                }
            }
            if (character.GunShooter.OtherAmmo < character.GunShooter.OtherMaxAmmo)
            {
                character.GunShooter.OtherAmmo += ammoToGive/2;
                if (character.GunShooter.OtherAmmo > character.GunShooter.OtherMaxAmmo)
                {
                    character.GunShooter.OtherAmmo = character.GunShooter.OtherMaxAmmo;
                }
            }
        }
        else
        {
            character.GunShooter.AddAmmo(ammoType, ammoToGive);
        }

        string ammoName = genericAmmo ? "AMMO" : ammoType.ToUpper();
        character.Collect(this);
        UIManager.DrawNotificationText(Helper.RandomID, transform.position, "+" + ammoToGive + " " + ammoName, 0.5f);
        GameManager.AddScore(50);

        return true;
    }
}
