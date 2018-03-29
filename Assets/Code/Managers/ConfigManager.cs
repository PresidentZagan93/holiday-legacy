using UnityEngine;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System;

[Serializable]
public class Config { }

[Serializable]
public class ConfigTemporary : Config
{
    public int gold = 0;
    public int keys = 0;
    public float time = 0f;
    public int kills = 0;
    public int secretsFound = 0;
    public int pickups = 0;
    public int score;

    public string generatorLevel = "";
    public string generatorProgress = "";
    public string generatorTip = "";
    public Sprite generatorBanner;

    public string deathCause = "";
    public string deathTip = "";

    public Sprite gunPrimaryIcon;
    public string gunPrimaryAmmo;
    public Sprite gunSecondaryIcon;
    public string gunSecondaryAmmo;

    public string consumableName;
    public Sprite consumableIcon;
    public string consumableLevel;
    public string consumableTime;

    public Vector2Int finish;
    public Vector2Int spawn;
}

public enum Language
{
    English,
    Russian
}

[Serializable]
public class ConfigSettings : Config
{
    public float volumeMaster = 50f;
    public float volumeMusic = 65f;
    public float volumeEffects = 80f;
    public float volumeVoices = 50f;
    public float screenshakeUi = 100f;
    public float screenshakeWorld = 100f;
    public float minimapScale = 1;
    public Language language = Language.English;

    public bool showFps = false;
    public bool debug = false;
}

[Serializable]
public class ConfigGame : Config
{
    public int kills = 0;
    public int deaths = 0;
    public int version = 0;
    public float time = 0f;
    public int score = 0;
    public int highscore = 0;
    public int lastBuild = 0;
    public int shots = 0;
}

[Serializable]
public class ConfigControls : Config
{
    public KeyCode moveLeft = KeyCode.A;
    public KeyCode moveRight = KeyCode.D;
    public KeyCode moveForward = KeyCode.W;
    public KeyCode moveBack = KeyCode.S;

    public KeyCode attack = KeyCode.Mouse0;
    public KeyCode interact = KeyCode.E;
    public KeyCode activate = KeyCode.Space;
    public KeyCode swap = KeyCode.Q;
}

public static class Settings
{
    public static ConfigManager configManager;

    public static ConfigGame Game
    {
        get
        {
            return configManager.configGame;
        }
    }

    public static ConfigTemporary Temporary
    {
        get
        {
            return configManager.configTemporary;
        }
    }

    public static ConfigControls Controls
    {
        get
        {
            return configManager.configControls;
        }
    }

    public static ConfigSettings Setting
    {
        get
        {
            return configManager.configSettings;
        }
    }
}

[Serializable]
public class ConfigField
{
    FieldInfo field;
    object value;
    Type valueType;

    public object Value
    {
        get
        {
            return field.GetValue(value);
        }
        set
        {
            field.SetValue(this.value, value);
        }
    }

    public bool IsEnum
    {
        get
        {
            return field.GetValue(value).GetType().IsEnum;
        }
    }

    public Type Type
    {
        get
        {
            if (valueType == null) valueType = field.FieldType;
            return valueType;
        }
    }

    public ConfigField(FieldInfo field, Type configType, string variableName, object variableValue)
    {
        this.field = field;
        this.value = variableValue;
    }
}

public class ConfigManager : MonoBehaviour {
    
    public ConfigTemporary configTemporary;
    public ConfigGame configGame;
    public ConfigControls configControls;
    public ConfigSettings configSettings;

    string configsPath = "";

    public static ConfigManager singleton;

    private void OnEnable()
    {
        singleton = this;
        Settings.configManager = this;
    }

    void Awake()
    {
        singleton = this;
        Settings.configManager = this;

        Load();
    }

    private void OnApplicationQuit()
    {
        Save();
    }

    private void OnDisable()
    {
        Save();
    }

    public void Save()
    {
        if (!Application.isPlaying) return;

        if (!Directory.Exists(singleton.configsPath))
        {
            Directory.CreateDirectory(singleton.configsPath);
        }

        List<Type> types = ConfigTypes;
        FieldInfo[] fields = GetType().GetFields();
        for (int i = 0; i < types.Count; i++)
        {
            for (int f = 0; f < fields.Length; f++)
            {
                if(fields[f].FieldType == types[i] && types[i] != typeof(ConfigTemporary))
                {
                    string json = JsonUtility.ToJson(fields[f].GetValue(this), true);
                    File.WriteAllText(configsPath + "/" + types[i].Name + ".txt", json);

                    RefreshConfig(types[i]);
                }
            }
        }
    }

    public static void ResetAll()
    {
        if (!singleton) singleton = FindObjectOfType<ConfigManager>();

        if (!Directory.Exists(singleton.configsPath))
        {
            Directory.CreateDirectory(singleton.configsPath);
        }

        List<Type> types = ConfigTypes;
        FieldInfo[] fields = singleton.GetType().GetFields();
        for (int i = 0; i < types.Count; i++)
        {
            for (int f = 0; f < fields.Length; f++)
            {
                if (fields[f].FieldType == types[i] && types[i] != typeof(ConfigTemporary))
                {
                    object instance = Activator.CreateInstance(types[i]);
                    string json = JsonUtility.ToJson(instance, true);
                    File.WriteAllText(singleton.configsPath + "/" + types[i].Name + ".txt", json);

                    RefreshConfig(types[i]);
                }
            }
        }
    }

    public static void Load()
    {
        if (!singleton) singleton = FindObjectOfType<ConfigManager>();

        singleton.configsPath = Directory.GetParent(Application.dataPath).FullName + "/Configs";
#if UNITY_EDITOR
        singleton.configsPath = Application.dataPath + "/Game/Configs";
#endif   
    	if (!Directory.Exists(singleton.configsPath))
        {
            Directory.CreateDirectory(singleton.configsPath);
        }
        
        List<Type> types = ConfigTypes;
        for (int i = 0; i < types.Count; i++)
        {
            RefreshConfig(types[i]);
        }
    }

    public static ConfigField GetField(string configName, string variableName)
    {
        Type configType = GetConfig(configName);
        return GetField(configType, variableName);
    }

    public static ConfigField GetField(Type configType, string variableName)
    {
        if (!singleton) singleton = FindObjectOfType<ConfigManager>();
        
        FieldInfo[] fields = singleton.GetType().GetFields();
        for(int i = 0; i < fields.Length;i++)
        {
            if (fields[i].FieldType == configType)
            {
                object config = fields[i].GetValue(singleton);
                fields = fields[i].FieldType.GetFields();
                i = 0;
                for (i = 0; i < fields.Length; i++)
                {
                    if (fields[i].Name == variableName)
                    {
                        ConfigField field = new ConfigField(fields[i], configType, variableName, config);

                        return field;
                    }
                }
            }
        }

        return null;
    }

    public static object GetValue(Type configType, string variableName)
    {
        if (!singleton) singleton = FindObjectOfType<ConfigManager>();
        
        FieldInfo[] fields = singleton.GetType().GetFields();
        for (int i = 0; i < fields.Length; i++)
        {
            if (fields[i].FieldType == configType)
            {
                object config = fields[i].GetValue(singleton);
                fields = fields[i].FieldType.GetFields();
                i = 0;
                for (i = 0; i < fields.Length; i++)
                {
                    if (fields[i].Name == variableName)
                    {
                        object value = fields[i].GetValue(config);
                        return value;
                    }
                }
            }
        }

        return null;
    }

    public static object GetValue(string configName, string variableName)
    {
        Type configType = GetConfig(configName);
        return GetValue(configType, variableName);
    }

    public static void SetValue(string configName, string variableName, object variableValue)
    {
        Type configType = GetConfig(configName);
        SetValue(configType, variableName, variableValue);
    }

    public static void SetValue(Type configType, string variableName, object variableValue)
    {
        if (!singleton) singleton = FindObjectOfType<ConfigManager>();
        
        FieldInfo[] fields = singleton.GetType().GetFields();
        for (int i = 0; i < fields.Length; i++)
        {
            if (fields[i].FieldType == configType)
            {
                object config = fields[i].GetValue(singleton);
                fields = fields[i].FieldType.GetFields();
                i = 0;
                for (i = 0; i < fields.Length; i++)
                {
                    if (fields[i].Name == variableName)
                    {
                        fields[i].SetValue(config, variableValue);
                    }
                }
            }
        }
    }

    public static Type GetConfig(string name)
    {
        if (!singleton) singleton = FindObjectOfType<ConfigManager>();

        List<Type> types = ConfigTypes;
        for (int i = 0; i < types.Count;i++)
        {
            if(types[i].Name.ToLower().Contains(name.ToLower()))
            {
                return types[i];
            }
        }

        return null;
    }

    public static List<Type> ConfigTypes
    {
        get
        {
            return Assembly.GetAssembly(typeof(Config)).GetTypes().Where(type => type.IsSubclassOf(typeof(Config))).ToList();
        }
    }
    
    static void RefreshConfig(Type type)
    {
        FieldInfo[] fields = singleton.GetType().GetFields();
        for (int i = 0; i < fields.Length; i++)
        {
            if (fields[i].FieldType == type)
            {
                if (File.Exists(singleton.configsPath + "/" + type.Name + ".txt"))
                {
                    string json = File.ReadAllText(singleton.configsPath + "/" + type.Name + ".txt");
                    JsonUtility.FromJsonOverwrite(json, fields[i].GetValue(singleton));
                }
                else
                {
                    string json = JsonUtility.ToJson(fields[i].GetValue(singleton), true);
                    File.WriteAllText(singleton.configsPath + "/" + type.Name + ".txt", json);
                }

                return;
            }
        }
    }
}
