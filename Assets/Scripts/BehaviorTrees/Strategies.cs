using BossFight.BehaviorTrees;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor.TerrainTools;
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
                Debug.Log($"Waited Done: {Time.time} - {timeStarted} this is over {timeToWait} ");
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
        readonly Animator animator;
        readonly string animationTriggerName;
        readonly float animDuration;

        private bool animTriggeredThisRun = false;
        private float animTimeStarted = 0f;

        public AnimationWaitStrategy(Animator animator, string animationTriggerName, float animDuration)
        {
            this.animator = animator;
            this.animationTriggerName = animationTriggerName;
            this.animDuration = animDuration;
        }

        public Node.Status Process()
        {
            if ( animator == null ) return Node.Status.Failure;

            if ( !animTriggeredThisRun )
            {
                animator.SetTrigger(animationTriggerName);
                animTimeStarted = Time.time;
                animTriggeredThisRun = true;
                return Node.Status.Running;
            }

      
            if (animTriggeredThisRun && Time.time - animTimeStarted <= animDuration)
            {
                return Node.Status.Running;
            }
            else
            {
                Debug.Log("Animation Done");
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

        // You might also want variables for movement speed and a stopping distance
        readonly float moveSpeed;
        readonly float stopDistance;


        public ChasePlayerStrategy(Rigidbody rb, Transform playerTransform, float moveSpeed = 10f, float stopDistance = 2f)
        {
            this.rb = rb;
            this.playerTransform = playerTransform;
            this.moveSpeed = moveSpeed;
            this.stopDistance = stopDistance;
    }

        public Node.Status Process()
        {
            if (rb == null)
            {
                Debug.Log("ChasePlayerStrat: PlayerTransform Null");
                return Node.Status.Failure;
            }

            float distance = Vector3.Distance(rb.position, playerTransform.position);

            if (distance <= stopDistance)
            {
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }
                return Node.Status.Success;
            }

            Vector3 directionToPlayer = (playerTransform.position - rb.position).normalized;
            directionToPlayer = new Vector3(directionToPlayer.x, 0, directionToPlayer.z);
            rb.AddForce(directionToPlayer * moveSpeed, ForceMode.Force);

            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity to the max
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
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
        private float t;
        private bool hasJumped = false;


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

            if (!hasJumped)
            {
                if (!IsGrounded())
                {
                    Debug.Log("JumpOnPlayerStrategy: Not grounded, waiting to jump.");
                    return Node.Status.Running;
                }

                Vector3 start = rb.position;
                Vector3 end = player.position;
                Vector3 gravity = Physics.gravity;
                float g = -gravity.y; // Usually 9.81

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

                hasJumped = true;
                Debug.Log($"Jump started: Hvel={Vx:F2}, Vvel={Vy:F2}, flightTime={t:F2}, TotalVelocity={initialVelocity}");
                return Node.Status.Running;
            }

            // Landing check
            if (rb.velocity.y <= 0.1f && IsGrounded())
            {
                Debug.Log("Boss landed after jump.");
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                return Node.Status.Success;
            }

            return Node.Status.Running;
        }

        public void Reset()
        {
            hasJumped = false;
            // Do NOT reset lastJumpTime here, as it's for cooldown
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            Debug.Log("JumpOnPlayerStrategy: Resetting.");
        }

        // You NEED to implement this IsGrounded() method somewhere reliable for your boss.
        // For example, in your main BossAI script that manages the Rigidbody,
        // using a SphereCast or Raycast downwards.
        private bool IsGrounded()
        {
            float groundCheckDistance = 0.3f; // Adjust based on your character's size and collider
            LayerMask groundLayer = LayerMask.GetMask("whatIsGround"); // Ensure your ground/terrain is on this layer

            // SphereCast from slightly above the feet to detect ground
            // Adjust 'rb.position.y - (collider.bounds.extents.y - 0.1f)' to be near the feet.
            // A common way for CapsuleCollider is to cast from collider.bounds.center + Vector3.down * (collider.bounds.extents.y - 0.1f)
            Collider mainCollider = rb.GetComponent<Collider>();
            if (mainCollider == null) return false;

            // Cast from slightly above the bottom of the main collider
            Vector3 origin = mainCollider.bounds.center;
            origin.y = mainCollider.bounds.min.y; // Just slightly above the bottom edge

            // Use a small radius for the sphere cast
            float sphereRadius = mainCollider.bounds.extents.x * 0.9f; // Or a fixed small value like 0.1f

            // Perform the SphereCast
            bool isGrounded = Physics.SphereCast(origin, sphereRadius, Vector3.down, out RaycastHit hitInfo, groundCheckDistance, groundLayer);
            Color debugColor = isGrounded ? Color.green : Color.red;
            Debug.DrawLine(origin, origin + Vector3.down * groundCheckDistance, debugColor, 0f, true);
            return isGrounded;
        }
    } // jumps on player given a time its arc can take
}
