﻿using UnityEngine;
using System.Collections;
// using System.Math;
using System;

public class HeroKnight : CombatCharacter {

    [SerializeField] float      m_speed = 4.0f;
    [SerializeField] float      m_jumpForce = 7.5f;
    [SerializeField] float      m_rollForce = 6.0f;
    [SerializeField] bool       m_noBlood = false;
    [SerializeField] GameObject m_slideDust;

    [SerializeField] bool       m_myopia = false;
    [SerializeField] bool       m_associative_agnosia = false;
    [SerializeField] bool       m_ice_skater = false;
    [SerializeField] bool       m_limb_length_discrepancy = false;
    // [SerializeField] float      m_coefficient = 0.0f;
    // [SerializeField] float      m_acceleration = 0.0f;
    [SerializeField] float      m_slip_time = 1.0f;
    private float elapsedTime = 0f;
    public bool onRhythm = false; 

    public float attackRange = 0.5f;
    public int attackDamage = 40;
    public float attackRate = 2;
    public Transform attackPoint;
    public LayerMask enemyLayers;
    
    // music energy system
    public float musicEnergy = 0;
    public float energyIncrement = 20.0f;
    public float maxMusicEnergy = 100.0f;

    private Animator            m_animator;
    private Rigidbody2D         m_body2d;
    private BoxCollider2D       m_collider2d;
    private Sensor_HeroKnight   m_groundSensor;
    private Sensor_HeroKnight   m_wallSensorR1;
    private Sensor_HeroKnight   m_wallSensorR2;
    private Sensor_HeroKnight   m_wallSensorL1;
    private Sensor_HeroKnight   m_wallSensorL2;
    private GameObject          m_attackSensor;
    private bool                m_isWallSliding = false;
    private bool                m_grounded = false;
    private bool                m_rolling = false;
    private int                 m_facingDirection = 1;
    private int                 m_currentAttack = 0;
    private float               m_timeSinceAttack = 0.0f;
    private float               m_delayToIdle = 0.0f;
    private float               m_rollDuration = 1.0f;
    private float               m_rollCurrentTime;
    private float               timer = 0.0f;
    public GameObject           goodSignal;
    public GameObject           badSignal;

// string[] weekDays = new string[] { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };
    private string[] attackSequence = new string[] {"Attack1","Attack1","Attack2","Attack3"};



    // Use this for initialization
    void Start ()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_collider2d = GetComponent<BoxCollider2D>();

        health.dieCB = PlayerDie;

        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();
    }

    // Update is called once per frame
    void FixedUpdate ()
    {
        elapsedTime += Time.fixedDeltaTime;
        ShowOnRhythm();
        musicEnergyCalculation();
        // if (m_ice_skater) {
        //     m_coefficient = 1.2f;
        //     m_acceleration = -0.5f;
        // }
        // Increase timer that controls attack combo
        m_timeSinceAttack += Time.deltaTime;

        // Increase timer that checks roll duration
        if(m_rolling) {
            m_rollCurrentTime += Time.deltaTime;
            GetComponent<Collider2D>().enabled = false;
        }
            

        // Disable rolling if timer extends duration
        if(m_rollCurrentTime > m_rollDuration) {
            m_rollCurrentTime = 0;
            m_rolling = false;
            GetComponent<Collider2D>().enabled = true;
        }
            
        //Check if character just landed on the ground
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        //Check if character just started falling
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        // -- Handle input and movement --
        float inputX = Input.GetAxis("Horizontal");

        // Swap direction of sprite depending on walk direction
        if (inputX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;
            timer = 0.0f;
        }
            
        else if (inputX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
            timer = 0.0f;
        }

        // Move
        if (!m_rolling )
            m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);
            // During ice_skater status
            if (m_ice_skater && Math.Abs(inputX) < 1e-6 && timer <= m_slip_time && Math.Abs(m_body2d.velocity.y) < 1e-6) {
              timer += Time.deltaTime;
              m_body2d.velocity = new Vector2(m_speed * m_facingDirection * (m_slip_time - timer), m_body2d.velocity.y); 
            }

        //Set AirSpeed in animator
        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);

        // -- Handle Animations --
        //Wall Slide
        m_isWallSliding = (m_wallSensorR1.State() && m_wallSensorR2.State()) || (m_wallSensorL1.State() && m_wallSensorL2.State());
        m_animator.SetBool("WallSlide", m_isWallSliding);

        //Death
        if (Input.GetKeyDown("e") && !m_rolling)
        {
            m_animator.SetBool("noBlood", m_noBlood);
            m_animator.SetTrigger("Death");
        }
            
        //Hurt
        else if (Input.GetKeyDown("q") && !m_rolling) {
            m_animator.SetTrigger("Hurt");
            health.changeHP(-1);
        }
            
        //Attack
        else if(Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f && !m_rolling)
        {
            OnRhythmAttack();
            m_currentAttack++;

            // Loop back to one after third attack
            if (m_currentAttack > 4)
                m_currentAttack = 1;

            // Reset Attack combo if time since last attack is too large
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            // Call one of three attack animations "Attack1", "Attack2", "Attack3"
            m_animator.SetTrigger(attackSequence[m_currentAttack-1]);

            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

            foreach(Collider2D enemy in hitEnemies) {
                Debug.Log("Hit " + enemy.name);
                enemy.GetComponent<CombatCharacter>().takeDamage(new AttackProp(attackDamage));
            }

            // Reset timer
            m_timeSinceAttack = 0.0f;
        }

        // Block
        else if (Input.GetMouseButtonDown(1) && !m_rolling)
        {
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
        }

        else if (Input.GetMouseButtonUp(1))
            m_animator.SetBool("IdleBlock", false);

        // Roll
        else if (Input.GetKeyDown("left shift") && !m_rolling && !m_isWallSliding)
        {
            m_rolling = true;
            m_animator.SetTrigger("Roll");
            m_body2d.velocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.velocity.y);
            // m_collider2d.enabled = false;
        }
            

        // Jump Banned
        // else if (Input.GetKeyDown("space") && m_grounded && !m_rolling)
        // {
        //     m_animator.SetTrigger("Jump");
        //     m_grounded = false;
        //     m_animator.SetBool("Grounded", m_grounded);
        //     m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
        //     m_groundSensor.Disable(0.2f);
        // }

        //Run
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            // Reset timer
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }

        //Idle
        else
        {
            // Prevents flickering transitions to idle
            m_delayToIdle -= Time.deltaTime;
                if(m_delayToIdle < 0)
                    m_animator.SetInteger("AnimState", 0);
        }
    }

    // Animation Events
    // Called in slide animation.
    void AE_SlideDust()
    {
        Vector3 spawnPosition;

        if (m_facingDirection == 1)
            spawnPosition = m_wallSensorR2.transform.position;
        else
            spawnPosition = m_wallSensorL2.transform.position;

        if (m_slideDust != null)
        {
            // Set correct arrow spawn position
            GameObject dust = Instantiate(m_slideDust, spawnPosition, gameObject.transform.localRotation) as GameObject;
            // Turn arrow in correct direction
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }

    void OnDrawGizmosSelected() {
        if (!attackPoint) {
            return;
        }

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    int isHitOnRhythm = -1;

    void musicEnergyCalculation() {
        if ( musicEnergy <= 0) {
            musicEnergy = 0;
            return;
        } else if( musicEnergy <= maxMusicEnergy && musicEnergy >= 0) {
            elapsedTime += Time.fixedDeltaTime;
            if (elapsedTime >= 1) {
                // decrease 5% of max energy per second
                musicEnergy -= maxMusicEnergy * 0.05f;
            }
        }
    }

    void ShowOnRhythm() {
        // Debug.Log("m_timeSinceAttack " + m_timeSinceAttack);
        if (m_timeSinceAttack >= 0 && m_timeSinceAttack < 0.3f) {
            if (isHitOnRhythm < 0) {
                if (onRhythm) {
                    isHitOnRhythm = 1;
                    musicEnergy += energyIncrement;
                } else {
                    isHitOnRhythm = 0;
                    // musicEnergy -= energyIncrement / 2;
                }
            }

            if (isHitOnRhythm > 0) {
                goodSignal.SetActive(true);
            } else {
                badSignal.SetActive(true);
            }
        } else {
            goodSignal.SetActive(false);
            badSignal.SetActive(false);
            isHitOnRhythm = -1;
        }
    }

    void OnRhythmAttack() {
        Debug.Log("onRhythm" + onRhythm);
        if (onRhythm) {
            attackDamage = 60;
        } else {
            attackDamage = 20;
        }
    }

    // Death
    void PlayerDie() {
        if (m_animator.GetBool("IsDead")) {
            return;
        }
        m_animator.SetBool("noBlood", m_noBlood);
        m_animator.SetTrigger("Death");
        m_animator.SetBool("IsDead", true);
        this.enabled = false;
        // GetComponent<Collider2D>().enabled = false;  
    }
}
