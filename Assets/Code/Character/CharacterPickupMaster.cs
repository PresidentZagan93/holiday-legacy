using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPickupMaster : GameBehaviour {

    public delegate void OnPickup(ObjectPickup pickup);
    public OnPickup onPickup;

    float nextPickup;
    float lastGemPickup;

    public bool CanPickup
    {
        get
        {
            if(Character.inputMode == Character.CharacterInputMode.User)
            {
                if (!Character.input.pickupItem) return false;
            }
            
            return Time.time > nextPickup;
        }
    }

    public override void Awake()
    {
        base.Awake();

        ObjectManager.Add(this);
        
        Sound.CreateSource("Pickups", AudioManager.AudioType.Pickups);
    }

    private void OnEnable()
    {
        if (Application.isPlaying)
        {
            ObjectManager.Remove(this);
        }
    }

    public void Collect(ObjectPickup pickup)
    {
        ObjectKit kit = pickup as ObjectKit;
        nextPickup = Time.time + 0.2f;
        if(onPickup != null) onPickup.Invoke(pickup);

        if (pickup is ObjectItemKit)
        {
            Sound.PlaySound(kit.Item.GetAudio().GetClip("Equip"), "Pickups");
        }
        if (pickup is ObjectConsumableKit)
        {
            Sound.PlaySound(kit.Item.GetAudio().GetClip("Equip"), "Pickups");
        }
        if (pickup is ObjectGunKit)
        {
            if (GunShooter.guns.Count > 1 || Armor.HasShield)
            {
                if(GunShooter.Gun)
                {
                    ItemManager.DropGun(transform.position, GunShooter.Gun.name);
                }
            }
            
            GunShooter.Equip(kit.Item.GetName());
            Sound.PlaySound("GunCock", "Pickups");
        }
        if (pickup is ObjectCurrency)
        {
            Sound.PlaySound("PickupCoin", "Pickups");
        }
        if (pickup is ObjectAmmoBox)
        {
            Sound.PlaySound("PickupAmmo", "Pickups");
        }
        if (pickup is ObjectHealthKit)
        {
            Health.Heal((pickup as ObjectHealthKit).amount);
            Sound.PlaySound("Pickup2", "Pickups");
        }
        if (pickup is ObjectArmorKit)
        {
            Armor.Equip(kit.Item.GetName());
            Sound.PlaySound("ArmorEquip", "Pickups");
        }
    }
}
