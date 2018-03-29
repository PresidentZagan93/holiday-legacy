using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Visibility : MonoBehaviour {

	public static float Multiplier
    {
        get
        {
            if (!singleton) singleton = FindObjectOfType<Visibility>();

            return singleton.multiplier;
        }
        set
        {
            if (!singleton) singleton = FindObjectOfType<Visibility>();

            singleton.multiplier = value;
        }
    }
    public static bool Enabled
    {
        get
        {
            if(!singleton) singleton = FindObjectOfType<Visibility>();

            return singleton.enabled;
        }
        set
        {
            if (!singleton) singleton = FindObjectOfType<Visibility>();

            singleton.enabled = value;
            singleton.Refresh();
        }
    }

    new public bool enabled;
    public float multiplier = 1f;
    public float lerpness = 7f;

    float nextFlicker;

    static Visibility singleton;
    int lastRange;
    bool lastEnabled;
    public RectTransform mask;
    public RectTransform gameView;
    RectTransform rectTransform;

    private void Awake()
    {
        singleton = FindObjectOfType<Visibility>();
        rectTransform = GetComponent<RectTransform>();
    }

    private void Update()
    {
        gameView.position = rectTransform.position;

        if (GameManager.Paused) return;

        float maskSize = 600f;
        Vector2 maskPosition = Vector2.zero;
        if(GeneratorManager.Generating || PickerItems.Active || !Character.Player)
        {
            if(HUD.Layer != "Dead")
            {
                maskSize = 0f;
            }
        }
        else
        {
            if(!Character.Player.RoomChecker.room)
            {
                float distanceFromFinish = (Settings.Temporary.finish.ToVector().ToWorldPos().x - Character.Player.transform.position.x) + 230f;
                if (distanceFromFinish < 230f)
                {
                    maskSize = (distanceFromFinish - 25f) * 2.3f;
                    maskPosition = new Vector2(212f - distanceFromFinish, 0f);
                }
            }
        }

        if (maskSize < 0f) maskSize = 0f;

        mask.sizeDelta = Vector2.Lerp(mask.sizeDelta, Vector2.one * maskSize, Time.deltaTime * lerpness);
        mask.anchoredPosition = Vector2.Lerp(mask.anchoredPosition, maskPosition, Time.deltaTime * lerpness);
    }

    void Refresh()
    {

    }
}
