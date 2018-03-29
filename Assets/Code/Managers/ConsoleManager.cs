#pragma warning disable

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using Data;

public class Console
{
    public static bool Open
    {
        get
        {
            return console.open;
        }
    }

    private static ConsoleManager console
    {
        get
        {
            if (!ConsoleManager.singleton) ConsoleManager.singleton = GameObject.FindObjectOfType<ConsoleManager>();

            return ConsoleManager.singleton;
        }
    }

    public static void Run(string command)
    {
        console.Run(command);
    }

    public static bool GetBoolean(string name)
    {
        return console.GetBoolean(name);
    }

    public static float GetFloat(string name, float defaultValue = 0)
    {
        return console.GetFloat(name, defaultValue);
    }

    public static string GetString(string name, string defaultValue = null)
    {
        return console.GetString(name, defaultValue);
    }

    public static void WriteLine(string text, ConsoleManager.MessageType type = ConsoleManager.MessageType.Log)
    {
        if(console) console.WriteLine(text, type);
    }
}

public class ConsoleManager : MonoBehaviour {

    public enum MessageType
    {
        Log,
        Error,
        Warning,
        Message,
        User
    }

    [Serializable]
    public class Bind
    {
        public string command;
        public string key;
        public KeyCode keyCode;

        public Bind(string key, string command)
        {
            this.key = key;
            this.keyCode = Helper.ToKeyCode(key);
            this.command = command;
        }
    }

    public List<Bind> binds = new List<Bind>();
    public List<Bind> defaultBinds = new List<Bind>();

    public RectTransform consoleWindow;
    public RectTransform consoleMask;
    public Text consoleText;
    public InputField consoleInput;

    public Color logColor;
    public Color errorColor;
    public Color warningColor;
    public Color userColor;
    public Color valueColor;
    public Color identifierColor;
    public Color messageColor;

    public static ConsoleManager singleton;
    public bool open;
    GameManager gameManager;

    public List<string> entries = new List<string>();

    public enum ParamType
    {
        Float,
        String,
        Bool
    }

    [Serializable]
    public class Command
    {
        public string name;
        public ParamType[] parameters;
        public string methodName = "";
        public MethodInfo method;
    }

    [Serializable]
    public class Variable
    {
        public string name;
        public ParamType type = ParamType.Bool;
        public string value;

        public Variable(string name, ParamType type, string value)
        {
            this.name = name;
            this.type = type;
            this.value = value;
        }
    }
    
    public List<Variable> variables = new List<Variable>();
    public List<Command> commands = new List<Command>();
    public List<string> history = new List<string>();
    int historyIndex = -1;
    string savedCommand;

    void Awake()
    {
        singleton = this;
        gameManager = FindObjectOfType<GameManager>();
        LoadBinds();
        LoadVariables();
        StartCommands();

        WriteLine("Yo", MessageType.Message);
    }

    #region Variable stuff
    #region Getters
    public float GetFloat(string name, float defaultValue = float.MaxValue)
    {
        for (int i = 0; i < variables.Count; i++)
        {
            if (variables[i].name == name && variables[i].type == ParamType.Float)
            {
                return float.Parse(variables[i].value);
            }
        }
        return defaultValue;
    }

    public string GetString(string name, string defaultValue = null)
    {
        for (int i = 0; i < variables.Count; i++)
        {
            if (variables[i].name == name && variables[i].type == ParamType.String)
            {
                return variables[i].value;
            }
        }
        return defaultValue;
    }

    public bool GetBoolean(string name)
    {
        for (int i = 0; i < variables.Count; i++)
        {
            if (variables[i].name == name && variables[i].type == ParamType.Bool)
            {
                return variables[i].value == "true";
            }
        }
        return false;
    }

    #endregion
    #region Setters
    public void SetString(string name, string value)
    {
        if (!VarOfType(name, ParamType.String)) return;
        SetVar(name, value);
    }

    public void SetFloat(string name, float value)
    {
        if (!VarOfType(name, ParamType.Float)) return;
        SetVar(name, value+"");
    }

    public void SetBoolean(string name, bool value)
    {
        if (!VarOfType(name, ParamType.Bool)) return;
        SetVar(name, value ? "true" : "false");
    }

    void SetVar(string name, string value)
    {
        for (int i = 0; i < variables.Count; i++)
        {
            if (variables[i].name == name)
            {
                variables[i].value = value;
                SaveVariables();
            }
        }
    }
    #endregion

    bool VarExists(string name)
    {
        for(int i = 0; i < variables.Count;i++)
        {
            if (variables[i].name == name) return true;
        }
        return false;
    }

    bool VarOfType(string name, ParamType type)
    {
        for (int i = 0; i < variables.Count; i++)
        {
            if (variables[i].name == name && variables[i].type == type) return true;
        }
        return false;
    }


    void AddVariable(string name, ParamType type, string value)
    {
        for (int i = 0; i < variables.Count; i++)
        {
            if (variables[i].name == name)
            {
                variables.RemoveAt(i); break;
            }
        }

        variables.Add(new Variable(name, type, value));
        SaveVariables();
    }

    void RemoveVariable(string name)
    {
        for (int i = 0; i < variables.Count; i++)
        {
            if (variables[i].name == name)
            {
                variables.RemoveAt(i); break;
            }
        }
        if(name == "all")
        {
            variables.Clear();
        }
        SaveVariables();
    }

    void SaveVariables()
    {
        if(variables.Count == 0)
        {
            PlayerPrefs.SetString("vars", "");
            return;
        }
        string varText = "";
        for (int i = 0; i < variables.Count; i++)
        {
            varText += variables[i].name + "`" + variables[i].type.ToString() + "`" + variables[i].value + "~";
        }
        varText = varText.Substring(0, varText.Length - 1);
        PlayerPrefs.SetString("vars", varText);
    }

    void LoadVariables()
    {
        variables.Clear();

        string[] varData = PlayerPrefs.GetString("vars", "").Split('~');
        for (int i = 0; i < varData.Length; i++)
        {
            if (varData[i].Contains("`"))
            {
                string name = varData[i].Split('`')[0];
                ParamType type = (ParamType)Enum.Parse(typeof(ParamType), varData[i].Split('`')[1]);
                string value = varData[i].Split('`')[2];
                
                variables.Add(new Variable(name, type, value));
            }
        }
    }

    #endregion
    #region Commands

    public void Command_Clear()
    {
        entries.Clear();
    }

    public void Command_HelpHelp(string command)
    {
        for (int i = 0; i < commands.Count; i++)
        {
            if(commands[i].name == command)
            {
                string paramText = GetParamsText(commands[i]);
                WriteLine("Command signature for " + command, MessageType.Message);
                WriteLine("    " + commands[i].name + paramText, MessageType.Message);
                return;
            }
        }
    }

    string GetParamsText(Command command)
    {
        string paramText = "";
        if (command.parameters != null)
        {
            for (int i = 0; i < command.parameters.Length; i++)
            {
                paramText += "<" + command.parameters[i].ToString() + "> ";
            }
            if (command.parameters.Length > 0)
            {
                paramText = " " + paramText;
                paramText = paramText.Substring(0, paramText.Length - 1);
            }
        }
        return "<color="+valueColor.ToHex()+">"+paramText+"</color>";
    }

    public void Command_Help()
    {
        WriteLine("List of all commands : ", MessageType.Message);
        for (int i = 0; i < commands.Count;i++)
        {
            string paramText = GetParamsText(commands[i]);
            WriteLine("    "+commands[i].name + paramText, MessageType.Message);
        }
    }

    public void Command_List(string listType)
    {
        listType = listType.ToLower();

        if (listType == "armor")
        {
            WriteLine("List of all armor with IDs : ", MessageType.Message);
            for (int i = 0; i < ItemManager.singleton.armor.Count; i++)
            {
                WriteLine("    <color=" + identifierColor.ToHex() + ">" + ItemManager.singleton.armor[i].ID + "</color> = <color=" + valueColor.ToHex() + ">" + ItemManager.singleton.armor[i].name + "</color>", MessageType.Message);
            }
        }
        if (listType == "items")
        {
            WriteLine("List of all items with IDs : ", MessageType.Message);
            for (int i = 0; i < ItemManager.singleton.items.Count; i++)
            {
                WriteLine("    <color=" + identifierColor.ToHex() + ">" + ItemManager.singleton.items[i].ID + "</color> = <color="+ valueColor.ToHex() + ">" + ItemManager.singleton.items[i].name + "</color>", MessageType.Message);
            }
        }
        if (listType == "cons")
        {
            WriteLine("List of all consumables with IDs : ", MessageType.Message);
            for (int i = 0; i < ItemManager.singleton.consumables.Count; i++)
            {
                WriteLine("    <color=" + identifierColor.ToHex() + ">" + ItemManager.singleton.consumables[i].ID + "</color> = <color=" + valueColor.ToHex() + ">" + ItemManager.singleton.consumables[i].name + "</color>", MessageType.Message);
            }
        }
        if (listType == "guns")
        {
            WriteLine("List of all guns with IDs : ", MessageType.Message);
            for (int i = 0; i < ItemManager.singleton.guns.Count; i++)
            {
                WriteLine("    <color=" + identifierColor.ToHex() + ">" + ItemManager.singleton.guns[i].ID + "</color> = <color="+ valueColor.ToHex() + ">" + ItemManager.singleton.guns[i].name + "</color>", MessageType.Message);
            }
        }
        if (listType == "vars")
        {
            LoadVariables();
            WriteLine("List of all variables : ", MessageType.Message);
            for (int i = 0; i < variables.Count; i++)
            {
                WriteLine("    <color=" + identifierColor.ToHex() + ">" + variables[i].name + "</color> : " + variables[i].type.ToString().ToLower() + " = <color=" + valueColor.ToHex() + ">" + variables[i].value + "</color>", MessageType.Message);
            }
        }
        if (listType == "levels")
        {
            LoadBinds();
            WriteLine("List of all levels : ", MessageType.Message);
            for (int i = 0; i < LevelManager.Order.Count; i++)
            {
                WriteLine("    <color=" + identifierColor.ToHex() + ">" + i + "</color> = <color=" + valueColor.ToHex() + ">" + LevelManager.Order[i].levelName + " (" + LevelManager.Order[i].tileset + ")" + "</color>", MessageType.Message);
            }
        }
        if (listType == "binds")
        {
            LoadBinds();
            WriteLine("List of all binds : ", MessageType.Message);
            for (int i = 0; i < binds.Count; i++)
            {
                WriteLine("    <color=" + identifierColor.ToHex() + ">" + binds[i].key + "</color> = <color=" + valueColor.ToHex() + ">" + binds[i].command + "</color>", MessageType.Message);
            }
        }
    }

    public void Command_Stage(float stage)
    {
        GeneratorManager.singleton.startStage = stage.RoundToInt();
    }

    public void Command_KillAllEnemies()
    {
        EnemyManager.KillAll();
    }

    public void Command_Bind(string key, string command)
    {
        AddBind(key, command);
        WriteLine("Binded <color="+ identifierColor.ToHex() + ">" + command + "</color> to <color="+ valueColor.ToHex() + ">" + key + "</color>", MessageType.Message);
    }

    public void Command_Unbind(string key)
    {
        RemoveBind(key);
        WriteLine("Unbinded all commands from <color="+ valueColor.ToHex() + ">" + key + "</color>", MessageType.Message);
    }

    public string Command_Drop(string getType, float id)
    {
        getType = getType.ToLower();
        IItem item = null;
        Character player = Character.Player;

        if (getType == "armor") item = ItemManager.GetArmor((int)id);
        else if (getType == "gun") item = ItemManager.GetGun((int)id);
        else if(getType == "item") item = ItemManager.GetItem((int)id);
        else if(getType == "cons") item = ItemManager.GetConsumable((int)id);
        else if(getType == "curr")
        {
            Loot loot = ScriptableObject.CreateInstance<Loot>();
            Loot.LootProp lootProp = new Loot.LootProp();
            lootProp.amount = new IntRange((int)id, (int)id);
            lootProp.chance = 100;
            lootProp.prop = ObjectManager.GetPrefab("CoinGold");

            loot.loot.Add(lootProp);

            ObjectManager.DropHealthProps(loot, Character.Player, Character.Player.transform.position);
        }

        if(item != null)
        {
            ItemManager.DropItem(player.transform.position, item);
            return "Dropped " + item.GetName();
        }
        else
        {
            return "Item " + id + " of type " + getType + " does not exist!";
        }
    }

    public void Command_Get(string getType, float id)
    {
        getType = getType.ToLower();

        Character player = Character.Player;
        if (getType == "armor")
        {
            Armor armor = ItemManager.GetArmor((int)id);
            player.Armor.Equip(armor.name);
            WriteLine("<color=" + identifierColor.ToHex() + ">" + armor.name + "</color> equipped", MessageType.Message);
        }
        if (getType == "hp")
        {
            player.Health.Heal((int)id);
        }
        if (getType == "ammo")
        {
            player.GunShooter.Ammo += (int)id;
            player.GunShooter.OtherAmmo += (int)id;
        }
        if (getType == "gun")
        {
            Gun gun = ItemManager.GetGun((int)id);
            player.GunShooter.Equip(gun.name);
            WriteLine("<color=" + identifierColor.ToHex() + ">" + gun.name + "</color> equipped", MessageType.Message);
        }
        if (getType == "item")
        {
            Item item = ItemManager.GetItem((int)id);

            player.Inventory.Add(item);
            WriteLine("<color=purple>" + item.name + "</color> added to player inventory", MessageType.Message);
        }
        if (getType == "cons")
        {
            Consumable cons = ItemManager.GetConsumable((int)id);

            player.Inventory.Add(cons);
            WriteLine("<color=purple>" + cons.name + "</color> added to player inventory", MessageType.Message);
        }
    }

    public void Command_Reload()
    {
        Command_Clear();
        StartCommands();
    }

    public void Command_AddVarBool(string name, bool value)
    {
        AddVariable(name, ParamType.Bool, value ? "true" : "false");
    }

    public void Command_AddVarFloat(string name, float value)
    {
        AddVariable(name, ParamType.Float, value+"");
    }

    public void Command_AddVarString(string name, string value)
    {
        AddVariable(name, ParamType.String, value);
    }

    public void Command_RemoveVar(string name)
    {
        RemoveVariable(name);
    }

    public void Command_MoveTo(string type)
    {
        Character player = Character.Player;
        if (type == "finish")
        {
            player.transform.position = Settings.Temporary.finish.ToVector().ToWorldPos();
        }
        else if(type == "shop")
        {
            player.transform.position = FindObjectOfType<ObjectRoomShop>().transform.position;
        }
        else if(type == "boss")
        {
            player.transform.position = FindObjectOfType<ObjectRoomBoss>().transform.position;
        }
    }

    public void Command_Reset()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public string Command_God()
    {
        Character player = Character.Player;
        if(player)
        {
            player.Health.invincible = !player.Health.invincible;
            return player.Health.invincible ? "You became god." : "No longer god.";
        }

        return "No player found";
    }

    public string Command_Debug(bool state)
    {
        Settings.Setting.debug = state;
        return "Debug mode "+(state ? "enabled" : "disabled");
    }

    public string Command_Exec(string staticMethod)
    {
        string className = staticMethod.Split('.')[0];
        string methodName = staticMethod.Split('.')[1];

        Type classType = Type.GetType(className);
        if(classType == null)
        {
            return "Class of type "+className+" not found.";
        }

        var methods = classType.GetMethods();
        for (int i = 0; i < methods.Length; i++)
        {
            if (methods[i].Name == methodName && methods[i].IsStatic)
            {
                return methods[i].Invoke(null, null).ToString();
            }
        }

        return "Method of type "+methodName+" not found.";
    }

    #endregion

    void StartCommands()
    {
        commands.Clear();

        RegisterCommand("clear", null, "Command_Clear");
        RegisterCommand("bind", new ParamType[] { ParamType.String, ParamType.String }, "Command_Bind");
        RegisterCommand("unbind", new ParamType[] { ParamType.String }, "Command_Unbind");
        RegisterCommand("help", null, "Command_Help");
        RegisterCommand("help", new ParamType[] { ParamType.String }, "Command_HelpHelp");
        RegisterCommand("reload", null, "Command_Reload");
        RegisterCommand("get", new ParamType[] { ParamType.String, ParamType.Float }, "Command_Get");
        RegisterCommand("drop", new ParamType[] { ParamType.String, ParamType.Float }, "Command_Drop");
        RegisterCommand("list", new ParamType[] { ParamType.String }, "Command_List");
        RegisterCommand("killall", null, "Command_KillAllEnemies");
        RegisterCommand("addvarfloat", new ParamType[] { ParamType.String, ParamType.Float }, "Command_AddVarFloat");
        RegisterCommand("addvarstring", new ParamType[] { ParamType.String, ParamType.String }, "Command_AddVarString");
        RegisterCommand("addvarbool", new ParamType[] { ParamType.String, ParamType.Bool}, "Command_AddVarBool");
        RegisterCommand("removevar", new ParamType[] { ParamType.String }, "Command_RemoveVar");
        RegisterCommand("tp", new ParamType[] { ParamType.String }, "Command_MoveTo");
        RegisterCommand("reset", null, "Command_Reset");
        RegisterCommand("god", null, "Command_God");
        RegisterCommand("exec", new ParamType[] { ParamType.String }, "Command_Exec");
        RegisterCommand("debug", new ParamType[] { ParamType.Bool }, "Command_Debug");
        RegisterCommand("stage", new ParamType[] { ParamType.Float }, "Command_Stage");
    }
    
    void RegisterCommand(string name, ParamType[] parameters, string methodName)
    {
        Command com = new Command()
        {
            name = name,
            parameters = parameters ?? (new ParamType[] { }),
            methodName = methodName
        };
        AssignMethod(com);

        commands.Add(com);
    }

    void AssignMethod(Command command)
    {
        var methods = GetType().GetMethods();
        for (int i = 0; i < methods.Length; i++)
        {
            if (methods[i].Name == command.methodName && methods[i].GetParameters().Length == command.parameters.Length)
            {
                command.method = methods[i];
                return;
            }
        }
    }
    
    void Update()
    {
        if(!open)
        {
            for (int i = 0; i < binds.Count; i++)
            {
                if (Input.GetKeyDown(binds[i].keyCode))
                {
                    Run(binds[i].command);
                }
            }
        }

        if (open)
        {
            if (!consoleWindow.gameObject.activeSelf)
            {
                consoleWindow.gameObject.SetActive(true);
            }

            if(Input.GetKeyDown(KeyCode.UpArrow) && history.Count > 0)
            {
                if(historyIndex == -1)
                {
                    savedCommand = consoleInput.text;
                }
                historyIndex++;
                if(historyIndex >= history.Count)
                {
                    historyIndex = history.Count - 1;
                }
                consoleInput.text = history[historyIndex];
            }
            if (Input.GetKeyDown(KeyCode.DownArrow) && history.Count > 0)
            {
                if(historyIndex == 0)
                {
                    consoleInput.text = savedCommand;
                }
                historyIndex--;
                if(historyIndex < -1)
                {
                    historyIndex = -1;
                }
                else if(historyIndex >= 0)
                {
                    consoleInput.text = history[historyIndex];
                }
            }

            consoleText.rectTransform.anchoredPosition += Vector2.up * Mathf.Clamp(Input.GetAxisRaw("Mouse ScrollWheel") * 100000f, -1, 1) * consoleText.fontSize * -1f;
            if (consoleText.rectTransform.anchoredPosition.y > 0f)
            {
                consoleText.rectTransform.anchoredPosition -= Vector2.up * consoleText.rectTransform.anchoredPosition.y * 1f;
            }
            if (consoleText.rectTransform.anchoredPosition.y < (entries.Count * consoleText.fontSize * -1f) + 150)
            {
                consoleText.rectTransform.anchoredPosition = new Vector2(consoleText.rectTransform.anchoredPosition.x, (entries.Count * consoleText.fontSize * -1f) + 150);
            }
            consoleInput.Select();
            consoleInput.ActivateInputField();
        }
        else
        {
            if (consoleWindow.anchoredPosition.y > 175f && consoleWindow.gameObject.activeSelf)
            {
                consoleWindow.gameObject.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            open = !open;
            if(open)
            {
                if(!gameManager.paused)
                {
                    gameManager.PauseForce(true);
                }
                UpdateText();
            }
            else
            {
                consoleInput.DeactivateInputField();
            }
            consoleInput.text = consoleInput.text.Replace("`", "");
        }

        consoleText.enabled = open;
        consoleWindow.anchoredPosition = Vector2.Lerp(consoleWindow.anchoredPosition, open ? Vector2.zero : new Vector2(0f, 180f), Time.deltaTime * 25f);
        singleton = this;
    }

    void UpdateText()
    {
        consoleText.text = "";
        for (int i = 0; i < entries.Count;i++)
        {
            consoleText.text += "\n" + entries[i];
        }
    }

    public void WriteLine(string text, MessageType type)
    {
        if (type == MessageType.Warning && !GetBoolean("debug_warnings")) return;
        if (type == MessageType.Error && !GetBoolean("debug_errors")) return;
        if (type == MessageType.Log && !GetBoolean("debug_logs")) return;

        Color color = logColor;
        if (type == MessageType.Error) color = errorColor;
        if (type == MessageType.Message) color = messageColor;
        if (type == MessageType.User) color = userColor;
        if (type == MessageType.Warning) color = warningColor;

        entries.Add("<color=" + color.ToHex() + ">" + text + "</color>");
        UpdateText();
    }

    #region Binds

    void LoadBinds()
    {
        binds.Clear();

        string[] bindsData = PlayerPrefs.GetString("binds", "").Split('~');
        for (int i = 0; i < bindsData.Length; i++)
        {
            if (bindsData[i].Contains("`"))
            {
                string key = bindsData[i].Split('`')[0];
                string command = bindsData[i].Split('`')[1];

                binds.Add(new Bind(key, command));
            }
        }

        binds.AddRange(defaultBinds);
    }

    void AddBind(string key, string command)
    {
        for(int i = 0; i < binds.Count;i++)
        {
            if(binds[i].key == key)
            {
                binds.RemoveAt(i); break;
            }
        }

        binds.Add(new Bind(key, command));
        List<string> bindsData = new List<string>();
        for(int i = 0; i < binds.Count;i++)
        {
            bindsData.Add(binds[i].key + "`" + binds[i].command);
        }

        string text = string.Join("~", bindsData.ToArray());
        PlayerPrefs.SetString("binds", text);
    }

    void RemoveBind(string key)
    {
        for (int i = 0; i < binds.Count; i++)
        {
            if (binds[i].key == key)
            {
                binds.RemoveAt(i); break;
            }
        }
        
        List<string> bindsData = new List<string>();
        for (int i = 0; i < binds.Count; i++)
        {
            bindsData.Add(binds[i].key + "`" + binds[i].command);
        }

        string text = string.Join("~", bindsData.ToArray());
        PlayerPrefs.SetString("binds", text);
    }

#endregion

    void ProcessCommand(string cmd, List<string> parameters)
    {
        for(int i = 0; i < parameters.Count;i++)
        {
            if (parameters[i] == "") parameters.RemoveAt(i);
        }

        for(int i = 0; i < commands.Count;i++)
        {
            if(commands[i].name == cmd)
            {
                if (commands[i].parameters.Length == parameters.Count)
                {
                    if (commands[i].method == null) AssignMethod(commands[i]);

                    if (commands[i].method != null)
                    {
                        List<object> parameterObjects = new List<object>();
                        if (commands[i].parameters != null && commands[i].parameters.Length > 0)
                        {
                            if (commands[i].parameters.Length != parameters.Count)
                            {
                                WriteLine("Incorrect command signature, type in `help " + cmd + "` for more information", MessageType.Error);
                            }
                            else
                            {
                                for (int p = 0; p < commands[i].parameters.Length; p++)
                                {
                                    object paramObject = parameters[p];
                                    if (commands[i].parameters[p] == ParamType.Bool)
                                    {
                                        paramObject = parameters[p].ToLower() == "true" ? true : false;
                                    }
                                    if (commands[i].parameters[p] == ParamType.Float)
                                    {
                                        paramObject = float.Parse(parameters[p]);
                                    }
                                    if (commands[i].parameters[p] == ParamType.Float)
                                    {
                                        paramObject = float.Parse(parameters[p]);
                                    }
                                    parameterObjects.Add(paramObject);
                                }
                            }
                        }

                        object returnValue = commands[i].method.Invoke(this, parameterObjects.ToArray());
                        if(returnValue != null)
                        {
                            WriteLine(returnValue.ToString(), MessageType.Message);
                        }
                    }
                }
            }
        }

        if (parameters.Count > 0)
        {
            bool isBool = VarOfType(cmd, ParamType.Bool);
            bool isString = VarOfType(cmd, ParamType.String);
            bool isFloat = VarOfType(cmd, ParamType.Float);

            if (isBool)
            {
                bool value = parameters[0].ToLower() == "true";
                SetBoolean(cmd, value);
            }
            if (isString)
            {
                SetString(cmd, parameters[0]);
            }
            if (isFloat)
            {
                float value = float.Parse(parameters[0]);
                SetFloat(cmd, value);
            }
        }
        else
        {
            for (int i = 0; i < variables.Count; i++)
            {
                if(variables[i].name == cmd)
                {
                    WriteLine("<color="+identifierColor.ToHex()+">"+variables[i].name+"</color> = <color="+valueColor.ToHex()+">"+variables[i].value+"</color>", MessageType.Message);
                }
            }
        }

        UpdateText();
    }

    public void Run(string txt)
    {
        if (txt == "" || txt == "`") return;

        history.Insert(0, txt);
        List<string> commandsFound = new List<string>();

        if(txt.Contains(";"))
        {
            bool insideQuotes = false;
            int lastCommandStart = 0;
            for (int i = 0; i < txt.Length; i++)
            {
                if (txt[i] == '"') insideQuotes = !insideQuotes;
                if (txt[i] == ';' && !insideQuotes)
                {
                    string commFound = txt.Substring(lastCommandStart, i - lastCommandStart);
                    commandsFound.Add(commFound);
                    lastCommandStart = i+1;
                }
                if(i == txt.Length-1)
                {
                    string commFound = txt.Substring(lastCommandStart);
                    commandsFound.Add(commFound);
                }
            }
        }
        else
        {
            commandsFound.Add(txt);
        }
        
        for (int i = 0; i < commandsFound.Count; i++)
        {
            string command = commandsFound[i];
            string[] paramaterArray = new string[] { };
            if (command.Contains(" "))
            {
                command = command.Split(' ')[0];
                paramaterArray = commandsFound[i].Substring(command.Length + 1).Split(' ');
            }

            string parameters = "";
            string stringFound = "";
            int startingIndex = -1;

            for (int p = 0; p < paramaterArray.Length; p++)
            {
                if (paramaterArray[p].StartsWith("\"") && startingIndex == -1)
                {
                    startingIndex = p;
                }
                if(startingIndex != -1)
                {
                    stringFound += paramaterArray[p] + " ";
                    if (paramaterArray[p].EndsWith("\""))
                    {
                        parameters += stringFound.TrimEnd().Replace("\"", "") + "*";

                        startingIndex = -1;
                        stringFound = "";
                    }
                }
                else
                {
                    parameters += paramaterArray[p] + "*";
                }
            }

            if (parameters.Length > 1)
            {
                parameters = parameters.Substring(0, parameters.Length - 1);
            }
            ProcessCommand(command, parameters.Split('*').ToList());
        }
    }

    public void Run()
    {
        if (!Input.GetKey(KeyCode.Return)) return;

        string txt = consoleInput.text;
        if (txt == "" || txt == "`") return;
        consoleInput.text = "";

        consoleInput.Select();
        consoleInput.ActivateInputField();

        WriteLine(txt, MessageType.User);
        Run(txt);
    }
}
