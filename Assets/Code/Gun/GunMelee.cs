using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GunMelee : GunProjectile {

    new Rigidbody2D rigidbody;
    Vector3 originalSize;
    List<Health> hits = new List<Health>();
    SpritePlayer spritePlayer;
    Collider2D[] colliders;

    void Awake()
    {
        originalSize = transform.localScale;
        rigidbody = GetComponent<Rigidbody2D>();
        spritePlayer = GetComponentInChildren<SpritePlayer>();
        rigidbody.gravityScale = 0f;
        colliders = GetComponentsInChildren<Collider2D>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        hits.Clear();
    }

    void FixedUpdate()
    {
        if (!isAlive)
        {
            rigidbody.velocity = Vector2.zero;
            PoolManager.PoolDestroy(gameObject);
        }
        else
        {
            transform.localScale = originalSize * Multipliers.sizeMultiplier * Gun.scale;
            rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, Vector2.zero, Time.fixedDeltaTime * 15f);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        Collided(col.transform);
    }

    void SetColors()
    {
        if (!Gun) return;

        SpriteRenderer[] rends = spritePlayer.GetComponentsInChildren<SpriteRenderer>();
        for(int i = 0; i < rends.Length;i++)
        {
            rends[i].color = Gun.Color;
        }
    }

	public override void Initialize(GunShooter gunShooter, int shotIndex)
    {
        spritePlayer.Play("Melee", true);

        base.Initialize(gunShooter, shotIndex);

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].gameObject.SetLayer(gunShooter.Team == GameTeam.Good ? "PlayerBullet" : "EnemyBullet");
        }
        if (rigidbody) rigidbody.velocity = Vector2.zero;

        Vector3 dir = GetDirection;
        transform.LookAt2D(transform.position + (Vector3)dir);
        
        rigidbody.freezeRotation = false;
        rigidbody.velocity = dir * 100f * Multipliers.speedMultiplier * 10f;

        SetColors();
    }

    void Collided(Transform t)
    {
        if (!Shooter) return;
        if (!isAlive) return;

        bool shouldHit = Random.Range(0, 100) < Gun.successChance;
        bool meleeBlocked = false;
        Health health = t.GetComponentInParent<Health>();
        if(health && health.Armor)
        {
            meleeBlocked = health.Armor.Block(transform.position, true);
        }

        if (health && !hits.Contains(health) && shouldHit && !meleeBlocked)
        {
            hits.Add(health);
            Hit(health);

            HitEffect(Color.red, transform.position, Random.rotationUniform);
        }
        else
        {
            //Helper.ChangeColor(hitEffect, Gun.color);
        }
    }
}
