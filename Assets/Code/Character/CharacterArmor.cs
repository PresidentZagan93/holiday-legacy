using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;

public class CharacterArmor : MonoBehaviour {

    [System.Serializable]
    public class ArmorSet
    {
        public Armor armor;
        public SpriteRenderer renderer;
        public bool shieldEnabled;

        public ArmorSet(Armor armor)
        {
            this.armor = armor;
        }
    }

    //adding references to qualities
    //Inventory.GetMultiplier("ArmorReflect");
    //Inventory.GetMultiplier("ArmorBlock");
    //Inventory.GetMultiplier("ArmorBlockContact");
    //Inventory.GetMultiplier("ArmorBlockMelee");
    //Inventory.GetMultiplier("PlayerContactDamage");

    int id;
    public int maxArmor = 2;
    public SpritePlayer spritePlayer;
    public SpritePlayerOffsets offsets;
    public List<ArmorSet> armor = new List<ArmorSet>();
    public List<Armor> startingArmor = new List<Armor>();

    public Transform root;
    Character character;
    CharacterLook looker;
    ObjectSoundEmitter sound;

    public int headOffset = 4;

    public bool Equip(string name)
    {
        Armor newArmor = ItemManager.GetArmor(name);
        if (armor.Count == maxArmor)
        {
            if(HasType(newArmor.type))
            {
                DropType(newArmor.type);
            }
            else
            {
                return false;
            }
        }


        bool addToLast = newArmor.type == Armor.ArmorType.Shield;

        if (character.GunShooter && newArmor.type == Armor.ArmorType.Shield)
        {
            if (character.GunShooter.guns.Count == 2)
            {
                character.GunShooter.DropOtherGun();
            }
        }

        for (int i = 0; i < armor.Count; i++)
        {
            if (armor[i].armor.type == newArmor.type)
            {
                ItemManager.DropArmor(transform.position, armor[i].armor.name);

                armor[i].armor = newArmor;

                if(newArmor.audio)
                {
                    AudioClip clip = newArmor.audio.GetClip("Equip");
                    if(clip)
                    {
                        PlayArmorSound(newArmor, clip.name);
                    }
                }
                Refresh();

                return true;
            }
        }

        if (addToLast)
        {
            armor.Add(new ArmorSet(newArmor));
        }
        else
        {
            armor.Insert(0, new ArmorSet(newArmor));
        }
        if(newArmor.audio)
        {
            AudioClip clip = newArmor.audio.GetClip("Equip");
            if(clip)
            {
                PlayArmorSound(newArmor, clip.name);
            }
        }
        Refresh();

        return true;
    }

    public bool CanPickup(Armor armor)
    {
        return !(this.armor.Count == maxArmor && !HasType(armor.type));
    }

    public void DropType(Armor.ArmorType type, bool spawnPickup = true)
    {
        for (int i = 0; i < armor.Count; i++)
        {
            if (armor[i].armor.type == type)
            {
                if (armor[i].armor.audio && spawnPickup)
                {
                    AudioClip clip = armor[i].armor.audio.GetClip("ArmorDrop");
                    if (clip)
                    {
                        PlayArmorSound(armor[i].armor, clip.name);
                    }
                }

                if (spawnPickup) ItemManager.DropArmor(transform.position, armor[i].armor.name);
                armor.RemoveAt(i);
                Refresh();
            }
        }
    }

    public bool HasType(Armor.ArmorType type)
    {
        for (int i = 0; i < armor.Count; i++)
        {
            if (armor[i].armor.type == type) return true;
        }

        return false;
    }

    public void Dispose()
    {
        for (int i = 0; i < armor.Count; i++)
        {
            armor[i].renderer.color = Color.gray;
            armor[i].renderer.transform.localPosition = new Vector2(armor[i].armor.position.x * (spritePlayer.spriteRenderer.flipX ? -1 : 1), 0f);
        }
    }

    private void Awake()
    {
        id = Helper.RandomID;

        armor.Clear();

        character = GetComponent<Character>();
        sound = GetComponent<ObjectSoundEmitter>();
        looker = GetComponent<CharacterLook>();

        sound.CreateSource("Armor", AudioManager.AudioType.Pickups);

        if (!root) root = transform.Find("Armor");
        if (!root)
        {
            root = new GameObject("Armor").transform;
            root.SetParent(transform);
            root.localPosition = Vector3.zero;
        }
        for (int i = 0; i < startingArmor.Count;i++)
        {
            Equip(startingArmor[i].name);
        }
    }

    public void RemoveAllArmor()
    {
        DropType(Armor.ArmorType.Arms, false);
        DropType(Armor.ArmorType.Chest, false);
        DropType(Armor.ArmorType.Feet, false);
        DropType(Armor.ArmorType.Head, false);
        DropType(Armor.ArmorType.Shield, false);
    }

    void Refresh()
    {
        for (int i = 0; i < armor.Count; i++)
        {
            if (armor[i].renderer)
            {
                Destroy(armor[i].renderer.gameObject);
            }

            InterfaceAlmanacLayer.Found(armor[i].armor);
            armor[i].renderer = new GameObject(armor[i].armor.name).AddComponent<SpriteRenderer>();
            armor[i].renderer.transform.SetParent(root);
        }

        if (!root) return;

        for (int i = 0; i < root.childCount; i++)
        {
            if (!HasArmor(root.GetChild(i).name))
            {
                Destroy(root.GetChild(i).gameObject);
            }
        }
    }

    public bool HasArmor(string name)
    {
        for (int i = 0; i < armor.Count; i++)
        {
            if (armor[i].armor.name == name) return true;
        }
        return false;
    }

    public bool HasShield
    {
        get
        {
            for (int i = 0; i < armor.Count; i++)
            {
                if (armor[i].armor.type == Armor.ArmorType.Shield) return true;
            }

            return false;
        }
    }

    public bool ContactDamage
    {
        get
        {
            for (int i = 0; i < armor.Count; i++)
            {
                if (armor[i].armor.type == Armor.ArmorType.Shield)
                {
                    Quality qual = armor[i].armor.GetQuality("PlayerContactDamage");
                    return qual.Valid ? qual.Bool : false;
                }
            }

            return false;
        }
    }

    public bool ShieldEnabled
    {
        get
        {
            for (int i = 0; i < armor.Count; i++)
            {
                if (armor[i].armor.type == Armor.ArmorType.Shield) return armor[i].shieldEnabled;
            }

            return false;
        }
    }

    public bool Reflect(Vector2 hitPosition)
    {
        if (armor.Count == 0) return false;

        bool success = ChanceResult("ArmorReflect", hitPosition);
        if(success)
        {
            ShowText("REFLECT!");
        }

        return success;
    }

    public bool BlockContact(Vector2 hitPosition)
    {
        if (armor.Count == 0) return false;
        bool success = ChanceResult("ArmorBlockContact", hitPosition);
        if (success)
        {
            ShowText("BLOCK!");
        }

        return success;
    }

    public bool Block(Vector2 hitPosition, bool isMelee)
    {
        if (armor.Count == 0) return false;
        bool success = ChanceResult(isMelee ? "ArmorBlockMelee" : "ArmorBlock", hitPosition);
        if (success)
        {
            ShowText("BLOCK!");
        }
        return success;
    }

    int GetChanceValue(List<Armor.Chance> chances, string name)
    {
        return GetChanceValue(chances.ToArray(), name);
    }

    int GetChanceValue(Armor.Chance[] chances, string name)
    {
        for (int i = 0; i < chances.Length; i++)
        {
            if (chances[i].name == name) return chances[i].chance;
        }

        return 0;
    }

    bool ChanceResult(string name, Vector2 hitPosition)
    {
        Vector2 dir = hitPosition - (Vector2)transform.position;
        bool isHead = hitPosition.y > transform.position.y + headOffset;
        bool fromRight = hitPosition.x > transform.position.x;

        bool success = false;
        for (int i = 0; i < armor.Count; i++)
        {
            Quality quality = armor[i].armor.GetQuality(name);
            float chance = quality.Valid ? quality.Float : 0f;
            bool register = chance >= Random.Range(0, 100);
            if (armor[i].armor.type == Armor.ArmorType.Shield)
            {
                if(armor[i].shieldEnabled)
                {
                    if (looker)
                    {
                        //only register if the angle diff is between -25 and 25
                        float lookAngle = Vector2.Angle(dir.normalized, looker.lookDirection.normalized);
                        if (lookAngle > 70)
                        {
                            register = false;
                        }
                    }
                    else
                    {
                        register = armor[i].renderer.flipX == fromRight;
                    }
                }
                else
                {
                    register = false;
                }
            }
            else if (armor[i].armor.type == Armor.ArmorType.Head && !isHead)
            {
                register = false;
            }
            else if (armor[i].armor.type != Armor.ArmorType.Head && isHead)
            {
                register = false;
            }

            if (register)
            {
                success = true;
                if(armor[i].armor && armor[i].armor.audio)
                {
                    AudioClip clip = armor[i].armor.audio.GetClip(name);
                    if(clip)
                    {
                        PlayArmorSound(armor[i].armor, clip.name);
                    }
                }
            }
        }
        return success;
    }

    void PlayArmorSound(Armor armor, string clip)
    {
        sound.GetSource("Armor").pitch = Random.Range(0.9f, 1.1f);
        sound.PlaySound(clip, "Armor");
    }

    void ShowText(string text)
    {
        UIManager.DrawNotificationText(id, transform.position, "<color=red>"+text+"</color>", 0.3f, "console");
    }

    private void Update()
    {
        if (!character.Process) return;

        if (GameManager.Paused) return;

        if (spritePlayer) spritePlayer.armor = this;

        if (character && character.input.dropArmor && armor.Count > 0)
        {
            if (armor[0].armor.audio)
            {
                AudioClip clip = armor[0].armor.audio.GetClip("ArmorDrop");
                if (clip)
                {
                    PlayArmorSound(armor[0].armor, clip.name);
                }
            }

            ItemManager.DropArmor(transform.position, armor[0].armor.name);
            armor.RemoveAt(0);
            Refresh();

            return;
        }
        
        for (int i = 0; i < armor.Count; i++)
        {
            if (!armor[i].armor.visible)
            {
                armor[i].renderer.sprite = null;
            }
            else
            {
                int order = armor[i].armor.order == Armor.ArmorOrder.Front ? 1 : -1;

                armor[i].renderer.sprite = armor[i].armor.icon;
                armor[i].renderer.sortingLayerName = spritePlayer.spriteRenderer.sortingLayerName;

                if (spritePlayer.spriteRenderer)
                {
                    armor[i].renderer.flipX = spritePlayer.spriteRenderer.flipX;
                    armor[i].renderer.flipY = spritePlayer.spriteRenderer.flipY;
                }

                if (armor[i].armor.type == Armor.ArmorType.Shield)
                {
                    if(armor[i].shieldEnabled != Input.GetKey(KeyCode.Mouse1))
                    {
                        armor[i].shieldEnabled = Input.GetKey(KeyCode.Mouse1);
                        if(armor[i].shieldEnabled)
                        {
                            sound.PlaySound("ArmorEquip2", "Armor");
                        }
                    }
                    if (looker)
                    {
                        Vector3 lookPos = transform.position;
                        lookPos += (Vector3)looker.lookDirection.normalized * 64f;

                        armor[i].renderer.flipX = looker.lookDirection.x < 0f && armor[i].shieldEnabled;
                        armor[i].renderer.flipY = looker.lookDirection.x < 0f;
                        armor[i].renderer.transform.LookAt2D(lookPos, 90f + armor[i].armor.rotation);
                    }

                    armor[i].renderer.sortingOrder = spritePlayer.spriteRenderer.sortingOrder + 5 * order;
                    armor[i].renderer.sprite = armor[i].shieldEnabled ? armor[i].armor.icon : armor[i].armor.shieldDisabledIcon;
                }
                else if (armor[i].armor.type == Armor.ArmorType.Head)
                {
                    armor[i].renderer.sortingOrder = spritePlayer.spriteRenderer.sortingOrder + 4 * order;
                }
                else
                {
                    armor[i].renderer.sortingOrder = spritePlayer.spriteRenderer.sortingOrder + 3 * order;
                }

                if (armor[i].armor.type == Armor.ArmorType.Shield)
                {
                    if (armor[i].shieldEnabled)
                    {
                        Vector3 vec = new Vector2(armor[i].armor.position.x * (spritePlayer.spriteRenderer.flipX ? -1 : 1), armor[i].armor.position.y);
                        armor[i].renderer.transform.localPosition = vec;
                    }
                    else
                    {
                        Vector3 vec = new Vector2(armor[i].armor.shieldIdlePosition.x * (spritePlayer.spriteRenderer.flipX ? -1 : 1), armor[i].armor.shieldIdlePosition.y);
                        armor[i].renderer.transform.localPosition = vec;
                    }
                }
                else
                {
                    armor[i].renderer.transform.localPosition = new Vector2(armor[i].armor.position.x * (spritePlayer.spriteRenderer.flipX ? -1 : 1), armor[i].armor.position.y);
                }
                if (armor[i].armor.type != Armor.ArmorType.Shield)
                {
                    armor[i].renderer.transform.localEulerAngles = new Vector3(0, 0, armor[i].armor.rotation * (spritePlayer.spriteRenderer.flipX ? -1 : 1));
                }

                if (spritePlayer) armor[i].renderer.transform.localPosition += new Vector3(spritePlayer.ArmorOffset.x * (spritePlayer.spriteRenderer.flipX ? -1 : 1), spritePlayer.ArmorOffset.y);
            }
        }
    }
}
