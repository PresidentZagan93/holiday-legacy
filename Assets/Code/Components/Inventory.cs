using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;

[System.Serializable]
public class Multiplier
{
    public MultiplierType multiplierType = MultiplierType.Multiply;
    public ItemType type = ItemType.None;
    public float value = 1f;
    public bool downside = false;

    public Multiplier(ItemType type, MultiplierType multiplierType, float value)
    {
        this.multiplierType = multiplierType;
        this.type = type;
        this.value = value;
    }

    public Multiplier()
    {
        this.multiplierType = MultiplierType.Multiply;
        this.type = ItemType.None;
        this.value = 1f;
    }

    public override string ToString()
    {
        string toRet = type.ToString() + " ";
        if (multiplierType == MultiplierType.Additive)
        {
            toRet += "+" + value;
        }
        if (multiplierType == MultiplierType.Multiply)
        {
            toRet += "x" + value;
        }
        if (multiplierType == MultiplierType.Set)
        {
            toRet += "=" + value;
        }

        string color = downside ? "red" : "lime";
        return "<color=" + color + ">" + toRet + "</color>";
    }

    public static bool ContainsWord(Multiplier[] arr, string word)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i].type.ToString().ToLower().Contains(word.ToLower()))
            {
                return true;
            }
        }
        return false;
    }
}

public class Inventory : MonoBehaviour {
    
    public enum InventoryItemType
    {
        Item,
        Armor,
        Gun,
        Consumable
    }

    [System.Serializable]
    public class ConsumableSlot
    {
        public Consumable item;
        public bool empty;
        public float time;
        public float duration;
        public int level;

        bool active;

        public bool Active
        {
            get
            {
                return active;
            }
        }

        public ConsumableSlot(Consumable item)
        {
            this.item = item;

            level = 1;
            active = false;
            time = 0f;
            if(item)
            {
                duration = item.duration;
            }
        }

        public void Enable()
        {
            active = true;
        }
    }


    public ConsumableSlot consumable;
    public List<Item> items = new List<Item>();

    public bool Contains(string name)
    {
        if(HasConsumable)
        {
            if(consumable.Active && consumable.item.name == name)
            {
                return true;
            }
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].name == name)
            {
                return true;
            }
        }

        return false;
    }

    public bool Full(string name)
    {
        if (HasConsumable)
        {
            if (consumable.Active && consumable.item.name == name)
            {
                return true;
            }
        }

        int repeats = 0;
        Item item = null;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].name == name)
            {
                item = items[i];
                repeats++;
            }
        }

        if (!item) return false;
        return repeats == item.maxStacks;
    }

    public ConsumableSlot GetConsumable()
    {
        if (!HasConsumable) return null;

        return consumable;
    }

    public bool HasConsumable
    {
        get
        {
            return !consumable.empty;
        }
    }

    public void Remove(Consumable consumable)
    {
        if (!this.consumable.empty && !this.consumable.Active)
        {
            this.consumable.empty = true;
        }
    }

    public void Add(Consumable consumable)
    {
        if(this.consumable.empty)
        {
            this.consumable = new ConsumableSlot(consumable)
            {
                empty = false
            };
        }
    }

    public void Add(Item item)
    {
        for (int i = 0; i < item.qualities.Count; i++)
        {
            //Inventory.GetMultiplier("HealthIncrease");
            //Inventory.GetMultiplier("HealthHeal");

            if (item.qualities[i].name == "HealthIncrease")
            {
                character.Health.maxHp += item.qualities[i].Int;
            }
            if (item.qualities[i].name == "HealthHeal")
            {
                character.Health.Heal(item.qualities[i].Int);
            }
        }

        items.Add(item);

        if (item.type == Consumable.ConsumableType.Companion)
        {
            CreateCompanion(item.name);
        }
        if(item.name == "Map")
        {
            MinimapManager.Generate();
            MinimapManager.Refresh();
        }
    }

    public bool HasItemRequirements(string itemName)
    {
        Item item = ItemManager.GetItem(itemName);
        if (item == null) return false;
        if (item.requirements.Length == 0) return true;

        for (int i = 0; i < item.requirements.Length; i++)
        {
            if (!Contains(item.requirements[i].name))
            {
                return false;
            }
        }

        return true;
    }

    public bool HasQuality(string name)
    {
        for (int i = 0; i < items.Count; i++)
        {
            for (int m = 0; m < items[i].qualities.Count; m++)
            {
                if (items[i].qualities[m].name == name)
                {
                    return true;
                }
            }
        }

        if(HasConsumable && consumable.Active)
        {
            for (int m = 0; m < consumable.item.qualities.Count; m++)
            {
                if (consumable.item.qualities[m].name == name)
                {
                    return true;
                }
            }
        }

        if (character.Armor)
        {
            for (int i = 0; i < character.Armor.armor.Count; i++)
            {
                for (int m = 0; m < character.Armor.armor[i].armor.qualities.Count; m++)
                {
                    if (character.Armor.armor[i].armor.qualities[m].name == name)
                    {
                        return true;
                    }
                }
            }
        }

        if(character.Health)
        {
            for (int i = 0; i < character.Health.effectsApplied.Count; i++)
            {
                if (character.Health.effectsApplied[i].quality.name == name)
                {
                    return true;
                }
            }
        }

        if (character.GunShooter)
        {
            for (int i = 0; i < character.GunShooter.guns.Count; i++)
            {
                for (int m = 0; m < character.GunShooter.guns[i].gun.qualities.Count; m++)
                {
                    if (character.GunShooter.guns[i].gun.qualities[m].name == name)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public bool GetState(string name)
    {
        for (int i = 0; i < items.Count; i++)
        {
            for (int m = 0; m < items[i].qualities.Count; m++)
            {
                if (items[i].qualities[m].name == name)
                {
                    return items[i].qualities[m].Bool;
                }
            }
        }

        if (HasConsumable)
        {
            if (consumable.Active)
            {
                for (int m = 0; m < consumable.item.qualities.Count; m++)
                {
                    if (consumable.Active && consumable.item.qualities[m].name == name)
                    {
                        return consumable.item.qualities[m].Bool;
                    }
                }
            }
        }

        if(character.Armor)
        {
            for (int i = 0; i < character.Armor.armor.Count; i++)
            {
                for (int m = 0; m < character.Armor.armor[i].armor.qualities.Count; m++)
                {
                    if (character.Armor.armor[i].armor.qualities[m].name == name)
                    {
                        return character.Armor.armor[i].armor.qualities[m].Bool;
                    }
                }
            }
        }

        if (character.Health)
        {
            for (int i = 0; i < character.Health.effectsApplied.Count; i++)
            {
                if (character.Health.effectsApplied[i].quality.name == name)
                {
                    return character.Health.effectsApplied[i].quality.Bool;
                }
            }
        }

        if (character.GunShooter)
        {
            for (int i = 0; i < character.GunShooter.guns.Count; i++)
            {
                for (int m = 0; m < character.GunShooter.guns[i].gun.qualities.Count; m++)
                {
                    if (character.GunShooter.guns[i].gun.qualities[m].name == name)
                    {
                        return character.GunShooter.guns[i].gun.qualities[m].Bool;
                    }
                }
            }
        }

        return false;
    }

    public float GetMultiplier(string name)
    {
        float value = 1f;
        for (int i = 0; i < items.Count; i++)
        {
            for (int m = 0; m < items[i].qualities.Count; m++)
            {
                if (items[i].qualities[m].name == name)
                {
                    value *= items[i].qualities[m].Float;
                }
            }
        }

        if (HasConsumable)
        {
            if(consumable.Active)
            {
                for (int m = 0; m < consumable.item.qualities.Count; m++)
                {
                    if (consumable.Active && consumable.item.qualities[m].name == name)
                    {
                        value *= consumable.item.qualities[m].Float;
                    }
                }
            }
        }

        if (character.Armor)
        {
            for (int i = 0; i < character.Armor.armor.Count; i++)
            {
                for (int m = 0; m < character.Armor.armor[i].armor.qualities.Count; m++)
                {
                    if (character.Armor.armor[i].armor.qualities[m].name == name)
                    {
                        value *= character.Armor.armor[i].armor.qualities[m].Float;
                    }
                }
            }
        }

        if (character.Health)
        {
            for (int i = 0; i < character.Health.effectsApplied.Count; i++)
            {
                if (character.Health.effectsApplied[i].quality.name == name)
                {
                    value *= character.Health.effectsApplied[i].quality.Float;
                }
            }
        }

        if (character.GunShooter)
        {
            for (int i = 0; i < character.GunShooter.guns.Count; i++)
            {
                for (int m = 0; m < character.GunShooter.guns[i].gun.qualities.Count; m++)
                {
                    if (character.GunShooter.guns[i].gun.qualities[m].name == name)
                    {
                        value *= character.GunShooter.guns[i].gun.qualities[m].Float;
                    }
                }
            }
        }

        return value;
    }

    public void UpdateKilled(Health victim)
    {
        if(HasConsumable)
        {
            if (consumable.Active)
            {
                consumable.level++;
                if (victim.transform.name == consumable.item.name)
                {
                    consumable.empty = true;
                }
                else
                {
                    if (consumable.item.extendOnKill)
                    {
                        consumable.duration += consumable.item.extendOnKillAmount;
                        sound.PlaySound("TimerExtended", "Inventory");
                    }
                }
            }
        }
    }

    public List<Character> companions = new List<Character>();

    SpritePlayer spritePlayer;
    Character character;
    Transform child;
    ObjectSoundEmitter sound;
    int uiId;
    bool hasConsumable;
    float nextUndraw;
    Transform itemsRoot;

    private void Awake()
    {
        uiId = Helper.RandomID;

        spritePlayer = GetComponentInChildren<SpritePlayer>();
        character = GetComponent<Character>();
        sound = GetComponent<ObjectSoundEmitter>();

        sound.CreateSource("Inventory", AudioManager.AudioType.Pickups);

        Transform createRoot = character ? (character.Movement ? character.Movement.root : character.transform) : transform;
        itemsRoot = Instantiate(ObjectManager.GetPrefab("InventoryEffects")).transform;
        itemsRoot.SetParent(createRoot);
        itemsRoot.localPosition = Vector3.zero;

        consumable = new ConsumableSlot(null)
        {
            empty = true
        };
    }

    public void CompanionDied(Character companion)
    {
        if(HasConsumable)
        {
            if(consumable.item.name == companion.name)
            {
                sound.PlaySound(consumable.item.audio.GetClip("Deactivate"), "Inventory");
                consumable.empty = true;
            }
        }
    }

    void CreateCompanion(string companion)
    {
        //spawn an egg
        CompanionEgg egg = Instantiate(ObjectManager.GetPrefab("CompanionEgg")).GetComponent<CompanionEgg>();
        egg.Initialize(companion, character);
    }

    void OnConsumableStarted(Consumable item)
    {
        if(item.type == Consumable.ConsumableType.Companion)
        {
            //spawn companion by this name
            CreateCompanion(item.name);
        }
    }

    void OnConsumableFinished(Consumable item)
    {
        sound.PlaySound(item.audio.GetClip("Deactivate"), "Inventory");

        if(item.type == Consumable.ConsumableType.Companion)
        {
            for (int i = 0; i < companions.Count; i++)
            {
                if (companions[i] && companions[i].name == item.name)
                {
                    //item deactivated is a companion
                    //kill this companion

                    companions[i].Owner = null;
                    companions.RemoveAt(i);
                    break;
                }
                if (!companions[i])
                {
                    companions.RemoveAt(i);
                }
            }
        }
    }

    void PlayerCode()
    {
        if (GameManager.Paused)
        {
            nextUndraw += Time.deltaTime;
            return;
        }

        if (hasConsumable && Time.time < nextUndraw)
        {
            UIManager.DrawText(uiId, transform.position + Vector3.up * -32, "PRESS SPACE TO ACTIVATE");
        }

        if (HasConsumable)
        {
            if (!hasConsumable)
            {
                nextUndraw = Time.time + 2f;
                hasConsumable = true;
            }

            if (character.input.activateItem && !consumable.Active)
            {
                consumable.Enable();
                sound.PlaySound(consumable.item.audio.GetClip("Activate"), "Inventory");

                OnConsumableStarted(consumable.item);
                nextUndraw = 0f;
            }

            Settings.Temporary.consumableName = consumable.item.name;
            Settings.Temporary.consumableIcon = consumable.item.icon;
            Settings.Temporary.consumableLevel = consumable.level + "";
            Settings.Temporary.consumableTime = consumable.item.forever ? "FOREVER" : (consumable.Active ? (consumable.duration - consumable.time).RoundToInt() + "" : consumable.duration + "");
        }
        else
        {
            hasConsumable = false;

            Settings.Temporary.consumableName = "";
            Settings.Temporary.consumableIcon = null;
            Settings.Temporary.consumableLevel = "";
            Settings.Temporary.consumableTime = "";
        }

        itemsRoot.localPosition = spritePlayer.ArmorOffset;
        if (character.Look) itemsRoot.localScale = new Vector3(character.Look.lookDirection.x > 0f ? 1f : -1f, 1f, 1f);

        for (int i = 0; i < itemsRoot.childCount; i++)
        {
            child = itemsRoot.GetChild(i);
            child.gameObject.SetActive(Contains(child.name));
        }
    }

    private void Update()
    {
        PlayerCode();
       
        if(HasConsumable && consumable.Active)
        {
            if (consumable.item.clearOnFinish && GeneratorManager.Generating) consumable.time = consumable.duration;

            if (!GameManager.Paused)
            {
                if (!GeneratorManager.Generating && !consumable.item.forever) consumable.time += Time.deltaTime;

                if (consumable.time >= consumable.duration)
                {
                    OnConsumableFinished(consumable.item);
                    consumable.empty = true;
                }
            }
        }
    }
}
