using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Yorozu.EditorTool.Importer
{
    [CreateAssetMenu]
    public class PresetImporterSetting : ScriptableObject
    {
        [SerializeField]
        internal List<Group> Groups = new List<Group>();

        [Serializable]
        internal class FolderData
        {
            [SerializeField]
            private DefaultAsset _folderAsset;
            internal DefaultAsset FolderAsset => _folderAsset;
            [NonSerialized]
            private string _path;
            internal string Path
            {
                get
                {
                    if (string.IsNullOrEmpty(_path) && _folderAsset != null)
                    {
                        _path = AssetDatabase.GetAssetPath(_folderAsset);
                    }
                    return _path;
                }
            }
            
            internal FolderData(DefaultAsset folderAsset)
            {
                Change(folderAsset);
            }

            internal void Change(DefaultAsset folderAsset)
            {
                if (folderAsset != null)
                {
                    var path = AssetDatabase.GetAssetPath(folderAsset);
                    if (!AssetDatabase.IsValidFolder(path))
                    {
                        Debug.Log("Folder Only");
                        return;
                    }
                }

                _folderAsset = folderAsset;
            }
        }
        
        [Serializable]
        internal class Group
        {
            [SerializeField]
            internal List<FolderData> Folders = new List<FolderData>();

            [SerializeField]
            internal Preset Preset;

            [SerializeField, HideInInspector]
            internal string PresetTypeName;
            
            [NonSerialized]
            internal bool Foldout;
            
            [NonSerialized]
            private Editor _editor;

            internal Editor Editor
            {
                get
                {
                    if (Preset == null)
                        return null;

                    if (_editor == null) 
                        _editor = Editor.CreateEditor(Preset);

                    return _editor;
                }
            }

            internal Group(Preset preset)
            {
                Preset = preset;
                PresetTypeName = preset.GetPresetType().GetManagedTypeName();
            }
            
            /// <summary>
            /// 対象のフォルダーかどうか
            /// </summary>
            internal bool IsTargetFolder(string path)
            {
                return Folders.Any(f => path.StartsWith(f.Path));
            }
        }

        [NonSerialized]
        private HashSet<string> _typeHash;
        [NonSerialized]
        private List<string> _targetPaths;
        
        /// <summary>
        /// 対象かどうか
        /// </summary>
        internal bool IsApply(string path, Type type)
        {
            if (_typeHash == null)
            {
                var types = Groups
                    .Where(g => g.Preset != null)
                    .Select(g => g.PresetTypeName)
                    .Distinct();

                _typeHash = new HashSet<string>();
                foreach (var typeName in types) 
                    _typeHash.Add(typeName);
            }

            if (!_typeHash.Contains(type.FullName))
                return false;

            if (_targetPaths == null)
            {
                _targetPaths = Groups
                    .Where(g => g.Preset != null)
                    .SelectMany(g => g.Folders)
                    .Select(f => AssetDatabase.GetAssetPath(f.FolderAsset))
                    .Distinct()
                    .ToList();
            }

            if (!_targetPaths.Any(path.StartsWith))
                return false;

            return true;
        }
        
        /// <summary>
        /// パスが一致すれば Preset の適応
        /// </summary>
        internal void Apply(string path, AssetImporter importer)
        {
            var fullName = importer.GetType().FullName;
            var findPresets = Groups
                .Where(g => g.PresetTypeName == fullName)
                .Where(g => g.Preset != null)
                .Where(g => g.IsTargetFolder(path));
            
            // 見つからなかったら終わり
            if (!findPresets.Any())
                return;

            // 見つかったら最小マッチで並び替え
            var sorted = findPresets.OrderBy(p => p.Folders.Max(f => Regex.Match(path, AssetDatabase.GetAssetPath(f.FolderAsset)).Length));
            
            // 適応していく
            foreach (var presetGroup in sorted)
            {
                if (!presetGroup.Preset.CanBeAppliedTo(importer))
                    continue;
                
                presetGroup.Preset.ApplyTo(importer);
            }
        }
    }

}
