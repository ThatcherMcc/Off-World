using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGrabbable : MonoBehaviour
{
    private Rigidbody rb;
    private Transform objectGrabPointTransform;
    private IEnemy enemy;


    public bool equipped = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        enemy = GetComponent<IEnemy>();
    }

    private void FixedUpdate()
    {
        if (objectGrabPointTransform != null)
        {
            float lerpSpeed = 9f;
            Vector3 newPosition = Vector3.Lerp(transform.position, objectGrabPointTransform.position, Time.deltaTime * lerpSpeed);
            rb.MovePosition(newPosition);
        }
    }

    public void Capture(Transform objectGrabPointTransform)
    {
        equipped = true;

        enemy.EnableAI(false);

        this.objectGrabPointTransform = objectGrabPointTransform;
        rb.useGravity = false;
    }

    public void Release()
    {
        //Vector3 momentum = new Vector3()
        equipped = false;

        enemy.EnableAI(true);

        this.objectGrabPointTransform = null;
        rb.useGravity = true;
    }
}
