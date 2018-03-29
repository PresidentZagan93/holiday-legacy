using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InterfaceButton : InterfaceSelectable {
    
    public string toLayer = string.Empty;
    
    protected virtual void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, Time.deltaTime * 20f);
    }

    public override void Click()
    {
        base.Click();
        if (!toLayer.StartsWith("Inventory"))
        {
            if (toLayer != string.Empty)
            {
                HUD.ChangeLayer(toLayer);
            }
        }
    }

    public override void Select()
    {
        base.Select();
        transform.localScale = Vector3.one * 1.25f;
    }
}
