using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceHealthBar : InterfaceObject {

    public TextAlignment alignment = TextAlignment.Center;
    float shakeAmount;
    public float shakeSettle = 8f; //lerpness of how long it takes to settle screenshake
    public float shakePower = 4f; //shake impact

    public bool vertical = false; //order vertically?
    public int gap = 14; //gap between each slot

    public Sprite background; //slot that isnt filled
    public Sprite fill; //slot that is filled

    Health health;
    bool lastVertical = false;
    int lastGap = -1;
    int lastValue = -1;
    int lastMax = -1;
    float nextBlink;

    string keyConfigType;
    string keyMaxConfigType;

    List<RectTransform> slots = new List<RectTransform>();
    IRefreshable refreshable;

    protected override void Awake()
    {
        if (refreshable == null) refreshable = GetComponent<IRefreshable>();
    }

    public void Assign(Health health)
    {
        this.health = health;
    }

    void Update()
    {
        if (GameManager.Paused) return;

        int value = health ? health.hp : 0;
        int maxValue = health ? health.maxHp : 0;

        if(Time.time > nextBlink && health && value != maxValue)
        {
            float percentage = (float)value / (float)maxValue;
            nextBlink = Time.time + percentage.Remap(0f, 1f, 0.4f, 3f);

            StartCoroutine(Heartbeat());
        }

        if (lastValue != value || lastMax != maxValue || lastVertical != vertical || lastGap != gap)
        {
            slots.Clear();

            lastVertical = vertical;
            lastGap = gap;
            lastMax = maxValue;
            lastValue = value;

            Image[] allImages = transform.GetComponentsInChildren<Image>();

            //Removes previous slots
            for (int i = 0; i < allImages.Length; i++)
            {
                if (allImages[i].name == "Slot")
                {
                    Destroy(allImages[i].gameObject);
                }
            }

            //Generates new ones
            for (int i = 0; i < maxValue; i++)
            {
                Image newSlot = new GameObject("Slot").AddComponent<Image>();
                newSlot.rectTransform.SetParent(transform);

                if (value <= i) newSlot.sprite = background;
                else newSlot.sprite = fill;

                slots.Add(newSlot.rectTransform);
                Outline outline = newSlot.gameObject.AddComponent<Outline>();
                outline.effectDistance = new Vector2(1, -1);
                outline.effectColor = Color.black;

                newSlot.rectTransform.sizeDelta = new Vector2(newSlot.sprite.textureRect.width, newSlot.sprite.textureRect.height);
                newSlot.transform.SetAsFirstSibling();
            }

            //Starts shaking
            shakeAmount = 1f;

            if (refreshable != null) refreshable.Refresh();
        }

        //Screenshake settle
        Vector2 shakeVec = Random.insideUnitCircle * shakeAmount;
        shakeAmount = Mathf.Lerp(shakeAmount, 0f, Time.fixedDeltaTime * shakeSettle);
        shakeVec *= shakePower * Settings.Setting.screenshakeUi / 100f;

        //Screenshakes
        bool state = (Character.Player && !PickerItems.Active && !GeneratorManager.Generating);
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].localScale = Vector3.Lerp(slots[i].localScale, state ? Vector2.one : Vector2.zero, Time.deltaTime * 10f);

            float x = Remap(i, 0, maxValue - 1, 1 - maxValue, maxValue - 1);
            if (maxValue == 1) x = 0;

            float xStart = 0f;
            if (alignment == TextAlignment.Left)
            {
                xStart = maxValue / 2f * -14f;
            }
            else if (alignment == TextAlignment.Right)
            {
                xStart = maxValue / 2f * 14f;
            }

            if (vertical) slots[i].localPosition = new Vector3(shakeVec.x, xStart + x * gap + shakeVec.y, 0f);
            else slots[i].localPosition = new Vector3(xStart + x * gap + shakeVec.x, shakeVec.y, 0f);
        }
    }

    IEnumerator Heartbeat()
    {
        float scale = ((float)health.hp / (float)health.maxHp).Remap(0f, 1f, 2f, 1.2f);
        bool state = (Character.Player && !PickerItems.Active && !GeneratorManager.Generating);
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].localScale = state ? Vector2.one * scale : Vector2.zero;
        }
        yield return new WaitForSeconds(0.15f);
        for (int i = 0; i < slots.Count; i++)
        {
            slots[i].localScale = state ? Vector2.one * scale : Vector2.zero;
        }
    }

    float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}
