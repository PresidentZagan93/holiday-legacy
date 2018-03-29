using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterfaceLayer : MonoBehaviour {
    
    public bool Active
    {
        get
        {
            return active;
        }
        set
        {
            active = value;
        }
    }

    bool active;
    InterfaceSelector selector;

    public InterfaceSelector Selector
    {
        get
        {
            return selector;
        }
    }

    protected virtual void Awake()
    {
        selector = GetComponentInChildren<InterfaceSelector>();
    }
}
