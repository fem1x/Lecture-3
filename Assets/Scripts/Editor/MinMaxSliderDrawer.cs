#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
public class MinMaxSliderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        MinMaxSliderAttribute attr = (MinMaxSliderAttribute)attribute;

        if (property.propertyType == SerializedPropertyType.Vector2)
        {
            Vector2 val = property.vector2Value;
            float minVal = val.x;
            float maxVal = val.y;

            position = EditorGUI.PrefixLabel(position, label);

            float fieldWidth = 45f;
            float sliderWidth = position.width - (fieldWidth * 2) - 10f;

            Rect minRect = new Rect(position.x, position.y, fieldWidth, position.height);
            Rect sliderRect = new Rect(position.x + fieldWidth + 5f, position.y, sliderWidth, position.height);
            Rect maxRect = new Rect(position.x + fieldWidth + 5f + sliderWidth + 5f, position.y, fieldWidth, position.height);

            minVal = EditorGUI.FloatField(minRect, minVal);

            EditorGUI.MinMaxSlider(sliderRect, ref minVal, ref maxVal, attr.min, attr.max);

            maxVal = EditorGUI.FloatField(maxRect, maxVal);

            if (minVal > maxVal) minVal = maxVal;
            if (minVal < attr.min) minVal = attr.min;
            if (maxVal > attr.max) maxVal = attr.max;

            property.vector2Value = new Vector2(minVal, maxVal);
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Используйте MinMaxSlider только с Vector2!");
        }
    }
}
#endif
