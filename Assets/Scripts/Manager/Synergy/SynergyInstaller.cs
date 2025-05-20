using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
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
}
