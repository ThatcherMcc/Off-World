using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHurtBox : MonoBehaviour
{
    private PlayerHealth healthbar;
    private Collider collider;

    void Start()
    {
        healthbar = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
        collider = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            healthbar.PlayerTakeDMG(10);
            collider.enabled = false;
        }
    }
}
