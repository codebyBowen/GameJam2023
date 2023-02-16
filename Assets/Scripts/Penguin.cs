using UnityEngine;
using System.Collections;
// using System.Math;
using System;

public class Penguin : MonoBehaviour {
    public Transform player;
    public bool isFlipped = false;

    public Transform ShockWavePosition;
    public GameObject shockWavePrefab;

    public void LookAtPlayer()
    {
        Vector3 flipped = transform.localScale;
		flipped.z *= -1f;

		if (transform.position.x > player.position.x && isFlipped)
		{
			transform.localScale = flipped;
			transform.Rotate(0f, 180f, 0f);
			isFlipped = false;
		}
		else if (transform.position.x < player.position.x && !isFlipped)
		{
			transform.localScale = flipped;
			transform.Rotate(0f, 180f, 0f);
			isFlipped = true;
		}
    }


	public void SummonShockWave()
    {
        Debug.Log("Penguin Summoning ShockWave");
        if (ShockWavePosition)
        {
            if (player)
            {
                Transform summonPoint = ShockWavePosition.transform;
                // summonPoint.position = player.transform.position;
                // Shoot from the front of the penguin
                var bullet = Instantiate(shockWavePrefab, summonPoint.position, summonPoint.rotation);
				var speed = bullet.GetComponent<BulletLike>().bulletSpeed;
				// bullet.GetComponent<Rigidbody2D>().AddForce(bullet.transform.right);
				bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.right * speed;
            }
        }
    }

	public void LaunchSelf()
    {
        Debug.Log("Penguin starts accelerating!");
		var rb = GetComponent<Rigidbody2D>();

		var target = new Vector2(player.position.x, rb.position.y);
		var currPos = new Vector2(rb.position.x, rb.position.y);
		var moveDir = Vector2.MoveTowards(currPos, target, 1f);

		float speed = 10f;

		rb.AddForce(moveDir * speed);
    }

	private void OnCollisionEnter2D(Collision2D other) {
		var attProp = GetComponent<AttackProp>();
        other.gameObject.GetComponent<CombatCharacter>().takeDamage(attProp);
    }
}

