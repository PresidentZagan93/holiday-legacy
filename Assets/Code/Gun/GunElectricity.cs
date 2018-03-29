using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GunElectricity : GunProjectile {
    
    float lineWidth;
    
    LineRenderer line;
    List<LineRenderer> lines = new List<LineRenderer>();
    List<Transform> hitPoint = new List<Transform>();
    List<Vector2> lastPoints = new List<Vector2>();

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 0;
        line.startWidth = 0;
        line.endWidth = 0;
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        healthsFound.Clear();
        lines.Clear();
        hitPoint.Clear();
        lastPoints.Clear();

        LineRenderer[] linesFound = transform.GetComponentsInChildren<LineRenderer>();
        for (int i = 0; i < linesFound.Length; i++)
        {
            if (linesFound[i].transform != transform)
            {
                Destroy(linesFound[i].gameObject);
            }
        }
    }

    public override void Initialize(GunShooter shooter, int shotIndex)
    {
        base.Initialize(shooter, shotIndex);

        lineWidth = Gun.scale;
        line.material.SetTexture("_MainTex", Gun.laserTexture);
        
        for(int i = 0; i < Gun.shots;i++)
        {
            Cast();
        }

        DealDamage();
    }

    void AddLine(Transform end)
    {
        LineRenderer newLine = new GameObject("Line"+ lines.Count).AddComponent<LineRenderer>();
        newLine.transform.SetParent(transform);
        newLine.material = line.material;

        lines.Add(newLine);
        hitPoint.Add(end);
        lastPoints.Add(end.position);
    }

    float AngleFromVector(Vector2 vec)
    {
        return (Mathf.Atan2(vec.x, vec.y) / Mathf.PI) * 180;
    }

    List<Health> healthsFound = new List<Health>();

    void Cast()
    {
        Health closestHealth = null;
        float closestDist = float.MaxValue;

        var allHealths = ObjectManager.GetAllOfType<Health>();

        for (int i = 0; i < allHealths.Count;i++)
        {
            if (healthsFound.Contains(allHealths[i])) continue;

            Health healthHit = allHealths[i];
            if (healthHit.Character && healthHit.Character.isDead) continue;
            if (healthHit.transform.root == Shooter.transform.root) continue;
            if (Team == healthHit.team) continue;
            
            float dist = Vector2.Distance(healthHit.transform.position, Shooter.transform.position);

            if (dist < Gun.speed)
            {
                //Raycast to see if the spark can see the health
                Vector2 enemyDirection = healthHit.transform.position - Shooter.transform.position;
                float enemyAngle = AngleFromVector(enemyDirection.normalized);
                float lookAngle = AngleFromVector(GetDirection);
                
                if(Mathf.Abs(lookAngle - enemyAngle) < Gun.recoil)
                {
                    if (closestDist > dist)
                    {
                        closestDist = dist;
                        closestHealth = healthHit;
                    }
                }
            }
        }

        if(closestHealth)
        {
            healthsFound.Add(closestHealth);
        }
    }
    
    void DealDamage()
    {
        for (int i = 0; i < healthsFound.Count; i++)
        {
            if(healthsFound[i])
            {
                Vector2 enemyDirection = (healthsFound[i].transform.position + (Vector3)healthsFound[i].effectsOffset) - Shooter.transform.position;
                RaycastHit2D hit = Physics2D.Raycast((Vector2)Shooter.transform.position + GetDirection * 10f, enemyDirection, Mathf.Infinity, LaserMask);

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

    void Update()
    {
        lineWidth = Mathf.Lerp(lineWidth, 0f, time);
        
        for (int i = 0; i < lines.Count;i++)
        {
	        lines[i].startColor = Color.white;
	        lines[i].endColor = Color.white;
	        
	        lines[i].startWidth = lineWidth;
	        lines[i].endWidth = lineWidth;

            if (Shooter)
            {
                if (hitPoint[i]) lastPoints[i] = hitPoint[i].position;

                Vector2 dir = (Vector3)lastPoints[i] - Shooter.transform.position;
                int toMake = Mathf.RoundToInt(dir.magnitude / 16f);
                Vector3[] points = new Vector3[toMake];
                
                for (int l = 0; l < toMake; l++)
                {
                    points[l] = Vector2.Lerp(Shooter.transform.position, lastPoints[i], (float)l / (float)toMake);

                    if (l == 0) points[l] = Shooter.transform.position;
                    else if (l == toMake - 1) points[l] = lastPoints[i];
                    else points[l] += (Vector3)Random.insideUnitCircle * lineWidth * 0.5f * (toMake - l);

                    points[l] = new Vector3(points[l].x, points[l].y, 0);
                }

	        	lines[i].positionCount = toMake;
                lines[i].SetPositions(points);
            }
        }
        if(!isAlive)
        {
            PoolManager.PoolDestroy(gameObject);
        }
    }
}
