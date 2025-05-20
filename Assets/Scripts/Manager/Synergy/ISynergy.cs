using System.Collections.Generic;
using UnityEngine;

public interface ISynergy
{
    /// <summary>
    /// 시너지 태그
    /// </summary>
    string Tag { get; }
    /// <summary>
    /// 시너지 이름
    /// </summary>
    string Name { get; }
    /// <summary>
    /// 시너지 중복 허용 여부
    /// </summary>
    public bool allowDuplicate { get; }

    /// <summary>
    /// 시너지 설명
    /// </summary>
    string synergyDescription { get; }

    /// <summary>
    /// 시너지 아이콘
    /// </summary>
    Sprite synergyIcon { get; }
    /// <summary>
    /// 시너지 티어 임계값
    /// </summary>
    public int[] tierThresholds { get; }
    /// <summary>
    /// 현재 시너지 티어
    /// </summary>
    public int currentTier { get; }
    /// <summary>
    /// 시너지 초기화
    /// </summary>
    /// <param name="unit">적용할 유닛 입니다.</param>
    public void Initialize(UnitController unit);
    /// <summary>
    /// 시너지 카운트 업데이트
    /// 시너지 카운트가 변경될 때마다 호출됩니다.
    /// </summary>
    /// <param name="count">적용할 시너지 카운트</param>
    public void OnCountUpdate(int count);

}

public interface ISynergyGlobal
{
    /// <summary>
    /// 전역 시너지 적용
    /// 시너지 카운트가 변경될 때마다 호출됩니다.
    /// </summary>
    /// <param name="count">적용할 시너지 카운트</param>
    public void ApplyToGlobal(int count);
}

