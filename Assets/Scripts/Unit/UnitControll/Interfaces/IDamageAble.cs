using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 유닛이 데미지를 받을 수 있는 인터페이스입니다.
/// 유닛은 이 인터페이스를 구현하여 데미지를 받을 수 있습니다.
/// </summary>
public interface IDamageAble
{
    /// <summary>
    /// 유닛이 파괴되었을 때 호출되는 이벤트입니다.
    /// </summary>
    public event Action<IDamageAble> OnDestroyed;
    /// <summary>
    /// 유닛이 파괴되었는지 여부를 반환합니다.
    /// </summary>
    public bool IsDestroyed();
    /// <summary>
    /// 유닛이 데미지를 받는 메서드입니다.
    /// </summary>
    /// <param name="damage">가해지는 <see cref="DamageData"/> 입니다. </param>
    public void ReceiveDamage(DamageData damage);
}
/// <summary>
/// 상태이상이 적용되는 객체 인터페이스입니다.
/// 상태이상은 DamageData를 통해 적용됩니다.
/// </summary>
public interface IStatusAble : IDamageAble
{
    public void ApplyEffect(DamageData damage);
}