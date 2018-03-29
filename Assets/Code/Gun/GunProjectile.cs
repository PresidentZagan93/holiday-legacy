using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Data;

public class GunProjectile : MonoBehaviour {

    [System.Serializable]
    public class GunProjectileInfo
    {
        public GunMultipliers multipliers;
        public Vector2 direction;
        public Health health;
        public Gun gun;
        public GunShooter shooter;
        public int shotIndex;
        public int recursionIndex = 0;
        public int recursionRotation = 0;
        public GameTeam team;
    }

    public bool isAlive;
    public float lifeTime;
    public float time = 0f;
    public GameObject splashEffect;
    public GunProjectileInfo info = new GunProjectileInfo();

    TrailRenderer[] trails = null;
    LayerMask projectileMask;
    LayerMask laserMask;
    public GameObject deathPrefab;

    public int Mask
    {
        get
        {
            return projectileMask.value;
        }
    }

    public int LaserMask
    {
        get
        {
            return laserMask.value;
        }
    }

    public Gun Gun
    {
        get
        {
            return info.gun;
        }
    }

    public GunShooter Shooter
    {
        get
        {
            return info.shooter;
        }
    }

    public GameTeam Team
    {
        get
        {
            return info.team;
        }
    }

    public GunMultipliers Multipliers
    {
        get
        {
            return info.multipliers;
        }
    }

    public Vector2 GetDirection
	{
		get
		{
            if (Gun.type == GunType.Electricity) return info.direction.normalized;
            
			float minRecoil = 0f;
			int shotsToFire = Gun.shots;
			
			if(Shooter && Shooter.Inventory)
			{
				shotsToFire = Mathf.RoundToInt(shotsToFire * Shooter.Inventory.GetMultiplier("GunShotsMultiplier"));
				bool hasBacteria = Shooter.Inventory.Contains("Bacteria");
				if (hasBacteria)
				{
					minRecoil = 2f;
				}
			}
			
			Vector2 dir = info.direction.normalized;
            bool forceRecursion = Shooter.Inventory ? Shooter.Inventory.HasQuality("GunForceRecursion") : false;
            if (Gun.evenRecoil || ((Gun.recursion.enabled || forceRecursion) && info.recursionIndex > 0))
            {
                if (Gun.recursion.enabled || forceRecursion)
                {
                    int instances = Gun.recursion.instancesPerRecursion;
                    if (forceRecursion) instances = Shooter.Inventory.GetMultiplier("GunRecursionsInstances").RoundToInt();
                    
                    float rot = (360f / instances) * info.shotIndex;
                    dir = dir.normalized.Rotate(rot + info.recursionRotation);
                }
                else
                {
                    float mp = (info.shotIndex - shotsToFire / 2f);
                    if (Gun.shots % 2 == 0) mp += 0.5f;
                    dir = dir.normalized.Rotate(mp * Gun.recoil * 3f);
                }
			}
			else
			{
                dir = (dir * 5f) + (Random.insideUnitCircle * Random.Range(-(Gun.recoil + minRecoil), (minRecoil + Gun.recoil)) * info.multipliers.recoilMultiplier);
				dir.Normalize();
			}
			
			return dir;
		}
	}

    public void AssignShooter(GunShooter shooter)
    {
        info.team = shooter.Team;
        RefreshMasks();

        lifeTime = 0f;
        info.shooter = shooter;
        info.direction = shooter.direction;
        info.gun = shooter.guns[shooter.index].gun;
        info.health = shooter.GetComponent<Health>();

        info.multipliers = shooter.multipliers;
        if (info.multipliers == null) info.multipliers = new GunMultipliers();
    }

    public void RefreshMasks()
    {
        if (info.team == GameTeam.Evil)
        {
            gameObject.layer = LayerMask.NameToLayer("EnemyBullet");
            projectileMask = ObjectManager.GetLayerMask("HitPlayerAndCompanionAndBullet");
            laserMask = ObjectManager.GetLayerMask("HitPlayerAndCompanion");
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("PlayerBullet");
            projectileMask = ObjectManager.GetLayerMask("HitEnemyAndBullet");
            laserMask = ObjectManager.GetLayerMask("HitEnemy");
        }
    }
	
	public void SplashDamage(int damage, float range, LayerMask layerMask, Vector3 position)
	{
		if(splashEffect)
		{
			PoolManager.PoolInstantiate(splashEffect, position, Random.rotation);
		}
		
		List<Health> healthsInRange = ObjectManager.GetAllOfType<Health>();
        for (int i = 0; i < healthsInRange.Count;i++)
		{
            if(!Gun.selfHarm && healthsInRange[i].team == info.team)
            {
                continue;
            }

			float dist = 0f;
            float resistExplosion = healthsInRange[i].Inventory ? healthsInRange[i].Inventory.GetMultiplier("IgnoreExplosion") : 0;
            if (Random.Range(0, 100) < resistExplosion) continue;

            dist = Vector2.Distance(position, healthsInRange[i].transform.position);
			if (dist < range && Shooter)
			{
				healthsInRange[i].AddEffects(Gun.effects, info.health);
				if (damage > 0)
				{
					healthsInRange[i].Damage(damage, info.health);
				}
			}
		}
	}

    public bool Hit(Health health, bool applyEffects = true)
    {
        int damage = Mathf.RoundToInt(Gun.damage * info.multipliers.damageMultiplier);
        return Hit(health, damage, applyEffects);
    }

    public bool Hit(Health health, int damage, bool applyEffects)
    {
        if (!health) return false;
        if (!Gun.selfHarm && health.team == info.team)
        {
            return false;
        }
        
        if (applyEffects) health.AddEffects(Gun.effects, info.health);
        health.Damage(damage, info.health);

        return true;
    }

    void LateUpdate()
    {
        if (!Gun) return;
        if (!isAlive) time = 1f;
        if (GameManager.Paused && isAlive) return;

        lifeTime += Time.deltaTime;
        time = (float)lifeTime / Gun.lifeTime;

        if (lifeTime >= Gun.lifeTime && isAlive)
		{
            //the moment it died
            if (deathPrefab)
            {
                GameObject deathPrefab = Instantiate(this.deathPrefab, transform.position, Quaternion.identity);
                
                SparkBall sparkBall = deathPrefab.GetComponent<SparkBall>();
                if (sparkBall)
                {
                    sparkBall.Initialize(this);
                }
            }

            isAlive = false;
            Splash();
        }
	}

    public void HitEffect()
    {
        HitEffect(Color.white);
    }

    public void HitEffect(Vector2 pos, Quaternion rot)
    {
        HitEffect(Color.white, pos, rot);
    }

    public void HitEffect(Color color)
    {
        HitEffect(color, transform.position, Random.rotation);
    }

    public void HitEffect(Color color, Vector2 pos, Quaternion rot)
    {
        GameObject hitEffect = PoolManager.PoolInstantiate(Gun.hitEffect, pos, rot);
        if (!hitEffect) return;

        ParticleSystem.MainModule main = hitEffect.GetComponent<ParticleSystem>().main;
        main.startColor = color;
    }

    public void ForceDie()
    {
        lifeTime = Gun.lifeTime;
    }

    public void ReflectLaser(Vector2 position)
    {
        int maxRecursions = 3;

        if (info.recursionIndex < maxRecursions)
        {
            Shooter.PlayShootSound();

            int recursionRotation = Random.Range(0, 360);
            int instances = 1;

            for (int i = 0; i < instances; i++)
            {
                GameObject newBullet = PoolManager.PoolInstantiate(Gun.GetProjectile(), position, Quaternion.identity);
                GunProjectile gunProjectile = newBullet.GetComponent<GunProjectile>();
                gunProjectile.Initialize(Shooter, i);
                gunProjectile.SetRecursion(info.recursionIndex + 1, recursionRotation);
            }
        }
    }

    public void Recursion(Vector2 position, Vector2 normal)
    {
        bool forceRecursion = Shooter && Shooter.Inventory ? Shooter.Inventory.HasQuality("GunForceRecursion") : false;
        if (Gun.recursion.enabled || forceRecursion)
        {
            int maxRecursions = Gun.recursion.maxRecursions;
            if (forceRecursion) maxRecursions = Shooter.Inventory.GetMultiplier("GunRecursionsMax").RoundToInt();

            if (info.recursionIndex < maxRecursions)
            {
                Shooter.PlayShootSound();

                int recursionRotation = Random.Range(0, 360);
                int instances = Gun.recursion.instancesPerRecursion;
                if (forceRecursion) instances = Shooter.Inventory.GetMultiplier("GunRecursionsInstances").RoundToInt();

                for (int i = 0; i < instances; i++)
                {
                    GameObject newBullet = PoolManager.PoolInstantiate(Gun.GetProjectile(), position, Quaternion.identity);
                    GunProjectile gunProjectile = newBullet.GetComponent<GunProjectile>();
                    gunProjectile.Initialize(Shooter, i);
                    gunProjectile.SetRecursion(info.recursionIndex + 1, recursionRotation);
                }
            }
        }
    }

    public void SetRecursion(int start, int rotation)
    {
        info.recursionIndex = start;
        info.recursionRotation = rotation;
    }
	
	public virtual void Initialize(GunShooter shooter, int shotIndex)
    {
        lifeTime = 0f;

        info.shotIndex = shotIndex;

        AssignShooter(shooter);

        SparkBall sparkBall = GetComponent<SparkBall>();
        if(sparkBall)
        {
            sparkBall.Initialize(this);
        }
    }

    public virtual void Splash()
    {
        if (Gun.splash)
        {
            SplashDamage(Gun.splashDamage, Gun.splashRange, Mask, transform.position);
        }
        else if (Shooter && Shooter.Inventory)
        {
            bool explode = Shooter.Inventory.GetState("ExplosiveAmmo");
            if (explode)
            {
                float explosionRange = Shooter.Inventory.GetMultiplier("ExplosionRange");

                SplashDamage(1, explosionRange, Mask, transform.position);
            }
        }
    }

    public void ClearTrails()
    {
        if (trails == null) trails = GetComponentsInChildren<TrailRenderer>();
        for (int i = 0; i < trails.Length; i++)
        {
            trails[i].Clear();
        }
    }

	protected virtual void OnDisable()
    {
        ClearTrails();
        isAlive = true;
    }

    protected virtual void OnEnable()
    {
        ClearTrails();
        isAlive = true;
        transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
    }
}
