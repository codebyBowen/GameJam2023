using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenguinMove : StateMachineBehaviour
{
    Transform player;
    Rigidbody2D rb;
    Penguin penguin;

    public float speed = 2.5f;
    public float attackRange = 2f;

    public float castCoolOff = 4.0f;
    public float firstCastAt = 1.0f;
    int interval = 1; 
    float nextTime = 0;
    public int castProbabilityPercentage = 50;
    float timeSinceCast = 0;

    public bool shouldMoveToPlayer = false;


    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       player = GameObject.FindGameObjectWithTag("Player").transform;

       rb = animator.transform.GetComponent<Rigidbody2D>();

       penguin = animator.transform.GetComponent<Penguin>();

       timeSinceCast = 0;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timeSinceCast += Time.fixedDeltaTime;
        penguin.LookAtPlayer();

        // if (timeSinceCast > firstCastAt && !animator.GetBool("isSliding")) {
        //     animator.SetTrigger("isSliding");
        //     animator.SetBool("Slide", true);
        // }


        if (shouldMoveToPlayer) {
            Vector2 target = new Vector2(player.position.x, rb.position.y);
            Vector2 newPos = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
        }


        if (Vector2.Distance(player.position, rb.position) <= attackRange) {
            animator.SetTrigger("Attack");
        } else {
            // Check every interval, not checking every frame 
            if (Time.time >= nextTime) {
                if (Random.Range(0, 100) < castProbabilityPercentage && timeSinceCast > castCoolOff ) { 
                    bool atLeftOfPlayer = animator.transform.position.x < player.position.x;
                    var randomOffset = Random.Range(1f, 10f);
                    var playerX = player.position.x;

                    // Slide beyond the player, not just stop right at the position
                    animator.SetFloat("MoveToX", atLeftOfPlayer ? playerX + randomOffset : playerX - randomOffset);
                    animator.SetTrigger("Slide");
                    animator.SetBool("IsSliding", true);
                } else if (Random.Range(0, 100) > castProbabilityPercentage) {
                    animator.SetBool("Jump", true);
                }
                nextTime += interval;
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       animator.ResetTrigger("Attack");
       timeSinceCast = 0;
    }
}
