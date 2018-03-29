using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDShakeOnChange : MonoBehaviour {
    
    float shakeAmount;
    public float shakeSettle = 4f;
    public float shakePower = 4f;

    public bool bloatOnChange = false;
    public float bloatAmount = 2f;
    public float bloatSettle = 20f;
    
    Vector2 originalScale;
    Text text;
    Image image;
    string lastText;
    float lastFill;
    Vector3 originalPos;
    IRefreshable refreshable;

    void Awake()
    {
        refreshable = GetComponent<IRefreshable>();
        text = GetComponent<Text>();
        image = GetComponent<Image>();

        if (text)
        {
            originalScale = text.rectTransform.localScale;
            originalPos = text.rectTransform.anchoredPosition3D;
        }
        if(image)
        {
            originalScale = image.rectTransform.localScale;
            originalPos = image.rectTransform.anchoredPosition3D;
        }

        if (refreshable != null) refreshable.Refresh();
    }

    void Update()
    {
        Vector2 shakeVec = Random.insideUnitCircle * shakeAmount;
        shakeAmount = Mathf.Lerp(shakeAmount, 0f, Time.deltaTime * shakeSettle);

        if (text || image)
        {
            bool changed = false;
            if (text) changed = lastText != text.text;
            if (image) changed = image.fillAmount != lastFill;

            if (changed)
            {
                shakeAmount = 1;
                if (text) lastText = text.text;
                if (image) lastFill = image.fillAmount;

                if (bloatOnChange)
                {
                    if (text) text.rectTransform.localScale = originalScale * bloatAmount;
                    if (image) image.rectTransform.localScale = originalScale * bloatAmount;
                }
            }
        }

        if (text)
        {
            Vector3 newScale = Vector2.Lerp(text.rectTransform.localScale, originalScale, Time.deltaTime * bloatSettle);
            newScale.z = 1f;
            text.rectTransform.localScale = newScale;

            Vector3 newPos = originalPos + (Vector3)shakeVec * shakePower * (Settings.Setting.screenshakeUi / 100f);
            newPos.z = 0;
            text.rectTransform.anchoredPosition3D = newPos;
        }
        if(image)
        {
            Vector3 newScale = Vector2.Lerp(image.rectTransform.localScale, originalScale, Time.deltaTime * bloatSettle);
            newScale.z = 1f;
            image.rectTransform.localScale = newScale;

            Vector3 newPos = originalPos + (Vector3)shakeVec * shakePower * (Settings.Setting.screenshakeUi / 100f);
            newPos.z = 0;
            image.rectTransform.anchoredPosition3D = newPos;
        }
    }
}
