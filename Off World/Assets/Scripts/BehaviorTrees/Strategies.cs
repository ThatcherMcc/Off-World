using BossFight.BehaviorTrees;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
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

      
            if ( animTriggeredThisRun && Time.time - animTimeStarted <= animDuration)
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
    }

    public class ChasePlayerStrategy : IStrategy
    {
        readonly Rigidbody entityRB; // The boss's Transform
        readonly Transform playerTransform; // The player's Transform

        // You might also want variables for movement speed and a stopping distance
        readonly float moveSpeed;
        readonly float stopDistance;

        public ChasePlayerStrategy(Rigidbody entityRB, Transform playerTransform, float moveSpeed = 10f, float stopDistance = 2f)
        {
            this.entityRB = entityRB;
            this.playerTransform = playerTransform;
            this.moveSpeed = moveSpeed;
            this.stopDistance = stopDistance;
        }

        public Node.Status Process()
        {
            if (entityRB == null)
            {
                Debug.Log("ChasePlayerStrat: PlayerTransform Null");
                return Node.Status.Failure;
            }

            float distance = Vector3.Distance(entityRB.position, playerTransform.position);

            if (distance <= stopDistance)
            {
                return Node.Status.Success;
            }

            Vector3 directionToPlayer = (playerTransform.position - entityRB.position).normalized;
            directionToPlayer = new Vector3(directionToPlayer.x, 0, directionToPlayer.z);
            entityRB.AddForce(directionToPlayer * moveSpeed, ForceMode.Force);

            return Node.Status.Running;
        }

        public void Reset()
        {
            if (entityRB != null)
            {
                entityRB.velocity = Vector3.zero;
                entityRB.angularVelocity = Vector3.zero;
            }
        }
    }

}
