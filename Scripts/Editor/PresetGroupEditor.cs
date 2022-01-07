using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Yorozu.EditorTool.Importer
{
    [Serializable]
    internal class PresetGroupEditor
    {
        private static class Styles
        {
            internal static GUIContent Plus;
            internal static GUIContent Minus;
            internal static GUILayoutOption IconWidth;
            internal static GUILayoutOption Width;

            static Styles()
            {
                Plus = EditorGUIUtility.TrIconContent("Toolbar Plus");
                Minus = EditorGUIUtility.TrIconContent("Toolbar Minus");
                IconWidth = GUILayout.Width(22f);
                Width = GUILayout.Width(150f);
            }
        }
        
        private string FullName;
        [SerializeField]
        private GUIContent _content;

        [SerializeField]
        public PresetImporterSetting.Group[] _groups;

        internal PresetGroupEditor(IGrouping<string, PresetImporterSetting.Group> grouping)
        {
            FullName = grouping.Key;
            var lastIndex = grouping.Key.LastIndexOf(".", StringComparison.Ordinal);
            var name = FullName.Substring(lastIndex + 1);
            
            var icon = PresetImporterUtil.GetIcon(FullName);

            _content = new GUIContent(name, icon);
            
            _groups = grouping.ToArray();
        }

        /// <summary>
        /// Body
        /// </summary>
        internal void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope("HelpBox"))
            {
                using (new EditorGUILayout.HorizontalScope("dockarea"))
                {
                    EditorGUILayout.LabelField(_content);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.LabelField($"({FullName})");
                }

                foreach (var group in _groups)
                {
                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        Body(group);
                    }
                }
            }
        }

        private void Body(PresetImporterSetting.Group group)
        {
            Preset(group);

            Folders(group);
            
            Editor(group);
        }

        /// <summary>
        /// 対象となる Preset
        /// </summary>
        private void Preset(PresetImporterSetting.Group group)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                group.Foldout = EditorGUILayout.Toggle(GUIContent.none, group.Foldout, "Foldout", Styles.IconWidth);
                using (var check = new EditorGUI.ChangeCheckScope())
                {
                    var preset = group.Preset;
                    preset = (Preset) EditorGUILayout.ObjectField(
                            GUIContent.none,
                            preset,
                            typeof(Preset),
                            false 
                            );

                    if (check.changed)
                    {
                        if (preset == null)
                            return;

                        // AssetImporter の Preset じゃない場合
                        if (!PresetImporterUtil.IsImporterPreset(preset))
                        {
                            Debug.Log("Preset it not importType");
                            return;
                        }

                        // 型は一致する必要がある
                        // TODO PresetSelector を利用したい
                        if (group.PresetTypeName != preset.GetPresetType().GetManagedTypeName())
                        {
                            Debug.Log("Preset Type is not different");
                            return;
                        }

                        group.Preset = preset;
                    }
                }
                // フォルダ追加ボタン
                if (GUILayout.Button(Styles.Plus, Styles.Width))
                {
                    group.Folders.Add(new PresetImporterSetting.FolderData(null));
                }
            }
        }
        
        /// <summary>
        /// フォルダ一覧
        /// </summary>
        private void Folders(PresetImporterSetting.Group group)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                for (var i = 0; i < group.Folders.Count; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField(group.Folders[i].Path);
                        
                        using (var check = new EditorGUI.ChangeCheckScope())
                        {
                            var folder = (DefaultAsset) EditorGUILayout.ObjectField(
                                GUIContent.none,
                                group.Folders[i].FolderAsset,
                                typeof(DefaultAsset),
                                false,
                                Styles.Width);
                            if (check.changed)
                            {
                                group.Folders[i].Change(folder);
                            }
                        }

                        if (GUILayout.Button(Styles.Minus, Styles.IconWidth))
                        {
                            group.Folders.RemoveAt(i);
                            GUIUtility.ExitGUI();
                        }
                    }
                }
            }
        }

        private void Editor(PresetImporterSetting.Group @group)
        {
            if (!group.Foldout)
                return;
            
            var editor = group.Editor;
            if (editor == null)
                return;

            using (new EditorGUILayout.VerticalScope("HelpBox"))
            {

                editor.OnInspectorGUI();
            }
        }
    }
}