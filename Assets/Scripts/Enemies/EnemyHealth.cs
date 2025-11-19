using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int health;
    [SerializeField] private Healthbar healthbar;
    [SerializeField] private CharacterInteraction bossController;
    private bool activated = false;
    [SerializeField] private int maxHealth = 100;

    private void Awake()
    {
        if (healthbar == null)
        {
            healthbar = GameObject.FindGameObjectWithTag("BOSSHEALTHBAR").GetComponent<Healthbar>();
        }
        if (bossController == null)
        {
            bossController = GetComponent<CharacterInteraction>();
        }
    }

    private void Start()
    {
        health = maxHealth;
    }
   
    public void HurtEnemy(int dmg)
    {
        if (activated)
        {
            health -= dmg;
            healthbar.SetHealth(health);

            if (health <= 0)
            {
                Die();
            }
        }

    }

    private void Die()
    {
        bossController.DeactivateHealthbar();
        Destroy(gameObject);
    }

    public void SetActivated(bool state)
    {

        activated = state;
    }
}
