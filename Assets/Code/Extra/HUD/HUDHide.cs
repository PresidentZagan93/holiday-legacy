using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDHide : MonoBehaviour, IRefreshable {

    public bool invert;

    public bool hideIfLoading = true;
    public bool hideIfChoosingBuild = true;

    Graphic[] gr;

    void Awake()
    {
        gr = GetComponentsInChildren<Graphic>();
    }

    public int GetValue()
    {
        return gr.Length;
    }

    void Update()
    {
        bool shouldHide = false;
        if(hideIfLoading)
        {
            shouldHide = !shouldHide;
        }
        if(hideIfChoosingBuild)
        {
            shouldHide = shouldHide | PickerItems.Active;
        }
        for (int i = 0; i < gr.Length; i++)
        {
            gr[i].enabled = invert ? !shouldHide : shouldHide;
        }
    }

    public void Refresh()
    {
        gr = GetComponentsInChildren<Graphic>();
    }
}
