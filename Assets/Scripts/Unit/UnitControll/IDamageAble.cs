using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageAble //데미지를 입는 속성을 위한 인터페이스
{
    public void ReceiveDamage(float damageAmount);
}
