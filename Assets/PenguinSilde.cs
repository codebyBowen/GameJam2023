using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PenguinSilde : StateMachineBehaviour
{
    Rigidbody2D rb;

    public float slideDuration = 5.0f;
    public bool slideFlag = false;
    public float speed = 8.0f;
    private float slideStartTime = 0;
    private float moveToX;
    
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
      rb = animator.transform.GetComponent<Rigidbody2D>();
      moveToX = animator.GetFloat("MoveToX");
    //    penguin = animator.transform.GetComponent<Penguin>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        slideStartTime += Time.fixedDeltaTime;
        Debug.Log("testSlideTime" + slideStartTime );
         if (slideStartTime >= slideDuration || Mathf.Abs(rb.position.x - moveToX) < 0.0001f) {
             Debug.Log("it should be stop" );
             animator.SetBool("Walk", true);
            //  animator.setTrigger("isSliding", false);
         }

         Vector2 target = new Vector2(moveToX, rb.position.y);
         Vector2 newPos = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
         rb.MovePosition(newPos);
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
    //    animator.ResetTrigger("Slide");
       slideStartTime = 0;
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
