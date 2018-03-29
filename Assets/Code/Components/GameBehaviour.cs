using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBehaviour : MonoBehaviour {
    
	public Health Health
    {
        get
        {
            return health;
        }
    }

    public GameTeam Team
    {
        get
        {
            if(!health)
            {
                return GameTeam.Both;
            }
            return health.team;
        }
    }

    public Character Character
    {
        get
        {
            return character;
        }
    }

    public GunShooter GunShooter
    {
        get
        {
            return gunShooter;
        }
    }

    public CharacterLook Look
    {
        get
        {
            return characterLook;
        }
    }

    public Inventory Inventory
    {
        get
        {
            return inventory;
        }
    }

    public CharacterMovement Movement
    {
        get
        {
            return characterMovement;
        }
    }

    public CharacterArmor Armor
    {
        get
        {
            return armor;
        }
    }

    public SpritePlayer SpritePlayer
    {
        get
        {
            return spritePlayer;
        }
    }

    public ObjectSoundEmitter Sound
    {
        get
        {
            return sound;
        }
    }

    public ObjectRoomChecker RoomChecker
    {
        get
        {
            return roomChecker;
        }
    }

    public CharacterPickupMaster PickupMaster
    {
        get
        {
            return pickupMaster;
        }
    }

    public CharacterAnimations Animations
    {
        get
        {
            return characterAnimations;
        }
    }

    public Generator Generator
    {
        get
        {
            if(!generator)
            {
                generator = Generator.singleton;
            }
            return generator;
        }
    }

    Health health;
    Character character;
    GunShooter gunShooter;
    CharacterLook characterLook;
    CharacterMovement characterMovement;
    CharacterPickupMaster pickupMaster;
    CharacterArmor armor;
    ObjectRoomChecker roomChecker;
    ObjectSoundEmitter sound;
    SpritePlayer spritePlayer;
    Generator generator;
    CharacterAnimations characterAnimations;
    Inventory inventory;

    public virtual void Awake()
    {
        health = GetComponent<Health>();
        inventory = GetComponent<Inventory>();
        character = GetComponent<Character>();
        gunShooter = GetComponent<GunShooter>();
        characterLook = GetComponent<CharacterLook>();
        characterMovement = GetComponent<CharacterMovement>();
        pickupMaster = GetComponent<CharacterPickupMaster>();
        armor = GetComponent<CharacterArmor>();
        roomChecker = GetComponent<ObjectRoomChecker>();
        sound = GetComponent<ObjectSoundEmitter>();
        spritePlayer = GetComponentInChildren<SpritePlayer>();
        characterAnimations = GetComponent<CharacterAnimations>();
    }
}
