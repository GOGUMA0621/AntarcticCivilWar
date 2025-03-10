using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitAnimationOverride : Unit // 유닛의 애니메이션 덧씨우는 장치
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
