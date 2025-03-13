using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageAble //데미지를 입는 속성을 위한 인터페이스
{
    public event Action<GameObject> OnDestroyed;
    public bool IsDestroyed();

    public void ReceiveDamage(DamageData damage);
    
}

public interface IStatusAble : IDamageAble
{
    public void ApplyEffect(DamageData damage);
}