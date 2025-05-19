using UnityEngine;

public class RayDown : MonoBehaviour
{
    public Transform hipAligned;
    [SerializeField] private float maxDist = 4f;
    [SerializeField] private Vector3 raycastOffset = Vector3.zero; // Offset from the hip

    private void Start()
    {
        Debug.Log("Hip Aligned is: " + hipAligned);
        if (hipAligned == null)
        {
            Debug.LogError("Hip Aligned Transform is not assigned!");
            return;
        }
    }
    private void Update()
    {
        Vector3 raycastStartPoint = hipAligned.position + raycastOffset;
        RaycastHit groundHit;

        if (Physics.Raycast(raycastStartPoint, Vector3.down, out groundHit, maxDist))
        {
            // If the ray hits something, set this object's position to the hit point
            transform.position = groundHit.point;
        }
        else
        {
            // If the ray doesn't hit anything within maxDist,
            // you might want to set a default position or handle this case differently.
            // For now, we'll keep it at the raycast start point's Y with the offset.
            transform.position = new Vector3(raycastStartPoint.x, hipAligned.position.y - maxDist, raycastStartPoint.z);
            Debug.LogWarning($"Raycast from {raycastStartPoint} didn't hit anything within {maxDist} units.");
        }
    }
}