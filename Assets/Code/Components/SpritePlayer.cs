using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpritePlayer : MonoBehaviour {

	[System.Serializable]
    public class SpriteAnimation
    {
        public string name = "";
        public Sprite[] sprites;
        public ulong speed = 1;
        public bool loop = true;
        public bool flipX = false;
    }

    public List<SpriteAnimation> animations = new List<SpriteAnimation>();
    public int fps = 10;
    public float fpsMultiplier = 1f;
    public string defaultAnimation = "";

    [HideInInspector]
    public CharacterArmor armor;
    Vector3 armorOffset;

    public SpriteRenderer spriteRenderer;
    Image image;
    float nextFrame;
    int animIndex;
    int spriteIndex;

    public string CurrentAnimation
    {
        get
        {
            return animations[animIndex].name;
        }
    }

    public Vector3 ArmorOffset
    {
        get
        {
            return new Vector3(armorOffset.x.RoundToInt(), armorOffset.y.RoundToInt());
        }
    }

    void Awake()
    {
        animIndex = -1;

        image = GetComponent<Image>();
        if(!spriteRenderer)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        Play(defaultAnimation);
    }

    void Update()
    {
        if (GameManager.Paused) return;
        if (frozen) return;

        if(Time.time > nextFrame && animIndex != -1)
        {
            if(armor)
            {
                if(armor.offsets.offsets.Count > animIndex)
                {
                    for (int i = 0; i < armor.offsets.offsets.Count; i++)
                    {
                        if(armor.offsets.offsets[i].name == animations[animIndex].name)
                        {
                            if(armor.offsets.offsets[i].frames.Count > spriteIndex)
                            {
                                armorOffset = armor.offsets.offsets[i].frames[spriteIndex];
                            }
                        }
                    }
                }
            }

            nextFrame = Time.time + 1f / (fps * fpsMultiplier * (float)animations[animIndex].speed);

            float xScale = transform.localScale.x;
            if (xScale > 0 && animations[animIndex].flipX) xScale *= -1f;
            else if(xScale < 0 && !animations[animIndex].flipX) xScale *= -1f;

            transform.localScale = new Vector3(xScale, transform.localScale.y, transform.localScale.z);

            if (spriteRenderer)
            {
                spriteRenderer.sprite = animations[animIndex].sprites[spriteIndex];
                //spriteRenderer.FixSpriteRenderer();
            }
            if (image)
            {
                image.sprite = animations[animIndex].sprites[spriteIndex];
                //image.FixImageRenderer();
            }

            spriteIndex++;
            if (spriteIndex == animations[animIndex].sprites.Length) spriteIndex = animations[animIndex].loop ? 0 : animations[animIndex].sprites.Length-1;
        }
    }

    bool frozen = false;
    public void Freeze(bool freeze)
    {
        frozen = freeze;
    }

    public void Play(string animation, bool force = false)
    {
        for(int i = 0; i < animations.Count;i++)
        {
            if(animations[i].name == animation && ((!force && animIndex != i) || force))
            {
                animIndex = i;
                spriteIndex = 0;
                break;
            }
        }
    }
}
