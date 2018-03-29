using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CameraManager : MonoBehaviour {

    public bool snapToGrid = true;
    public float lookDistance = 1f;
    public int moveSpeed = 1; //how many units to move
    public int moveDistanceLimit = 64; //player doesnt ever leave the 64 radius circle
    public float smoothness = 5f; //how often, 2 means ever 2 frames, 6 means ever 6 means, 1 means every frame

    public Vector2 adjustment;
    [HideInInspector]
    public Vector2 offset;
    [HideInInspector]
    public Character target;
    Camera cam;
    public Camera uiCamera;
    Vector2 currentPosition;
    Vector2 lerpedPosition;
    static CameraManager singleton;
    AudioReverbFilter filter;
    [HideInInspector]
    public Vector2 focus;

    public static Vector3 Position
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<CameraManager>();

            return singleton.transform.position;
        }
    }

    public static Vector2 Adjustment
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<CameraManager>();

            return singleton.adjustment;
        }
    }

    public static Vector3 ScreenToWorldPoint(Vector3 vector)
    {
        if (!singleton) singleton = FindObjectOfType<CameraManager>();

        return singleton.cam.ScreenToWorldPoint(vector);
    }

    public static void Focus(Vector2 focus)
    {
        singleton.focus = focus;
    }

    void Awake()
    {
        filter = FindObjectOfType<AudioReverbFilter>();
        cam = GetComponent<Camera>();
    }

    public static bool Contains(Vector2 position)
    {
        Vector2 pos = singleton.cam.transform.position;
        Rect bounds = new Rect(pos.x - GameManager.GameWidth * 0.5f, pos.y - GameManager.GameHeight * 0.5f, GameManager.GameWidth, GameManager.GameHeight);
        return bounds.Contains(position);
    }

    void MoveToPosition(Vector3 toPos)
    {
        Vector3 newPos = toPos;
        if (snapToGrid)
        {
            newPos.x = Mathf.RoundToInt(toPos.x);
            newPos.y = Mathf.RoundToInt(toPos.y);
        }
        newPos.z = -800;

        transform.position = newPos;
    }
    
    public static void Snap()
    {
        if (!singleton) singleton = FindObjectOfType<CameraManager>();

        singleton.lerpedPosition = singleton.currentPosition;
    }

    private void OnEnable()
    {
        singleton = this;
    }

    void Move()
    {
        if (target && !target.isDead)
        {
            //move towards player if available
            currentPosition.x = target.transform.position.x;
            currentPosition.y = target.transform.position.y;
        }
    }

    void Update()
    {
        if (GameManager.Paused) return;

        cam.transparencySortAxis = Vector3.up;
        cam.transparencySortMode = TransparencySortMode.CustomAxis;

        if (Generator.singleton && Generator.singleton.preset)
        {
            filter.enabled = true;
            filter.reverbPreset = Generator.singleton.preset.effects.reverb;
        }
        else
        {
            filter.enabled = false;
        }

        uiCamera.transform.position = transform.position;

        if (!target)
        {
            focus = Vector2.zero;
            target = Character.Player;
        }
        else
        {
            if (!target.isDead)
            {
                if(focus == Vector2.zero)
                {
                    float minX = Settings.Temporary.spawn.ToVector().ToWorldPos().x + (3 * 32);
                    if (Generator.singleton.preset.turns == 0) minX = float.MinValue;
                    float maxX = Settings.Temporary.finish.ToVector().ToWorldPos().x + 32;

                    if (currentPosition.x > maxX)
                    {
                        currentPosition.x = maxX;
                    }
                    if (currentPosition.x < minX)
                    {
                        currentPosition.x = minX;
                    }
                }

                float len = target.Look.lookDirection.magnitude * lookDistance;
                adjustment = new Vector2(target.Look.lookDirection.x, target.Look.lookDirection.y).normalized;
                adjustment *= len / 15f;

                adjustment.x /= Helper.Width / GameManager.GameHeight;
                adjustment.y /= Helper.Height / GameManager.GameHeight;

                float viewMp = target.Inventory.GetMultiplier("Visibility");
                adjustment *= viewMp;

                lerpedPosition = Vector2.Lerp(lerpedPosition, currentPosition, Time.deltaTime * smoothness);

                MoveToPosition(lerpedPosition + adjustment + offset);
            }
        }

        if (GeneratorManager.Generating || PickerItems.Active)
        {
            if (target)
            {
                lerpedPosition = currentPosition;
                MoveToPosition(target.transform.position);
                Snap();
            }
            
        }
        else
        {

        }

        if (Generator.singleton && Generator.singleton.preset)
        {
            cam.backgroundColor = Generator.singleton.preset.effects.backgroundColor;
        }
        
        Move();
    }
}
