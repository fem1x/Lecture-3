using System;
using System.IO;
using _Scripts.Combinations;
using _Scripts.Combinations.Rules;
using _Scripts.Configs;
using _Scripts.Interfaces;
using UnityEditor;
using UnityEngine;

namespace _Scripts.Editor
{
    public static class CombinationConfigGenerator
    {
        private const string OutputFolder = "Assets/!Configs/Combinations";

        [MenuItem("Tools/Generate All Combination Configs")]
        public static void GenerateConfigs()
        {
            if (!Directory.Exists(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
                AssetDatabase.Refresh();
            }

            // 1. Solo (Any single dice, Frequency = 1)
            CreateConfig("SoloConfig", "solo", 10, "Solo", "Any single dice", 10, 1, 0,
                new FrequencyRule(),
                prop => prop.FindPropertyRelative("<RequiredFrequency>k__BackingField").intValue = 1,
                new[]
                {
                    new[] { 1 },
                    new[] { 2 },
                    new[] { 3 },
                    new[] { 4 },
                    new[] { 5 },
                    new[] { 6 }
                });

            // 2. Duplet (2 dice of the same value, Frequency = 2)
            CreateConfig("DupletConfig", "duplet", 20, "Duplet", "2 matching dice", 15, 2, 0,
                new FrequencyRule(),
                prop => prop.FindPropertyRelative("<RequiredFrequency>k__BackingField").intValue = 2,
                new[]
                {
                    new[] { 1, 1 },
                    new[] { 2, 2 },
                    new[] { 3, 3 },
                    new[] { 4, 4 },
                    new[] { 5, 5 },
                    new[] { 6, 6 }
                });

            // 3. Opposites (2 dice that sum to 7)
            CreateConfig("OppositesConfig", "opposites", 30, "Opposites", "2 dice sum to 7", 25, 2, 0,
                new SumRule(),
                prop =>
                {
                    prop.FindPropertyRelative("<Comparison>k__BackingField").enumValueIndex = (int)ComparisonType.Equal;
                    prop.FindPropertyRelative("<RequiredSum>k__BackingField").intValue = 7;
                },
                new[]
                {
                    new[] { 1, 6 },
                    new[] { 2, 5 },
                    new[] { 3, 4 },
                    new[] { 4, 3 },
                    new[] { 5, 2 },
                    new[] { 6, 1 }
                });

            // 4. Triad (3 dice of the same value, Frequency = 3)
            CreateConfig("TriadConfig", "triad", 40, "Triad", "3 matching dice", 30, 3, 1,
                new FrequencyRule(),
                prop => prop.FindPropertyRelative("<RequiredFrequency>k__BackingField").intValue = 3,
                new[]
                {
                    new[] { 1, 1, 1 },
                    new[] { 2, 2, 2 },
                    new[] { 3, 3, 3 },
                    new[] { 4, 4, 4 },
                    new[] { 5, 5, 5 },
                    new[] { 6, 6, 6 }
                });

            // 5. Two Duplets (Two pairs of matching dice)
            CreateConfig("TwoDupletsConfig", "two_duplets", 50, "Two Duplets", "2 pairs of matching dice", 30, 4, 1,
                new TwoDupletsRule(),
                null,
                new[]
                {
                    new[] { 1, 1, 2, 2 },
                    new[] { 2, 2, 3, 3 },
                    new[] { 3, 3, 4, 4 },
                    new[] { 4, 4, 5, 5 },
                    new[] { 5, 5, 6, 6 },
                    new[] { 1, 1, 6, 6 }
                });

            // 6. Quad (4 dice of the same value, Frequency = 4)
            CreateConfig("QuadConfig", "quad", 60, "Quad", "4 matching dice", 55, 7, 1,
                new FrequencyRule(),
                prop => prop.FindPropertyRelative("<RequiredFrequency>k__BackingField").intValue = 4,
                new[]
                {
                    new[] { 1, 1, 1, 1 },
                    new[] { 2, 2, 2, 2 },
                    new[] { 3, 3, 3, 3 },
                    new[] { 4, 4, 4, 4 },
                    new[] { 5, 5, 5, 5 },
                    new[] { 6, 6, 6, 6 }
                });

            // 7. Mono (All 5 dice are even or odd)
            CreateConfig("MonoConfig", "mono", 70, "Mono", "All 5 dice are even or odd", 60, 7, 2,
                new MonoRule(),
                null,
                new[]
                {
                    new[] { 1, 1, 3, 5, 5 },
                    new[] { 1, 3, 3, 3, 5 },
                    new[] { 2, 2, 4, 6, 6 },
                    new[] { 2, 4, 4, 4, 6 },
                    new[] { 1, 1, 1, 3, 5 },
                    new[] { 2, 2, 2, 4, 6 }
                });

            // 8. Sequence (5 consecutive numbers)
            CreateConfig("SequenceConfig", "sequence", 80, "Sequence", "5 consecutive numbers", 65, 7, 2,
                new SequenceRule(),
                null,
                new[]
                {
                    new[] { 1, 2, 3, 4, 5 },
                    new[] { 2, 3, 4, 5, 6 }
                });

            // 9. Overload (Sum >= 24)
            CreateConfig("OverloadConfig", "overload", 90, "Overload", "Total sum is 24 or greater", 70, 7, 2,
                new SumRule(),
                prop =>
                {
                    prop.FindPropertyRelative("<Comparison>k__BackingField").enumValueIndex = (int)ComparisonType.GreaterOrEqual;
                    prop.FindPropertyRelative("<RequiredSum>k__BackingField").intValue = 24;
                },
                new[]
                {
                    new[] { 4, 5, 5, 5, 5 },
                    new[] { 5, 5, 5, 5, 5 },
                    new[] { 4, 5, 5, 6, 6 },
                    new[] { 5, 5, 5, 6, 6 },
                    new[] { 5, 5, 6, 6, 6 },
                    new[] { 6, 6, 6, 6, 6 }
                });

            // 10. Underload (Sum <= 9)
            CreateConfig("UnderloadConfig", "underload", 100, "Underload", "Total sum is 9 or less", 75, 9, 3,
                new SumRule(),
                prop =>
                {
                    prop.FindPropertyRelative("<Comparison>k__BackingField").enumValueIndex = (int)ComparisonType.LessOrEqual;
                    prop.FindPropertyRelative("<RequiredSum>k__BackingField").intValue = 9;
                },
                new[]
                {
                    new[] { 1, 1, 1, 1, 1 },
                    new[] { 1, 1, 1, 1, 2 },
                    new[] { 1, 1, 1, 1, 3 },
                    new[] { 1, 1, 1, 2, 2 },
                    new[] { 1, 1, 1, 2, 3 },
                    new[] { 1, 1, 2, 2, 3 }
                });

            // 11. Singularity (5 dice of the same value, Frequency = 5)
            CreateConfig("SingularityConfig", "singularity", 110, "Singularity", "5 matching dice", 100, 10, 4,
                new FrequencyRule(),
                prop => prop.FindPropertyRelative("<RequiredFrequency>k__BackingField").intValue = 5,
                new[]
                {
                    new[] { 1, 1, 1, 1, 1 },
                    new[] { 2, 2, 2, 2, 2 },
                    new[] { 3, 3, 3, 3, 3 },
                    new[] { 4, 4, 4, 4, 4 },
                    new[] { 5, 5, 5, 5, 5 },
                    new[] { 6, 6, 6, 6, 6 }
                });

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("<color=green>All combination configs successfully generated in Assets/!Configs/Combinations!</color>");
        }

        private static void CreateConfig(
            string fileName,
            string id,
            int priority,
            string displayName,
            string description,
            int basePoints,
            int multiplier,
            int refund,
            object ruleInstance,
            Action<SerializedProperty> configureRuleAction,
            int[][] patternsValues)
        {
            var path = $"{OutputFolder}/{fileName}.asset";
            var config = AssetDatabase.LoadAssetAtPath<CombinationConfig>(path);

            if (config == null)
            {
                config = ScriptableObject.CreateInstance<CombinationConfig>();
                AssetDatabase.CreateAsset(config, path);
            }

            var serializedObject = new SerializedObject(config);

            serializedObject.FindProperty("<Id>k__BackingField").stringValue = id;
            serializedObject.FindProperty("<Priority>k__BackingField").intValue = priority;
            serializedObject.FindProperty("<DisplayName>k__BackingField").stringValue = displayName;
            serializedObject.FindProperty("<Description>k__BackingField").stringValue = description;
            serializedObject.FindProperty("<BasePoints>k__BackingField").intValue = basePoints;
            serializedObject.FindProperty("<Multiplier>k__BackingField").intValue = multiplier;
            serializedObject.FindProperty("<DiceRefund>k__BackingField").intValue = refund;

            var ruleProperty = serializedObject.FindProperty("<Rule>k__BackingField");
            if (ruleProperty != null)
            {
                ruleProperty.managedReferenceValue = ruleInstance;
                configureRuleAction?.Invoke(ruleProperty);
            }

            var patternsProperty = serializedObject.FindProperty("<Patterns>k__BackingField");
            if (patternsProperty != null)
            {
                patternsProperty.arraySize = patternsValues.Length;
                for (int i = 0; i < patternsValues.Length; i++)
                {
                    var patternElement = patternsProperty.GetArrayElementAtIndex(i);
                    var valuesProperty = patternElement.FindPropertyRelative("Values");

                    valuesProperty.arraySize = patternsValues[i].Length;
                    for (int j = 0; j < patternsValues[i].Length; j++)
                    {
                        valuesProperty.GetArrayElementAtIndex(j).intValue = patternsValues[i][j];
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
        }
    }
}