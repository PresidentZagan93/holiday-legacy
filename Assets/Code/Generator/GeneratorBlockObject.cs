using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorBlockObject : MonoBehaviour {

    public Generator.GeneratorBlock block = new Generator.GeneratorBlock(0, 0, 0);
    SpritePlayer spritePlayer;
    public GameObject wallBreakPrefab;

    private void Awake()
    {
        ObjectManager.Add(this);
    }

    private void OnDestroy()
    {
        ObjectManager.Remove(this);
    }

    public void Hurt()
    {
        if (!spritePlayer)
        {
            SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = 15;

            spritePlayer = gameObject.AddComponent<SpritePlayer>();

            SpritePlayer.SpriteAnimation newAnimation = new SpritePlayer.SpriteAnimation()
            {
                loop = false,
                name = "Hurt",
                speed = 1,
                sprites = new Sprite[] { ObjectManager.GetSprite("WallHit"), null }
            };
            spritePlayer.animations.Add(newAnimation);
        }

        spritePlayer.Play("Hurt", true);
    }
}
