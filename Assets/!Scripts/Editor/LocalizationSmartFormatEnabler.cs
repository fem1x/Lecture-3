#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;

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
            var sharedData = collection.SharedData;

            // Обходим все доступные таблицы локалей (ru, en, es и т.д.)
            foreach (var tableAsset in collection.Tables)
            {
                var table = tableAsset.asset as StringTable;
                if (table == null)
                    continue;

                foreach (var sharedEntry in sharedData.Entries)
                {
                    var entry = table.GetEntry(sharedEntry.Id);
                    if (entry == null || string.IsNullOrEmpty(entry.Value))
                        continue;

                    if (entry.Value.Contains("{") && entry.Value.Contains("}"))
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

            Debug.Log($"[LocalizationSmartFormatEnabler] Успешно включён Smart Format для {updatedEntries} записей во всех локалях.");
        }
    }
}
#endif