using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Reflection;
using System.Linq;

public class InterfaceLabel : InterfaceObject {
    
    [ConfigSetting]
    public string setting = "";
    
    public string prefix = "";
    public string suffix = "";
	public float iconScale = 2f;
	public bool forceUppercase = true;
    float nextUpdate;

    object lastValue;
    object value;
    string configType;
    Text text;
    Image image;
    ConfigField field;
    Dropdown dropdown;

    protected override void Awake()
    {
        Refresh();
    }

    private void OnEnable()
    {
        Refresh();
    }

    void Refresh()
    {
        if (!setting.Contains("+"))
        {
            setting = "+";
        }
        configType = setting.Split('+')[0];

        if (!text) text = GetComponent<Text>();
        if (!image) image = GetComponent<Image>();
        if (!dropdown) dropdown = GetComponent<Dropdown>();

        field = ConfigManager.GetField(configType, setting.Split('+')[1]);

        UpdateLabel();
    }

    private void Update()
    {
        UpdateLabel();
    }

    void UpdateLabel()
    {
        value = field.Value;

        if (text)
        {
            string stringValue = "";
            if (value is string)
            {
                stringValue = (string)value;
            }
            else if (value is int)
            {
                stringValue = ((int)value).ToString();
            }
            else if (value is float)
            {
                stringValue = ((float)value).ToString();
            }
            else if (value is bool)
            {
                stringValue = ((bool)value).ToString();
            }
            else if(value.GetType().IsEnum)
            {
                stringValue = ((System.Enum)value).ToString();
            }

            text.text = prefix + stringValue + suffix;
        }
        else if (image)
        {
            if (value is Sprite)
            {
                image.sprite = (Sprite)value;

                if(image.sprite == null || value == null)
                {
                    image.sprite = null;
                    image.rectTransform.sizeDelta = Vector2.zero;
                }
                else
                {
                    image.rectTransform.sizeDelta = new Vector2(image.sprite.rect.width, image.sprite.rect.height);
                }
            }
            else if(ReferenceEquals(value, null))
            {
                image.sprite = null;
                image.rectTransform.sizeDelta = Vector2.zero;
            }
        }
    }
}
