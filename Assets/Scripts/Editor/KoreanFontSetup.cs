#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace _Scripts.Editor
{
    internal static class KoreanFontSetup
    {
        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        private const string SourcePath = "Assets/Fonts/NotoSansKR-Regular.ttf";
        private const string AssetPath = "Assets/Fonts/NotoSansKR SDF.asset";

        // ===== ===== ===== ===== ===== ===== ===== ===== ===== =====
        [InitializeOnLoadMethod]
        private static void ScheduleSetup()
        {
            EditorApplication.delayCall += Setup;
        }

        private static void Setup()
        {
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetPath);
            if (fontAsset == null)
            {
                var source = AssetDatabase.LoadAssetAtPath<Font>(SourcePath);
                fontAsset = TMP_FontAsset.CreateFontAsset(source, 90, 9, GlyphRenderMode.SDFAA,
                    1024, 1024, AtlasPopulationMode.Dynamic, true);
                AssetDatabase.CreateAsset(fontAsset, AssetPath);
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
                AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
            }

            var settings = new SerializedObject(TMP_Settings.instance);
            var fallbacks = settings.FindProperty("m_fallbackFontAssets");
            for (var i = 0; i < fallbacks.arraySize; i++)
            {
                if (fallbacks.GetArrayElementAtIndex(i).objectReferenceValue == fontAsset)
                    return;
            }

            fallbacks.InsertArrayElementAtIndex(fallbacks.arraySize);
            fallbacks.GetArrayElementAtIndex(fallbacks.arraySize - 1).objectReferenceValue = fontAsset;
            settings.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
