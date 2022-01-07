using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Yorozu.EditorTool.Importer
{
    [CustomEditor(typeof(PresetImporterSetting), false)]
    internal class PresetImporterSettingEditor : Editor
    {
        private PresetImporterSetting _setting;
        private List<PresetGroupEditor> _presetGroups;
        private void OnEnable()
        {
            _setting = target as PresetImporterSetting;
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
        
        public override void OnInspectorGUI()
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
                var del = false;
                foreach (var pg in _presetGroups)
                {
                    var d = pg.OnGUI(_setting); 
                    del |= d;
                }

                if (check.changed)
                {
                    if (del)
                        CreateGroup();
                    
                    EditorUtility.SetDirty(_setting);
                    AssetDatabase.SaveAssets();
                    Repaint();
                }
            }
        }
    }
}