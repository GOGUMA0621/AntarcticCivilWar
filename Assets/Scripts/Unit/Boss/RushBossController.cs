using DG.Tweening;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RushBossController : BossController
{
    private bool isSkillActive = false;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        isSkillActive = false;
        currentState = GetInitialState();
        currentState.Enter(this);
        ChangeState(new RushBossBattlefieldCrusherState());
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (CanSkill() && !isSkillActive)
        {
            isSkillActive = true;
            UseSkill();
        }
    }

    protected override IUnitState GetInitialState()
    {
        return new RushBossIdleState();
    }

    public override void ReceiveDamage(DamageData damage)
    {
        float damageBase = damage.damage;
        float damageAmount = damageBase * 0.9f;
        if (currentHP <= maxHP * 0.5f)
        {
            float reducedDamage = damageBase * 0.8f;
            damage.damage = reducedDamage;
            base.ReceiveDamage(damage);
        }
        else
        {
            damage.damage = damageAmount;
            base.ReceiveDamage(damage);
        }
    }

    protected override bool CanSkill()
    {
        return currentHP <= maxHP * 0.5f;
    }

    protected override void UseSkill()
    {
        ChangeState(new RushBossBattlefieldCrusherState());
    }

    public void AdvanceForward()
    {
        StartCoroutine(MoveForwardCoroutine());
    }

    private IEnumerator MoveForwardCoroutine()
    {
        PauseAnimation();
        unit.aiPath.canMove = false; // AIPath 중지
        unit.rb.velocity = Vector2.zero;

        Vector2 direction = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        float moveDistance = 3f;
        float moveTime = 0.3f;

        float elapsed = 0f;
        while (elapsed < moveTime)
        {
            unit.rb.MovePosition(unit.rb.position + direction * (moveDistance / moveTime) * Time.fixedDeltaTime);

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        unit.rb.velocity = Vector2.zero;
        ResumeAnimation();
    }

    public void JumpStart()
    {
        unit.aiPath.canMove = false;
        PauseAnimation();

        float jumpHeight = 2f;
        float jumpDuration = 0.3f; // 상승 시간

        transform.DOMoveZ(transform.position.z - jumpHeight, jumpDuration)
            .SetEase(Ease.OutQuad).OnComplete(() => 
            { 
                ResumeAnimation();
            });
    }

    public void FallStart()
    {
        PauseAnimation();
        float fallDuration = 0.3f;

        transform.DOMoveZ(transform.position.z + 2f, fallDuration) // 다시 원래 Z로
            .SetEase(Ease.InQuad).OnComplete(() =>
            { 
                ResumeAnimation();
            });
    }

    public void SmashImpact()
    {
        Vector2 center = transform.position;
        float radius = 4f;
        float knockbackForce = 2f;
        float stunDuration = 3f;
        float damage = 100f;

        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out IDamageAble target) && hit.gameObject.tag != gameObject.tag)
            {
                // 넉백
                if (hit.TryGetComponent(out Rigidbody2D targetRb))
                {
                    Vector2 knockDir = ((Vector2)hit.transform.position - center).normalized;
                    targetRb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
                }

                // 피해 + 스턴 효과
                target.ReceiveDamage(new DamageData(damage, StatusEffectType.Stun, stunDuration));
            }
        }
    }

    public void ToggleAI(bool toggle)
    {
        unit.aiPath.canMove = toggle;
    }

    public void MeleeAttack(float damage)
    {
        DamageData damageData = new DamageData(damage, StatusEffectType.None, 0);

        Collider2D[] collider = Physics2D.OverlapCircleAll(transform.position, unit.data.UnitSenseRadius);
        foreach (Collider2D targetCollider in collider)
        {
            if (targetCollider.transform == unit.detectTarget.targetToAttack)
            {
                IDamageAble target = targetCollider.GetComponent<IDamageAble>();
                target.ReceiveDamage(damageData);
            }
        }
    }
}
