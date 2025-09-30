using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISkill
{
}

public interface IActiveSkill : ISkill
{
    bool IsDurationSkill { get; }
    bool IsStandingSkill { get; }
    float Duration { get; }
    public abstract void ActivateSkill(UnitController unit);
    public abstract void DeactivateSkill(UnitController unit);
}

public interface IPasseiveSkillAttack
{
    public abstract void DoPassiveSkill();
    public abstract bool PassiveCondition();
}

public interface IShowtime : ISkill
{
    public abstract void StartShowtimeSkill();
    public abstract void EndShowtimeSkill();
}
