using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public Healthbar healthbar;
    public int maxHealth = 100;
    [SerializeField] private GameObject deathScreen;
    private CanvasGroup cg;
    private int health;
    private bool dead;
    private Vector3 respawnPosition;


    private void Start()
    {
        health = maxHealth;
        GameObject temp = GameObject.FindGameObjectWithTag("Healthbar");
        healthbar = temp.GetComponent<Healthbar>();
        GameObject temp2 = GameObject.FindGameObjectWithTag("DeathScreen");
        cg = temp2.GetComponent<CanvasGroup>();
        cg.alpha = 0;
        respawnPosition = transform.position;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            PlayerHeal(20);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            PlayerTakeDMG(20);
        }
        if (dead)
        {
            transform.position = respawnPosition;
        }
    }

    public void PlayerHeal(int healing)
    {
        health += healing;
        healthbar.SetHealth(health);

        if (health > 100)
        {
            health = maxHealth;
        }
    }

    public void PlayerTakeDMG(int damage)
    {
        health -= damage;
        healthbar.SetHealth(health);

        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }

    private void Die()
    {
        cg.alpha = 1;
        dead = true;
        transform.position = respawnPosition;
        Invoke("ResetDead", 3);
    }

    private void ResetDead()
    {
        dead = false;
        cg.alpha = 0;
        PlayerHeal(1000);
    }
}
