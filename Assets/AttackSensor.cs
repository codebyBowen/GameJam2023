using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackSensor : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other) {
        Debug.Log("Collision detected " + other);
        var animator = other.GetComponent<Animator>();
        Debug.Log("animator " + animator);
        animator.SetTrigger("Death");
    }
}
