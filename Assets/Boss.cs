using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    public int maxHealth = 1000;
    public int currentHealth;
    public Animator animator;
    public BossHealthBar bossHealthBar;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth; 
        bossHealthBar.SetMaxHealth(maxHealth);   
    }

    public void TakeDamage(int damage) {
        currentHealth -= damage;
        bossHealthBar.SetHealth(currentHealth);
        animator.SetTrigger("Hurt");

        if (currentHealth <= 0) {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Enemy died!");

        animator.SetBool("IsDead", true);

        GetComponent<Collider2D>().enabled = false;

        this.enabled = false;
    }
}
