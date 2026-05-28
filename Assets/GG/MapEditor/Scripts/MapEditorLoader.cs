using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading.Tasks;

namespace GG.BeanBattles.MapEditor
{
    public static class MapEditorLoader
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
    }
}
