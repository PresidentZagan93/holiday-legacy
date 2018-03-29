using UnityEngine;
using System.Collections.Generic;

public class GunBullet : GunProjectile
{
    new Rigidbody2D rigidbody;
    Vector3 originalSize;
    int bounces;
    bool killedParticles;
    ParticleSystem[] parts;
    Vector2 initialDirection;
    float rotationSpeed;

    Transform shadow;

    List<Health> hits = new List<Health>();

    void Awake()
    {
        parts = GetComponentsInChildren<ParticleSystem>();
        originalSize = transform.localScale;
        rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.gravityScale = 0f;

        shadow = transform.Find("Shadow");
    }

    bool alreadyHit = false;
    void FixedUpdate()
    {
        //TestingManager.DrawBox(transform, transform.localScale * 4f, Color.white);

        if (!Gun) return;

        if (!isAlive)
        {
            if (!killedParticles)
            {
                for (int i = 0; i < parts.Length; i++)
                {
                    ParticleSystem.EmissionModule emit = parts[i].emission;
                    emit.enabled = false;
                }

                killedParticles = true;
            }

            rigidbody.velocity = Vector2.zero;
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * 24f);
            if (transform.localScale.magnitude < 0.1f)
            {
                PoolManager.PoolDestroy(gameObject);
            }
        }
        else
        {
            if (Gun.projectileRotation == ProjectileRotationMode.Velocity)
            {
                if (Gun.projectileArcs)
                {
                    transform.LookAt2D(transform.position + (Vector3)rigidbody.velocity * 100);
                }
                else
                {
                    transform.LookAt2D(transform.position + (Vector3)initialDirection);
                }
            }
            else if (Gun.projectileRotation == ProjectileRotationMode.Random)
            {
                transform.Rotate(Vector3.forward, Time.fixedDeltaTime * 100 * rotationSpeed * Gun.speed);
            }

            bool tooHigh = false;
            if (Gun.projectileArcs)
            {
                rigidbody.AddForce(Vector2.down * Gun.projectileArcGravity);
                rigidbody.velocity = new Vector2(rigidbody.velocity.x, rigidbody.velocity.y * 0.995f);
                tooHigh = lifeTime > 0.15f && lifeTime < Gun.lifeTime - 0.15f;
                if (shadow) shadow.position = Vector2.one * 55555;
            }
            else
            {
                if (shadow)
                {
                    shadow.position = transform.position + Vector3.down * 3f;
                    shadow.rotation = transform.rotation;
                }
            }

            #region Raycast

            if (!tooHigh)
            {
                Vector2 raycastPosition = transform.position + transform.forward.normalized * 2;
                Vector2 raycastDirection = rigidbody.velocity;
                if(Gun.projectileArcs)
                {
                    raycastDirection = initialDirection;
                    raycastPosition = new Vector2(transform.position.x, transform.position.y - (rigidbody.velocity.y * Time.fixedDeltaTime));
                }
                RaycastHit2D hit = Physics2D.CircleCast(raycastPosition, Gun.scale * 4f * Gun.sizeOverLifetime.Evaluate(lifeTime / Gun.lifeTime), raycastDirection, 8f, Mask);
                if (hit.collider)
                {
                    bool success = false;
                    bool forceBounce = false;

                    Health health = hit.collider.GetComponentInParent<Health>();

                    if (health && !hits.Contains(health))
                    {
                        if (health.Armor && health.Armor.Block(hit.point, false))
                        {

                        }
                        else
                        {
                            bool reflect = health.Armor && health.Armor.Reflect(hit.point);
                            if (reflect)
                            {
                                AssignShooter(health.GunShooter);
                                forceBounce = true;
                            }
                            else
                            {
                                HitEffect(Color.red);
                                alreadyHit = true;

                                success = Hit(health, !Gun.splash);
                            }
                        }

                        hits.Add(health);
                    }
                    else
                    {
                        if (Team == GameTeam.Evil)
                        {
                            GunMelee meleeSwing = hit.collider.GetComponentInParent<GunMelee>();
                            if (meleeSwing)
                            {
                                bool meleeSuccess = false;
                                if (meleeSwing.Gun.meleeReflects)
                                {
                                    meleeSuccess = true;
                                    Vector2 inDir = Vector2.Reflect(rigidbody.velocity, hit.normal);
                                    rigidbody.velocity = inDir;

                                    AssignShooter(meleeSwing.Shooter);
                                }
                                else if (meleeSwing.Gun.meleeBreaks)
                                {
                                    meleeSuccess = true;
                                    Die();
                                }

                                if (meleeSuccess)
                                {
                                    //got reflected by melee
                                    if (!alreadyHit)
                                    {
                                        HitEffect();
                                        alreadyHit = true;
                                    }
                                }

                                return;
                            }
                        }
                        success = false;
                    }

                    int bouncesToMake = Gun.maxBounces;
                    if (Shooter && Shooter.Inventory && Shooter.Inventory.GetState("ProjectileForceBounce"))
                    {
                        int extraBounces = Shooter.Inventory.GetMultiplier("GunBulletBouncesMultiplier").RoundToInt();
                        bouncesToMake += extraBounces;
                    }
                    bool bounce = bounces < bouncesToMake && !success;
                    if (bounce || forceBounce)
                    {
                        bounces++;

                        Vector2 inDir = Vector2.Reflect(rigidbody.velocity, hit.normal);
                        rigidbody.velocity = inDir;

                        if (Gun.projectileRotation == ProjectileRotationMode.Direction)
                        {
                            transform.LookAt2D(transform.position + (Vector3)inDir);
                        }

                        if (bounces == Gun.maxBounces)
                        {
                            Die();
                            return;
                        }

                        //not last bounce
                        if (!alreadyHit)
                        {
                            HitEffect();
                            alreadyHit = true;
                        }
                    }
                    else if (bounces >= Gun.maxBounces || success)
                    {
                        //last bounce
                        if (!alreadyHit)
                        {
                            HitEffect();
                            alreadyHit = true;
                        }

                        Die();
                        return;
                    }
                }
            }
            #endregion

            float sof = 1f;
            if (Gun.sizeOverLifetime.keys.Length > 0) sof = Gun.sizeOverLifetime.Evaluate(lifeTime / Gun.lifeTime);
            transform.localScale = originalSize * Multipliers.sizeMultiplier * Gun.scale * sof;

            if (Gun.slowdown > 0f)
            {
                rigidbody.velocity = Vector2.Lerp(rigidbody.velocity, Vector2.zero, Time.deltaTime * Gun.slowdown);
            }
        }
    }

    void Die()
    {
        Recursion(transform.position - (Vector3)rigidbody.velocity.normalized * 8f, Vector2.zero);
        ForceDie();
    }

    public override void Initialize(GunShooter gunShooter, int id)
    {
        base.Initialize(gunShooter, id);

        rigidbody.gravityScale = 0f;
        for (int i = 0; i < parts.Length; i++)
        {
            ParticleSystem.EmissionModule emit = parts[i].emission;
            emit.enabled = true;
        }

        initialDirection = GetDirection;
        if (rigidbody) rigidbody.velocity = Vector2.zero;
        if (Gun.projectileRotation == ProjectileRotationMode.Direction) transform.LookAt2D((Vector2)transform.position + initialDirection);

        rotationSpeed = Random.Range(-1f, 1f);

        Vector2 vel = initialDirection * Gun.speed * info.multipliers.speedMultiplier * 10f;
        if (Gun.projectileArcs)
        {
            vel += Vector2.up * Gun.projectileArcUpwardsKick;
        }

        rigidbody.freezeRotation = Gun.projectileRotation == ProjectileRotationMode.Off;
        rigidbody.velocity = vel;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        alreadyHit = false;
        hits.Clear();
        if (shadow)
        {
            shadow.position = transform.position + Vector3.down * 3f;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        transform.localScale = originalSize;
        bounces = 0;
        killedParticles = false;
        if (shadow)
        {
            shadow.position = transform.position + Vector3.down * 3f;
        }
    }
}
