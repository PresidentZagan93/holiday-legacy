using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpriteMimic : MonoBehaviour {

    public SpriteRenderer originalSpriteRenderer;
    SpriteRenderer spriteRenderer;

    public bool invertY = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        spriteRenderer.sprite = originalSpriteRenderer.sprite;
        spriteRenderer.flipX = originalSpriteRenderer.flipX;
        spriteRenderer.flipY = invertY ? !originalSpriteRenderer.flipY : originalSpriteRenderer.flipY;
    }
}
