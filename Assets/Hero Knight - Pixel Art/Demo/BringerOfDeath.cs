using UnityEngine;
using System.Collections;
// using System.Math;
using System;

public class BringerOfDeath : MonoBehaviour {

    [SerializeField] float      m_speed = 4.0f;
    public bool isFlipped = false;
    public Transform player;

    [SerializeField] public Animator            m_animator;
    // private Rigidbody2D         m_body2d;
    private BoxCollider2D       m_collider2d;
    private int                 m_facingDirection = 1;
    private int                 m_currentAttack = 0;
    private float               m_timeSinceAttack = 0.0f;
    private float               m_delayToIdle = 0.0f;
    private float               timer = 0.0f;


    // Use this for initialization
    void Start ()
    {
        m_collider2d = GetComponent<BoxCollider2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        // Increase timer that controls attack combo
        m_timeSinceAttack += Time.deltaTime;
    }
    public void LookAtPlayer()
    {
        Vector3 flipped = transform.localScale;
		flipped.z *= -1f;

        // Debug.Log("transform.position.x,  player.position.x, isFlipped" + transform.position.x + ", " + player.position.x + ", " + isFlipped);

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

    public bool CanAttack()
    {
        return m_timeSinceAttack > 0.25f;
    }
}
