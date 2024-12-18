using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "DummySkill", menuName = "ManaSkills/DummySkill")]

public class DummySkill : ScriptableObject, IUnitSkill
{

    public void Execute(UnitController unit)
    {
        Debug.Log("스킬 사용");

    }

    public class ChefSkill : ScriptableObject, IUnitSkill
    {

        public void Execute(UnitController unit)
        {
            Debug.Log("스킬 사용");
            

        }
    }
}