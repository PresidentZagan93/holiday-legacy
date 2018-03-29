using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class HUDConsumableUI : MonoBehaviour {

    public Text timer;

    public Text level;
    public Text timerExtended;
    NicerOutline levelOutline; 

    public Image selector;

    Vector3 originalTimerPos;
    float lastTime = 0f;
    float nextTimerExtended;

    private void Awake()
    {
        originalTimerPos = timer.transform.localPosition;

        levelOutline = level.gameObject.AddComponent<NicerOutline>();
        levelOutline.effectColor = Color.white;
        levelOutline.effectDistance = Vector2.one;
        levelOutline.enabled = false;
    }

    void Update()
    {
        bool focus = false;
        if (Character.Player && Character.Player.Inventory.HasConsumable)
        {
            focus = Character.Player.Inventory.GetConsumable().Active;
            if(focus)
            {
                if(lastTime == 0f)
                {
                    lastTime = Character.Player.Inventory.GetConsumable().duration;
                }
                if(Character.Player.Inventory.GetConsumable().duration - lastTime > 0)
                {
                    nextTimerExtended = Time.time + 1f;
                    timerExtended.transform.localScale = Vector3.one;

                    lastTime = Character.Player.Inventory.GetConsumable().duration;
                }
                selector.enabled = true;
                levelOutline.enabled = true;
            }
            else
            {
                selector.enabled = false;
                levelOutline.enabled = false;
            }
        }
        else
        {
            timer.text = "";
            selector.enabled = false;
            levelOutline.enabled = false;
        }

        if(Time.time > nextTimerExtended)
        {
            timerExtended.transform.localScale = Vector3.Lerp(timerExtended.transform.localScale, Vector3.zero, Time.deltaTime * 2f);
        }

        if (focus)
        {
            timer.transform.localPosition = Vector3.Lerp(timer.transform.localPosition, new Vector3(0f, -70f), Time.deltaTime * 15f);
        }
        else
        {
            timer.transform.localPosition = Vector3.Lerp(timer.transform.localPosition, originalTimerPos, Time.deltaTime * 15f);
        }
    }
}
