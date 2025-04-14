using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PengstoAssualtForward :MonoBehaviour, IActiveSkill
{
    private bool isPaused = false;
    private float pausedTime;
    private string pausedStateName;
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
