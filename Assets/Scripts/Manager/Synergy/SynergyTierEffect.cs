using System.Collections.Generic;

public class SynergyTierEffect
{
    public int RequiredCount; // 해당 티어에 필요한 캐릭터 수
    public string Description; // 효과 설명
    public Dictionary<StatType, float> StatModifiers; // 적용할 스탯 변화
    // 필요시 추가 효과(예: 첫 공격 추가 피해, 마나 회복 등) 필드 추가
}