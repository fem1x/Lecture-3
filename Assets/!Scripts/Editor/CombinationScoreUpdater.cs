#if UNITY_EDITOR
using System.Collections.Generic;
using _Scripts.Combinations;
using _Scripts.Configs;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Editor
{
    public static class CombinationScoreUpdater
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
        private static readonly Dictionary<string, (int BaseScore, int Multiplier, int Priority)> Scores = new()
        {
            ["DumpConfig"]        = (0, 1, 10),
            ["DupletConfig"]      = (20, 2, 20),
            ["OppositesConfig"]   = (30, 2, 30),
            ["TriadConfig"]       = (40, 4, 40),
            ["TwoDupletsConfig"]  = (45, 5, 50),
            ["MonoConfig"]        = (50, 6, 60),
            ["UnderloadConfig"]   = (75, 7, 70),
            ["OverloadConfig"]    = (60, 7, 80),
            ["QuadConfig"]        = (75, 9, 90),
            ["SequenceConfig"]    = (100, 12, 100),
            ["SingularityConfig"] = (100, 15, 110)
        };
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 

        [MenuItem("Tools/Balance/Apply New Combination Scores")]
        public static void ApplyScores()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(CombinationConfig)}");

            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var config = AssetDatabase.LoadAssetAtPath<CombinationConfig>(path);

                if (!Scores.TryGetValue(config.name, out var scoreData))
                    continue;

                var so = new SerializedObject(config);

                var baseProp = so.FindProperty("<BaseScore>k__BackingField")
                            ?? so.FindProperty("BaseScore")
                            ?? so.FindProperty("<BasePoints>k__BackingField")
                            ?? so.FindProperty("BasePoints")
                            ?? so.FindProperty("_baseScore")
                            ?? so.FindProperty("_basePoints");

                var multProp = so.FindProperty("<Multiplier>k__BackingField")
                            ?? so.FindProperty("Multiplier")
                            ?? so.FindProperty("<Mult>k__BackingField")
                            ?? so.FindProperty("Mult")
                            ?? so.FindProperty("_multiplier")
                            ?? so.FindProperty("_mult");

                var priorityProp = so.FindProperty("<Priority>k__BackingField")
                                ?? so.FindProperty("Priority")
                                ?? so.FindProperty("_priority");

                baseProp.intValue = scoreData.BaseScore;
                multProp.intValue = scoreData.Multiplier;

                if (priorityProp != null)
                    priorityProp.intValue = scoreData.Priority;

                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(config);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Balance] Базовые очки, множители и приоритеты успешно обновлены в ScriptableObject.");
        }
    }
}
#endif