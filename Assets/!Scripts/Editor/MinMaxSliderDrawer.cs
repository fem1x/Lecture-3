using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
public class MinMaxSliderDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        MinMaxSliderAttribute attr = (MinMaxSliderAttribute)attribute;

        // Атрибут работает только с типом Vector2 (x = min, y = max)
        if (property.propertyType == SerializedPropertyType.Vector2)
        {
            Vector2 val = property.vector2Value;
            float minVal = val.x;
            float maxVal = val.y;

            // Рисуем имя переменной
            position = EditorGUI.PrefixLabel(position, label);

            // Резервируем место под слайдер и текстовые поля по бокам
            float fieldWidth = 45f;
            float sliderWidth = position.width - (fieldWidth * 2) - 10f;

            Rect minRect = new Rect(position.x, position.y, fieldWidth, position.height);
            Rect sliderRect = new Rect(position.x + fieldWidth + 5f, position.y, sliderWidth, position.height);
            Rect maxRect = new Rect(position.x + fieldWidth + 5f + sliderWidth + 5f, position.y, fieldWidth, position.height);

            // Текстовое поле MIN
            minVal = EditorGUI.FloatField(minRect, minVal);

            // Сам Min/Max Слайдер
            EditorGUI.MinMaxSlider(sliderRect, ref minVal, ref maxVal, attr.min, attr.max);

            // Текстовое поле MAX
            maxVal = EditorGUI.FloatField(maxRect, maxVal);

            // Страховка значений
            if (minVal > maxVal) minVal = maxVal;
            if (minVal < attr.min) minVal = attr.min;
            if (maxVal > attr.max) maxVal = attr.max;

            // Записываем обратно
            property.vector2Value = new Vector2(minVal, maxVal);
        }
        else
        {
            EditorGUI.LabelField(position, label.text, "Используйте MinMaxSlider только с Vector2!");
        }
    }
}
