using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletLike : MonoBehaviour
{
    public float Duration = 3f;
    private float currentTime = 0f;
    public float bulletSpeed = 10f;

    // Start is called before the first frame update
    void Start()
    {
        currentTime = 0f;
        GetComponent<Rigidbody2D>().AddForce(transform.forward * bulletSpeed);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (currentTime > Duration) {
            Destroy(gameObject);
        }
        currentTime += Time.fixedDeltaTime;
    }

    void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        // Damage the target

        AttackProp attProp = GetComponent<AttackProp>();

        if (other.gameObject.tag == "Player") {
            var player = other.GetComponentInParent<CombatCharacter>();
            
            if (player) {
                player.takeDamage(attProp);
            }

            Destroy(gameObject);
        }
    }
}
