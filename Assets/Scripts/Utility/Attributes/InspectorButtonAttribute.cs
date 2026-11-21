using System;

[AttributeUsage(AttributeTargets.Method)]
public sealed class InspectorButtonAttribute : Attribute
{
    public string Label { get; }
    public bool AllowInEditMode { get; set; }

    public InspectorButtonAttribute(string label = null)
    {
        Label = label;
    }
}
