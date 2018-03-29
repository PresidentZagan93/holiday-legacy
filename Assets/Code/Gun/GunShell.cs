using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunShell : MonoBehaviour {

    public AnimationCurve curve = new AnimationCurve();
    public Transform shell;
    public float duration = 10f;
    float lifetime;

    private void OnEnable()
    {
        lifetime = 0f;    
    }

    private void Update()
    {
        if (GameManager.Paused) return;

        lifetime += Time.deltaTime;
        if (shell) shell.localPosition = new Vector2(0, curve.Evaluate(lifetime / duration * 6f) * 12f);

        if(lifetime >= duration)
        {
            PoolManager.PoolDestroy(gameObject, 10f);
            lifetime = 0f;
        }
    }
}
