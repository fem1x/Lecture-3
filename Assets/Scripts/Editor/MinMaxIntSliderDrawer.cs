#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MinMaxIntSliderAttribute))]
public class MinMaxIntSliderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        MinMaxIntSliderAttribute attr = (MinMaxIntSliderAttribute)attribute;

        if (property.propertyType == SerializedPropertyType.Vector2Int)
        {
            Vector2Int val = property.vector2IntValue;
            float minVal = val.x;
            float maxVal = val.y;

            position = EditorGUI.PrefixLabel(position, label);

            float fieldWidth = 45f;
            float sliderWidth = position.width - (fieldWidth * 2) - 10f;

            Rect minRect = new Rect(position.x, position.y, fieldWidth, position.height);
            Rect sliderRect = new Rect(position.x + fieldWidth + 5f, position.y, sliderWidth, position.height);
            Rect maxRect = new Rect(position.x + fieldWidth + 5f + sliderWidth + 5f, position.y, fieldWidth, position.height);

            minVal = EditorGUI.IntField(minRect, Mathf.RoundToInt(minVal));

            EditorGUI.MinMaxSlider(sliderRect, ref minVal, ref maxVal, attr.min, attr.max);

            maxVal = EditorGUI.IntField(maxRect, Mathf.RoundToInt(maxVal));

            int finalMin = Mathf.Clamp(Mathf.RoundToInt(minVal), attr.min, attr.max);
            int finalMax = Mathf.Clamp(Mathf.RoundToInt(maxVal), attr.min, attr.max);

            if (finalMin > finalMax) finalMax = finalMin;

            property.vector2IntValue = new Vector2Int(finalMin, finalMax);
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Используйте MinMaxIntSlider только с Vector2Int!");
        }
    }
}
#endif