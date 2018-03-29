using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public enum UIType
    {
        Text,
        Image
    }

    [System.Serializable]
    public class UIElement
    {
        public int id;
        public float time;
        public UIType type = UIType.Text;
        public RectTransform transform;
        public List<MonoBehaviour> components = new List<MonoBehaviour>();
        public bool oddX;
        public bool oddY;

        public UIElement(int id, RectTransform transform, List<MonoBehaviour> components)
        {
            this.id = id;
            this.transform = transform;
            this.components = components;
        }
    }

    [System.Serializable]
    public class UINotification
    {
        public int id;
        public string content;
        public float timer;
        public string font;
        public float duration;
        public Vector2 position;
        public Vector2 endPosition;
        public UIType type;
        public bool worldSpace;

        public UINotification(int id, string content, float duration, Vector2 origin, string font, UIType type, bool worldSpace)
        {
            this.font = font;
            this.id = id;
            this.content = content;
            this.duration = duration;
            this.type = type;
            this.worldSpace = worldSpace;

            this.position = origin;
            this.endPosition = origin + Vector2.up * 32f;
        }
    }
    
    public FontType[] fonts;

    [System.Serializable]
    public class FontType
    {
        public string name = "";
        public Font font;
        public Material material;
        public int size = 16;
    }

    public TextAnchor textAnchor = TextAnchor.MiddleCenter;

    List<UIElement> elements = new List<UIElement>();
    List<UINotification> notifications = new List<UINotification>();
    static UIManager singleton;

    public Canvas overlayCanvas;
    public static Canvas OverlayCanvas
    {
        get
        {
            return singleton.overlayCanvas;
        }
    }

    public void Awake()
    {
        singleton = this;
    }

    private void OnEnable()
    {
        singleton = this;
    }

    private void Update()
    {
        if(overlayCanvas.transform.Find("Overlay").gameObject.activeSelf == GameManager.Paused)
        {
            overlayCanvas.transform.Find("Overlay").gameObject.SetActive(!GameManager.Paused);
        }

        for (int i = 0; i < notifications.Count; i++)
        {
            if(notifications[i].type == UIType.Text)
            {
                DrawTextInternal(notifications[i].id, notifications[i].position, notifications[i].content, notifications[i].worldSpace, false, notifications[i].font, HorizontalWrapMode.Wrap, TextAnchor.LowerCenter);
            }
            else
            {
                DrawImageInternal(notifications[i].id, notifications[i].position, ObjectManager.GetSprite(notifications[i].content), notifications[i].worldSpace);
            }

            //Vector3 vector = Vector2.Lerp(notifications[i].position, notifications[i].endPosition, Time.deltaTime * 15f);
            //notifications[i].position = new Vector3(vector.x.RoundToInt(), vector.y.RoundToInt(), vector.z.RoundToInt());

            notifications[i].timer += Time.deltaTime;
            if (notifications[i].timer > notifications[i].duration)
            {
                notifications.RemoveAt(i); break;
            }
        }

        for (int i = 0; i < elements.Count;i++)
        {
            if(!elements[i].transform)
            {
                elements.RemoveAt(i);
                return;
            }

            if(elements[i].transform.gameObject.activeSelf)
            {
                if (Time.time > elements[i].time)
                {
                    elements[i].transform.localScale = Vector3.Lerp(elements[i].transform.localScale, Vector3.zero, Time.deltaTime * 20f);
                    if(elements[i].transform.localScale.x < 0.1f)
                    {
                        elements[i].transform.gameObject.SetActive(false);
                        elements[i].transform.localScale = Vector3.zero;
                    }
                }
                else
                {
                    elements[i].transform.localScale = Vector3.Lerp(elements[i].transform.localScale, Vector3.one, Time.deltaTime * 20f);
                }
            }
        }
    }

    public static void DrawNotificationText(int id, Vector3 origin, string content, float duration = 1f, string font = "arcade", bool worldSpace = true)
    {
        if (NotificationExists(id))
        {
            return;
        }
        
        UINotification notification = new UINotification(id, content, duration, origin, font, UIType.Text, worldSpace);
        singleton.notifications.Add(notification);
    }

    public static void DrawNotificationImage(int id, Vector2 origin, Sprite content, float duration = 1f, bool worldSpace = true)
    {
        if (!content) return;

        if (NotificationExists(id))
        {
            return;
        }

        UINotification notification = new UINotification(id, content.name, duration, origin, "", UIType.Image, worldSpace);
        singleton.notifications.Add(notification);
    }

    static void DrawImageInternal(int id, Vector2 pos, Sprite content, bool worldSpace)
    {
        UIElement element = null;
        RectTransform transform = null;
        List<MonoBehaviour> components = new List<MonoBehaviour>();

        if (!Exists(id))
        {
            transform = new GameObject("Element_" + id).AddComponent<RectTransform>();
            transform.SetParent(singleton.overlayCanvas.transform.Find("Overlay"));
            transform.localScale = Vector3.zero;
            
            GameObject keyImageObject = new GameObject("Keybind Image");
            Image keyImage = keyImageObject.AddComponent<Image>();
            keyImageObject.transform.localPosition = new Vector3(-1, -1);
            keyImageObject.transform.SetParent(transform);
            keyImage.rectTransform.localScale = Vector3.one;

            keyImage.sprite = content;
            keyImage.rectTransform.sizeDelta = new Vector2(keyImage.sprite.rect.width, keyImage.sprite.rect.height);
            keyImage.transform.SetAsFirstSibling();
            
            components.Add(keyImage);

            element = new UIElement(id, transform, components);
            singleton.elements.Add(element);
        }
        else
        {
            element = Find(id);
            transform = element.transform;
            if (!element.transform.gameObject.activeSelf)
            {
                element.transform.gameObject.SetActive(true);
            }
        }

        element.time = Time.time + Time.fixedDeltaTime * 10f;

        if (worldSpace)
        {
            transform.position = new Vector3(pos.x.RoundToInt(), pos.y.RoundToInt(), transform.position.z);
        }
        else
        {
            transform.anchoredPosition = new Vector3(pos.x.RoundToInt(), pos.y.RoundToInt(), transform.position.z);
        }

        if (element.type == UIType.Image)
        {
            for (int i = 0; i < element.components.Count; i++)
            {
                if (element.components[i] is Image)
                {
                    ((Image)element.components[i]).sprite = content;
                }
            }
        }
    }

    static void DrawTextInternal(int id, Vector2 pos, string content, bool worldSpace, bool isKeybind, string font, HorizontalWrapMode horizontalOverflow = HorizontalWrapMode.Overflow, TextAnchor anchor = TextAnchor.MiddleCenter)
    {
        UIElement element = null;
        RectTransform transform = null;
        List<MonoBehaviour> components = new List<MonoBehaviour>();

        if (!Exists(id))
        {
            transform = new GameObject("Element_" + id).AddComponent<RectTransform>();
            transform.SetParent(singleton.overlayCanvas.transform.Find("Overlay"));
            transform.localScale = Vector3.zero;

            Text textComponent = new GameObject("Text").AddComponent<Text>();

            textComponent.alignment = anchor;
            textComponent.horizontalOverflow = HorizontalWrapMode.Wrap;
            textComponent.verticalOverflow = VerticalWrapMode.Overflow;
            textComponent.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 180);
            textComponent.transform.SetParent(transform);
            textComponent.rectTransform.localScale = Vector3.one;

            if (isKeybind)
            {
                GameObject keyImageObject = new GameObject("Keybind Image");
                Image keyImage = keyImageObject.AddComponent<Image>();
                keyImageObject.transform.localPosition = new Vector3(-2, -1);
                keyImageObject.transform.SetParent(transform);

                keyImage.sprite = ObjectManager.GetSprite("Controllers_21");
                keyImage.rectTransform.sizeDelta = new Vector2(keyImage.sprite.rect.width, keyImage.sprite.rect.height);
                keyImage.transform.SetAsFirstSibling();
                keyImage.rectTransform.localScale = Vector3.one;

                textComponent.color = Color.black;
                components.Add(keyImage);
            }
            else
            {
                UnityEngine.UI.Extensions.NicerOutline outline = textComponent.gameObject.AddComponent<UnityEngine.UI.Extensions.NicerOutline>();
                outline.effectColor = new Color(0f, 0f, 0f, 1f);
            }

            components.Add(textComponent);

            element = new UIElement(id, transform, components);
            singleton.elements.Add(element);
        }
        else
        {
            element = Find(id);
            transform = element.transform;
            if (!element.transform.gameObject.activeSelf)
            {
                element.transform.gameObject.SetActive(true);
            }
        }

        element.time = Time.time + Time.fixedDeltaTime * 10f;

        if (element.type == UIType.Text)
        {
            for (int i = 0; i < element.components.Count; i++)
            {
                if (element.components[i] is Text)
                {
                    Text text = element.components[i] as Text;
                    FontType fontType = GetFont(font);

                    text.fontSize = fontType.size;
                    text.font = fontType.font;
                    text.material = fontType.material;
                    text.text = content;
                    text.material = GetFont(font).material;

                    element.oddX = text.preferredWidth % 2 == 0;
                    element.oddY = text.preferredHeight % 2 != 0;
                }
            }
        }
        if (element.type == UIType.Image && !isKeybind)
        {
            for (int i = 0; i < element.components.Count; i++)
            {
                if (element.components[i] is Image)
                {
                    ((Image)element.components[i]).sprite = ObjectManager.GetSprite(content);
                }
            }
        }

        if (worldSpace)
        {
            transform.position = new Vector3(pos.x.RoundToInt() + (element.oddX ? 0.5f : 0f), pos.y.RoundToInt() + (element.oddY ? 0.5f : 0), transform.position.z);
        }
        else
        {
            transform.anchoredPosition = new Vector3(pos.x.RoundToInt() + (element.oddX ? 0.5f : 0f), pos.y.RoundToInt() + (element.oddY ? 0.5f : 0), transform.position.z);
        }
    }

    public static FontType GetFont(string name)
    {
        if (!singleton) singleton = FindObjectOfType<UIManager>();

        for(int i = 0; i < singleton.fonts.Length;i++)
        {
            if (singleton.fonts[i].name == name) return singleton.fonts[i];
        }

        return null;
    }

    public static void DrawImage(int id, Vector2 pos, Sprite content, bool worldSpace = true)
    {
        DrawImageInternal(id, pos, content, worldSpace);
    }

    public static void DrawText(int id, Vector2 pos, string content, string font = "arcade", bool worldSpace = true)
    {
        DrawTextInternal(id, pos, content, worldSpace, false, font);
    }

    public static void DrawKeybind(int id, Vector2 pos, string content, string font = "arcade", bool worldSpace = true)
    {
        DrawTextInternal(id, pos, content, worldSpace, true, font);
    }

    static bool NotificationExists(int id)
    {
        for (int i = 0; i < singleton.notifications.Count; i++)
        {
            if (singleton.notifications[i].id == id) return true;
        }

        return false;
    }

    static UINotification NotificationFind(int id)
    {
        for (int i = 0; i < singleton.notifications.Count; i++)
        {
            if (singleton.notifications[i].id == id) return singleton.notifications[i];
        }

        return null;
    }

    static UIElement Find(int id)
    {
        for (int i = 0; i < singleton.elements.Count; i++)
        {
            if (singleton.elements[i].id == id) return singleton.elements[i];
        }

        return null;
    }

    static bool Exists(int id)
    {
        for(int i = 0; i < singleton.elements.Count;i++)
        {
            if (singleton.elements[i].id == id) return true;
        }

        return false;
    }
}