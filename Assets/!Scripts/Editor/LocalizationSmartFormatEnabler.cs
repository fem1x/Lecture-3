#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;

namespace _Scripts.Editor
{
    public static class LocalizationSmartFormatEnabler
    {
        private const string TableCollectionName = "String Table";

        [MenuItem("Tools/Localization/Charms/Enable Smart Strings For Table")]
        public static void EnableSmartStrings()
        {
            var collection = LocalizationEditorSettings.GetStringTableCollection(TableCollectionName);
            if (collection == null)
            {
                Debug.LogError($"[LocalizationSmartFormatEnabler] Таблица '{TableCollectionName}' не найдена!");
                return;
            }

            var updatedEntries = 0;

            foreach (var table in collection.StringTables)
            {
                foreach (var entry in table.Values)
                {
                    // Включаем Smart Format только для строк с плейсхолдерами {}
                    if (entry.Value != null && entry.Value.Contains("{") && entry.Value.Contains("}"))
                    {
                        if (!entry.IsSmart)
                        {
                            entry.IsSmart = true;
                            updatedEntries++;
                        }
                    }
                }

                EditorUtility.SetDirty(table);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[LocalizationSmartFormatEnabler] Успешно включён Smart Format для {updatedEntries} записей.");
        }
    }
}
#endif