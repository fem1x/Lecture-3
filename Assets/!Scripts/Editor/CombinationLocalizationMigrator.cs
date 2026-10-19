#if UNITY_EDITOR
using _Scripts.Combinations;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization;

namespace _Scripts.Editor
{
    public static class CombinationLocalizationMigrator
    {
        private const string TableCollectionName = "String Table";

        [MenuItem("Tools/Localization/Migrate Combinations to String Table")]
        public static void RebindCombinations()
        {
            var guids = AssetDatabase.FindAssets("t:CombinationConfig");
            if (guids.Length == 0)
            {
                Debug.LogWarning("Конфиги не найдены!");
                return;
            }

            var count = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<CombinationConfig>(path);
                if (config == null) continue;

                var configId = string.IsNullOrWhiteSpace(config.Id) 
                    ? config.name.ToLower().Replace(" ", "_") 
                    : config.Id.ToLower().Replace(" ", "_");

                var nameKey = $"combo_{configId}_name";
                var descKey = $"combo_{configId}_desc";

                var nameLocalizedString = new LocalizedString(TableCollectionName, nameKey);
                var descLocalizedString = new LocalizedString(TableCollectionName, descKey);

                Undo.RecordObject(config, "Rebind Localized Strings");
                config.SetLocalizedStrings(nameLocalizedString, descLocalizedString);
                EditorUtility.SetDirty(config);

                count++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Готово! Перепривязано {count} конфигов.");
        }
    }
}
#endif