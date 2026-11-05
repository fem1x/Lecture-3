#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using _Scripts.Charms;

namespace _Scripts.Editor
{
    public static class CharmLocalizationFiller
    {
        // Имя твоей String Table в окне Localization Tables
        private const string TableCollectionName = "String Table";

        private readonly struct LocalizationPair
        {
            public readonly string NameKey;
            public readonly string DescKey;

            public LocalizationPair(string nameKey, string descKey)
            {
                NameKey = nameKey;
                DescKey = descKey;
            }
        }

        private static readonly Dictionary<string, LocalizationPair> KeyMapping = new()
        {
            { nameof(AirbagCharmConfig), new("charm_airbag_name", "charm_airbag_desc") },
            { nameof(CassetteTapeCharmConfig), new("charm_cassette_tape_name", "charm_cassette_tape_desc") },
            { nameof(HeartShapedBoxCharmConfig), new("charm_heart_shaped_box_name", "charm_heart_shaped_box_desc") },
            { nameof(HourglassCharmConfig), new("charm_hourglass_name", "charm_hourglass_desc") },
            { nameof(LuckyDimeCharmConfig), new("charm_lucky_dime_name", "charm_lucky_dime_desc") },
            { nameof(PorcelainCharmConfig), new("charm_porcelain_name", "charm_porcelain_desc") },
            { nameof(SafetyPinCharmConfig), new("charm_safety_pin_name", "charm_safety_pin_desc") },
            { nameof(TeddyPickerCharmConfig), new("charm_teddy_picker_name", "charm_teddy_picker_desc") },
            { nameof(VelvetGloveCharmConfig), new("charm_velvet_glove_name", "charm_velvet_glove_desc") }
        };

        [MenuItem("Tools/Localization/Charms/Assign All Localization Keys")]
        public static void AssignAllKeys()
        {
            var guids = AssetDatabase.FindAssets("t:CharmConfigBase");
            var updatedCount = 0;

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<CharmConfigBase>(path);

                if (config == null)
                    continue;

                var typeName = config.GetType().Name;

                if (!KeyMapping.TryGetValue(typeName, out var pair))
                    continue;

                var serializedObject = new SerializedObject(config);

                // Заполняем Title и Description
                // Заполняем Name и Description
                SetLocalizedStringKey(serializedObject, "Name", pair.NameKey);
                SetLocalizedStringKey(serializedObject, "Description", pair.DescKey);

                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(config);
                updatedCount++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[CharmLocalizationFiller] Успешно привязаны ключи Title и Desc для конфигов: {updatedCount}");
        }

        private static void SetLocalizedStringKey(SerializedObject so, string propertyName, string entryKey)
        {
            var prop = so.FindProperty($"<{propertyName}>k__BackingField")
                       ?? so.FindProperty($"_{char.ToLowerInvariant(propertyName[0])}{propertyName[1..]}")
                       ?? so.FindProperty(propertyName);

            if (prop == null)
            {
                Debug.LogWarning($"[CharmLocalizationFiller] Не найдено поле '{propertyName}' в объекте {so.targetObject.name}");
                return;
            }

            var tableRefProp = prop.FindPropertyRelative("m_TableReference");
            var entryRefProp = prop.FindPropertyRelative("m_TableEntryReference");

            if (tableRefProp != null)
            {
                var tableNameProp = tableRefProp.FindPropertyRelative("m_TableCollectionName");
                if (tableNameProp != null)
                    tableNameProp.stringValue = TableCollectionName;
            }

            if (entryRefProp != null)
            {
                var entryKeyProp = entryRefProp.FindPropertyRelative("m_Key");
                if (entryKeyProp != null)
                    entryKeyProp.stringValue = entryKey;
            }
        }
    }
}
#endif