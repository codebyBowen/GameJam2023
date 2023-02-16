using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttack : MonoBehaviour
{
    private Boss boss;

    void Start() {
        boss = transform.parent.GetComponent<Boss>();
    }

    public void Attack()
    {
        boss.Attack();
    }
}
