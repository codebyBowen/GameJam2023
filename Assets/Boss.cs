using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    public int maxHealth = 1000;
    public int currentHealth;
    public Animator animator;
    public BossHealthBar bossHealthBar;

    public int attackDamage = 20;
    public Vector3 attackOffset;
    public float attackRange = 1f;
    public LayerMask attackMask;

    public GameObject spellPrefab;
    public Transform SpellGenerator;
    public int tenacity; // or called armor?
    public bool armorBroken = false;

    // break flags
    private bool firstBreak = false;
    private bool secondBreak = false;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth; 
        bossHealthBar.SetMaxHealth(maxHealth);
        tenacity = 1000;
        armorBroken = false;
        firstBreak = false;
        secondBreak = false;
    }

    void FixedUpdate() {
        tenacitySystem();
    }

    public void TakeDamage(int damage) {
        // take damage only when armor is broken
        // currentHealth -= damage;
        bossHealthBar.SetHealth(currentHealth);
        reduceTenacity(damage);
        if (armorBroken) {
            animator.SetTrigger("Hurt");
            currentHealth -= damage;
        }
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

    public void Attack()
    {
        Debug.Log("Boss Start Attack");

        Vector3 pos = transform.position;
        pos += transform.right * attackOffset.x;
        pos += transform.up * attackOffset.y;

        Collider2D colInfo = Physics2D.OverlapCircle(pos, attackRange, attackMask);
        Debug.Log("colInfo " + colInfo);

        if (colInfo != null)
        {
            colInfo.GetComponent<HeroKnight>().ReceiveDamage(attackDamage);
        }
    }

    void OnDrawGizmosSelected() 
    {
        Vector3 pos = transform.position;
        pos += transform.right * attackOffset.x;
        pos += transform.up * attackOffset.y;
        Gizmos.DrawWireSphere(pos, attackRange);
    }

    void reduceTenacity(int value) {
        tenacity -= value;
        Debug.Log("reduceTenacity" + value + "tenacity" + tenacity);
    }

    void tenacitySystem() {
        // recover tenacity when health is below 2 of 3 and 1 of 3 max health
        if ( !armorBroken && tenacity <= 0 ) {
            armorBroken = true;
            // TODO: play broken sound
        } else if ( armorBroken && !secondBreak && (currentHealth <= (int)(maxHealth / 3))) {
            armorBroken = false;
            tenacity = 1200;
            secondBreak = true;
        } else if ( armorBroken && !firstBreak && (currentHealth <= (int)(maxHealth / 3 * 2))) {
            armorBroken = false;
            tenacity = 1100;
            firstBreak = true;
        } 
    }

    public void SummonSpell()
    {
        Debug.Log("Boss Summoning Spell");
        // player.pos.y == -1.04, the spellPrefab need to be placed at 1.3,
        // so offset = 1.3 + 1.04 = 2.34
        Vector3 offset = new Vector3(0, 2.34f, 0);
        if (SpellGenerator)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
        Debug.Log("Player " + player);
            if (player)
            {
                Transform summonPoint = SpellGenerator.transform;
                summonPoint.position = player.transform.position + offset;
                // Summon above the player
        Debug.Log("Summon point " + summonPoint);
                Instantiate(spellPrefab, summonPoint.position, summonPoint.rotation);
            }
        }
    }
}
