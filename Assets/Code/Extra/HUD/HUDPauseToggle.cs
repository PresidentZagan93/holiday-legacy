using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class HUDPauseToggle : MonoBehaviour, IRefreshable {
    
    public bool invert = true;
    public bool state;
    List<Graphic> gr = new List<Graphic>();

    GameManager gameManager;

    void Awake()
    {
        gameManager = FindObjectOfType<GameManager>();
        Refresh();
    }

    public int GetValue()
    {
        return gr.Count;
    }

    void SetState()
    {
        if (!gameManager) gameManager = FindObjectOfType<GameManager>();

        bool newState = gameManager.paused || !Character.Player;

        if(state != newState)
        {
            state = newState;
            SetState(state);
        }
    }

    void SetState(bool state)
    {
        if (invert) state = !state;
        for (int i = 0; i < gr.Count; i++)
        {
            if (gr[i])
            {
                gr[i].enabled = state;
            }
        }
    }


    void Update()
    {
        SetState();
    }

    public void Refresh()
    {
        gr = GetComponentsInChildren<Graphic>().ToList();
    }
}
