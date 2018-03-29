using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMinimapIcon : MonoBehaviour {

    public bool show = false;
    public Sprite icon;
    [HideInInspector]
    public Character character;
    [HideInInspector]
    public ObjectPickup pickup;
    bool notVisibile;

    private void Awake()
    {
        character = GetComponent<Character>();
        pickup = GetComponent<ObjectPickup>();
    }

    private void Update()
    {
        if (!show) return;

        if(character && character.isDead && !notVisibile && icon)
        {
            ObjectManager.Remove(this);
            MinimapManager.Refresh();
            notVisibile = true;
        }
    }

    private void OnEnable()
    {
        ObjectManager.Add(this);

        if (!show) return;
        MinimapManager.Refresh();
    }

    private void OnDisable()
    {
        ObjectManager.Remove(this);

        if (!show) return;
        MinimapManager.Refresh();
    }
}
