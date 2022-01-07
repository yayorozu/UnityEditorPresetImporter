#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using UnityEngine.UIElements;

namespace Yorozu.EditorTool.Importer
{
    public class PresetImporterProvider : SettingsProvider
    {
        private PresetImporterProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        private PresetImporterSetting _setting;
        private List<PresetGroupEditor> _presetGroups;
        
        /// <summary>
        /// Project Settings に表示させる
        /// </summary>
        /// <returns></returns>
        [SettingsProvider]
        public static SettingsProvider RegisterProject()
        {
            return new PresetImporterProvider("Yorozu/", SettingsScope.Project)
            {
                label = "Preset Importer",
            };
        }
        
        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);
            var guids = AssetDatabase.FindAssets($"t:{nameof(PresetImporterSetting)}");
            if (guids.Length <= 0)
                return;

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _setting = AssetDatabase.LoadAssetAtPath<PresetImporterSetting>(path);
            CreateGroup();
        }
        
        /// <summary>
        /// Preset の型で区分する
        /// </summary>
        private void CreateGroup()
        {
            if (_setting == null)
                return;
            
            var groupBy = _setting.Groups
                    .GroupBy(g => g.PresetTypeName)
                ;

            _presetGroups = new List<PresetGroupEditor>(groupBy.Count());
            
            foreach (var g in groupBy)
            {
                var pg = new PresetGroupEditor(g);
                _presetGroups.Add(pg);
            }
        }
        
        public override void OnGUI(string searchContext)
        {
            if (_setting == null)
            {
                if (GUILayout.Button("Create"))
                {
                    var path = EditorUtility.SaveFilePanelInProject("Select Folder", nameof(PresetImporterSetting), "asset", "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        var instance = ScriptableObject.CreateInstance<PresetImporterSetting>();
                        AssetDatabase.CreateAsset(instance, path);
                        AssetDatabase.Refresh();
                        _setting = AssetDatabase.LoadAssetAtPath<PresetImporterSetting>(path);
                        CreateGroup();
                    }
                }
                return;
            }
            
            if (GUILayout.Button("Add Importer Preset"))
            {
                var path = EditorUtility.OpenFilePanel("Select Preset Asset", "Assets/", "preset");
                if (!string.IsNullOrEmpty(path))
                {
                    path = path.Replace(Application.dataPath, "Assets");
                    var preset = AssetDatabase.LoadAssetAtPath<Preset>(path);
                    if (preset == null)
                        return;
                        
                    if (!PresetImporterUtil.IsImporterPreset(preset))
                    {
                        Debug.Log("Select Importer Preset");
                        return;
                    }
                    
                    _setting.Groups.Add(new PresetImporterSetting.Group(preset));
                    CreateGroup();
                    EditorUtility.SetDirty(_setting);
                    AssetDatabase.SaveAssets();
                    GUIUtility.ExitGUI();
                }
            }

            using (var check = new EditorGUI.ChangeCheckScope())
            {
                foreach (var pg in _presetGroups)
                {
                    pg.OnGUI();
                }

                if (check.changed)
                {
                    EditorUtility.SetDirty(_setting);
                    AssetDatabase.SaveAssets();
                }
            }
        }
    }
}
#endif