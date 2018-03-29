using UnityEngine;
using System.Collections;

using System.Collections.Generic;

[System.Serializable]
public class CursorSetup
{
    public Vector2Int worldPosition;
    public Vector2Int screenPosition;
}

public class CursorManager : MonoBehaviour, IRefreshable {
    
    public List<CursorSetup> cursors = new List<CursorSetup>();
    public Texture2D cursorImage;

    CameraManager cameraManager;
    Camera cam;
    
    public int mouseCount = 1;
    static CursorManager singleton;
    GameManager gameManager;

    float lastRatio;
    public Texture2D newCursor;
    public Vector2Int scaledHotspot;
    public Vector2Int screenspaceOffset = new Vector2Int(100, 0);

    public static List<CursorSetup> Cursors
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<CursorManager>();

            return singleton.cursors;
        }
    }

    public int GetValue()
    {
        return cursors.Count;
    }

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();

        singleton = this;
        cursors = new List<CursorSetup>(mouseCount);
        for(int i = 0; i < mouseCount;i++)
        {
            CursorSetup newCursor = new CursorSetup();
            cursors.Add(newCursor);
        }
        cameraManager = FindObjectOfType<CameraManager>();
        cam = cameraManager.GetComponent<Camera>();
    }
    
    void OnGUI()
    {
        GUI.depth = int.MinValue;
        for (int i = 0; i < cursors.Count; i++)
        {
            bool show = true;
            if(show && Character.Player)
            {
                float xRatio = Helper.Width / gameManager.gameWidth;
                float yRatio = Helper.Height / gameManager.gameHeight;

                float x = cursors[i].screenPosition.x * xRatio + Screen.width / 2f;
                float y = Screen.height - cursors[i].screenPosition.y * yRatio - Screen.height / 2f;

                int res = Mathf.RoundToInt(cursorImage.width * yRatio);

                x -= res / 2f;
                y -= res / 2f;

                GUI.DrawTexture(new Rect(x, y, res, res), cursorImage);
            }
        }
    }

    public void Refresh()
    {
        for (int i = 0; i < cursors.Count; i++)
        {
            Vector2Int vPos = new Vector2((int)Input.mousePosition.x - Screen.width / 2f, (int)Input.mousePosition.y - Screen.height / 2f).ToVector2Int();
            vPos.x = Mathf.RoundToInt(vPos.x / (Helper.Width / gameManager.gameWidth));
            vPos.y = Mathf.RoundToInt(vPos.y / (Helper.Height / gameManager.gameHeight));

            cursors[i].screenPosition = vPos;
            cursors[i].worldPosition = ScreenToWorld(cursors[i].screenPosition);
        }
    }

    Vector2Int ScreenToWorld(Vector2Int screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos.ToVector() + new Vector2(240f, 160f));
        RaycastHit hit = new RaycastHit();
        if (Physics.Raycast(ray, out hit))
        {
            return hit.point.ToVector2Int();
        }

        return Vector2Int.Zero;
    }

    void Update()
    {
        singleton = this;
        Cursor.visible = false;
        Refresh();
    }

    public static bool Inside(float x, float y, float width, float height)
    {
        if (!singleton) singleton = FindObjectOfType<CursorManager>();

        for (int i = 0; i < singleton.cursors.Count; i++)
        {
            Vector2Int pos = singleton.cursors[i].worldPosition;
            
            if (pos.x > x && pos.x < width && pos.y < y && pos.y > height)
            {
                return true;
            }
        }
        return false;
    }

    bool lastGamepad;

    void SetCursor()
    {
        float newRatio = Helper.Height / gameManager.gameHeight;
        if (Screen.width < Screen.height * Helper.Ratio)
        {
            newRatio = Screen.height / gameManager.gameHeight;
        }
        if (lastRatio != newRatio)
        {
            lastRatio = newRatio;

            int res = Mathf.RoundToInt(cursorImage.width * newRatio);

            //newCursor = cursorImage;

            newCursor = new Texture2D(cursorImage.width, cursorImage.height, TextureFormat.ARGB32, false);
            newCursor.SetPixels(cursorImage.GetPixels());
            newCursor.filterMode = FilterMode.Point;

            newCursor.Apply();

            TextureScale.Point(newCursor, res, res);

            scaledHotspot = new Vector2Int(newCursor.width / 2f, newCursor.height / 2f);
            Cursor.SetCursor(newCursor, scaledHotspot.ToVector(), CursorMode.ForceSoftware);
        }
    }
}
