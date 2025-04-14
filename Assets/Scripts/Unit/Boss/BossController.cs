using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BossController : UnitController, IDamageAble
{
    protected IUnitState currentState;

    protected override void Start()
    {
        base.Start();
    }

    protected virtual void Update()
    {
        currentState?.Update();
    }
    
    protected abstract bool CanSkill();

    protected abstract void UseSkill();
}
