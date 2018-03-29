using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterLook : MonoBehaviour {
    
    public enum CharacterLookMode
    {
        Mouse,
        Direction,
        Target
    }

    //Global variables
    public CharacterLookMode mode = CharacterLookMode.Mouse;
    public Vector2 lookDirection;
    public float lerpness = 0f;
    public float reactionDelay = 0.5f;

    //Target variables
    public Transform target;
    public float maxTargetDistance = 200;
    public GameTeam lookAtTeam = GameTeam.Both;
    public AudioSource targetAcquired;

    //Direction variables
    public float direction = 0f;

    float nextCheck;
    LayerMask layerMask;
    ObjectSoundEmitter sound;
    CharacterMovement movement;
    Character character;

    bool lastTarget;

    void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        sound = GetComponent<ObjectSoundEmitter>();
        character = GetComponent<Character>();
        sound.CreateSource("Look", AudioManager.AudioType.Health);
    }

    private void Update()
    {
        if (!character.Process) return;

        if (GameManager.Paused) return;

        if (mode == CharacterLookMode.Mouse)
        {
            Vector2Int mousePosition = CursorManager.Cursors[0].worldPosition;
            lookDirection = mousePosition.ToVector() - (Vector2)transform.position;
        }
        if(mode == CharacterLookMode.Target)
        {
            if (Time.time > nextCheck)
            {
                nextCheck = Time.time + 0.5f;
                
                if (lookAtTeam == GameTeam.Good) layerMask = ObjectManager.GetLayerMask("HitPlayerAndCompanion");
                if (lookAtTeam == GameTeam.Evil) layerMask = ObjectManager.GetLayerMask("HitEnemy");

                target = null;

                Health newTarget = Helper.FindClosestHealth(transform, maxTargetDistance, lookAtTeam, layerMask);
                if (newTarget)
                {
                    target = newTarget.transform;
                }
            }
            if (target)
            {
                if(!lastTarget)
                {
                    lastTarget = true;
                }
                lookDirection = (target.position - transform.position);
            }
            else
            {
                lastTarget = false;
                if(movement)
                {
                    lookDirection = movement.MovingDirection * 32;
                }
                else
                {

                }
            }
        }
        if(mode == CharacterLookMode.Direction)
        {
            lookDirection = new Vector2(Mathf.Cos(direction * Mathf.Deg2Rad), Mathf.Sin(direction * Mathf.Deg2Rad)).normalized;
        }
    }
}
