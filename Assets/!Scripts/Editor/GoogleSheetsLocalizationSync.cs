#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.CSV;
using UnityEngine;
using UnityEngine.Networking;

namespace _Scripts.Editor
{
    public static class GoogleSheetsLocalizationSync
    {
        // Вставь сюда ID своей таблицы и GID листа:
        private const string SpreadsheetId = "1POx7CpQw7xOuYOVnc3mKFRZr6LcvntBIpQ428c9HWJw";
        private const string SheetGid = "672052263";

        // Имя твоей коллекции таблиц в Unity
        private const string TableCollectionName = "String Table";

        [MenuItem("Tools/Localization/Sync from Google Sheets")]
        public static void SyncFromGoogleSheets()
        {
            var url = $"https://docs.google.com/spreadsheets/d/{SpreadsheetId}/gviz/tq?tqx=out:csv&gid={SheetGid}";

            EditorUtility.DisplayProgressBar("Localization Sync", "Загрузка данных из Google Sheets...", 0.2f);

            var request = UnityWebRequest.Get(url);
            var operation = request.SendWebRequest();

            operation.completed += _ =>
            {
                try
                {
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"[Localization Sync] Ошибка загрузки: {request.error}");
                        return;
                    }

                    var csvContent = request.downloadHandler.text;
                    if (string.IsNullOrWhiteSpace(csvContent))
                    {
                        Debug.LogError("[Localization Sync] Получен пустой ответ от Google Sheets.");
                        return;
                    }

                    EditorUtility.DisplayProgressBar("Localization Sync", "Применение таблицы в Unity...", 0.7f);

                    var collection = LocalizationEditorSettings.GetStringTableCollection(TableCollectionName);
                    if (collection == null)
                    {
                        Debug.LogError($"[Localization Sync] Коллекция '{TableCollectionName}' не найдена!");
                        return;
                    }

                    Undo.RecordObject(collection, "Sync Localization from Google Sheets");

                    // Импортируем CSV напрямую в коллекцию
                    using (var reader = new StringReader(csvContent))
                    {
                        Csv.ImportInto(reader, collection);
                    }

                    EditorUtility.SetDirty(collection);
                    EditorUtility.SetDirty(collection.SharedData);
                    foreach (var table in collection.StringTables)
                        EditorUtility.SetDirty(table);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    Debug.Log("[Localization Sync] Успешно синхронизировано из Google Sheets!");
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                    request.Dispose();
                }
            };
        }
    }
}
#endif