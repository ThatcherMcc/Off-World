using BossFight.BehaviorTrees;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BossFight.Strategies
{
    // interface that every strategy will inherit from.
    public interface IStrategy
    {
        Node.Status Process();
        void Reset()
        {
            // Noop
        }
    }

    public class ActionStrategy : IStrategy
    {
        readonly Action action;
        public ActionStrategy(Action action)
        {
            this.action = action;
        }
        public Node.Status Process()
        {
            action();
            return Node.Status.Success;
        }
    }

    public class Condition : IStrategy
    {
        readonly Func<bool> predicate;

        public Condition(Func<bool> predicate)
        {
            this.predicate = predicate;
        }

        public Node.Status Process() => predicate() ? Node.Status.Success : Node.Status.Failure;
    }

    public class WaitStrategy : IStrategy
    {
        readonly float timeToWait;

        private bool started = false;
        private float timeStarted = 0f;

        public WaitStrategy(float timeToWait)
        {
            this.timeToWait = timeToWait;
        }

        public Node.Status Process()
        {
            if (!started)
            {
                timeStarted = Time.time;
                started = true;
                return Node.Status.Running;
            }

            if (started && Time.time - timeStarted > timeToWait)
            {
#if UNITY_EDITOR
                Debug.Log($"Waited Done: {Time.time} - {timeStarted} this is over {timeToWait} ");
#endif
                return Node.Status.Success;
            }
            return Node.Status.Running;
        }

        public void Reset()
        {
            started = false;
            timeStarted = 0f;
        }
    }

    public class AnimationWaitStrategy : IStrategy
    {
        readonly Animator animator; // The animator to play the animation on
        readonly string animationTriggerName; // The name of the trigger parameter to start the animation
        readonly float animDuration; // Duration of the animation in seconds

        private bool animTriggeredThisRun = false; // Keeps track if the animation has been triggered in the current run
        private float animTimeStarted; // The time when the animation was started

        public AnimationWaitStrategy(Animator animator, string animationTriggerName, float animDuration)
        {
            this.animator = animator;
            this.animationTriggerName = animationTriggerName;
            this.animDuration = animDuration;
        }

        public Node.Status Process()
        {
            if ( animator == null ) return Node.Status.Failure;

            // if we haven't triggered the animation yet, trigger it
            if ( !animTriggeredThisRun)
            {
                animator.SetTrigger(animationTriggerName); // trigger the animation
                animTimeStarted = Time.time; // record the time the animation started
                animTriggeredThisRun = true; // mark that we've triggered the animation this run
                return Node.Status.Running;
            }

            if (animTriggeredThisRun && Time.time - animTimeStarted <= animDuration)
            {
                return Node.Status.Running;
            }
            else
            {
#if UNITY_EDITOR
                Debug.Log("Animation Done");
#endif
                return Node.Status.Success;
            }
        }

        public void Reset()
        {
            animTriggeredThisRun = false;
            animTimeStarted = 0f;
            if ( animator != null ) animator.ResetTrigger(animationTriggerName);
        }
    } // plays animation and waits for it to finish

    public class ChasePlayerStrategy : IStrategy
    {
        readonly Rigidbody rb; // The boss's Transform
        readonly Transform playerTransform; // The player's Transform

        readonly float moveSpeed;
        readonly float velocitySmoothingFactor;
        readonly float stopDistance;


        public ChasePlayerStrategy(Rigidbody rb, Transform playerTransform, float moveSpeed=10f, float stopDistance = 2f, float velocitySmoothingFactor=10f)
        {
            this.rb = rb;
            this.playerTransform = playerTransform;
            this.moveSpeed = moveSpeed;
            this.velocitySmoothingFactor = velocitySmoothingFactor;
            this.stopDistance = stopDistance;
    }

        public Node.Status Process()
        {
            if (rb == null || playerTransform == null)
                return Node.Status.Failure;

            float distance = Vector3.Distance(rb.position, playerTransform.position);

            if (distance <= stopDistance)
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
                rb.angularVelocity = Vector3.zero;
                return Node.Status.Success;
            }

            Vector3 directionToPlayer = (playerTransform.position - rb.position).normalized;
            directionToPlayer.y = 0f;
            directionToPlayer.Normalize();

            Vector3 targetVelocity = directionToPlayer * moveSpeed;

            Vector3 currentVelocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            Vector3 newVelocity = Vector3.Lerp(
                currentVelocity,
                targetVelocity,
                velocitySmoothingFactor * Time.fixedDeltaTime // Use Time.fixedDeltaTime in FixedUpdate/Process
            );

            rb.velocity = new Vector3(newVelocity.x, rb.velocity.y, newVelocity.z);

            // limit velocity to the max
            if (directionToPlayer.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime);
            }

            return Node.Status.Running;
        }

        public void Reset()
        {
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    } // moves towards player

    public class JumpOnPlayerStrategy : IStrategy {
        readonly Rigidbody rb;
        readonly Transform player;
        private readonly float t; // time from jump to landing


        // We can remove 'moveSpeed' and 'angle' from the constructor as they are calculated or not directly used
        // Instead, the jump calculation implicitly determines the necessary horizontal speed.
        public JumpOnPlayerStrategy(Rigidbody rb, Transform player, float flightTime) // Constructor updated
        {
            this.rb = rb;
            this.player = player;
            this.t = flightTime;
        }

        public Node.Status Process()
        {
            if (rb == null || player == null)
            {
                Debug.LogWarning("JumpOnPlayerStrategy: Rigidbody or Player Transform is null.");
                return Node.Status.Failure;
            }

            Vector3 start = rb.position;
            Vector3 end = player.position;
            Vector3 gravity = Physics.gravity;
            float g = -gravity.y; // 9.81

            // calc Range or dx
            Vector3 startXZ = new Vector3(start.x, 0, start.z);
            Vector3 endXZ = new Vector3(end.x, 0, end.z);
            float R = Vector3.Distance(startXZ, endXZ);

            // calc H or dy
            float H = end.y - start.y;

            // calc Vx
            float Vx = (t > 0.001f) ? R / t : 0f;

            //calc Vy
            float Vy = (H / t) + (0.5f * g * t);

            // horizontal direction
            Vector3 horizontalDirection = (endXZ - startXZ).normalized;
            if (R < 0.01f) // If horizontal distance is negligible, default to boss's forward direction
            {
                horizontalDirection = rb.transform.forward;
            }

            // initial velocity
            Vector3 initialVelocity = (horizontalDirection * Vx) + (Vector3.up * Vy);

            // Apply it
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.AddForce(initialVelocity, ForceMode.VelocityChange);

#if UNITY_EDITOR
            Debug.Log($"Jump started: Hvel={Vx:F2}, Vvel={Vy:F2}, flightTime={t:F2}, TotalVelocity={initialVelocity}");
#endif

            return Node.Status.Success;
        }
        public void Reset()
        {
            // Do NOT reset lastJumpTime here, as it's for cooldown
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
#if UNITY_EDITOR
            Debug.Log("JumpOnPlayerStrategy: Resetting.");
#endif
        }
    } // jumps on player given a time its arc can take

    /// <summary>
    /// Rigidbody-based wander near a spawn point.
    /// Picks a random direction, walks for stepDuration seconds, then returns Success
    /// so the parent node can re-evaluate priorities (e.g. check for threats).
    /// </summary>
    public class WanderStrategy : IStrategy
    {
        readonly Rigidbody rb;
        readonly Vector3 spawnPoint;
        readonly float wanderRadius;
        readonly float moveSpeed;
        readonly float smoothing;
        readonly float stepDuration;

        private Vector3 targetDirection;
        private float stepTimer;
        private bool hasDirection;

        public WanderStrategy(Rigidbody rb, Vector3 spawnPoint, float wanderRadius,
                              float moveSpeed, float smoothing = 5f, float stepDuration = 3f)
        {
            this.rb = rb;
            this.spawnPoint = spawnPoint;
            this.wanderRadius = wanderRadius;
            this.moveSpeed = moveSpeed;
            this.smoothing = smoothing;
            this.stepDuration = stepDuration;
        }

        public Node.Status Process()
        {
            if (rb == null) return Node.Status.Failure;

            if (!hasDirection)
            {
                float distFromSpawn = Vector3.Distance(rb.position, spawnPoint);
                if (distFromSpawn > wanderRadius)
                {
                    // Head back toward spawn
                    targetDirection = (spawnPoint - rb.position).normalized;
                    targetDirection.y = 0f;
                    targetDirection.Normalize();
                }
                else
                {
                    float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    targetDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                }
                stepTimer = stepDuration;
                hasDirection = true;
            }

            // Move via Rigidbody velocity
            Vector3 targetVelocity = targetDirection * moveSpeed;
            Vector3 currentHorizontal = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            Vector3 newVelocity = Vector3.Lerp(currentHorizontal, targetVelocity, smoothing * Time.deltaTime);
            rb.velocity = new Vector3(newVelocity.x, rb.velocity.y, newVelocity.z);

            // Rotate toward movement direction
            if (targetDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, smoothing * Time.deltaTime);
            }

            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                hasDirection = false;
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
                return Node.Status.Success; // re-evaluate priorities
            }

            return Node.Status.Running;
        }

        public void Reset()
        {
            hasDirection = false;
            stepTimer = 0f;
        }
    }

    /// <summary>
    /// Hop-based flee from a target. Performs a burst of hops away from the player,
    /// then returns Success so the parent can re-evaluate.
    /// </summary>
    public class HopFleeStrategy : IStrategy
    {
        readonly Rigidbody rb;
        readonly Transform target;
        readonly float jumpForce;
        readonly float hopCooldown;
        readonly int hopsPerBurst;

        private float cooldownTimer;
        private int hopsRemaining;
        private bool started;

        public HopFleeStrategy(Rigidbody rb, Transform target, float jumpForce = 5f,
                                float hopCooldown = 0.8f, int hopsPerBurst = 3)
        {
            this.rb = rb;
            this.target = target;
            this.jumpForce = jumpForce;
            this.hopCooldown = hopCooldown;
            this.hopsPerBurst = hopsPerBurst;
        }

        public Node.Status Process()
        {
            if (rb == null || target == null) return Node.Status.Failure;

            if (!started)
            {
                hopsRemaining = hopsPerBurst;
                cooldownTimer = 0f;
                started = true;
            }

            // Face away from target
            Vector3 fleeDir = (rb.position - target.position).normalized;
            fleeDir.y = 0f;
            if (fleeDir.sqrMagnitude > 0.01f)
            {
                fleeDir.Normalize();
                Quaternion targetRotation = Quaternion.LookRotation(fleeDir);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.deltaTime);
            }

            if (cooldownTimer > 0f)
            {
                cooldownTimer -= Time.deltaTime;
                return Node.Status.Running;
            }

            if (hopsRemaining > 0)
            {
                Vector3 hopForce = new Vector3(fleeDir.x * 0.5f, 1f, fleeDir.z * 0.5f) * jumpForce;
                rb.AddForce(hopForce, ForceMode.Impulse);
                hopsRemaining--;
                cooldownTimer = hopCooldown;
                return Node.Status.Running;
            }

            // Burst complete
            started = false;
            return Node.Status.Success;
        }

        public void Reset()
        {
            started = false;
            cooldownTimer = 0f;
            hopsRemaining = 0;
        }
    }

    /// <summary>
    /// Hop-based idle wander near spawn. Rests between hops.
    /// After each hop, returns Success so the parent can re-evaluate priorities.
    /// </summary>
    public class HopWanderStrategy : IStrategy
    {
        readonly Rigidbody rb;
        readonly Vector3 spawnPoint;
        readonly float maxWanderDist;
        readonly float jumpForce;
        readonly float restMin;
        readonly float restMax;

        private Vector3 hopDirection;
        private float restTimer;
        private bool isResting;

        public HopWanderStrategy(Rigidbody rb, Vector3 spawnPoint, float maxWanderDist = 10f,
                                  float jumpForce = 5f, float restMin = 2f, float restMax = 4f)
        {
            this.rb = rb;
            this.spawnPoint = spawnPoint;
            this.maxWanderDist = maxWanderDist;
            this.jumpForce = jumpForce;
            this.restMin = restMin;
            this.restMax = restMax;
            isResting = true;
            restTimer = UnityEngine.Random.Range(restMin, restMax);
        }

        public Node.Status Process()
        {
            if (rb == null) return Node.Status.Failure;

            if (isResting)
            {
                restTimer -= Time.deltaTime;
                if (restTimer <= 0f)
                {
                    isResting = false;
                    PickDirection();
                }
                else
                {
                    return Node.Status.Running;
                }
            }

            // Hop
            Vector3 hopForce = new Vector3(hopDirection.x * 0.5f, 1f, hopDirection.z * 0.5f) * jumpForce;
            rb.AddForce(hopForce, ForceMode.Impulse);

            // Face hop direction
            if (hopDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(hopDirection);
                rb.rotation = Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.deltaTime);
            }

            isResting = true;
            restTimer = UnityEngine.Random.Range(restMin, restMax);

            // Return Success to let parent re-evaluate (check for threats)
            return Node.Status.Success;
        }

        private void PickDirection()
        {
            float distFromSpawn = Vector3.Distance(rb.position, spawnPoint);
            if (distFromSpawn > maxWanderDist)
            {
                hopDirection = (spawnPoint - rb.position).normalized;
                hopDirection.y = 0f;
                hopDirection.Normalize();
            }
            else
            {
                float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                hopDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            }
        }

        public void Reset()
        {
            isResting = true;
            restTimer = UnityEngine.Random.Range(restMin, restMax);
        }
    }
}
