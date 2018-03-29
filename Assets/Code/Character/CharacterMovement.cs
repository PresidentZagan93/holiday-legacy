using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterMovement : GameBehaviour {
    
    public enum CharacterMovementMode
    {
        Player,
        Wander,
        FollowTeam,
        FollowOwner,
        Charge
    }

    public CharacterMovementMode mode = CharacterMovementMode.Player;
    public bool footsteps = true;

    public bool overrideFollowTeam;
    public GameTeam overrideTeam = GameTeam.Good;
    public float followDistance = 200f;
    public LayerMask layerMask;
    public bool ignoreRaycast = false;
    public Transform followTarget;
    public float chargeRate;
    public float chargeDuration;
    public float chargeSpeed = 32f;

    //Global settings
    public int speed = 2;
    public int runSpeed = 4;
    public int acceleration = 10;
    public bool runOnTarget = false;
    public bool wanderIfNoTarget = true;

    //Navmesh settings
    public IntRange moveRate = new IntRange(2, 4);
    public IntRange moveDuration = new IntRange(5, 8);

    //Privates
    new Rigidbody2D rigidbody;
    float nextStep;
    float nextMove;
    NavMeshAgent agent;
    Generator.GeneratorBlock block;
    Vector3 lastBlockPos;
    public Transform root;
    ParticleSystem walkEffect;

    public override void Awake()
    {
        base.Awake();

        rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.freezeRotation = true;
        if(root && root.Find("WalkEffect"))
        {
            walkEffect = root.Find("WalkEffect").GetComponent<ParticleSystem>();
        }
        else if(!root && transform.Find("WalkEffect"))
        {
            walkEffect = transform.Find("WalkEffect").GetComponent<ParticleSystem>();
        }

        Sound.CreateSource("Footsteps", AudioManager.AudioType.Health);
    }

    void Start()
    {
        if(mode != CharacterMovementMode.Player)
        {
            agent = GeneratorNavmesh.AddAgent(gameObject);
        }
    }

    float Acceleration
    {
        get
        {
            float multiplier = 1f;

            if (Inventory) multiplier *= Inventory.GetMultiplier("PlayerAcceleration");

            return acceleration * multiplier * 32f * blockAcceleration;
        }
    }

    public bool Running
    {
        get
        {
            if (runOnTarget)
            {
                if (overrideFollowTeam && followTarget)
                {
                    return true;
                }
                else
                {
                    if (Look && Look.target)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    float Speed
    {
        get
        {
            float multiplier = 1f;

            if (Inventory)
            {
                if (Inventory) multiplier *= Inventory.GetMultiplier("PlayerSpeed");
                if (Health.HasEffect("EffectFreeze")) multiplier = 0f;
                if (Armor && Armor.ShieldEnabled) multiplier *= 0.75f;

                if (Inventory.HasQuality("StressSpeed"))
                {
                    float ratio = (float)Health.hp / (float)Health.maxHp;
                    multiplier *= ratio.Remap(0f, 1f, Inventory.GetMultiplier("StressSpeed"), 1f);
                }
            }
            
            return (Running ? runSpeed : speed) * multiplier * blockSpeed;
        }
    }

    float blockSpeed;
    float blockAcceleration;

    void Multipliers()
    {
        if (lastBlockPos != transform.position.ToBlockPos())
        {
            lastBlockPos = transform.position.ToBlockPos();
            block = Generator.FindBlock(transform.position.ToBlockPos());

            if (block != null && block.type != GeneratorBlockType.Wall)
            {
                float speedResist = Inventory && Inventory.HasQuality("PlayerSpeedResistance") ? Inventory.GetMultiplier("PlayerSpeedResistance") : 0f;
                float accelResist = Inventory && Inventory.HasQuality("PlayerAccelerationResistance") ? Inventory.GetMultiplier("PlayerAccelerationResistance") : 0f;

                blockSpeed = Mathf.Lerp(block.speedMultiplier, 1f, speedResist);
                blockAcceleration = Mathf.Lerp(block.accelerationMultiplier, 1f, accelResist);
            }
        }
    }

    bool isMoving;
    Vector2 moveDirection = Vector2.zero;
    public bool IsMoving
    {
        get
        {
            return isMoving;
        }
    }

    public Vector2 MovingDirection
    {
        get
        {
            return moveDirection.normalized;
        }
    }

    void Footsteps()
    {
        if (!footsteps) return;
        
        if (isMoving && !GameManager.Paused && Character.Active)
        {
            if (Time.time > nextStep)
            {
                Generator.GeneratorBlock block = Generator.FindBlock((Vector3.up * 4 + transform.position).ToBlockPos());
                string tilesetOn = block != null ? block.tileset : "";
                AudioClip clip = GeneratorManager.GetFootstep(tilesetOn);

                nextStep = Time.time + 0.25f;
                Sound.GetSource("Footsteps").pitch = Random.Range(0.95f, 1.1f);
                Sound.GetSource("Footsteps").volume = Random.Range(0.7f, 1f);
                Sound.PlaySound(clip, "Footsteps");
            }
        }
    }

    bool ShouldMove
    {
        get
        {
            if (Console.Open) return false;
            if (PickerItems.Active) return false;
            if (GeneratorManager.Generating) return false;
            if (GameManager.Paused) return false;
            if (!Character.Active) return false;

            return true;
        }
    }

    public void Disable()
    {
        if (mode != CharacterMovementMode.Player)
        {
            GeneratorNavmesh.RemoveAgent(gameObject);
            agent = null;
        }

        if(rigidbody)
        {
            rigidbody.isKinematic = true;
            rigidbody.simulated = false;
        }
    }

    Vector2 Round(Vector2 vector)
    {
        Vector2[] headings = new Vector2[] { new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(-1f, 1f), new Vector2(-1f, 0f), new Vector2(-1f, -1f), new Vector2(0f, -1f), new Vector2(1f, -1f) };

        // actual conversion code:
        float angle = Mathf.Atan2(vector.y, vector.x);
        int octant = Mathf.RoundToInt(8f * angle / (2f * Mathf.PI) + 8f) % 8;
        return headings[octant];
    }

    private void FixedUpdate()
    {
        if(walkEffect)
        {
            ParticleSystem.EmissionModule emission = walkEffect.emission;
            emission.enabled = isMoving;
        }
        if(agent && Character) agent.isStopped = !Character.Process;
        if(GameManager.Paused || !Character.Process)
        {
            rigidbody.velocity = Vector2.zero;
            return;
        }
        if (agent)
        {
            agent.updatePosition = false;
            agent.updateRotation = false;
        }
        if (mode == CharacterMovementMode.Player || agent)
        {
            moveDirection = Vector2.zero;

            if(agent)
            {
                if (mode == CharacterMovementMode.Wander)
                {
                    DoWander();
                }
                else if (mode == CharacterMovementMode.Charge)
                {
                    DoCharge();
                }
                else if (mode == CharacterMovementMode.FollowOwner || mode == CharacterMovementMode.FollowTeam)
                {
                    DoFollow();
                }
                
                if(!agent.isStopped)
                {
                    moveDirection = Round(new Vector2(agent.velocity.x, agent.velocity.z).normalized);
                }

                bool reached = agent.pathStatus == NavMeshPathStatus.PathComplete && agent.remainingDistance  < 2f;
                if(reached)
                {
                    moveDirection = Vector2.zero;
                }
            }
            else
            {
                if (mode == CharacterMovementMode.Player)
                {
                    if (Character.input.left) moveDirection += Vector2.left;
                    if (Character.input.right) moveDirection += Vector2.right;
                    if (Character.input.up) moveDirection += Vector2.up;
                    if (Character.input.down) moveDirection += Vector2.down;
                }
            }

            if (!ShouldMove) moveDirection = Vector2.zero;

            isMoving = velocity.magnitude > 1f;
            float accel = 0.05f;
            float decell = 0.02f;

            if(moveDirection == Vector2.zero)
            {
                velocity = Vector2.Lerp(velocity, Vector2.zero - (knockbackDir * knockbackAmount * -300f), Time.fixedDeltaTime * Acceleration * decell);
            }
            else
            {
                velocity = Vector2.Lerp(velocity, moveDirection.normalized * Speed * 50f - (knockbackDir * knockbackAmount * -500f), Time.fixedDeltaTime * Acceleration * accel);
            }

            knockbackAmount -= Time.fixedDeltaTime * 2f;
            if (knockbackAmount < 0f)
            {
                knockbackAmount = 0f;
            }

            if(RoomChecker && RoomChecker.block != null)
            {
                root.position = new Vector3(transform.position.x, transform.position.y + RoomChecker.block.height);
            }
        }

        if (agent)
        {
            agent.acceleration = float.MaxValue;
            agent.nextPosition = GeneratorNavmesh.ToNavMesh(rigidbody.position);
        }

        if (isCharging) return;
        rigidbody.velocity = velocity;
    }

    Vector2 velocity;
    float knockbackAmount;
    Vector2 knockbackDir;

    public void Knockback(Vector2 dir, float amount)
    {
        nextUncharge -= 0.2f;
        knockbackAmount = amount * 0.065f * (Inventory ? Inventory.GetMultiplier("PlayerKnockback") : 1f);
        knockbackDir = dir.normalized;
    }

    bool isCharging;
    float nextUncharge;
    float nextCharge;
    float nextMoveDuration;
    Vector3 chargeDir;

    void DoWander()
    {
        agent.isStopped = Time.time > nextMoveDuration || Health.HasEffect("EffectFreeze");
        if (Time.time > nextMove)
        {
            agent.isStopped = false;
            float duration = moveDuration.Random();
            nextMove = Time.time + moveRate.Random() + duration;
            nextMoveDuration = Time.time + duration;

            int tries = 0;
            while (tries < 20)
            {
                Vector3 newDestination = transform.position + (Vector3)Random.insideUnitCircle.normalized * moveDuration.Random() * GeneratorManager.TileDimension;
                if(Character.isBoss && RoomChecker.room)
                {
                    float randomX = Random.Range(RoomChecker.room.rect.x, RoomChecker.room.rect.x + RoomChecker.room.rect.width);
                    float randomY = Random.Range(RoomChecker.room.rect.y, RoomChecker.room.rect.y + RoomChecker.room.rect.height);

                    //newDistanation is inside our room
                    newDestination = new Vector3(randomX, randomY, 0f);
                }

                Generator.GeneratorBlock block = Generator.FindBlock(newDestination.ToBlockPos());
                if (block != null)
                {
                    if (block.type == GeneratorBlockType.Door || block.type == GeneratorBlockType.DoorClosed || block.type == GeneratorBlockType.Wall)
                    {
                        //invalid block
                        continue;
                    }
                }

                if (Settings.Temporary.spawn.x + 4 > newDestination.ToBlockPos().x && Generator.preset.turns != 0) continue;
                if (block != null && block.room is ObjectRoomShop) continue;
                if (block != null && block.room is ObjectRoomPlayer) continue;
                if (block != null && Character.parentRoom)
                {
                    if (Character.parentRoom != block.room)
                    {
                        continue;
                    }
                }

                Vector3 agentDestination = GeneratorNavmesh.ToNavMesh(newDestination);
                if (NavMesh.CalculatePath(agent.transform.position, agentDestination, agent.areaMask, agent.path))
                {
                    agent.isStopped = false;
                    agent.SetDestination(agentDestination);
                    break;
                }
                tries++;
            }
        }
    }

    void DoCharge()
    {
        if (Time.time > nextCharge && !isCharging && !Health.HasEffect("EffectFreeze"))
        {
            followTarget = null;

            if (overrideTeam == GameTeam.Good) layerMask = ObjectManager.GetLayerMask("HitPlayerAndCompanion");
            if (overrideTeam == GameTeam.Evil) layerMask = ObjectManager.GetLayerMask("HitEnemy");

            Health newTarget = Helper.FindClosestHealth(transform, followDistance, overrideTeam, layerMask, ignoreRaycast);

            if (newTarget)
            {
                followTarget = newTarget.transform;
                chargeDir = (followTarget.position - transform.position).normalized;
                isCharging = true;

                Sound.PlaySound("JermaWierd", "Footsteps");

                nextUncharge = Time.time + chargeDuration;

                agent.isStopped = true;
            }
        }
        if (isCharging)
        {
            if(Health.HasEffect("EffectFreeze"))
            {
                rigidbody.velocity = Vector2.zero;
            }
            else
            {
                rigidbody.velocity = chargeDir * chargeSpeed * 128f;
            }

            if (Time.time > nextUncharge)
            {
                Sound.PlaySound("Charge", "Footsteps");
                isCharging = false;
                nextCharge = Time.time + chargeRate;

                agent.isStopped = false;
            }
        }
        else
        {
            DoWander();
        }
    }

    void DoFollow()
    {
        if (Health.HasEffect("EffectFreeze")) return;

        if (Time.time > nextMove)
        {
            agent.isStopped = false;

            nextMove = Time.time + 0.2f;
            if (overrideFollowTeam)
            {
                followTarget = null;

                if (mode == CharacterMovementMode.FollowTeam)
                {
                    if (overrideTeam == GameTeam.Good) layerMask = ObjectManager.GetLayerMask("HitPlayerAndCompanion");
                    if (overrideTeam == GameTeam.Evil) layerMask = ObjectManager.GetLayerMask("HitEnemy");

                    Health healthFound = Helper.FindClosestHealth(transform, followDistance, overrideTeam, layerMask, ignoreRaycast);
                    followTarget = healthFound ? healthFound.transform : null;
                }
                else
                {
                    if (Character.Owner) followTarget = Character.Owner.transform;
                }

                if (followTarget)
                {
                    Vector3 extraDir = Vector3.zero;
                    if (Character.Owner)
                    {
                        if (GunShooter)
                        {
                            extraDir = new Vector3(Character.Owner.Look.lookDirection.x, Character.Owner.Look.lookDirection.y).normalized * followDistance;
                        }
                        else
                        {
                            extraDir = new Vector3(Character.Owner.Look.lookDirection.x, Character.Owner.Look.lookDirection.y).normalized * -followDistance;
                        }
                    }
                    
                    Vector3 newDest = GeneratorNavmesh.ToNavMesh(followTarget.position + extraDir);
                    agent.SetDestination(newDest);
                }
            }
            else
            {
                if (Look && Look.target)
                {
                    Vector3 newDest = GeneratorNavmesh.ToNavMesh(Look.target.position);
                    agent.SetDestination(newDest);
                }
                else if(Look && !Look.target)
                {

                }
            }
        }

        if (!followTarget)
        {
            if (overrideFollowTeam && wanderIfNoTarget)
            {
                DoWander();
            }
            else if (Look && !Look.target && wanderIfNoTarget)
            {
                DoWander();
            }
        }
    }

    private void Update()
    {
        Footsteps();
        Multipliers();
    }
}
