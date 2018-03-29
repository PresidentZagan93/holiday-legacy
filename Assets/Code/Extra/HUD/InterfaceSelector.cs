using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI.Extensions;

public class InterfaceSelector : InterfaceObject, IRefreshable {

    public bool wiggle = true;
    public bool noLerp = true;

    [Range(0f,1f)]
    public float threshold = 0.5f;
    public Vector2 adjustment;
    public InterfaceSelectable selected;
    InterfaceSelectable lastSelected;

    public int maxDistance = 25;
    public bool allowMouseInput = true;

    public GameObject listener;

    public AudioClip sound;
    AudioSource source;

    List<InterfaceSelectable> selectables = new List<InterfaceSelectable>();

    protected override void Awake()
    {
        source = GetComponent<AudioSource>();
        AudioManager.Assign(source, AudioManager.AudioType.UI);
        Refresh();
    }

    public void RefreshSelectables()
    {
        selectables.Clear();
        selectables = transform.parent.GetComponentsInChildren<InterfaceSelectable>().ToList();
    }

    public void Refresh()
    {
        if(selected)
        {
            selected.Deselect();
        }
        RefreshSelectables();
        selected = selectables[0];
        lastSelected = selected;
        if (selected.GetComponent<NicerOutline>())
        {
            selected.GetComponent<NicerOutline>().effectColor = Color.red;
        }

        selected.Select();
        selected.Selector = this;
        Refresh(false);
    }

    public int GetValue()
    {
        return selectables.Count;
    }

    void OnEnable()
    {
        Refresh();
    }

    public void Deselect()
    {
        if(selected)
        {
            selected.Deselect();
        }
    }

    void Click()
    {
        if (listener)
        {
            listener.SendMessage("Click", selected, SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            if (selected)
            {
                selected.Click();
                if (selected.GetComponent<NicerOutline>())
                {
                    selected.GetComponent<NicerOutline>().effectColor = Color.clear;
                }
            }
        }
    }

    void Select()
    {
        if(sound && source)
        {
            source.PlayOneShot(sound);
        }
        if (listener)
        {
            listener.SendMessage("Select", selected, SendMessageOptions.DontRequireReceiver);
        }
    }
    
    void MouseActions()
    {
        for(int i = 0; i < selectables.Count;i++)
        {
            if (!selectables[i]) continue;

            int minX = Mathf.RoundToInt(selectables[i].Rect.position.x - selectables[i].Rect.rect.width / 2f);
            int minY = Mathf.RoundToInt(selectables[i].Rect.position.y + selectables[i].Rect.rect.height / 2f);
            int maxX = Mathf.RoundToInt(selectables[i].Rect.position.x + selectables[i].Rect.rect.width / 2f);
            int maxY = Mathf.RoundToInt(selectables[i].Rect.position.y - selectables[i].Rect.rect.height / 2f);

            minX++;
            minY++;

            maxX--;
            maxY--;

            bool cursorInside = CursorManager.Inside(minX, minY, maxX, maxY);
            if(cursorInside)
            {
                selected = selectables[i];
                selected.Selector = this;
                mouseOn = true;
                return;
            }
        }
    }

    bool mouseOn;
    bool lastLeft;
    bool lastRight;
    bool lastUp;
    bool lastDown;

    float nextRepeat;
    float repeatDelay;

    void Refresh(bool select = true)
    {
        if (selected)
        {
            if (select)
            {
                Select();
                selected.Selector = this;
            }
        }
    }

    void Update()
    {
        if (Console.Open) return;
        if (selected)
        {
            float t = Mathf.Sin(Time.time * 15f);

            Vector3 altPos = new Vector3(Mathf.RoundToInt(t) * -1f, 0f, 0f);
            if (!wiggle) altPos = Vector3.zero;
            Vector3 newPos = selected.transform.position + (Vector3)adjustment + altPos;

            if(noLerp)
            {
                transform.position = newPos;
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime * 15f);
            }

            bool keyPress = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Confirm");
            if (keyPress || (Input.GetKeyUp(KeyCode.Mouse0) && mouseOn && allowMouseInput))
            {
                Click();
            }
        }

        if (lastSelected != selected)
        {
            if(lastSelected)
            {
                lastSelected.Deselect();
                selected.Selector = null;
                if (lastSelected.GetComponent<NicerOutline>())
                {
                    lastSelected.GetComponent<NicerOutline>().effectColor = Color.clear;
                }
            }
            Refresh(true);
            lastSelected = selected;
            if(selected)
            {
                selected.Select();
                selected.Selector = this;
                if(selected.GetComponent<NicerOutline>())
                {
                    selected.GetComponent<NicerOutline>().effectColor = Color.red;
                }
            }
        }

        bool simLeft = false, simRight = false, simUp = false, simDown = false;

        if(Input.GetAxisRaw("Horizontal2") < 0) simLeft = !lastLeft;
        else lastLeft = false;

        if (Input.GetAxisRaw("Horizontal2") > 0) simRight = !lastRight;
        else lastRight = false;

        if (Input.GetAxisRaw("Vertical2") < 0) simUp = !lastUp;
        else lastUp = false;

        if (Input.GetAxisRaw("Vertical2") > 0) simDown = !lastDown;
        else lastDown = false;

        Vector2 dir = Vector2.zero;
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A) || simLeft)
        {
            dir += Vector2.left;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D) || simRight)
        {
            dir += Vector2.right;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || simUp)
        {
            dir += Vector2.up;
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S) || simDown)
        {
            dir += Vector2.down;
        }

        if (allowMouseInput)
        {
            MouseActions();
        }

        if(dir != Vector2.zero)
        {
            mouseOn = false;

            int closestIndex = -1;
            float closestDist = float.MaxValue;
            //find the closest to the direction
            for(int i = 0; i < selectables.Count;i++)
            {
                selectables[i].selected = false;

                if(selectables[i] && selectables[i] != selected)
                {
                    Vector2 d = selectables[i].transform.position - (transform.position - (Vector3)adjustment);
                    //d.Normalize();

                    bool sameDirection = Vector2.Distance(d.normalized, dir) < threshold;
                    float newDistance = Vector2.Distance(d, dir);
                    if (newDistance < closestDist && sameDirection)
                    {
                        closestDist = newDistance;
                        closestIndex = i;
                    }
                }
                if (!selectables[i]) selectables.RemoveAt(i);
            }
            if (closestIndex != -1)
            {
                selected = selectables[closestIndex];
            }
        }

        if(selected) selected.selected = true;
    }
}
