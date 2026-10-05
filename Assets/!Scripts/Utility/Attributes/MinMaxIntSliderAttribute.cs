using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class MinMaxIntSliderAttribute : PropertyAttribute
{
    public int min;
    public int max;

    public MinMaxIntSliderAttribute(int min, int max)
    {
        this.min = min;
        this.max = max;
    }
}