#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Yorozu.EditorTool.Importer
{
    public class PresetImporterProvider : SettingsProvider
    {
        private PresetImporterProvider() : base("Project/Preset Importer", SettingsScope.Project)
        {
        }
        
        [SettingsProviderGroup]
        private static SettingsProvider[] CreateSettingsProvider()
        {
            var providers = new List<SettingsProvider> { new PresetImporterProvider() };

            if (Load() != null)
            {
                var provider = new AssetSettingsProvider("Project/Preset Importer/Settings", Load);
                providers.Add(provider);
            }

            return providers.ToArray();
        }

        private PresetImporterSetting _setting;
        private List<PresetGroupEditor> _presetGroups;

        private static PresetImporterSetting Load()
        {
            var guids = AssetDatabase.FindAssets($"t:{nameof(PresetImporterSetting)}");
            if (guids.Length <= 0)
                return null;
            
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<PresetImporterSetting>(path);
        }
        
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            _setting = Load();
        }
        
        public override void OnGUI(string searchContext)
        {
            using (new EditorGUI.DisabledScope(_setting != null))
            {
                if (GUILayout.Button("Create Setting"))
                {
                    var path = EditorUtility.SaveFilePanelInProject("Select Folder", nameof(PresetImporterSetting), "asset", "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        var instance = ScriptableObject.CreateInstance<PresetImporterSetting>();
                        AssetDatabase.CreateAsset(instance, path);
                        AssetDatabase.Refresh();
                        _setting = AssetDatabase.LoadAssetAtPath<PresetImporterSetting>(path);
                    }
                }
            }

            if (_setting != null)
            {
                if (GUILayout.Button("Ping Setting"))
                {
                    EditorGUIUtility.PingObject(_setting);
                }
            }
        }
    }
}
#endif