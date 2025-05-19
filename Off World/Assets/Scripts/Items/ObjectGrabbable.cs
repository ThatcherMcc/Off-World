using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGrabbable : MonoBehaviour, InteractableI
{
    private Rigidbody rb;
    public Transform objectGrabPointTransform;
    public bool equipped = false;
    public float lerpSpeed = 9f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (objectGrabPointTransform != null)
        {
            Vector3 newPosition = Vector3.Lerp(transform.position, objectGrabPointTransform.position, Time.deltaTime * lerpSpeed);
            rb.MovePosition(newPosition);
            Debug.Log("Moving");
        }
    }

    // Pickup
    public void Interact(InteractController controller)
    {
        if (!controller.isEquipped)
        {
            objectGrabPointTransform = controller.objectGrabPointTransform;
            Grab();
            controller.heldObject = this.gameObject;
            controller.isEquipped = equipped;
        }
        
    }

    public void Grab()
    {
        equipped = true;
        rb.useGravity = false;
        rb.drag = 5;
    }

    public void Drop()
    {
        //Vector3 momentum = new Vector3()
        equipped = false;
        objectGrabPointTransform = null;
        rb.useGravity = true;
        rb.drag = 0;
    }
}
