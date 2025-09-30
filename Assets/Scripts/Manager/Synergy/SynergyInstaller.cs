#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class SynergyInstaller
{
    /// <summary>
    /// SynergyTagAttribute에 등록된 SynergyTag와 클래스를 매핑하는 딕셔너리입니다.
    /// </summary>
    public static Dictionary<string, Type> synergyTypeMap = new();
    /// <summary>
    /// SynergyTagAttribute에 등록된 SynergyTag와 SynergyType을 매핑하는 딕셔너리입니다.
    /// SynergyType은 Trait, Effect, Passive로 나뉘어 있습니다.
    /// </summary>
    public static Dictionary<string, SynergyType> synergyTagTypeMap = new();

    /// <summary>
    /// synergyTypeMap과 synergyTagTypeMap을 초기화합니다.
    /// </summary>
    static SynergyInstaller()
    {
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(ISynergy).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract && !t.IsInterface && t.IsSubclassOf(typeof(MonoBehaviour)));

        foreach (var type in types)
        {
            var attr = type.GetCustomAttribute<SynergyTagAttribute>();
            if (attr != null)
            {
                synergyTypeMap[attr.Tag] = type;
            }
        }

        synergyTagTypeMap = synergyTypeMap
        .ToDictionary(a => a.Key,
                      a => a.Value.GetCustomAttribute<SynergyTagAttribute>()?.Type ?? SynergyType.Trait);
    }

    /// <summary>
    /// 영어 시너지 태그(이름)로 한글 이름을 반환합니다.
    /// </summary>
    public static string GetSynergyKoreanName(string tag)
    {
        if (synergyTypeMap.TryGetValue(tag, out var type))
        {
            var attr = type.GetCustomAttribute<SynergyTagAttribute>();
            if (attr != null && !string.IsNullOrEmpty(attr.Tag_KR))
                return attr.Tag_KR;
        }
        return tag; // 못 찾으면 영어 태그 그대로 반환
    }
#if UNITY_EDITOR
    /// <summary>
    /// <see cref="unit"/>에 Synergy를 부착합니다.
    /// </summary>
    /// <param name="unit">이 <see cref="unit"/>에 등록된 시너지를 부착합니다. </param>
    public static void AttachSynergy(UnitController unit)
    {
        foreach (string tag in unit.unit.data.unitSynergyTags)
        {
            if (synergyTypeMap.TryGetValue(tag, out var type))
            {
                var synergyType = Undo.AddComponent(unit.gameObject, type) as ISynergy;
            }
            else
            {
                Debug.LogWarning($"SynergyInstaller: {tag} 가 synergyTypeMap에 없습니다.");
            }
        }
    }
#else
    /// <summary>
    /// <see cref="unit"/>에 Synergy를 부착합니다.
    /// </summary>
    /// <param name="unit">이 <see cref="unit"/>에 등록된 시너지를 부착합니다. </param>
    public static void AttachSynergy(UnitController unit)
    {
        foreach (string tag in unit.unit.data.unitSynergyTags)
        {
            if (synergyTypeMap.TryGetValue(tag, out var type))
            {
                var synergyType = unit.gameObject.AddComponent(type) as ISynergy;
            }
            else
            {
                Debug.LogWarning($"SynergyInstaller: {tag} 가 synergyTypeMap에 없습니다.");
            }
        }
    }
#endif
}