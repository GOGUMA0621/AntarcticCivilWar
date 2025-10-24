using System;

public enum SynergyType
{
    Faction,
    ClassType,
    Trait,
}
/// <summary>
/// 시너지 태그 어트리뷰트
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SynergyTagAttribute : Attribute
{
    public string Tag { get; }
    public string Tag_KR { get;}
    public SynergyType Type { get; }

    public SynergyTagAttribute(string tag, string name, SynergyType type)
    {
        Tag = tag;
        Tag_KR = name;
        Type = type;
    }
}