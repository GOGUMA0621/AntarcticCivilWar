using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BossController : UnitController
{
    protected override void Start()
    {
        base.Start();
        currentState?.Enter(this);
    }

    protected virtual void Update()
    {
        currentState?.Update();
    }

    protected abstract bool CanSkill();

    protected abstract void UseSkill();
}
