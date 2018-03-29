using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropAlternative : MonoBehaviour {

    public List<Sprite> sprites = new List<Sprite>();

    public SpriteRenderer spriteRenderer;
    public SpriteRenderer shadowRenderer;
    Sprite sprite;

    private void Awake()
    {
        sprite = sprites[Random.Range(0, sprites.Count)];
        if (spriteRenderer) spriteRenderer.sprite = sprite;
        if (shadowRenderer) shadowRenderer.sprite = sprite;
    }
}
