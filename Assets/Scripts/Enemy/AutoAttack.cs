using UnityEngine;

public class AutoAttack : StateMachineBehaviour {

    Transform player;
    Rigidbody2D rb;
    public float attackRange = 4f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       player = GameObject.FindGameObjectWithTag("Player").transform;
       rb = animator.transform.parent.GetComponent<Rigidbody2D>();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      if (Vector2.Distance(player.position, rb.position) <= attackRange) {
          animator.SetTrigger("Attack");
      }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
      animator.ResetTrigger("Attack");
    }
}
