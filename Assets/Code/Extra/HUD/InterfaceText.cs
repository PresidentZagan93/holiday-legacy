using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InterfaceText : InterfaceObject {

    public string label = "test";
    Text text;

    protected override void Awake()
    {
        base.Awake();

        LocalizationManager.Add(this);
    }

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if(!text) text = GetComponent<Text>();
        text.text = LocalizationManager.ConvertFromName(label);
    }
}
