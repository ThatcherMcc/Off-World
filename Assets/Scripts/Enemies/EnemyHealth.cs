using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float health;
    public float maxHealth = 100;

    private void Start()
    {
        health = maxHealth;
    }

    private void Update()
    {
        if (health <= 0)
        {
            Die();
        }
    }

    public void HurtEnemy(float dmg)
    {
        health -= dmg;
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
