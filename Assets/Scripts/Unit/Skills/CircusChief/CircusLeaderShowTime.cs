using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircusLeaderShowTime : MonoBehaviour, ISkill
{
    [SerializeField] private float showTimeDuration = 5f;

    public void DoSkill()
    {
        StartCoroutine(ShowTimeRoutine());
    }

    private IEnumerator ShowTimeRoutine()
    {
        // 1. circus 시너지를 가진 모든 유닛 찾기
        var circusUnits = UnitManager.instance.allayList.FindAll(unit => unit.TryGetComponent<ISynergy>(out var synergy) && synergy.Tag == "Circus");
        List<IShowtime> showTimeSkills = new List<IShowtime>();

        foreach (var unit in circusUnits)
        {
            if (unit.TryGetComponent<ISynergy>(out var synergy) && synergy.Tag == "Circus") // HasSynergy는 예시, 실제 구현에 맞게 수정
            {
                foreach (var showTimeSkill in unit.GetComponents<IShowtime>())
                {
                    showTimeSkill.StartShowtimeSkill();
                }
            }
        }

        // 2. 일정 시간 대기
        yield return new WaitForSeconds(showTimeDuration);

        // 3. 모든 유닛의 showtime 스킬 종료
        foreach (var showTimeSkill in showTimeSkills)
        {
            showTimeSkill.EndShowtimeSkill();
        }
    }
}
