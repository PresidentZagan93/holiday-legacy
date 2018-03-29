using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class LocalizationManager : MonoBehaviour {

    public List<Localization> languages = new List<Localization>();
    public Localization current;
    static LocalizationManager singleton;

    List<InterfaceText> texts = new List<InterfaceText>();

    private void Awake()
    {
        singleton = this;
    }

    private void Start()
    {
        ChangeLanguage();
    }

    private void OnEnable()
    {
        singleton = this;
    }

    public static void Add(InterfaceText text)
    {
        if (!singleton) singleton = FindObjectOfType<LocalizationManager>();

        singleton.texts.Add(text);
    }

    public static void ChangeLanguage()
    {
        object value = ConfigManager.GetValue(typeof(ConfigSettings), "language");
        Language lang = (Language)Convert.ChangeType(value, Enum.GetUnderlyingType(value.GetType()));

        if (!singleton) singleton = FindObjectOfType<LocalizationManager>();

        for(int i = 0; i < singleton.languages.Count;i++)
        {
            if (singleton.languages[i].language == lang)
            {
                singleton.current = singleton.languages[i];

                for(int t = 0; t < singleton.texts.Count;t++)
                {
                    singleton.texts[t].Refresh();
                }

                return;
            }
        }

        Debug.LogError("Unable to set language to " + lang);
    }

    public static string ConvertFromName(string name)
    {
        if (!singleton) singleton = FindObjectOfType<LocalizationManager>();
        if (!singleton.current) return name;

        for (int i = 0; i < singleton.current.dict.Count; i++)
        {
            if (singleton.current.dict[i].name == name) return singleton.current.dict[i].text;
        }

        Debug.LogError("Unable to translate " + name + " to " + singleton.current.language);
        return name;
    }

    public static string ConvertFromText(string text)
    {
        if (!singleton) singleton = FindObjectOfType<LocalizationManager>();

        string name = "";
        for (int i = 0; i < singleton.languages.Count; i++)
        {
            for (int g = 0; g < singleton.languages[i].dict.Count; g++)
            {
                if (singleton.languages[i].dict[g].text == text)
                {
                    //found the original language its in, now find the translation to the current language
                    name = singleton.languages[i].dict[g].name;
                    break;
                }
            }
        }

        for (int i = 0; i < singleton.current.dict.Count; i++)
        {
            if (singleton.current.dict[i].name == name) return singleton.current.dict[i].text;
        }

        Debug.LogError("Unable to translate " + text + " to " + singleton.current.language);
        return text;
    }
}
