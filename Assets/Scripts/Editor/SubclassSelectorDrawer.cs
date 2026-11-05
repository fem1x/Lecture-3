using System;
using System.Linq;
using System.Reflection;
using _Scripts.Attributes;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Editor
{
    [CustomPropertyDrawer(typeof(SubclassSelectorAttribute))]
    public class SubclassSelectorDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            var typeName = property.managedReferenceFullTypename;
            var currentTypeName = string.IsNullOrEmpty(typeName)
                ? "<Null>"
                : typeName.Split(' ').Last().Split('.').Last();

            var buttonRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            if (GUI.Button(buttonRect, $"{label.text}: {currentTypeName}", EditorStyles.popup))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("<Null>"), string.IsNullOrEmpty(typeName), () =>
                {
                    property.managedReferenceValue = null;
                    property.serializedObject.ApplyModifiedProperties();
                });

                var fieldType = GetFieldOrPropertyType();
                if (fieldType != null)
                {
                    var types = TypeCache.GetTypesDerivedFrom(fieldType)
                        .Where(t => !t.IsAbstract && !t.IsInterface && t.GetConstructor(Type.EmptyTypes) != null);

                    foreach (var type in types)
                    {
                        var entryName = type.Name;
                        menu.AddItem(new GUIContent(entryName), currentTypeName == entryName, () =>
                        {
                            property.managedReferenceValue = Activator.CreateInstance(type);
                            property.serializedObject.ApplyModifiedProperties();
                        });
                    }
                }

                menu.ShowAsContext();
            }

            var bodyRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, position.height - EditorGUIUtility.singleLineHeight - 2);
            if (!string.IsNullOrEmpty(typeName))
            {
                EditorGUI.PropertyField(bodyRect, property, GUIContent.none, true);
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType != SerializedPropertyType.ManagedReference)
                return EditorGUI.GetPropertyHeight(property, label, true);

            var height = EditorGUIUtility.singleLineHeight;
            if (!string.IsNullOrEmpty(property.managedReferenceFullTypename))
            {
                height += EditorGUI.GetPropertyHeight(property, true) + 2;
            }

            return height;
        }

        private Type GetFieldOrPropertyType()
        {
            return fieldInfo != null ? fieldInfo.FieldType : null;
        }
    }
}