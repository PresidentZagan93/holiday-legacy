using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFirecracker : CharacterAnimations {

    public string runLeftAnim = "RunLeft";
    public string runRightAnim = "RunRight";

    protected override void Update()
    {
        if (!spritePlayer) return;
        if (Frozen) return;

        if (Time.time < nextAnimDelay)
        {
            return;
        }

        if (Character.isDead)
        {
            spritePlayer.Play(deadAnim);
            return;
        }

        if (CharacterLook)
        {
            if (CharacterLook.mode == CharacterLook.CharacterLookMode.Target)
            {
                Renderer.flipX = CharacterLook.lookDirection.x < 0f;
            }
            else if (CharacterLook.mode == CharacterLook.CharacterLookMode.Direction)
            {
                Renderer.flipX = CharacterLook.lookDirection.ToString().Contains("Left");
            }
            else if (CharacterLook.mode == CharacterLook.CharacterLookMode.Mouse)
            {
                Renderer.flipX = CharacterLook.lookDirection.x < 0f;
            }
        }

        if (CharacterMovement && CharacterMovement.IsMoving)
        {
            if (!Renderer.flipX)
            {
                if (CharacterMovement.MovingDirection.x < 0)
                {
                    spritePlayer.Play(CharacterMovement.Running ? runLeftAnim : walkLeftAnim);
                }
                else
                {
                    spritePlayer.Play(CharacterMovement.Running ? runRightAnim : walkRightAnim);
                }
            }
            else
            {
                if (CharacterMovement.MovingDirection.x < 0)
                {
                    spritePlayer.Play(CharacterMovement.Running ? runRightAnim : walkRightAnim);
                }
                else
                {
                    spritePlayer.Play(CharacterMovement.Running ? runLeftAnim : walkLeftAnim);
                }
            }
        }
        else
        {
            spritePlayer.Play(idleAnim);
        }
    }
}
