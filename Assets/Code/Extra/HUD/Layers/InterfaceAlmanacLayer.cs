using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Data;

public class InterfaceAlmanacLayer : InterfaceLayer {

    public SpritePlayer pageFlipper;
    List<Image> imageSlots = new List<Image>();
    public int page = 0;
    public int maxPage = 2;
    public int itemScale = 2;
    public int displayItemScale = 3;
    RectTransform selecseltor;
    public Image itemIcon;
    public Text nameText;
    public Text descText;
    public Text pager;
    public Text itemInfo;
    List<IItem> items = new List<IItem>();
    AudioSource source;
    public AudioClip selectSound;
    public AudioClip pageFlip;

    public int x;
    public int y;

    public Transform prefab;
    public int rows = 6;
    public int columns = 3;
    public int gap = 32;

    int lastRows;
    int lastColumns;
    int lastIndex;

    public int GetValue()
    {
        return 0;
    }

    private void OnEnable()
    {
        Refresh();
        LoadPage(page);
    }

    protected override void Awake()
    {
        base.Awake();
        source = GetComponent<AudioSource>();
    }

    void Update()
    {
        pager.text = "page "+(page + 1) + "";
        if (lastRows != rows || lastColumns != columns || items == null)
        {
            lastRows = rows;
            lastColumns = columns;
            
            Refresh();
            LoadPage(page);
        }

        itemInfo.text = "";

        bool empty = true;
        int i = 0;
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (i < imageSlots.Count)
                {
                    int itemIndex = i + (page * rows * columns);
                    if(itemIndex >= items.Count)
                    {
                        itemIndex = items.Count - 1;
                    }

                    if (x == this.x && y == this.y)
                    {
                        empty = false;

                        if (lastIndex != itemIndex)
                        {
                            source.PlayOneShot(selectSound);
                            lastIndex = itemIndex;
                            imageSlots[i].transform.parent.localScale = Vector3.one * 1.5f;
                        }

                        Shadow shadowComp = itemIcon.GetComponent<Shadow>();
                        shadowComp.enabled = items[itemIndex] is Gun && ((Gun)items[itemIndex]).wieldMode == Gun.GunWieldMode.Dual;

                        selecseltor.position = Vector3.Lerp(selecseltor.position, imageSlots[i].transform.position, Time.deltaTime * 15f);
                        selecseltor.SetAsLastSibling();

                        itemIcon.sprite = items[itemIndex].GetIcon();
                        itemIcon.rectTransform.sizeDelta = new Vector2(itemIcon.sprite.rect.width * displayItemScale, itemIcon.sprite.rect.height * displayItemScale);
                        itemIcon.rectTransform.anchoredPosition = new Vector2(itemIcon.rectTransform.sizeDelta.x % 2f == 0 ? -100 : -100.5f, itemIcon.rectTransform.sizeDelta.y % 2f == 0 ? 100 : 100.5f);

                        bool found = HasFound(items[itemIndex]);
                        nameText.text = items[itemIndex].GetName().ToUpper();
                        if (found)
                        {
                            itemIcon.color = Color.white;
                            shadowComp.effectColor = Color.white;
                            string itemDesc = items[itemIndex].GetDescription().ToUpper();
                            string desc = itemDesc + "\n\n<color=yellow>COSTS " + items[itemIndex].GetPrice() + " GOLD</color>";
                            if (itemDesc == "") desc = "<color=yellow>COSTS " + items[itemIndex].GetPrice() + " GOLD</color>";

                            descText.text = desc;
                        }
                        else
                        {
                            itemIcon.color = Color.black;
                            shadowComp.effectColor = Color.black;
                            descText.text = "<color=gray>UNKNOWN</color>";
                        }

                        itemInfo.text = items[itemIndex].GetType().ToString().ToUpper()+" #"+items[itemIndex].GetID();
                    }

                    imageSlots[i].enabled = true;
                    Image parentImage = imageSlots[i].transform.parent.GetComponent<Image>();
                    parentImage.enabled = true;

                    imageSlots[i].transform.parent.localPosition = new Vector3(37, 118, 0) + new Vector3(x * gap, y * -gap);
                    imageSlots[i].transform.parent.localScale = Vector3.Lerp(imageSlots[i].transform.parent.localScale, Vector3.one, Time.deltaTime * 15f);
                }
                i++;
            }
        }

        if(empty)
        {
            itemIcon.sprite = null;
            itemIcon.rectTransform.sizeDelta = Vector2.zero;
            descText.text = "";
            itemInfo.text = "";
            descText.text = "";
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(Character.Player)
            {
                HUD.ChangeLayer("InGame");
            }
            else
            {
                HUD.ChangeLayer("Menu");
            }
        }

        if((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) && y > 0)
        {
            y--;
        }
        if((Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) && y < rows-1)
        {
            y++;
        }
        if((Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)))
        {
            if (x == 0)
            {
                if(page > 0)
                {
                    page--;
                    pageFlipper.Play("Previous", true);
                    LoadPage(page);
                }
            }
            else
            {
                x--;
            }
        }
        if ((Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)))
        {
            if (x == columns - 1)
            {
                if(page < maxPage)
                {
                    page++;
                    pageFlipper.Play("Next", true);
                    LoadPage(page);
                }
            }
            else
            {
                x++;
            }
        }
    }

    public static void FindAll()
    {
        InterfaceAlmanacLayer almanac = FindObjectOfType<InterfaceAlmanacLayer>();
        for (int i = 0; i < almanac.items.Count; i++)
        {
            Found(almanac.items[i]);
        }
    }

    public static void ResetAll()
    {
        InterfaceAlmanacLayer almanac = FindObjectOfType<InterfaceAlmanacLayer>();
        for(int i = 0; i < almanac.items.Count;i++)
        {
            Forget(almanac.items[i]);
        }
    }

    public static void Forget(IItem item)
    {
        if (item == null) return;

        PlayerPrefs.SetInt(item.GetID() + ":" + item.GetType(), 0);
    }

    public static bool HasFound(IItem item)
    {
        if (item == null) return false;

        return PlayerPrefs.GetInt(item.GetID() + ":" + item.GetType(), 0) == 1;
    }

    public static void Found(IItem item)
    {
        if (item == null) return;

        PlayerPrefs.SetInt(item.GetID() + ":" + item.GetType(), 1);
    }
    
    void Refresh()
    {
        selecseltor = transform.Find("Selector").GetComponent<RectTransform>();

        int i = 0;
        for (i = 0; i < imageSlots.Count; i++)
        {
            Destroy(imageSlots[i].transform.parent.gameObject);
        }

        imageSlots.Clear();

        i = 0;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns;x++)
            {
                if (i < items.Count)
                {
                    GameObject newSlot = Instantiate(prefab.gameObject);

                    newSlot.transform.SetParent(transform);

                    newSlot.SetActive(true);

                    Image newImage = newSlot.transform.Find("Item").GetComponent<Image>();

                    imageSlots.Add(newImage);
                }

                i++;
            }
        }
    }

    void LoadPage(int page)
    {
        source.PlayOneShot(pageFlip);
        if (!ItemManager.singleton) return;

        items = new List<IItem>();
        for(int i = 0; i < ItemManager.singleton.items.Count;i++)
        {
            if (!ItemManager.singleton.items[i].ready) continue;

            items.Add(ItemManager.singleton.items[i]);
        }
        for (int i = 0; i < ItemManager.singleton.consumables.Count; i++)
        {
            if (!ItemManager.singleton.consumables[i].ready) continue;

            items.Add(ItemManager.singleton.consumables[i]);
        }
        for (int i = 0; i < ItemManager.singleton.armor.Count; i++)
        {
            if (!ItemManager.singleton.armor[i].ready) continue;

            items.Add(ItemManager.singleton.armor[i]);
        }
        for (int i = 0; i < ItemManager.singleton.guns.Count; i++)
        {
            if (ItemManager.singleton.guns[i].enemyOnly) continue;
            if (!ItemManager.singleton.guns[i].ready) continue;

            items.Add(ItemManager.singleton.guns[i]);
        }

        //n => HasFound(n) ? 1 : 0

        items = items.OrderBy(x => x.GetName()).ThenBy(n => HasFound(n) ? 1 : 0).ToList();

        maxPage = Mathf.CeilToInt(items.Count / (rows * columns));
        
        for (int i = 0; i < imageSlots.Count;i++)
        {
            if (items.Count > (rows * columns * page) + i)
            {
                IItem item = items[(rows * columns * page) + i];
                imageSlots[i].name = item.GetName();
                imageSlots[i].sprite = item.GetIcon();
                Color color = HasFound(item) ? Color.white : Color.black;
                imageSlots[i].color = color;

                if (imageSlots[i].sprite)
                {
                    imageSlots[i].rectTransform.sizeDelta = new Vector2(imageSlots[i].sprite.rect.width * itemScale, imageSlots[i].sprite.rect.height * itemScale);
                }

                imageSlots[i].rectTransform.anchoredPosition = new Vector2(imageSlots[i].rectTransform.sizeDelta.x % 2 == 0 ? 0 : 0.5f, imageSlots[i].rectTransform.sizeDelta.y % 2 == 0 ? 0 : 0.5f);

                if (item is Gun)
                {
                    Gun gun = (Gun)item;

                    if (gun.wieldMode == Gun.GunWieldMode.Dual)
                    {
                        Shadow shadowComp = imageSlots[i].GetComponent<Shadow>();
                        if(!shadowComp) shadowComp = imageSlots[i].gameObject.AddComponent<Shadow>();

                        shadowComp.effectDistance = Vector2.one * 3;
                        shadowComp.effectColor = color;
                    }
                    else
                    {
                        Shadow shadowComp = imageSlots[i].GetComponent<Shadow>();
                        if (shadowComp) Destroy(shadowComp);
                    }
                }
                else
                {
                    Shadow shadowComp = imageSlots[i].GetComponent<Shadow>();
                    if (shadowComp) Destroy(shadowComp);
                }
            }
            else
            {
                imageSlots[i].sprite = null;
                imageSlots[i].rectTransform.sizeDelta = Vector2.zero;
                imageSlots[i].name = "";
            }
        }
    }
}
