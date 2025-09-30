using System;

public enum SynergyType
{
    Faction,
    ClassType,
    Trait,
}

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