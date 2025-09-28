using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISkill
{
}

public interface IActiveSkill
{
    public abstract void DoActiveSkill();
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
