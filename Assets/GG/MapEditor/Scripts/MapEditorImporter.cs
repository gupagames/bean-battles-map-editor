using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GG.BeanBattles.MapEditor
{
    public static class MapEditorImporter
    {
        public static List<string> GetAllEditorMapsPaths()
        {
            List<string> maps = new List<string>();
            if (!Directory.Exists(MapEditorPaths.EditorMapsPath)) return maps;
            string[] files = Directory.GetFiles(MapEditorPaths.EditorMapsPath, $"*{MapEditorPaths.EditorMapExtension}");
            foreach (string file in files) maps.Add(file);
            return maps;
        }

        public static async Task<string> ExtractMapAsync(string bbmapPath)
        {
            string extractPath = null;

            await Task.Run(() =>
            {
                try
                {
                    if (!Directory.Exists(MapEditorPaths.EditorMapsCachePath)) Directory.CreateDirectory(MapEditorPaths.EditorMapsCachePath);

                    string mapName = Path.GetFileNameWithoutExtension(bbmapPath);
                    extractPath = Path.Combine(MapEditorPaths.EditorMapsCachePath, mapName);

                    if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);

                    Directory.CreateDirectory(extractPath);

                    using (ZipFile zip = new ZipFile(bbmapPath))
                    {
                        foreach (ZipEntry entry in zip)
                        {
                            if (!entry.IsFile) continue;

                            string outPath = Path.Combine(extractPath, entry.Name);

                            Directory.CreateDirectory(Path.GetDirectoryName(outPath));

                            using (Stream input = zip.GetInputStream(entry))
                            using (FileStream output = File.Create(outPath))
                            {
                                input.CopyTo(output);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("failed to extract map: " + e);
                }
            });

            return extractPath;
        }

#if UNITY_EDITOR

        public static bool ImportProject(string folderPath)
        {
            // the import location is just default assets, so the paths/refs should match
            string importPath = Path.Combine(Application.dataPath);
            if (!Directory.Exists(importPath)) Directory.CreateDirectory(importPath);

            string assetsPath = Path.Combine(folderPath, "Assets");

            // copy all files / assets / metas > techincally they could have no unique assets
            if (Directory.Exists(assetsPath)) CopyDirectory(assetsPath, importPath);

           // the main scene should be in the first folder, remember its path
            string scenePath = Directory.GetFiles(folderPath, "*.unity", SearchOption.TopDirectoryOnly).FirstOrDefault();

            if (string.IsNullOrEmpty(scenePath))
            { Debug.LogWarning("Imported files, but couldn't find scene"); return true; }

            string importatedScenePath = Path.Combine(importPath, Path.GetFileName(scenePath));
            File.Copy(scenePath, importatedScenePath, true);

            string sceneMeta = scenePath + ".meta";
            if (File.Exists(sceneMeta)) File.Copy(sceneMeta, importatedScenePath + ".meta", true);
   
            // refresh and open
            AssetDatabase.Refresh();
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(importatedScenePath);

            return true;
        }

        private static void CopyDirectory(string sourceDir, string targetDir)
        {
            if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir)) File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), true);
            foreach (var directory in Directory.GetDirectories(sourceDir)) CopyDirectory(directory, Path.Combine(targetDir, Path.GetFileName(directory)));
        }
#endif
    }
}
