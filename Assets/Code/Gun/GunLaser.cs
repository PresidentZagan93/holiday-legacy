using UnityEngine;
using System.Collections;

public class GunLaser : GunProjectile {

    LineRenderer line;
    Vector3 hitPoint;
    float hitDistance;
    Vector3 lineStart;

    float nextDamage;
    float lineWidth;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.endWidth = 0f;
        line.startWidth = 0f;
        lineWidth = 0f;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        line.endWidth = 1f;
        line.startWidth = 1f;
        nextDamage = 0f;
        lineWidth = 0f;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        line.endWidth = 0f;
        line.startWidth = 0f;
        lineWidth = 0f;
    }

    public override void Initialize(GunShooter shooter, int shotIndex)
    {
        lineWidth = 0f;
        hitPoint = transform.position;

        base.Initialize(shooter, shotIndex);

        line.material.SetTexture("_MainTex", Gun.laserTexture);
        
        line.positionCount = 0;
        
        line.startColor = Color.white;
        line.endColor = Color.white;
    }

    void Cast(Vector3 from, Vector3 direction, bool doDamage)
    {
        lineStart = from;
        RaycastHit2D[] hits = Physics2D.CircleCastAll(from + direction * 10f, lineWidth * 6f, direction, Mathf.Infinity, Mask);

        if (hits.Length == 0) return;

        hitPoint = hits[0].point;
        hitDistance = hits[0].distance;

        Health healthHit = hits[0].collider.transform.GetComponentInParent<Health>();
        if (!healthHit || !doDamage)
        {
            Recursion(hits[0].point, hits[0].normal);

            HitEffect(Gun.color, hits[0].point, Random.rotationUniform);
            //first hit isnt a health object
            return;
        }
        else
        {
            bool reflect = healthHit.Armor && healthHit.Armor.Reflect(hits[0].point);
            if(reflect)
            {
                AssignShooter(healthHit.GunShooter);
                ReflectLaser(transform.position);
                HitEffect(Gun.color, hits[0].point, Random.rotationUniform);
            }
            else
            {
                Recursion(hits[0].point, hits[0].normal);
                HitEffect(Color.red, hits[0].point, Random.rotationUniform);

                Hit(healthHit);
            }
        }
    }

    public override void Splash()
    {
        if (!isAlive) return;

        if (Gun.splash)
        {
            SplashDamage(Gun.splashDamage, Gun.splashRange, Mask, hitPoint);
        }
        else if (Shooter && Shooter.Inventory)
        {
            bool explode = Shooter.Inventory.Contains("Explosive Ammo");
            if (explode)
            {
                float explosionRange = Shooter.Inventory.GetMultiplier("ExplosionRange");

                SplashDamage(1, explosionRange, Mask, hitPoint);
            }
        }
    }

    void Flicker()
    {
	    if (!Shooter) return;
	    //MakeHit(false);
        
        int toMake = Mathf.Clamp(Mathf.RoundToInt(hitDistance / 32f), 4, 24);
        Vector3[] points = new Vector3[toMake];

        for (int l = 0; l < toMake; l++)
        {
            points[l] = Vector2.Lerp(lineStart, hitPoint, l * 1f / toMake * 1f);

            if (Random.value < 0.5f) continue;

            if (l == 0) points[l] = lineStart;
            else if (l == toMake - 1) points[l] = hitPoint;
            else
            {
                float width = Mathf.Pow(l + 1, -2) * 12f;
                points[l] += (Vector3)Random.insideUnitCircle * line.widthMultiplier * width;
            }

            points[l] = new Vector3(points[l].x, points[l].y, 0);
        }

        line.positionCount = toMake;
        line.SetPositions(points);
    }

    void Update()
    {
        if (GameManager.Paused)
        {
            nextDamage += Time.deltaTime;
        }
        else
        {
            line.widthMultiplier = lineWidth;
        }

        if (Gun.type == GunType.ConstantLaser) Cast(transform.position, GetDirection, false);
        Flicker();

        if (isAlive)
        {
            if (Time.time > nextDamage)
            {
                lineWidth = Gun.scale * Multipliers.sizeMultiplier * 0.1f;
                nextDamage = Time.time + Gun.burstRate;

                Cast(transform.position, GetDirection, true);
                Splash();
            }
        }
        else
        {
            if (Gun.type == GunType.ConstantLaser)
            {
                PoolManager.PoolDestroy(gameObject);
            }
            else
            {
                if (lineWidth < 0.05f)
                {
                    PoolManager.PoolDestroy(gameObject);
                }
            }
        }
        lineWidth = Mathf.Lerp(lineWidth, 0f, Time.deltaTime * 10f);
    }
}
