using UnityEditor;

namespace Yorozu.EditorTool.Importer
{
    public class PresetImporterPostProcessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (importedAssets.Length <= 0)
                return;

            var guids = AssetDatabase.FindAssets($"t:{nameof(PresetImporterSetting)}");
            if (guids.Length <= 0)
                return;

            var settingPath = AssetDatabase.GUIDToAssetPath(guids[0]);
            var setting = AssetDatabase.LoadAssetAtPath<PresetImporterSetting>(settingPath);
            
            foreach (var path in importedAssets)
            {
                var importer = AssetImporter.GetAtPath(path);
                if (importer == null)
                    continue;

                // Default の Importer ならスキップ
                if (importer.GetType() == typeof(AssetImporter))
                    continue;

                // 設定にあるかどうか
                if (!setting.IsApply(path, importer.GetType()))
                    continue;

                setting.Apply(path, importer);
            }
            
        }
    }
}