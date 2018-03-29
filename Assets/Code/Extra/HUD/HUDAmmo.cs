using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDAmmo : MonoBehaviour {

    public RectTransform primary;
    public RectTransform secondary;
    public int scale = 2;
    public int secondaryPosition = -102;

    public Image primaryBorder;
    public Image secondaryBorder;

    public float lerpness = 5f;
    
    public GunShooter playerGun;

    static HUDAmmo singleton;

    private void Awake()
    {
        singleton = this;
    }

    public static void Assign(GunShooter playerGun)
    {
        if (!singleton) singleton = FindObjectOfType<HUDAmmo>();

        singleton.playerGun = playerGun;
    }

    private void Update()
    {
        if (!singleton) singleton = this;

        if (playerGun)
        {
            Vector2 primaryPosition = new Vector2(-50, 22);
            Vector2 secondaryPosition = new Vector2(this.secondaryPosition, 22);

            if (playerGun.index == 1)
            {
                primaryPosition = new Vector2(this.secondaryPosition, 22);
                secondaryPosition = new Vector2(-50, 22);

                primaryBorder.enabled = false;
                secondaryBorder.enabled = true;
            }
            else
            {
                primaryBorder.enabled = true;
                secondaryBorder.enabled = false;
            }

            if(playerGun.guns.Count > 1)
            {
                if (!secondary.gameObject.activeSelf)
                {
                    secondary.gameObject.SetActive(true);
                }

                secondary.anchoredPosition = Vector2.Lerp(secondary.anchoredPosition, secondaryPosition, Time.deltaTime * lerpness);
                secondary.localScale = Vector3.Lerp(secondary.localScale, Vector3.one * scale, Time.deltaTime * lerpness);
            }
            else
            {
                if(secondary.gameObject.activeSelf)
                {
                    secondary.gameObject.SetActive(false);
                }
            }

            primary.anchoredPosition = Vector2.Lerp(primary.anchoredPosition, primaryPosition, Time.deltaTime * lerpness);
            primary.localScale = Vector3.Lerp(primary.localScale, Vector3.one * scale, Time.deltaTime * lerpness);
        }
    }
}
