using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dancing : MonoBehaviour
{
    private Animator  m_animator;
    // Start is called before the first frame update
    void Start()
    {
        m_animator = GetComponent<Animator>();
        
    }

    void FixedUpdate() {
        StartCoroutine(Dance());
    }

    IEnumerator Dance() 
    {
        yield return new WaitForSeconds(1);
        m_animator.SetTrigger("Hurt");
    }
}
