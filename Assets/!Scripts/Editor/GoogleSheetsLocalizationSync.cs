#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.Plugins.CSV;
using UnityEngine;
using UnityEngine.Networking;

namespace _Scripts.Editor
{
    public static class GoogleSheetsLocalizationSync
    {
        private const string SpreadsheetId = "1POx7CpQw7xOuYOVnc3mKFRZr6LcvntBIpQ428c9HWJw";
        private const string SheetGid = "672052263";
        private const string TableCollectionName = "String Table";

        private const string WebAppUrl = "https://script.google.com/macros/s/AKfycbywNEs8c37bAE3qYTzxnNOoA8O5izgB2ivlvcXHb-T3fxmgC9YyQrohf1C360mKR2sj/exec";

        // =========================================================================
        // PULL (Google Sheets -> Unity)
        // =========================================================================
        [MenuItem("Tools/Localization/Pull from Google Sheets")]
        public static void PullFromGoogleSheets()
        {
            var url = $"https://docs.google.com/spreadsheets/d/{SpreadsheetId}/gviz/tq?tqx=out:csv&gid={SheetGid}";

            EditorUtility.DisplayProgressBar("Localization Sync", "Загрузка таблицы из Google Sheets...", 0.2f);

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
                        Debug.LogError("[Localization Sync] Пустой ответ от Google Sheets.");
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

                    using (var reader = new StringReader(csvContent))
                    {
                        Csv.ImportInto(reader, collection, createUndo: false, reporter: null, removeMissingEntries: true);
                    }

                    EditorUtility.SetDirty(collection);
                    EditorUtility.SetDirty(collection.SharedData);
                    foreach (var table in collection.StringTables)
                        EditorUtility.SetDirty(table);

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                    Debug.Log("[Localization Sync] Успешно затянуто из Google Sheets!");
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                    request.Dispose();
                }
            };
        }

        // =========================================================================
        // PUSH (Unity -> Google Sheets)
        // =========================================================================
        [MenuItem("Tools/Localization/Push to Google Sheets")]
        public static void PushToGoogleSheets()
        {
            if (string.IsNullOrWhiteSpace(WebAppUrl) || WebAppUrl.Contains("ВСТАВЬ_СЮДА"))
            {
                Debug.LogError("[Localization Sync] Укажи WebAppUrl в скрипте GoogleSheetsLocalizationSync.cs!");
                return;
            }

            var collection = LocalizationEditorSettings.GetStringTableCollection(TableCollectionName);
            if (collection == null)
            {
                Debug.LogError($"[Localization Sync] Коллекция '{TableCollectionName}' не найдена!");
                return;
            }

            EditorUtility.DisplayProgressBar("Localization Sync", "Экспорт данных из Unity...", 0.2f);

            string csvContent;
            using (var writer = new StringWriter())
            {
                Csv.Export(writer, collection);
                csvContent = writer.ToString();
            }

            csvContent = CleanCsvColumns(csvContent);

            EditorUtility.DisplayProgressBar("Localization Sync", "Отправка данных в Google Sheets...", 0.6f);

            var form = new WWWForm();
            form.AddField("csv", csvContent);

            var request = UnityWebRequest.Post(WebAppUrl, form);
            var operation = request.SendWebRequest();

            operation.completed += _ =>
            {
                try
                {
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"[Localization Sync] Ошибка отправки: {request.error}");
                        return;
                    }

                    var response = request.downloadHandler.text;
                    if (response.Contains("SUCCESS"))
                    {
                        Debug.Log("[Localization Sync] Успешно выгружено в Google Sheets!");
                    }
                    else
                    {
                        Debug.LogError($"[Localization Sync] Ответ сервера: {response}");
                    }
                }
                finally
                {
                    EditorUtility.ClearProgressBar();
                    request.Dispose();
                }
            };
        }

        private static string CleanCsvColumns(string csv)
        {
            if (string.IsNullOrWhiteSpace(csv)) return csv;

            var lines = csv.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            if (lines.Length == 0) return csv;

            var headers = lines[0].Split(',');
            var keepIndices = new List<int>();

            for (var i = 0; i < headers.Length; i++)
            {
                var cleanHeader = headers[i].Trim('"', ' ');
                var isId = string.Equals(cleanHeader, "Id", StringComparison.OrdinalIgnoreCase);
                var isComment = cleanHeader.EndsWith("Comments", StringComparison.OrdinalIgnoreCase) || 
                                cleanHeader.EndsWith("Comment", StringComparison.OrdinalIgnoreCase);

                if (!isId && !isComment)
                    keepIndices.Add(i);
            }

            var sb = new StringBuilder();
            for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var line = lines[lineIndex];
                if (string.IsNullOrEmpty(line) && lineIndex == lines.Length - 1) continue;

                var columns = line.Split(',');
                var filtered = new string[keepIndices.Count];
                for (var i = 0; i < keepIndices.Count; i++)
                {
                    var colIdx = keepIndices[i];
                    filtered[i] = colIdx < columns.Length ? columns[colIdx] : "";
                }

                sb.AppendLine(string.Join(",", filtered));
            }

            return sb.ToString();
        }
    }
}
#endif