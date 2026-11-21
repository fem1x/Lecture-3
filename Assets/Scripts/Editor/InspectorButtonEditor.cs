using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
[CanEditMultipleObjects]
public sealed class InspectorButtonEditor : Editor
{
    private readonly List<(MethodInfo Method, InspectorButtonAttribute Button)> _buttons = new();

    private void OnEnable()
    {
        _buttons.Clear();
        if (target is not MonoBehaviour component) return;

        const BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Instance |
                                   BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        var seen = new HashSet<MethodInfo>();
        for (var type = component.GetType(); type != typeof(MonoBehaviour); type = type.BaseType)
        {
            foreach (var method in type.GetMethods(flags))
            {
                var button = method.GetCustomAttribute<InspectorButtonAttribute>(inherit: false);
                if (button != null && seen.Add(method.GetBaseDefinition()))
                    _buttons.Add((method, button));
            }
        }
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (_buttons.Count == 0) return;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        var playMode = EditorApplication.isPlaying;
        foreach (var (method, button) in _buttons)
        {
            if (method.IsStatic || method.IsGenericMethod || method.ReturnType != typeof(void) ||
                method.GetParameters().Length != 0)
            {
                EditorGUILayout.HelpBox($"{method.Name} must be a non-generic instance method that returns void and takes no parameters.",
                    MessageType.Warning);
                continue;
            }

            var label = string.IsNullOrEmpty(button.Label) ? ObjectNames.NicifyVariableName(method.Name) : button.Label;
            var tooltip = button.AllowInEditMode ? string.Empty : "Available in Play Mode.";
            using (new EditorGUI.DisabledScope(!playMode && !button.AllowInEditMode))
            {
                if (GUILayout.Button(new GUIContent(label, tooltip)))
                    InvokeOnTargets(method, label, playMode);
            }
        }
    }

    private void InvokeOnTargets(MethodInfo method, string label, bool playMode)
    {
        foreach (var component in targets)
        {
            if (!component || !method.DeclaringType.IsInstanceOfType(component)) continue;

            if (!playMode) Undo.RecordObject(component, label);

            try
            {
                method.Invoke(component, null);
            }
            catch (TargetInvocationException exception)
            {
                Debug.LogException(exception.InnerException ?? exception, component);
            }
            finally
            {
                if (!playMode && component)
                    PrefabUtility.RecordPrefabInstancePropertyModifications(component);
            }
        }
    }
}
