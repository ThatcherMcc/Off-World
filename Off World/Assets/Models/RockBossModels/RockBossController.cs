using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockBossController : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Transform headBone;
    [SerializeField] float headMaxTurnAngle;
    [SerializeField] float headTrackingSpeed;

    void LateUpdate()
    {
        HeadTrackingUpdate();
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
}
