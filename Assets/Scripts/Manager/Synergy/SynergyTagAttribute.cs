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
    public SynergyType Type { get; }

    public SynergyTagAttribute(string tag, SynergyType type)
    {
        Tag = tag;
        Type = type;
    }
}