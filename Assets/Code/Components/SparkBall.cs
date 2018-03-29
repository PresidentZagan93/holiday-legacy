using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;

public class SparkBall : MonoBehaviour {

    public int range = 64;
    public float sparkRate = 0.25f;
    public int damage = 1;
    public float width = 8f;
    public int maxHits = 4;
    public int shocksToMake = 4;

    float lineWidth = 8f;
    int hits = 0;

    public GameTeam teamToHit = GameTeam.Evil;
    public Texture laserTexture;

    float nextSpark;
    List<LineRenderer> lines = new List<LineRenderer>();
    List<Transform> hitPoint = new List<Transform>();
    List<Vector2> lastPoints = new List<Vector2>();
    List<Health> healthsFound = new List<Health>();

    bool onProjectile;
    private void Awake()
    {
        onProjectile = GetComponent<GunProjectile>();
    }

    private void Update()
    {
        RefreshMasks();

        if (Time.time > nextSpark && hits < maxHits)
        {
            nextSpark = Time.time + sparkRate;

            healthsFound.Clear();
            for (int i = 0; i < shocksToMake; i++)
            {
                Cast();
            }

            DealDamage();

            hits++;

            if(hits >= maxHits)
            {
                nextSpark = Time.time + 1.5f;
            }
        }
        if (hits >= maxHits && !onProjectile)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * 3f);
        }
        if (hits >= maxHits && projectile && Time.time > nextSpark)
        {
            Destroy(gameObject);
        }
        
        lineWidth = Mathf.Lerp(lineWidth, 0f, Time.deltaTime * 10f);

        for (int i = 0; i < lines.Count; i++)
        {
            lines[i].startColor = Color.white;
            lines[i].endColor = Color.white;

            lines[i].startWidth = lineWidth;
            lines[i].endWidth = lineWidth;
            
            if (hitPoint[i]) lastPoints[i] = hitPoint[i].position;

            Vector2 dir = (Vector3)lastPoints[i] - transform.position;
            int toMake = Mathf.RoundToInt(dir.magnitude / 16f);
            Vector3[] points = new Vector3[toMake];

            for (int l = 0; l < toMake; l++)
            {
                points[l] = Vector2.Lerp(transform.position, lastPoints[i], (float)l / (float)toMake);

                if (l == 0) points[l] = transform.position;
                else if (l == toMake - 1) points[l] = lastPoints[i];
                else points[l] += (Vector3)Random.insideUnitCircle * lineWidth * 0.5f * (toMake - l);

                points[l] = new Vector3(points[l].x, points[l].y, 0);
            }

            lines[i].positionCount = toMake;
            lines[i].SetPositions(points);
        }
    }

    void Cast()
    {
        Health closestHealth = null;
        float closestDist = float.MaxValue;

        var allHealths = ObjectManager.GetAllOfType<Health>();

        for (int i = 0; i < allHealths.Count; i++)
        {
            if (healthsFound.Contains(allHealths[i])) continue;

            Health healthHit = allHealths[i];
            if (healthHit.Character && healthHit.Character.isDead) continue;
            if (teamToHit != healthHit.team) continue;

            float dist = Vector2.Distance(healthHit.transform.position, transform.position);

            if (dist < range)
            {
                if (closestDist > dist)
                {
                    closestDist = dist;
                    closestHealth = healthHit;
                }
            }
        }

        healthsFound.Add(closestHealth);
    }

    LayerMask laserMask;

    public void RefreshMasks()
    {
        if (teamToHit == GameTeam.Evil)
        {
            laserMask = ObjectManager.GetLayerMask("HitEnemy");
        }
        else
        {
            laserMask = ObjectManager.GetLayerMask("HitPlayerAndCompanion");
        }
    }

    void AddLine(Transform end)
    {
        LineRenderer newLine = new GameObject("Line" + lines.Count).AddComponent<LineRenderer>();
        newLine.transform.SetParent(transform);
        newLine.material = ObjectManager.GetMaterial("Laser");
        newLine.material.SetTexture("_MainTex", laserTexture);

        lines.Add(newLine);
        hitPoint.Add(end);
        lastPoints.Add(end.position);
    }

    Health shooterHealth;
    Gun gun;
    GunProjectile projectile;

    public void Initialize(GunProjectile projectile)
    {
        this.projectile = projectile;

        gun = projectile.Gun;
        shooterHealth = projectile.info.health;
    }
    
    bool Hit(Health health)
    {
        if (!health) return false;

        if(gun) health.AddEffects(gun.effects, shooterHealth);
        health.Damage(damage, shooterHealth);

        return true;
    }

    public GameObject hitEffect;

    void HitEffect(Color color, Vector2 pos, Quaternion rot)
    {
        GameObject hitEffect = PoolManager.PoolInstantiate(this.hitEffect, pos, rot);
        if (!hitEffect) return;

        ParticleSystem.MainModule main = hitEffect.GetComponent<ParticleSystem>().main;
        main.startColor = color;
    }

    void DealDamage()
    {
        for (int i = 0; i < healthsFound.Count; i++)
        {
            if (healthsFound[i])
            {
                Vector2 enemyDirection = (healthsFound[i].transform.position + (Vector3)healthsFound[i].effectsOffset) - transform.position;
                RaycastHit2D hit = Physics2D.Raycast((Vector2)transform.position, enemyDirection, Mathf.Infinity, laserMask.value);
                if (hit.transform)
                {
                    if (hit.transform == healthsFound[i].transform)
                    {
                        //Can see

                        HitEffect(Color.red, healthsFound[i].transform.position, Random.rotationUniform);
                        AddLine(healthsFound[i].transform);

                        Hit(healthsFound[i]);
                    }
                }
            }
        }
    }

    private void OnEnable()
    {
        transform.localScale = Vector3.one;
        hits = 0;
        lineWidth = width;

        LineRenderer[] linesFound = transform.GetComponentsInChildren<LineRenderer>();
        for (int i = 0; i < linesFound.Length; i++)
        {
            Destroy(linesFound[i].gameObject);
        }

        lines.Clear();
        hitPoint.Clear();
        lastPoints.Clear();
    }
}
