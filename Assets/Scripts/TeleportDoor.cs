using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportDoor : MonoBehaviour
{
    public Transform destination;
    public GameObject player;
    public Transform orientation;

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.GetComponent<PlayerMovement>())
        {
            player = collider.gameObject;
            player.transform.position = destination.position;
        }
    }
}
