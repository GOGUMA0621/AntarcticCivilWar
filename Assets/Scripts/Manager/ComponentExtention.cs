using UnityEngine;

public static class ComponentExtention
{
    public static Transform GetTransform(this object obj)
    {
        return (obj as Component)?.transform;
    }
}
