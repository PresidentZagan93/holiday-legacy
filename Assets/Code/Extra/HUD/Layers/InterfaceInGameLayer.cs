using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceInGameLayer : InterfaceLayer {

    bool lastDisplay;
    public Transform root;

    public UnityEngine.UI.Extensions.NicerOutline[] outlines;
    public UnityEngine.UI.Image[] images;

    public static void Refresh()
    {
        InterfaceInGameLayer layer = FindObjectOfType<InterfaceInGameLayer>();

        for (int i = 0; i < layer.outlines.Length; i++)
        {
            if(layer.outlines[i])
            {
                layer.outlines[i].effectColor = Generator.singleton.preset.effects.underground ? Color.white : Color.black;
            }
        }
        for (int i = 0; i < layer.images.Length; i++)
        {
            if (layer.images[i])
            {
                layer.images[i].color = Generator.singleton.preset.effects.underground ? Color.white : Color.black;
            }
        }

        bool display = true;
        if (GameManager.Paused) display = false;

        if (layer.lastDisplay != display)
        {
            layer.lastDisplay = display;
            layer.root.gameObject.SetActive(display);
        }
    }
}
