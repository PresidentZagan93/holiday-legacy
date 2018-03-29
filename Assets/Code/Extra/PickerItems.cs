using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Data;

public class PickerItems : MonoBehaviour
{
    public delegate void OnChosen(IItem item);
    OnChosen chosen;

    public enum PickerType
    {
        Items,
        Consumables,
        Guns,
        Builds,
        Armor
    }
    
    int options = 0;
    public PickerType type = PickerType.Builds;
    public float itemScale = 2f;
    public int itemGap = 24;
    public float selectedItemScale = 3f;

    public Text nameText;
    public Text descText;
    public Text extraInfo;

    public Transform selector;
    int selIndex = 0;
    int lastIndex = -1;
    public AudioClip select;
    AudioSource source;
    RectTransform rectTransform;

    List<IItem> choices = new List<IItem>();
    List<SpriteRenderer> images = new List<SpriteRenderer>();

    bool lastLeft;
    bool lastRight;

    static PickerItems singleton;

    public static bool Active
    {
        get
        {
            return singleton && singleton.gameObject.activeSelf;
        }
    }

    void Update()
    {
        rectTransform.anchoredPosition3D = new Vector3(0f, 0f, 3f);
        rectTransform.sizeDelta = new Vector2(0f, 0f);
        rectTransform.localScale = Vector3.one;

        singleton = this;
        for (int i = 0; i < images.Count; i++)
        {
            if (!images[i])
            {
                images.RemoveAt(i);
            }
        }

        if (images.Count > selIndex && selIndex >= 0 && selector)
        {
            if (images[selIndex])
            {
                selector.transform.position = Vector3.Lerp(selector.transform.position, images[selIndex].transform.position, Time.deltaTime * 20f);
            }
        }
        if (lastIndex != selIndex && images[selIndex])
        {
            lastIndex = selIndex;
            Select(images[selIndex].gameObject);
            images[selIndex].transform.localScale = Vector3.one * selectedItemScale * 1.5f;
        }

        int dir = 0;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            dir = 1;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            dir = -1;
        }

        selIndex += dir;

        if (selIndex > images.Count - 1) selIndex = 0;
        if (selIndex < 0) selIndex = images.Count - 1;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("Confirm"))
        {
            HUD.Shake(1);
            Click(images[selIndex].gameObject);
        }

        for (int i = 0; i < images.Count; i++)
        {
            if (images[i])
            {
                float y = 16f + Mathf.Sin((Time.time + i * 15f) * 10f);

                images[i].transform.localEulerAngles = Vector3.zero;
                float x = i - selIndex;
                x *= itemGap * itemScale;

                images[i].transform.localPosition = Vector3.Lerp(images[i].transform.localPosition, new Vector3(x, y, 0f), Time.deltaTime * 15f);
                images[i].transform.localPosition = new Vector3(images[i].transform.localPosition.x, y, 0f);

                images[i].sprite = GetItem(images[i].gameObject).GetIcon();
                images[i].transform.localScale = Vector3.Lerp(images[i].transform.localScale, Vector3.one * (selIndex == i ? selectedItemScale : itemScale), Time.deltaTime * 5f);
            }
        }
    }

    IItem GetItem(GameObject img)
    {
        for (int i = 0; i < choices.Count; i++)
        {
            if (img.name == choices[i].GetName())
            {
                return choices[i];
            }
        }
        return null;
    }

    void OnEnable()
    {
        nameText.text = string.Empty;
        descText.text = string.Empty;
        extraInfo.text = string.Empty;

#if UNITY_EDITOR
        if (options == 0) return;

        Initialize(type, options);
#endif
    }

    void Awake()
    {
        singleton = this;
        source = GetComponent<AudioSource>();
        AudioManager.Assign(source);
        rectTransform = GetComponent<RectTransform>();
    }

    public static void CreatePicker(PickerType type, OnChosen method, int options = -1)
    {
        GameObject builds = Instantiate(ObjectManager.GetPrefab("Picker"));
        builds.transform.position = Vector3.zero;
        builds.transform.SetParent(GameObject.Find("UI/GameCanvas").transform);

        PickerItems picker = builds.GetComponent<PickerItems>();
        picker.chosen = method;
        picker.Initialize(type, options);
    }

    void Initialize(PickerType type, int options = -1)
    {
        this.type = type;
        this.options = options;

        List<IItem> validOptions = new List<IItem>();
        if (type == PickerType.Armor)
        {
            for (int i = 0; i < ItemManager.singleton.armor.Count; i++)
            {
                validOptions.Add(ItemManager.singleton.armor[i]);
            }
        }
        if (type == PickerType.Builds)
        {
            for (int i = 0; i < ItemManager.AllPlayerBuilds.Count; i++)
            {
#if !UNITY_EDITOR
                if(!ItemManager.AllPlayerBuilds[i].editorOnly)
                {
                    validOptions.Add(ItemManager.AllPlayerBuilds[i]);
                }
#else
                validOptions.Add(ItemManager.AllPlayerBuilds[i]);
#endif
            }
        }
        if (type == PickerType.Consumables)
        {
            for (int i = 0; i < ItemManager.singleton.consumables.Count; i++)
            {
                validOptions.Add(ItemManager.singleton.consumables[i]);
            }
        }
        if (type == PickerType.Guns)
        {
            for (int i = 0; i < ItemManager.singleton.guns.Count; i++)
            {
                if (ItemManager.singleton.guns[i].ready && !ItemManager.singleton.guns[i].enemyOnly) validOptions.Add(ItemManager.singleton.guns[i]);
            }
        }
        if (type == PickerType.Items)
        {
            for (int i = 0; i < ItemManager.singleton.items.Count; i++)
            {
                validOptions.Add(ItemManager.singleton.items[i]);
            }
        }

        if (options == -1)
        {
            this.options = validOptions.Count;
            choices.AddRange(validOptions);
        }
        else
        {
            validOptions.Randomize();
            for (int i = 0; i < options; i++)
            {
                choices.Add(validOptions[i]);
            }
        }

        CreateOptions();
    }

    void CreateOptions()
    {
        Transform[] slotsFound = transform.Find("Slots").GetComponentsInChildren<Transform>();
        for (int i = 0; i < slotsFound.Length; i++)
        {
            if (slotsFound[i].parent.name == "Slots")
            {
                Destroy(slotsFound[i].gameObject);
            }
        }

        for (int i = 0; i < choices.Count; i++)
        {
            GameObject newItem = new GameObject("Slot");
            newItem.AddComponent<RectTransform>();
            newItem.SetLayer("UI");

            newItem.transform.SetParent(transform.Find("Slots"));
            newItem.transform.localScale = Vector2.zero;

            SpriteRenderer rend = newItem.AddComponent<SpriteRenderer>();
            rend.sortingLayerName = "Weather";
            rend.sprite = choices[i].GetIcon();
            rend.name = choices[i].GetName();
            
            images.Add(rend);
        }

        lastIndex = -1;
        if (type == PickerType.Builds)
        {
            selIndex = Mathf.Clamp(Settings.Game.lastBuild, 0, choices.Count - 1);
        }
    }

    void Select(GameObject img)
    {
        source.PlayOneShot(select);

        nameText.text = string.Empty;
        descText.text = string.Empty;
        extraInfo.text = string.Empty;

        IItem item = GetItem(img);

        nameText.text = item.GetName().ToUpper();
        descText.text = item.GetDescription().ToUpper();

        if (type == PickerType.Builds)
        {
            PlayerBuild build = item as PlayerBuild;
            for (int w = 0; w < build.startingWeapons.Length; w++)
            {
                extraInfo.text += build.startingWeapons[w].name.ToUpper() + "\n";
            }
            for (int w = 0; w < build.startingArmor.Length; w++)
            {
                extraInfo.text += build.startingArmor[w].name.ToUpper() + "\n";
            }
        }
    }

    void Click(GameObject img)
    {
        IItem item = GetItem(img);

        if (type == PickerType.Builds)
        {
            Settings.Game.lastBuild = selIndex;
        }

        if(chosen != null) chosen.Invoke(item);

        Destroy(gameObject);
    }
}
