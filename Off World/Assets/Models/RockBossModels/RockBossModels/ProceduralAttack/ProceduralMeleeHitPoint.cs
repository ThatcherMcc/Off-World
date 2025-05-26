using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralMeleeHitPoint : MonoBehaviour
{
    [Header("Points")]
    public Transform targetHand;
    public Transform targetShoulder;
    public Transform targetFoot;

    [Header("Demos")]
    public bool handIKDemo = false;
    public bool foorPivotDemo;
    public bool shoulderIKDemo;

    private void OnDrawGizmos()
    {
        if (handIKDemo)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetHand.position);
        }
    }


}
