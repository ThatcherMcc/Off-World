using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RockBossHeadLook : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Transform headBone;
    [SerializeField] float headMaxTurnAngle;
    [SerializeField] float headTrackingSpeed;

    // How fast we can turn and move full throttle
    [SerializeField] float startTurnSpeed;
    private float turnSpeed;
    // How fast we will reach the above speeds
    [SerializeField] float turnAcceleration;
    // If we are above this angle from the target, start turning
    [SerializeField] float maxAngToTarget;
    private float currentAngleToTarget;
    [SerializeField] float speedIncreaseInterval = 1.0f;
    private float nextSpeedIncreaseTime;
    // We are only doing a rotation around the up axis, so we only use a float here
    private float currentAngularVelocity;
    private bool canSee = false;

    private void Start()
    {
        turnSpeed = startTurnSpeed;
    }

    void LateUpdate()
    {
        SpeedUpTurning();
        RootMotionUpdate();
        HeadTrackingUpdate();
    }

    public bool IsLookingAtPlayer()
    {
        return canSee;
    }

    void SpeedUpTurning()
    {
        // Check if we need to speed up and if enough time has passed since the last speed increase
        if (Mathf.Abs(currentAngleToTarget) >= maxAngToTarget)
        {
            canSee = false;
            if (Time.time >= nextSpeedIncreaseTime)
            {
                turnSpeed *= 1.1f;
                nextSpeedIncreaseTime = Time.time + speedIncreaseInterval;
            }
        }
        else
        {
            canSee = true;
            turnSpeed = startTurnSpeed;
            // Reset the timer when we are facing the target so it's ready for the next time
            nextSpeedIncreaseTime = Time.time;
        }
    }

    void HeadTrackingUpdate()
    {
        // Store the current head rotation since we will be resetting it
        Quaternion currentLocalRotation = headBone.localRotation;
        // Reset the head rotation so our world to local space transformation will use the head's zero rotation. 
        // Note: Quaternion.Identity is the quaternion equivalent of "zero"
        headBone.localRotation = Quaternion.identity;

        Vector3 targetWorldLookDir = target.position - headBone.position;
        Vector3 targetLocalLookDir = headBone.InverseTransformDirection(targetWorldLookDir);

        // Apply angle limit
        targetLocalLookDir = Vector3.RotateTowards(
          Vector3.forward,
          targetLocalLookDir,
          Mathf.Deg2Rad * headMaxTurnAngle, // Note we multiply by Mathf.Deg2Rad here to convert degrees to radians
          0 // We don't care about the length here, so we leave it at zero
        );

        // Get the local rotation by using LookRotation on a local directional vector
        Quaternion targetLocalRotation = Quaternion.LookRotation(targetLocalLookDir, Vector3.up);

        // Apply smoothing
        headBone.localRotation = Quaternion.Slerp(
          currentLocalRotation,
          targetLocalRotation,
          1 - Mathf.Exp(-headTrackingSpeed * Time.deltaTime)
        );
    }

    void RootMotionUpdate()
    {
        // Get the direction toward our target
        Vector3 towardTarget = target.position - transform.position;
        // Vector toward target on the local XZ plane
        Vector3 towardTargetProjected = Vector3.ProjectOnPlane(towardTarget, transform.up);
        // Get the angle from the bosses forward direction to the direction toward toward our target
        // Here we get the signed angle around the up vector so we know which direction to turn in
        float angToTarget = Vector3.SignedAngle(transform.forward, towardTargetProjected, transform.up);
        currentAngleToTarget = angToTarget;
        float targetAngularVelocity = 0;

        // If we are within the max angle (i.e. approximately facing the target)
        // leave the target angular velocity at zero
        if (Mathf.Abs(angToTarget) > maxAngToTarget - 0.5f)
        {
            // Angles in Unity are clockwise, so a positive angle here means to our right
            if (angToTarget > 0)
            {
                targetAngularVelocity = turnSpeed;
            }
            // Invert angular speed if target is to our left
            else
            {
                targetAngularVelocity = -turnSpeed;
            }
        }

        // Use our smoothing function to gradually change the velocity
        currentAngularVelocity = Mathf.Lerp(
          currentAngularVelocity,
          targetAngularVelocity,
          1 - Mathf.Exp(-turnAcceleration * Time.deltaTime)
        );

        // Rotate the transform around the Y axis in world space, 
        // making sure to multiply by delta time to get a consistent angular velocity
        transform.Rotate(0, Time.deltaTime * currentAngularVelocity, 0, Space.World);
    }
}
