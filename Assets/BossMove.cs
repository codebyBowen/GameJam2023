using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMove : StateMachineBehaviour
{
    Transform player;
    Rigidbody2D rb;
    BringerOfDeath boss;

    public float speed = 2.5f;

    public float castCoolOff = 4.0f;
    public float firstCastAt = 1.0f;
    int interval = 1; 
    float nextTime = 0;
    public int castProbabilityPercentage = 20;
    float timeSinceCast = 0;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       player = GameObject.FindGameObjectWithTag("Player").transform;

       rb = animator.transform.parent.GetComponent<Rigidbody2D>();

       boss = animator.transform.parent.GetComponent<BringerOfDeath>();

       timeSinceCast = 0;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timeSinceCast += Time.fixedDeltaTime;
        boss.LookAtPlayer();

        if (timeSinceCast > firstCastAt && !animator.GetBool("HasFirstCasted")) {
            animator.SetTrigger("Cast");
            animator.SetBool("HasFirstCasted", true);
        }

        Vector2 target = new Vector2(player.position.x, rb.position.y);
        Vector2 newPos = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);


        // Check every interval, not checking every frame 
        if (Time.time >= nextTime) {
            if (Random.Range(0, 100) < castProbabilityPercentage && timeSinceCast > castCoolOff) {
                animator.SetTrigger("Cast");
            }
            nextTime += interval;
        }
        rb.MovePosition(newPos);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       timeSinceCast = 0;
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
