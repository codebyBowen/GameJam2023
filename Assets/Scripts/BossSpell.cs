using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AttackProp))]
public class BossSpell : MonoBehaviour
{
    public Vector3 attackOffset;
    public float attackRange = 1f;
    public LayerMask attackMask;

    public AttackProp attProp;

    void Start() {
        Destroy(gameObject, 4f);
    }

    public void Attack() {
        Debug.Log(gameObject.name + " Attack");

        Vector3 pos = transform.position;
        pos += transform.right * attackOffset.x;
        pos += transform.up * attackOffset.y;

        Collider2D colInfo = Physics2D.OverlapCircle(pos, attackRange, attackMask);
        Debug.Log("colInfo " + colInfo);

        if (colInfo != null)
        {
            colInfo.GetComponentInParent<CombatCharacter>().takeDamage(attProp);
        }
    }

    public void DestroySelf() {
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected() 
    {
        Vector3 pos = transform.position;
        pos += transform.right * attackOffset.x;
        pos += transform.up * attackOffset.y;
        Gizmos.DrawWireSphere(pos, attackRange);
    }
}
