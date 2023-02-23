using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BarthaSzabolcs.Tutorial_SpriteFlash;
using UnityEngine.SceneManagement;

public class Boss : CombatCharacter
{
    public Animator animator;

    public Vector3 attackOffset;
    public float attackRange = 1f;
    public LayerMask attackMask;

    public GameObject spellPrefab;
    public Transform SpellGenerator;
    public float tenacity; // or called armor?
    public bool armorBroken = false;

    private SimpleFlash flashEffect;

    // break flags
    private bool firstBreak = false;
    private bool secondBreak = false;
    private IEnumerator coroutine;
    private BoxCollider2D       m_collider2d;

    // Start is called before the first frame update
    void Start()
    {
        // tenacity = 1000;
        armorBroken = true;
        firstBreak = false;
        secondBreak = false;

        health.dieCB = Die;

        flashEffect = GetComponent<SimpleFlash>();
        coroutine = WaitAndSwapScene(5);
        m_collider2d = GetComponent<BoxCollider2D>();
        
    }

    void FixedUpdate() {
        // tenacitySystem();
    }

    public override void takeDamage(AttackProp ap) {

        // take damage only when armor is broken
        // currentHealth -= damage;
        if (flashEffect) {
            flashEffect.Flash();
        }
        reduceTenacity(Damage.CalculateDamage(ap, this.attProp));
        if (armorBroken) {
            base.takeDamage(ap);
        }
    }

    void Die()
    {
        Debug.Log("Enemy died!");
        float Timer = 0f;
        
        animator.SetBool("IsDead", true);

        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

        foreach(int eLayer in Utils.layersFromLayerMask(attackMask)) {
          Physics2D.IgnoreLayerCollision(gameObject.layer, eLayer, true);
        }
        
        m_collider2d.enabled = false;
        
    }

    private IEnumerator WaitAndSwapScene(int waitTime)
    {
        Debug.Log("WaitAndSwapScene");
        yield return new WaitForSeconds(waitTime);
        UnityEngine.SceneManagement.SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
            Debug.Log("Boss Hit Player!!" + attProp);
            colInfo.GetComponentInParent<CombatCharacter>().takeDamage(attProp);
        }
    }

    public void Disappear()
    {
        GetComponent<SpriteRenderer>().enabled = false;
    }

    void OnDrawGizmosSelected() 
    {
        Vector3 pos = transform.position;
        pos += transform.right * attackOffset.x;
        pos += transform.up * attackOffset.y;
        Gizmos.DrawWireSphere(pos, attackRange);
    }

    void reduceTenacity(float value) {
        tenacity -= value;
        Debug.Log("reduceTenacity" + value + "tenacity" + tenacity);
    }

    void tenacitySystem() {
        // recover tenacity when health is below 2 of 3 and 1 of 3 max health
        if ( !armorBroken && tenacity <= 0 ) {
            armorBroken = true;
            // TODO: play broken sound
        } else if ( armorBroken && !secondBreak && (health.currentHP <= (int)(health.maxHP / 3))) {
            armorBroken = false;
            tenacity = 1200;
            secondBreak = true;
        } else if ( armorBroken && !firstBreak && (health.currentHP <= (int)(health.maxHP / 3 * 2))) {
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
