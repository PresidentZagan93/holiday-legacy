using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using UnityEditor.SceneManagement;
using UnityEditor.Callbacks;
using System.Web;
using System.Net;
using System;
using System.Linq;

using Ionic.Zip;

using Debug = UnityEngine.Debug;

public class EditorBuild : EditorWindow
{
    int Version
    {
        get
        {
            GameManager gm = FindObjectOfType<GameManager>();
            return gm.version;
        }
    }

    public const string ExecutableName = "jhe"; //will be used to build (game.exe, game.app, etc)
    public const string ItchProjectName = "jhe"; //taken from the edit game section
    public const string ItchAccount = "popcron"; //your itch profile name

    [Serializable]
    public class EditorBuildBranches
    {
        public List<string> branches = new List<string>();
    }

    EditorBuildBranches branches;
    bool editBranch;
    int branchEditing;

    [MenuItem("Custom/Build")]
    public static void Init()
    {
        EditorWindow.GetWindow(typeof(EditorBuild));
    }

    [DidReloadScripts]
    public static void IncreaseVersion()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if(gm)
        {
            gm.version++;
        }
    }

    [PostProcessBuild(1)]
    public static void OnPostprocessBuild(BuildTarget target, string path)
    {
        string platform = TargetToPlatform(target);
        string branch = EditorPrefs.GetString(PlayerSettings.productName + "_builtBranch", "stable");
        int version = GetBuiltVersion(branch, platform);

        string root = Directory.GetParent(Application.dataPath).FullName;
        if (path.StartsWith("/"))
        {
            path = path.Substring(1);
        }
        path = Directory.GetParent(path).FullName;

        DateTime buildTime = DateTime.Now;
        string date = buildTime.ToString();
        date = date.Replace("/", "-");
        date = date.Replace(":", "-");
        date = date.Replace(" ", "_");
        
        string cloneZip = Directory.GetParent(path).FullName + "/" + platform + ".zip";

        if (File.Exists(cloneZip))
        {
            File.Delete(cloneZip);
        }

        if (!Directory.Exists(root + "/Builds/" + branch))
        {
            Directory.CreateDirectory(root + "/Builds/" + branch);
        }
        if (!Directory.Exists(root + "/Builds/" + branch + "/" + platform))
        {
            Directory.CreateDirectory(root + "/Builds/" + branch + "/" + platform);
        }

        string exportZip = root + "/Builds/" + branch + "/" + platform + "/" + version + " (" + date + ").zip";

        using (ZipFile zip = new ZipFile())
        {
            if(platform == "win")
            {
                zip.AddFile(path + "/" + ExecutableName + ".exe", "");
                zip.AddDirectory(path + "/" + ExecutableName + "_Data", ExecutableName + "_Data");
            }
            else if(platform == "mac")
            {
                zip.AddDirectory(path + "/" + ExecutableName + ".app", ExecutableName + ".app");
            }
            else if(platform == "linux")
            {
                zip.AddFile(path + "/" + ExecutableName + ".x86", "");
                zip.AddFile(path + "/" + ExecutableName + ".x86_64", "");
                zip.AddDirectory(path + "/" + ExecutableName + "_Data", ExecutableName + "_Data");
            }
            else
            {
                zip.AddDirectory(path);
            }
            zip.Save(exportZip);
        }

        File.Copy(exportZip, cloneZip);
        EditorPrefs.SetString(PlayerSettings.productName + "_builtArchive_" + branch + "_" + platform, cloneZip);
    }

    private void OnEnable()
    {
        Load();
    }

    void Load()
    {
        if (!Directory.Exists(Application.dataPath + "/Editor"))
        {
            Directory.CreateDirectory(Application.dataPath + "/Editor");
        }
        if (!File.Exists(Application.dataPath + "/Editor/Branches.txt"))
        {
            string json = JsonUtility.ToJson(new EditorBuildBranches(), true);
            File.WriteAllText(Application.dataPath + "/Editor/Branches.txt", json);
        }

        branches = JsonUtility.FromJson<EditorBuildBranches>(File.ReadAllText(Application.dataPath + "/Editor/Branches.txt"));
    }

    void Save()
    {
        if (!Directory.Exists(Application.dataPath + "/Editor"))
        {
            Directory.CreateDirectory(Application.dataPath + "/Editor");
        }

        string json = JsonUtility.ToJson(branches, true);
        File.WriteAllText(Application.dataPath + "/Editor/Branches.txt", json);
    }

    public static BuildTarget PlatformToTarget(string platform)
    {
        BuildTarget target = BuildTarget.NoTarget;
        if (platform == "win") target = BuildTarget.StandaloneWindows;
        if (platform == "linux") target = BuildTarget.StandaloneLinuxUniversal;
        if (platform == "mac") target = BuildTarget.StandaloneOSX;
        if (platform == "webgl") target = BuildTarget.WebGL;

        return target;
    }

    public static string TargetToPlatform(BuildTarget target)
    {
        string platform = "";
        if (target == BuildTarget.StandaloneWindows) platform = "win";
        if (target == BuildTarget.StandaloneOSX) platform = "mac";
        if (target == BuildTarget.StandaloneLinuxUniversal) platform = "linux";
        if (target == BuildTarget.WebGL) platform = "webgl";

        return platform;
    }

    public static string GetBuildPath(string branch, string platform)
    {
        string root = Directory.GetParent(Application.dataPath) + "/Game/";
        string path = "";
        if (platform == "win") path = root + branch + "/" + platform + "/" + ExecutableName + ".exe";
        if (platform == "mac") path = root + branch + "/" + platform + "/" + ExecutableName + ".app";
        if (platform == "linux") path = root + branch + "/" + platform + "/" + ExecutableName + ".x86";
        if (platform == "webgl") path = root + branch + "/" + platform;

        return path;
    }

    public static string GetBuiltPath(string branch, string platform)
    {
        return EditorPrefs.GetString(PlayerSettings.productName + "_builtArchive_" + branch + "_" + platform);
    }

    public static string GetPlayPath(string branch, string platform)
    {
        string root = Directory.GetParent(Application.dataPath) + "/Game/" + branch + "/" + platform + "/";
        string path = "";
        if (platform == "win") path = root + ExecutableName + ".exe";
        if (platform == "mac") path = root + ExecutableName + ".app";
        if (platform == "linux") path = root + ExecutableName + ".x86";
        if (platform == "webgl") path = root + "/index.html";

        return path;
    }

    void Build(string branch, string platform)
    {
        BuildTarget target = PlatformToTarget(platform);
        string path = GetBuildPath(branch, platform);

        EditorPrefs.SetInt(PlayerSettings.productName + "_builtVersion_" + branch + "_" + platform, Version);
        EditorPrefs.SetString(PlayerSettings.productName + "_builtBranch", branch);

        string[] levels = new string[] { "Assets/Scenes/Game.unity" };

        if (!Directory.Exists(Directory.GetParent(Application.dataPath) + "/Game"))
        {
            Directory.CreateDirectory(Directory.GetParent(Application.dataPath) + "/Game");
        }
        if (!Directory.Exists(Directory.GetParent(Application.dataPath) + "/Game/" + branch))
        {
            Directory.CreateDirectory(Directory.GetParent(Application.dataPath) + "/Game/" + branch);
        }
        if (!Directory.Exists(Directory.GetParent(Application.dataPath) + "/Game/" + branch + "/" + platform))
        {
            Directory.CreateDirectory(Directory.GetParent(Application.dataPath) + "/Game/" + branch + "/" + platform);
        }

        BuildPipeline.BuildPlayer(levels, path, target, BuildOptions.None);
    }

    public static int GetBuiltVersion(string branch, string platform)
    {
        return EditorPrefs.GetInt(PlayerSettings.productName + "_builtVersion_" + branch + "_" + platform);
    }

    public static int GetUploadVersion(string branch, string platform)
    {
        return EditorPrefs.GetInt(PlayerSettings.productName + "_uploadVersion_" + branch + "_" + platform);
    }

    void Upload(string branch, string platform)
    {
        string butlerPath = Application.dataPath + "/Editor/butler.exe";
        if(!File.Exists(butlerPath))
        {
            if (!Directory.Exists(Application.dataPath + "/Editor"))
            {
                Directory.CreateDirectory(Application.dataPath + "/Editor");
            }

            WebClient wc = new WebClient();
            wc.DownloadFile("https://dl.itch.ovh/butler/windows-386/head/butler.exe", butlerPath);
        }

        string path = GetBuiltPath(branch, platform);
        int version = GetBuiltVersion(branch, platform);
        Process buildProcess = new Process();

        buildProcess.StartInfo.CreateNoWindow = true;
        buildProcess.StartInfo.FileName = Application.dataPath + "/Editor/butler.exe";
        buildProcess.StartInfo.Arguments = "push \"" + path + "\" "+ItchAccount+"/" + ItchProjectName + ":" + branch + "-" + platform + " --userversion " + version;
        buildProcess.Start();

        EditorPrefs.SetInt(PlayerSettings.productName + "_uploadVersion_" + branch + "_" + platform, version);
    }

    void Play(string branch, string platform)
    {
        string path = GetPlayPath(branch, platform);

        Process gameProcess = new Process();

        gameProcess.StartInfo.CreateNoWindow = true;
        gameProcess.StartInfo.FileName = path;
        gameProcess.Start();
    }

    bool PlayExists(string branch, string platform)
    {
        if (platform == "mac") return Directory.Exists(GetPlayPath(branch, platform));

        return File.Exists(GetPlayPath(branch, platform));
    }

    bool UploadExists(string branch, string platform)
    {
        return File.Exists(GetBuiltPath(branch, platform));
    }

    void OnGUI()
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if(!gm)
        {
            EditorGUILayout.LabelField("No GameManager found.", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        Event e = Event.current;
        if(e != null)
        {
            if(e.isKey && e.keyCode == KeyCode.Return)
            {
                editBranch = false;
                Save();
                Repaint();
                return;
            }
            if (e.isKey && e.keyCode == KeyCode.Escape)
            {
                editBranch = false;
                Load();
                Repaint();
                return;
            }
        }

        GUILayoutOption miniButtonWidth = GUILayout.Width(25f);
        if(branches.branches.Count == 0)
        {
            if (GUILayout.Button("+", EditorStyles.miniButtonMid, miniButtonWidth))
            {
                branches.branches.Add("New branch");
                Save();
                return;
            }
        }
        
        EditorGUILayout.LabelField("Version " + gm.version, EditorStyles.boldLabel);

        string platform = EditorPrefs.GetString(PlayerSettings.productName + "_buildPlatform", "win");
        string[] platforms = new string[] { "win", "linux", "mac"};
        int currentIndex = platforms.IndexOf(platform);

        platform = platforms[GUI.Toolbar(new Rect(0, 18, Screen.width, 20), currentIndex, platforms, EditorStyles.toolbarButton)];
        EditorPrefs.SetString(PlayerSettings.productName + "_buildPlatform", platform);

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        for (int i = 0; i < branches.branches.Count;i++)
        {
            string branch = branches.branches[i];

            GUI.color = Color.black;
            GUI.Box(new Rect(0, 30 + (1 + i) * 96, Screen.width, 3), "");
            GUI.color = Color.white;

            #region Editing
            EditorGUILayout.BeginHorizontal();

            if(editBranch && branchEditing == i)
            {
                branches.branches[i] = EditorGUILayout.TextField(branches.branches[i], GUILayout.Width(Screen.width - ((25 * 3) + 15)));
                branches.branches[i] = branches.branches[i].Replace(" ", "");
            }
            else
            {
                EditorGUILayout.LabelField(branches.branches[i], GUILayout.Width(Screen.width - ((25 * 3) + 15)));
            }

            if(GUILayout.Button("<", EditorStyles.miniButtonLeft, miniButtonWidth))
            {
                editBranch = true;
                branchEditing = i;
            }
            if(GUILayout.Button("+", EditorStyles.miniButtonMid, miniButtonWidth))
            {
                branches.branches.Add(branches.branches[i]+" - copy");
                Save();
                return;
            }
            if(GUILayout.Button("-", EditorStyles.miniButtonRight, miniButtonWidth))
            {
                branches.branches.RemoveAt(i);
                Save();
                return;
            }
            
            EditorGUILayout.EndHorizontal();
            #endregion

            int buildDiff = Version - GetBuiltVersion(branch, platform);
            int uploadDiff = Version - GetUploadVersion(branch, platform);

            if (GUILayout.Button("Build diff:" + buildDiff))
            {
                Build(branch, platform);
            }
            if(UploadExists(branch, platform))
            {
                if (GUILayout.Button("Upload diff:" + uploadDiff))
                {
                    Upload(branch, platform);
                }
            }
            else
            {
                GUI.color = new Color(1f, 1f, 1f, 0.3f);

                GUILayout.Button("Upload");

                GUI.color = Color.white;
            }
            if(PlayExists(branch, platform))
            {
                if (GUILayout.Button("Play"))
                {
                    Play(branch, platform);
                }
            }
            else
            {
                GUI.color = new Color(1f, 1f, 1f, 0.3f);
                
                GUILayout.Button("Play");

                GUI.color = Color.white;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }
    }
}
