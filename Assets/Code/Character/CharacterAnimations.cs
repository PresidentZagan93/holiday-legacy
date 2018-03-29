using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimations : MonoBehaviour {
    
    public string walkLeftAnim = "WalkLeft";
    public string walkRightAnim = "WalkRight";
    public string idleAnim = "Idle";
    public string hurtAnim = "Hurt";
    public string deadAnim = "Dead";
    public SpritePlayer spritePlayer;
    SpriteRenderer spriteRenderer;
    CharacterLook look;
    CharacterMovement movement;
    Character character;
    protected float nextAnimDelay;

    void Awake()
    {
        character = GetComponent<Character>();
        look = GetComponent<CharacterLook>();
        movement = GetComponent<CharacterMovement>();

        if (!spritePlayer) spritePlayer = GetComponent<SpritePlayer>();
        spriteRenderer = spritePlayer.GetComponent<SpriteRenderer>();
    }

    public Character Character
    {
        get
        {
            return character;
        }
    }

    public SpriteRenderer Renderer
    {
        get
        {
            return spriteRenderer;
        }
    }


    public CharacterLook CharacterLook
    {
        get
        {
            return look;
        }
    }

    public CharacterMovement CharacterMovement
    {
        get
        {
            return movement;
        }
    }

    public bool Frozen
    {
        get
        {
            return frozen;
        }
    }

    public void Hurt()
    {
        spritePlayer.fpsMultiplier = 1f;
        spritePlayer.Play(hurtAnim, true);
        nextAnimDelay = Time.time + 0.3f;
    }

    bool frozen;
    public void Freeze(bool freeze)
    {
        frozen = freeze;
    }

    protected virtual void Update()
    {
        if (!spritePlayer) return;
        if (frozen) return;

        if (Time.time < nextAnimDelay)
        {
            return;
        }

        if (character.isDead)
        {
            spritePlayer.Play(deadAnim);
            return;
        }

        if (look)
        {
            if (look.mode == CharacterLook.CharacterLookMode.Target)
            {
                spriteRenderer.flipX = look.lookDirection.x < 0f;
            }
            else if (look.mode == CharacterLook.CharacterLookMode.Direction)
            {
                spriteRenderer.flipX = look.lookDirection.ToString().Contains("Left");
            }
            else if(look.mode == CharacterLook.CharacterLookMode.Mouse)
            {
                spriteRenderer.flipX = look.lookDirection.x < 0f;
            }
        }
        
        if (movement && movement.IsMoving)
        {
            if (!spriteRenderer.flipX)
            {
                if (movement.MovingDirection.x < 0)
                {
                    spritePlayer.Play(walkLeftAnim);
                }
                else
                {
                    spritePlayer.Play(walkRightAnim);
                }
            }
            else
            {
                if (movement.MovingDirection.x < 0)
                {
                    spritePlayer.Play(walkRightAnim);
                }
                else
                {
                    spritePlayer.Play(walkLeftAnim);
                }
            }
        }
        else
        {
            spritePlayer.Play(idleAnim);
        }
    }
}
