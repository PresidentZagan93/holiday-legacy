using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;

public class CharacterPlayerUI : MonoBehaviour {

    Character character;
    public LineRenderer laser;
    float nextBlink;
    string primaryColor;
    string secondaryColor;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Update()
    {
        if (PickerItems.Active)
        {
            return;
        }
        
        if (character.GunShooter.guns.Count > 0)
        {
            laser.enabled = character.Inventory.Contains("Laser Beam") && character.GunShooter.guns[0].gun.type != GunType.Melee || character.GunShooter.Gun.GetState("LaserBeam");
            RaycastHit2D hit = Physics2D.Raycast(transform.position, character.Look.lookDirection, Mathf.Infinity, ObjectManager.GetLayerMask("HitEnemy"));
            if(hit.transform)
            {
                laser.SetPosition(0, transform.position);
                laser.SetPosition(1, hit.point);
            }

            int ammo = character.GunShooter.GetAmmo(character.GunShooter.guns[0].gun).ammo;
            int max = character.GunShooter.GetAmmo(character.GunShooter.guns[0].gun).max;

            Gun gun = character.GunShooter.guns[0].gun;
            if (gun.ammoCost != 0 && max != -1 && gun.ammoType != "None")
            {
                float percentage = 0f;
                if (ammo != 0)
                {
                    percentage = (float)ammo / (float)max;
                }

                if (percentage >= 0.5f) primaryColor = "lime";
                else if (percentage < 0.5f && percentage >= 0.2f) primaryColor = "orange";
                else
                {
                    if(Time.time > nextBlink)
                    {
                        nextBlink = Time.time + 0.1f;
                        primaryColor = primaryColor == "white" ? "red" : "white";
                    }
                }

                Settings.Temporary.gunPrimaryIcon = character.GunShooter.guns[0].gun.GetAmmoIcon();
                Settings.Temporary.gunPrimaryAmmo = "<color=" + primaryColor + ">" + ammo + "</color>";
            }
            else
            {
                Settings.Temporary.gunPrimaryIcon = gun.icon;
                Settings.Temporary.gunPrimaryAmmo = "";
            }
        }
        else
        {
            Settings.Temporary.gunPrimaryIcon = null;
            Settings.Temporary.gunPrimaryAmmo = "";
        }

        if(character.GunShooter.guns.Count > 1)
        {
            int ammo = character.GunShooter.GetAmmo(character.GunShooter.guns[1].gun).ammo;
            int max = character.GunShooter.GetAmmo(character.GunShooter.guns[1].gun).max;

            Gun gun = character.GunShooter.guns[1].gun;
            if (gun.ammoCost != 0 && max != -1 && gun.ammoType != "None")
            {
                float percentage = 0f;
                if (ammo != 0)
                {
                    percentage = (float)ammo / (float)max;
                }
                
                if (percentage >= 0.5f) secondaryColor = "lime";
                else if (percentage < 0.5f && percentage >= 0.2f) secondaryColor = "orange";
                else
                {
                    if (Time.time > nextBlink)
                    {
                        nextBlink = Time.time + 0.1f;
                        secondaryColor = secondaryColor == "white" ? "red" : "white";
                    }
                }

                Settings.Temporary.gunSecondaryIcon = character.GunShooter.guns[1].gun.GetAmmoIcon();
                Settings.Temporary.gunSecondaryAmmo = "<color=" + secondaryColor + ">" + ammo + "</color>";
            }
            else
            {
                Settings.Temporary.gunSecondaryIcon = gun.icon;
                Settings.Temporary.gunSecondaryAmmo = "";
            }
        }
        else
        {
            Settings.Temporary.gunSecondaryIcon = null;
            Settings.Temporary.gunSecondaryAmmo = "";
        }
    }
}
