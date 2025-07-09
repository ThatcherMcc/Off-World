using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;

public class PlayerHitBox : MonoBehaviour
{
    private Collider hitBox;

    private void Start()
    {
        hitBox = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        EnemyHealth enemyHealth = other.gameObject.GetComponent<EnemyHealth>();

        // If not found, try to get it from sthe parent
        if (enemyHealth == null)
        {
            enemyHealth = other.gameObject.GetComponentInParent<EnemyHealth>();
        }
        if (enemyHealth != null)
        {
            enemyHealth.HurtEnemy(20);
            hitBox.enabled = false;
        }
    }
}
