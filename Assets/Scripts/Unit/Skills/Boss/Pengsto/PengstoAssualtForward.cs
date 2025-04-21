using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PengstoAssualtForward :MonoBehaviour, IActiveSkill
{
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void DoActiveSkill()
    {
        animator.Play("ManaSkill");
    }
}
