using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimationOverride : MonoBehaviour
{
    private Animator _animator;
    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    internal void SetAniamtion(AnimatorOverrideController animatorOverrideController)
    {
        _animator.runtimeAnimatorController = animatorOverrideController;
    }
}
