using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using Data;

[CustomEditor(typeof(Gun))]
public class EditorGun : Editor
{
    string[] projectiles;
    string[] shells;
    int selectedGun;
    int selectedShell;

    void OnEnable()
    {
        Gun gun = (Gun)target;
        List<string> newProjectiles = new List<string>();
        List<string> newShells = new List<string>();

        PoolManager pm = FindObjectOfType<PoolManager>();
        for (int i = 0; i < pm.poolItems.Count; i++)
        {
            if(pm.poolItems[i].gameObject.GetComponent<GunProjectile>())
            {
                string name = pm.poolItems[i].gameObject.name;
                if (gun.projectile == name)
                {
                    selectedGun = newProjectiles.Count;
                }
                newProjectiles.Add(name);
            }
            if (pm.poolItems[i].gameObject.name.EndsWith("Shell"))
            {
                string name = pm.poolItems[i].gameObject.name;
                if (gun.shellName == name)
                {
                    selectedShell = newShells.Count;
                }
                newShells.Add(name);
            }
        }

        projectiles = newProjectiles.ToArray();
        shells = newShells.ToArray();
    }

    void ShowDuplicate(int id)
    {
        if (!ItemManager.singleton) ItemManager.singleton = FindObjectOfType<ItemManager>();

        List<string> duplicates = new List<string>();

        for (int i = 0; i < ItemManager.singleton.guns.Count;i++)
        {
            if(ItemManager.singleton.guns[i].ID == id && ItemManager.singleton.guns[i] != (Gun)target)
            {
                duplicates.Add(ItemManager.singleton.guns[i].name + " already has this ID!");
            }
        }

        if (duplicates.Count > 0)
        {
            int smallestId = 0;
            for (int i = 0; i < ItemManager.singleton.guns.Count;i++)
            {
                if(!ItemManager.GetGun(i))
                {
                    smallestId = i;
                    break;
                }
            }
            EditorGUILayout.HelpBox(string.Join("\n", duplicates.ToArray())+"\nTry "+ smallestId, UnityEditor.MessageType.Error);
            if (GUILayout.Button("Fix ID"))
            {
                Gun gun = (Gun)target;
                gun.lockId = false;
                gun.ID = smallestId;
                gun.lockId = true;
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Gun gun = (Gun)target;
        
        GUIStyle style = "IN LockButton";
        gun.lockId = GUI.Toggle(new Rect(Screen.width - 34, 52, 25, 25), gun.lockId, GUIContent.none, style);

        EditorGUI.BeginDisabledGroup(gun.lockId);
        int id = EditorGUILayout.IntField("ID", gun.ID, GUILayout.Width(Screen.width-48));
        gun.ID = id;
        EditorGUI.EndDisabledGroup();

        ShowDuplicate(gun.ID);

        gun.price = EditorGUILayout.IntField("Price", gun.price);
        
        gun.level = EditorGUILayout.IntSlider("Level", gun.level, 1, 20);
        gun.ready = EditorGUILayout.Toggle("Ready for game", gun.ready);
        gun.enemyOnly = EditorGUILayout.Toggle("Enemy only", gun.enemyOnly);

        gun.type = (GunType)EditorGUILayout.EnumPopup("Type", gun.type);
        gun.wieldMode = (Gun.GunWieldMode)EditorGUILayout.EnumPopup("Wield", gun.wieldMode);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ammoType"));
        gun.icon = (Sprite)EditorGUILayout.ObjectField("Icon", gun.icon, typeof(Sprite), false);
        gun.description = EditorGUILayout.TextField("Description", gun.description);

        if (gun.type != GunType.Laser && gun.type != GunType.ConstantLaser && gun.type != GunType.Electricity)
        {
            gun.colorRandom = EditorGUILayout.Toggle("Random color", gun.colorRandom);
            if (!gun.colorRandom)
            {
                EditorGUI.indentLevel++;
                gun.color = EditorGUILayout.ColorField("Color", gun.color);
                EditorGUI.indentLevel--;
            }
        }

        if(gun.type != GunType.Melee)
        {
            gun.kickbackMode = (Gun.GunKickbackStyle)EditorGUILayout.EnumPopup("Kickback", gun.kickbackMode);
        }
        gun.shooterKnockback = EditorGUILayout.Slider("Knockback", gun.shooterKnockback, 0, 100);
        gun.damage = EditorGUILayout.IntSlider("Damage", gun.damage, 0, 100);
        gun.automatic = EditorGUILayout.Toggle("Auto", gun.automatic);
        gun.splash = EditorGUILayout.Foldout(gun.splash, "Splash");
        if (gun.splash)
        {
            EditorGUI.indentLevel = 1;
            gun.selfHarm = EditorGUILayout.Toggle("Splash Self Harm", gun.selfHarm);
            gun.splashRange = EditorGUILayout.FloatField("Splash Range", gun.splashRange);
            gun.splashDamage = EditorGUILayout.IntSlider("Splash Damage", gun.splashDamage, 0, 100);
            EditorGUI.indentLevel = 0;
        }

        Quality.ShowInspector(ref gun.qualities);

        SerializedProperty effects = serializedObject.FindProperty("effects");
        EditorGUILayout.PropertyField(effects, true);
        if (gun.effects != null)
        {
            for (int i = 0; i < gun.effects.Length; i++)
            {
                float ticks = gun.effects[i].duration / gun.effects[i].tickRate;
                if(gun.effects[i].tickRate == 0)
                {
                    EditorGUILayout.LabelField("No ticks for "+ gun.effects[i].quality.name, EditorStyles.miniLabel);
                }
                else
                {
                    EditorGUILayout.LabelField(ticks + " Ticks for " + gun.effects[i].quality.name, EditorStyles.miniLabel);
                }
            }
        }
        EditorGUILayout.Space();

        gun.canCharge = EditorGUILayout.Toggle("Charges", gun.canCharge);
        if(gun.canCharge)
        {
            EditorGUI.indentLevel++;
            gun.chargeDuration = EditorGUILayout.FloatField("Duration", gun.chargeDuration);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("chargeMultiplier"), true);
            EditorGUI.indentLevel--;
        }

        gun.fireRate = EditorGUILayout.FloatField("Attack rate", gun.fireRate);
        EditorGUILayout.LabelField((1f/gun.fireRate)+" "+gun.ammoType.ToString()+"/s");
        gun.fireDelay = EditorGUILayout.FloatField("Attack delay", gun.fireDelay);
        gun.shots = EditorGUILayout.IntSlider("Attacks", gun.shots, 1, 100);
        gun.ammoCost = EditorGUILayout.IntSlider("Attack cost", gun.ammoCost, 0, 100);

        if (gun.type != GunType.Melee && gun.type != GunType.Electricity && gun.type != GunType.Sprays)
        {
            gun.recoil = EditorGUILayout.FloatField("Recoil", gun.recoil);
            gun.evenRecoil = EditorGUILayout.Toggle("Recoil is even", gun.evenRecoil);
        }
        gun.burstRate = EditorGUILayout.FloatField("Burst rate", gun.burstRate);
        gun.bursts = EditorGUILayout.IntField("Burst amount", gun.bursts);

        gun.hitEffect = (GameObject)EditorGUILayout.ObjectField("Hit Effect", gun.hitEffect, typeof(GameObject), false);
        if (gun.type == GunType.Projectiles || gun.type == GunType.Sprays)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Projectile settings");
            EditorGUI.indentLevel++;
            //if (gun.type == GunType.Sprays)
            {
                selectedGun = EditorGUILayout.Popup("Projectile", selectedGun, projectiles);
                gun.projectile = projectiles[selectedGun];
            }
            gun.shellEjection = EditorGUILayout.Toggle("Projectile shell ejection", gun.shellEjection);
            if (gun.shellEjection)
            {
                EditorGUI.indentLevel++;
                gun.shellSound = (AudioClip)EditorGUILayout.ObjectField("Shell sound", gun.shellSound, typeof(AudioClip), false);
                selectedShell = EditorGUILayout.Popup("Shell prefab", selectedShell, shells);
                gun.shellName = shells[selectedShell];
                EditorGUI.indentLevel--;
            }
            
            gun.projectileRotation = (ProjectileRotationMode)EditorGUILayout.EnumPopup("Projectile rotation", gun.projectileRotation);

            gun.slowdown = EditorGUILayout.FloatField("Projectile slowdown", gun.slowdown);
            gun.projectileArcs = EditorGUILayout.Toggle("Projectile arcs", gun.projectileArcs);
            if (gun.projectileArcs)
            {
                EditorGUI.indentLevel++;
                gun.projectileArcGravity = EditorGUILayout.FloatField("Projectile gravity", gun.projectileArcGravity);
                gun.projectileArcUpwardsKick = EditorGUILayout.FloatField("Projectile upwards kick", gun.projectileArcUpwardsKick);
                EditorGUI.indentLevel--;
            }
            gun.successChance = EditorGUILayout.Slider("Projectile hit chance", gun.successChance, 0f, 100f);
            gun.speed = EditorGUILayout.FloatField("Projectile speed", gun.speed);
            gun.lifeTime = EditorGUILayout.FloatField("Projectile lifetime", gun.lifeTime);
            gun.scale = EditorGUILayout.FloatField("Projectile scale", gun.scale);
            gun.sizeOverLifetime = EditorGUILayout.CurveField("Projectile scale over lifetime", gun.sizeOverLifetime, Color.red, new Rect(0,0,1,1));
            gun.maxBounces = EditorGUILayout.IntField("Projectile bounces", gun.maxBounces);

            EditorGUI.indentLevel--;
        }
        else if (gun.type == GunType.Laser || gun.type == GunType.ConstantLaser || gun.type == GunType.Electricity)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Laser settings");
            EditorGUI.indentLevel++;
            gun.scale = EditorGUILayout.FloatField("Laser width", gun.scale);
            gun.lifeTime = EditorGUILayout.FloatField("Laser duration", gun.lifeTime);
            gun.successChance = EditorGUILayout.Slider("Laser hit chance", gun.successChance, 0f, 100f);
            if (gun.type == GunType.Electricity)
            {
                gun.recoil = EditorGUILayout.FloatField("View angle", gun.recoil);
                gun.speed = EditorGUILayout.FloatField("View distance", gun.speed);
            }
            else
            {
                gun.burstRate = EditorGUILayout.FloatField("Laser damage rate", gun.burstRate);
                EditorGUILayout.LabelField((gun.lifeTime / gun.burstRate) + " laser hits per beam");
            }
            gun.colorRandom = EditorGUILayout.Toggle("Random color", gun.colorRandom);
            if (!gun.colorRandom)
            {
                EditorGUI.indentLevel++;
                gun.color = EditorGUILayout.ColorField("Laser color", gun.color);
                EditorGUI.indentLevel--;
            }
            gun.laserTexture = (Texture)EditorGUILayout.ObjectField("Laser sprite", gun.laserTexture, typeof(Texture), false);
            EditorGUI.indentLevel--;
        }
        else if(gun.type == GunType.Melee)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Melee settings");
            EditorGUI.indentLevel++;

            gun.meleeType = (MeleeType)EditorGUILayout.EnumPopup("Melee type", gun.meleeType);
            gun.scale = EditorGUILayout.FloatField("Melee size", gun.scale);
            gun.successChance = EditorGUILayout.Slider("Melee hit chance", gun.successChance, 0f, 100f);
            gun.lifeTime = EditorGUILayout.FloatField("Melee lifetime", gun.lifeTime);
            gun.meleeSwingSpeed = EditorGUILayout.FloatField("Melee swing speed", gun.meleeSwingSpeed);
            gun.meleeBreaks = EditorGUILayout.Toggle("Melee breaks", gun.meleeBreaks);
            gun.meleeReflects = EditorGUILayout.Toggle("Melee reflects", gun.meleeReflects);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Recursion settings");
        EditorGUI.indentLevel++;
        gun.recursion.enabled = EditorGUILayout.Toggle("Enabled", gun.recursion.enabled);
        if(gun.recursion.enabled)
        {
            gun.recursion.maxRecursions = EditorGUILayout.IntField("Max recursions", gun.recursion.maxRecursions);
            gun.recursion.instancesPerRecursion = EditorGUILayout.IntField("Instances per recursion", gun.recursion.instancesPerRecursion);

            int rec = Mathf.RoundToInt(Mathf.Pow(gun.recursion.instancesPerRecursion, gun.recursion.maxRecursions));
            EditorGUILayout.LabelField(rec+" Projectiles made");
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Attack settings");

        EditorGUI.indentLevel++;
        gun.attack.screenshake = EditorGUILayout.Slider("Screenshake", gun.attack.screenshake, 0f, 64f);
        gun.attack.shootEffect = (GameObject)EditorGUILayout.ObjectField("Shoot Effect", gun.attack.shootEffect, typeof(GameObject), false);
        gun.attack.shootOffset = EditorGUILayout.Vector2Field("Offset", gun.attack.shootOffset);
        gun.holdPosition = EditorGUILayout.Vector2Field("Hold position", gun.holdPosition);
        gun.attack.soundType = (Gun.SoundType)EditorGUILayout.EnumPopup("Sound type", gun.attack.soundType);
        gun.attack.idleSound = (AudioClip)EditorGUILayout.ObjectField("Idle sound", gun.attack.idleSound, typeof(AudioClip), false);
        gun.attack.equipSound = (AudioClip)EditorGUILayout.ObjectField("Equip sound", gun.attack.equipSound, typeof(AudioClip), false);
        if (gun.type != GunType.Melee)
        {
            gun.attack.dryFireSound = (AudioClip)EditorGUILayout.ObjectField("Dry fire sound", gun.attack.dryFireSound, typeof(AudioClip), false);
        }
        if (gun.attack.soundType == Gun.SoundType.Single)
        {
            gun.attack.startSound = (AudioClip)EditorGUILayout.ObjectField("Sound", gun.attack.startSound, typeof(AudioClip), false);
        }
        else
        {
            gun.attack.startSound = (AudioClip)EditorGUILayout.ObjectField("Start sound", gun.attack.startSound, typeof(AudioClip), false);
            gun.attack.loopSound = (AudioClip)EditorGUILayout.ObjectField("Loop sound", gun.attack.loopSound, typeof(AudioClip), false);
            gun.attack.endSound = (AudioClip)EditorGUILayout.ObjectField("End sound", gun.attack.endSound, typeof(AudioClip), false);
        }

        gun.attack.successSound = (AudioClip)EditorGUILayout.ObjectField("Success Hit sound", gun.attack.successSound, typeof(AudioClip), false);
        EditorGUI.indentLevel--;

        EditorGUILayout.LabelField("Handle settings");
        EditorGUILayout.Space();

        EditorGUI.indentLevel++;
        gun.handle.type = (Gun.GunHandling.GunHandleType)EditorGUILayout.EnumPopup("Handle type", gun.handle.type);
        gun.handle.hand1Position = EditorGUILayout.Vector2Field("Hand 1", gun.handle.hand1Position);
        if (!gun.handle.type.ToString().Contains("Single"))
        {
            gun.handle.hand2Position = EditorGUILayout.Vector2Field("Hand 2", gun.handle.hand2Position);
        }
        EditorGUI.indentLevel--;

        serializedObject.ApplyModifiedProperties();
        EditorUtility.SetDirty(target);
    }
}