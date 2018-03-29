using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDTextFPS : MonoBehaviour
{
    public float updateInterval = 0.5F;
    public bool instant = false;

    private float accum = 0; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeleft; // Left time for current interval

    Text fpsText;

    void Awake()
    {
        fpsText = GetComponent<Text>();

        if (!fpsText)
        {
            Debug.Log("UtilityFramesPerSecond needs a GUIText component!");
            enabled = false;
            return;
        }
        timeleft = updateInterval;
    }

    void Update()
	{
        fpsText.enabled = Settings.Setting.showFps;
        if(instant)
        {
            float fps = 1.0f / Time.deltaTime;
            string format = string.Format("{0:F2} FPS", fps);
            fpsText.text = format;
            return;
        }
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        // Interval ended - update GUI text and start new interval
        if (timeleft <= 0.0)
        {
            // display two fractional digits (f2 format)
            float fps = accum / frames;
            string format = string.Format("{0:F2} FPS", fps);
            fpsText.text = format;

            timeleft = updateInterval;
            accum = 0.0F;
            frames = 0;
        }
    }
}