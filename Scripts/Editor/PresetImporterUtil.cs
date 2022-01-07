using System;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace Yorozu.EditorTool.Importer
{
    internal static class PresetImporterUtil
    {
        /// <summary>
        /// Preset が Importer なのかを判定する
        /// </summary>
        /// <param name="preset"></param>
        /// <returns></returns>
        internal static bool IsImporterPreset(Preset preset)
        {
            var typeName = preset.GetPresetType().GetManagedTypeName();

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var findType = assembly.GetType(typeName);
                if (findType == null)
                    continue;

                if (findType.BaseType == typeof(AssetImporter))
                    return true;
            }
            
            return false;
        }

        private static bool TryGetFindPresetType(string typeName, out Type type)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var findType = assembly.GetType(typeName);
                if (findType == null)
                    continue;

                if (findType.BaseType == typeof(AssetImporter))
                {
                    type = findType;
                    return true;
                }
            }

            type = null;
            return false;
        }

        internal static Texture2D GetIcon(string typeName)
        {
            if (TryGetFindPresetType(typeName, out var type))
            {
                return AssetPreview.GetMiniTypeThumbnail(type);
            }

            return null;
        }
    }
}