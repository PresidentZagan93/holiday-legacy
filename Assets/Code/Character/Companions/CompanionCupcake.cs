using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionCupcake : MonoBehaviour {

    Character character;
    public GameObject cherryPrefab;
    public float dropRate;
    float nextDrop;
    ObjectSoundEmitter sound;

    private void Awake()
    {
        sound = GetComponent<ObjectSoundEmitter>();
        character = GetComponent<Character>();
        sound.CreateSource("Companion", AudioManager.AudioType.Health);
    }

    private void Update()
    {
        if(Time.time > nextDrop && character.Owner.Health.hp < character.Owner.Health.maxHp)
        {
            nextDrop = Time.time + dropRate;

            GameObject newCherry = Instantiate(cherryPrefab);
            newCherry.transform.position = transform.position;

            sound.PlaySound("Fart3", "Companion");
        }
    }
}
