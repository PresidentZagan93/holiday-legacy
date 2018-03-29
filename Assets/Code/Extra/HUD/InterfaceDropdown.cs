using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class InterfaceDropdown : InterfaceButton
{
    [ConfigSetting]
    public string setting = "";

    string configType;
    ConfigField field;
    public Dropdown dropdown;
    bool isOpen;
    int originalIndex;

    Toggle[] dropdownItems;
    
    public override void Click()
    {
        base.Click();
        isOpen = !isOpen;

        if(isOpen) dropdown.Show();
        else dropdown.Hide();

        Selector.enabled = !isOpen;
        if(isOpen)
        {
            originalIndex = dropdown.value;
            dropdownItems = dropdown.transform.Find("Dropdown List/Viewport/Content").GetComponentsInChildren<Toggle>();
            for (int i = 0; i < dropdownItems.Length; i++)
            {
                bool current = dropdownItems[i].transform.Find("Item Label").GetComponent<Text>().text == dropdown.options[dropdown.value].text;
                dropdownItems[i].transform.Find("Item Checkmark").GetComponent<Image>().enabled = current;
            }
        }
    }

    public override void Deselect()
    {
        base.Deselect();
        dropdown.Hide();

        isOpen = false;
        Selector.enabled = true;
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
        dropdown.onValueChanged.AddListener(ValueChanged);

        field = ConfigManager.GetField(configType, setting.Split('+')[1]);

        UpdateDropdown();
    }

    private void ValueChanged(int arg0)
    {
        var values = Enum.GetValues(field.Value.GetType()).Cast<Enum>();
        field.Value = values.ElementAt(arg0);
    }

    protected override void Update()
    {
        base.Update();
        if(isOpen)
        {
            if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                dropdown.value++;
                dropdown.RefreshShownValue();
                for(int i = 0; i < dropdownItems.Length;i++)
                {
                    bool current = dropdownItems[i].transform.Find("Item Label").GetComponent<Text>().text == dropdown.options[dropdown.value].text;
                    dropdownItems[i].transform.Find("Item Checkmark").GetComponent<Image>().enabled = current;
                }
            }
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                dropdown.value--;
                dropdown.RefreshShownValue();
                for (int i = 0; i < dropdownItems.Length; i++)
                {
                    bool current = dropdownItems[i].transform.Find("Item Label").GetComponent<Text>().text == dropdown.options[dropdown.value].text;
                    dropdownItems[i].transform.Find("Item Checkmark").GetComponent<Image>().enabled = current;
                }
            }
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                dropdown.value = originalIndex;
                Selector.Deselect();
                dropdown.RefreshShownValue();
            }
            if(Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
            {
                if(field.Type == typeof(Language))
                {
                    LocalizationManager.ChangeLanguage();
                }
                Selector.Deselect();
                dropdown.RefreshShownValue();
            }
        }
    }

    void UpdateDropdown()
    {
        if (dropdown && field.IsEnum)
        {
            dropdown.options.Clear();

            Enum option = (Enum)field.Value;
            var values = Enum.GetValues(field.Value.GetType()).Cast<Enum>();
            int index = 0;
            for (int i = 0; i < values.Count(); i++)
            {
                Dropdown.OptionData newItem = new Dropdown.OptionData(values.ElementAt(i).ToString());
                dropdown.options.Add(newItem);
                
                if (values.ElementAt(i).ToString() == option.ToString())
                {
                    index = i;
                }
            }

            dropdown.value = index;
            dropdown.RefreshShownValue();
        }
    }
}