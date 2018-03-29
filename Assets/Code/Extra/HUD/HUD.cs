using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public interface IRefreshable
{
    void Refresh();
    int GetValue();
}

public class InterfaceObject : MonoBehaviour
{
    InterfaceLayer layer;
    RectTransform rectTransform;

    public InterfaceLayer Layer
    {
        get
        {
            return layer;
        }
    }
    public RectTransform Rect
    {
        get
        {
            return rectTransform;
        }
    }

    protected virtual void Awake()
    {
        layer = GetComponentInParent<InterfaceLayer>();
        rectTransform = GetComponent<RectTransform>();
    }
}

public class HUD : MonoBehaviour {

    static HUD singleton;

    public bool skipIntro = false;
    public float introTime = 3f;
    public bool allowMouseInput = true;
    InterfaceSelector[] selectors;
    GameManager gameManager;
    CameraManager cameraManager;
    float nextIntro;

    public Image bloodOverlay;
    public Image vignette;
    public AudioClip changeSound;
    public AudioClip introSound;
    AudioSource source;

    public InterfaceLayer[] layers;
    public InterfaceLayer layer;
    public string defaultLayer;
    string lastLayer;

    public static string Layer
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<HUD>();

            return singleton.layer.name;
        }
    }

    void Awake()
    {
        source = GetComponent<AudioSource>();
        AudioManager.Assign(source, AudioManager.AudioType.UI);
        gameManager = FindObjectOfType<GameManager>();
        selectors = GetComponentsInChildren<InterfaceSelector>(true);
        cameraManager = FindObjectOfType<CameraManager>();
        singleton = this;
        nextIntro = Time.time + introTime;

#if UNITY_EDITOR
        if (skipIntro) ChangeLayer("Menu-Pre");
        else ChangeLayer(defaultLayer);
#else
        ChangeLayer(defaultLayer);
#endif
    }

    public static void ChangeLayer(string newLayer)
    {
        if (!singleton) singleton = FindObjectOfType<HUD>();

        if (Application.isPlaying)
        {
            if (newLayer.StartsWith("Intro"))
            {
                if (newLayer == "Intro")
                {
                    singleton.source.clip = singleton.introSound;
                    singleton.source.Play();
                }
            }
            else
            {
                singleton.source.clip = singleton.changeSound;
                singleton.source.Play();
            }
        }
        
        if (newLayer == "Menu" && Character.Player)
        {
            if(singleton.lastLayer == "Settings") newLayer = "InGame-Menu";
            else newLayer = "Reload";
        }
        if (newLayer == "Retry")
        {
            newLayer = "Play";
        }
        if (newLayer == "Play" || newLayer == "Coop")
        {
            GeneratorManager.CleanUp();
            GameManager.Reset();

            GeneratorManager.StartGame(GeneratorManager.RandomSeed);
            newLayer = "Generating";
        }
        if(newLayer == "Exit")
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        if (newLayer == "Reload")
        {
            newLayer = "Menu";
            Destroy(FindObjectOfType<Generator>().gameObject);

            ObjectManager.DestroyAll();

            //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if(newLayer.StartsWith("Cheat"))
        {
            string cheatName = newLayer.Split('-')[1];
            if (cheatName == "DiscoverAll")
            {
                InterfaceAlmanacLayer.FindAll();
            }
            if (cheatName == "ResetAll")
            {
                PlayerPrefs.DeleteAll();
                InterfaceAlmanacLayer.ResetAll();
                ConfigManager.ResetAll();
            }
            return;
        }

        if (newLayer == "InGame" && GameManager.Paused)
        {
            if (Application.isPlaying) singleton.gameManager.PauseForce(false);
        }

        if(singleton.layer) singleton.lastLayer = singleton.layer.name;

        for (int i = 0; i < singleton.layers.Length; i++)
        {
            bool enable = singleton.layers[i].name == newLayer;
            if(newLayer == "InGame-Menu")
            {
                enable = enable || singleton.layers[i].name == "InGame";
            }
            if(newLayer == "Dead")
            {
                enable = enable || singleton.layers[i].name == "InGame";
            }
            singleton.layers[i].gameObject.SetActive(enable);

            if(enable)
            {
                singleton.layer = singleton.layers[i];
            }
        }
    }

    float shakeDuration;
    float shakePower;
    float shake;
    public float shakeSettle = 3f;
    public float maxScreenshake = 1f;

    public float uiShakeMultiplier = 1f;
    public float worldShakeMultiplier = 1f;
    Vector3 shakeVec;

    public static void BloodyHit(float power)
    {
        if (!singleton) singleton = FindObjectOfType<HUD>();

        Color newBloodyColor = singleton.bloodOverlay.color;
        newBloodyColor.a += power;
        if (newBloodyColor.a > 1f) newBloodyColor.a = 1f;
        singleton.bloodOverlay.color = newBloodyColor;
    }

    public static void Shake(float power, float duration = 0.25f)
    {
        if (!singleton) singleton = FindObjectOfType<HUD>();
        
        singleton.shakeDuration += duration;
        singleton.shakePower += power;

        if(singleton.shakeDuration > singleton.maxScreenshake * Settings.Setting.screenshakeWorld)
        {
            singleton.shakeDuration = singleton.maxScreenshake * Settings.Setting.screenshakeWorld;
        }

        singleton.shake = singleton.shakePower;
    }

    void Update()
    {
        singleton = this;
        vignette.enabled = Generator.singleton && Generator.singleton.preset.effects.underground;
        if (Character.Player)
        {
            Color vignetteColor = vignette.color;
            vignetteColor.a = Character.Player.Inventory.GetMultiplier("Visibility").Remap(1f, 3f, 1f, 0f);
            vignette.color = vignetteColor;
        }

        Color newBloodColor = bloodOverlay.color;
        newBloodColor.a = Mathf.Lerp(newBloodColor.a, 0f, Time.deltaTime * 12f);
        bloodOverlay.color = newBloodColor;

        for (int i = 0; i < selectors.Length; i++)
        {
            selectors[i].allowMouseInput = allowMouseInput;
        }

#if UNITY_EDITOR
        if ((layer.name == "Menu" || layer.name == "Menu-Pre") && GeneratorManager.singleton.autoGenerate)
        {
            ChangeLayer("Play");
        }
#endif

        if (layer.name == "Intro")
        {
            if (Time.time > nextIntro)
            {
                ChangeLayer("Menu-Pre");
            }
        }

        if (layer.name == "Menu-Pre" && !Console.Open)
        {
            if (Input.inputString != "" || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) ||
                Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                ChangeLayer("Menu");
            }
        }

        if (shakeDuration > 0f)
        {
            shake = Mathf.Lerp(shake, 0f, Time.deltaTime * shakeSettle * 0.2f);

            shakeVec = Random.insideUnitSphere * shake;
            shakeVec.z = 0;
            shakeDuration -= Time.deltaTime * shakeSettle;

            for (int i = 0; i < transform.childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (shakeDuration <= 0f)
                {
                    child.localPosition = Vector3.zero;
                }
                else
                {
                    child.localPosition = shakeVec * uiShakeMultiplier * Settings.Setting.screenshakeUi / 50f;
                    child.localPosition = new Vector3(child.localPosition.x.RoundToInt(), child.localPosition.y.RoundToInt(), 0f);
                }
            }

            if(shakeDuration <= 0f || shake <= 1f)
            {
                cameraManager.offset = Vector3.zero;
                shake = 0f;
                shakePower = 0f;
                shakeVec = Vector3.zero;
            }
            else
            {
                cameraManager.offset = shakeVec * worldShakeMultiplier * Settings.Setting.screenshakeWorld / 50f;
            }
        }
    }
}
