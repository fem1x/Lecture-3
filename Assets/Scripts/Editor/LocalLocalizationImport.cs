using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine;
using UnityEngine.Localization.Tables;

namespace _Scripts.Editor
{
    [InitializeOnLoad]
    internal static class LocalLocalizationImport
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private const string CollectionName = "String Table";
        private const string QueueName = "LocalizationChanges.json";
        private const string UndoLabel = "Apply local localization changes";
        private static readonly string[] Locales = { "en", "ru", "es", "ja", "ko", "zh-Hans" };
        private static readonly string ProjectRoot = Path.GetDirectoryName(Application.dataPath);
        private static readonly string QueuePath = Path.Combine(ProjectRoot, QueueName);
        private static readonly string ReceiptFolder = Path.Combine(ProjectRoot, "Library", "AppliedLocalization");

        private static string _lastAttempt;
        private static double _nextPoll;

        [Serializable]
        private sealed class Patch
        {
            public Row[] entries;
        }

        [Serializable]
        private sealed class Row
        {
            public string key;
            public string en;
            public string ru;
            public string es;
            public string ja;
            public string ko;
            public string zhHans;
            public bool smart;

            public string[] GetValues() => new[] { en, ru, es, ja, ko, zhHans };
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        static LocalLocalizationImport()
        {
            if (!AssetDatabase.IsAssetImportWorkerProcess())
                EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            var time = EditorApplication.timeSinceStartup;
            if (time < _nextPoll) return;

            _nextPoll = time + 1;
            if (CanImport()) Import(manual: false);
        }

        [MenuItem("Tools/Localization/Apply Local Changes")]
        private static void ApplyLocalChanges() => Import(manual: true);

        [MenuItem("Tools/Localization/Apply Local Changes", true)]
        private static bool CanImport() => !EditorApplication.isPlayingOrWillChangePlaymode &&
            !EditorApplication.isCompiling && !EditorApplication.isUpdating;

        private static void Import(bool manual)
        {
            if (!File.Exists(QueuePath))
            {
                _lastAttempt = null;
                if (manual) Debug.Log($"[Localization] No pending {QueueName}.");
                return;
            }

            string json;
            try
            {
                json = File.ReadAllText(QueuePath);
            }
            catch (IOException exception)
            {
                if (manual) Debug.LogWarning($"[Localization] {exception.Message}");
                return;
            }

            if (!manual && json == _lastAttempt) return;
            _lastAttempt = json;

            try
            {
                var rows = ReadPatch(json);
                ApplyPatch(rows);
                Directory.CreateDirectory(ReceiptFolder);
                var receipt = Path.Combine(ReceiptFolder, $"{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.json");
                File.Move(QueuePath, receipt);
                Debug.Log($"[Localization] Applied {rows.Length} key(s) to {CollectionName}. Receipt: {receipt}");
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Localization] {QueueName} is still pending: {exception.Message}");
            }
        }

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====

        private static Row[] ReadPatch(string json)
        {
            var rows = JsonUtility.FromJson<Patch>(json)?.entries;
            if (rows == null || rows.Length == 0)
                throw new InvalidDataException("Expected a nonempty entries array.");

            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var row in rows)
            {
                if (row == null || string.IsNullOrWhiteSpace(row.key) || row.key != row.key.Trim())
                    throw new InvalidDataException("Every row needs a key without surrounding whitespace.");
                if (!keys.Add(row.key))
                    throw new InvalidDataException($"Duplicate key '{row.key}'.");

                var values = row.GetValues();
                for (var i = 0; i < values.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(values[i]))
                        throw new InvalidDataException($"'{row.key}' is missing its {Locales[i]} translation.");
                }
            }

            return rows;
        }

        private static void ApplyPatch(Row[] rows)
        {
            var collection = LocalizationEditorSettings.GetStringTableCollection(CollectionName);
            if (collection == null)
                throw new InvalidOperationException($"Collection '{CollectionName}' was not found.");

            var tablesByLocale = collection.StringTables.ToDictionary(table => table.LocaleIdentifier.Code);
            var tables = Locales.Select(locale => tablesByLocale.TryGetValue(locale, out var table)
                ? table
                : throw new InvalidOperationException($"Collection '{CollectionName}' has no {locale} table.")).ToArray();
            var sharedData = collection.SharedData;
            var changedAssets = new HashSet<UnityEngine.Object>();

            Undo.IncrementCurrentGroup();
            var undoGroup = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName(UndoLabel);
            try
            {
                foreach (var row in rows)
                {
                    var key = sharedData.GetEntry(row.key);
                    if (key == null)
                    {
                        RecordChange(sharedData, changedAssets);
                        key = sharedData.AddKey(row.key);
                    }

                    var values = row.GetValues();
                    for (var i = 0; i < tables.Length; i++)
                    {
                        var table = tables[i];
                        var entry = table.GetEntry(key.Id);
                        if (entry != null && entry.Value == values[i]) continue;

                        RecordChange(table, changedAssets);
                        if (entry == null)
                            table.AddEntry(key.Id, values[i]).IsSmart = row.smart;
                        else
                            entry.Value = values[i];
                    }
                }
            }
            catch
            {
                Undo.RevertAllDownToGroup(undoGroup);
                throw;
            }

            Undo.CollapseUndoOperations(undoGroup);
            foreach (var asset in changedAssets)
            {
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssetIfDirty(asset);
            }

            LocalizationEditorSettings.EditorEvents.RaiseCollectionModified(null, collection);
        }

        private static void RecordChange(UnityEngine.Object asset, HashSet<UnityEngine.Object> changedAssets)
        {
            if (changedAssets.Add(asset))
                Undo.RegisterCompleteObjectUndo(asset, UndoLabel);
        }
    }
}
