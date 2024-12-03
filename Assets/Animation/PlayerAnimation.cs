using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{

    private Animator m_Anim;
    private AnimatorStateInfo m_CurrentBaseState;
    void Start()
    {
        m_Anim =GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            walk();
        }
        else
        {
            stopwalking();
        }
    }
    void walk()
    {
        m_Anim.SetBool("CanWalk",true) ;
        m_Anim.SetBool("CanWalk", false);

    }
    void stopwalking()
    {
        m_Anim.SetBool("CanWalk", false);
    }
}
