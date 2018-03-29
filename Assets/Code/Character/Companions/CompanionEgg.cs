using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionEgg : MonoBehaviour {

    string companion;
    Character owner;
    float yVelocity;
    public float gravity = 3f;
    public float creationTime = 2f;
    float nextCreate;
    bool isCreating;
    SpritePlayer player;
    Transform shadow;
    AudioSource source;
    public AudioClip hatchSound;
    public AudioClip hatchingSound;

    private void Awake()
    {
        source = GetComponent<AudioSource>();
        AudioManager.Assign(source, AudioManager.AudioType.Pickups);
        shadow = transform.Find("Shadow");
        player = GetComponent<SpritePlayer>();

        source.clip = hatchingSound;
        source.loop = true;
        source.Play();
    }

    public void Initialize(string companion, Character owner)
    {
        transform.position = owner.transform.position;
        this.companion = companion;
        this.owner = owner;

        yVelocity = 1;
    }

    private void FixedUpdate()
    {
        if(!isCreating)
        {
            transform.position += Vector3.up * yVelocity;
            yVelocity -= Time.fixedDeltaTime * gravity;
            shadow.position -= Vector3.up * yVelocity;
        }

        if(yVelocity <= -1f && !isCreating)
        {
            nextCreate = Time.time + creationTime;
            isCreating = true;
            if(player) player.Play("Hatching");
            gravity = 0f;
        }

        if(Time.time > nextCreate && isCreating)
        {
            CreateCompanion();
            if (player) player.Play("Hatched");

            source.Stop();
            source.loop = false;
            source.clip = hatchSound;
            source.Play();
            enabled = false;
        }
    }

    void CreateCompanion()
    {
        Character companion = Instantiate(ObjectManager.GetPrefab(this.companion)).GetComponent<Character>();
        companion.transform.position = transform.position;
        companion.Owner = owner;
        companion.name = this.companion;

        owner.Inventory.companions.Add(companion);
    }
}
