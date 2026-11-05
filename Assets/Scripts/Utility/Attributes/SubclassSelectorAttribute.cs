using System;
using UnityEngine;

namespace _Scripts.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class SubclassSelectorAttribute : PropertyAttribute
    {
    }
}