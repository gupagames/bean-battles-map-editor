using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace GG.BeanBattles.MapEditor
{
    public static class MapEditorExporter
    {
        public static string ExportMap(EditorMapSettings settings, bool bbMap)
        {
            Scene currentScene = EditorSceneManager.GetActiveScene();

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();

            ZipConstants.DefaultCodePage = 65001;

            if (string.IsNullOrEmpty(currentScene.path))
            { Debug.LogError("Scene must be saved before exporting."); return ""; }

            if (!EditorMapValidation.ValidateLoadedMap(currentScene))
            { Debug.LogError("Failed to export map, validation failed."); return ""; }

            // used to assign version
            string versionJson = File.ReadAllText(MapEditorPaths.PackagePath);
            MapEditorPackage package = JsonUtility.FromJson<MapEditorPackage>(versionJson);
            float version = -1f; float.TryParse(package.version, out version);

            if (version < 1)
            {  Debug.LogError("Failed to get editor version."); return ""; }

            // set values if needed, and update last update
            if (string.IsNullOrEmpty(settings.CreationDate)) settings.CreationDate = DateTime.UtcNow.ToString("O");
            if (string.IsNullOrEmpty(settings.Id)) settings.GenerateMapId();
            settings.LastUpdate = DateTime.UtcNow.ToString("O");

            string cacheMapPath = Path.Combine(MapEditorPaths.EditorMapsCachePath, settings.MapName);
            string rawPath = Path.Combine(cacheMapPath, "raw");

            string zipPath = Path.Combine(MapEditorPaths.EditorMapsPath, settings.MapName + MapEditorPaths.EditorMapExtension);

            if (Directory.Exists(cacheMapPath)) Directory.Delete(cacheMapPath, true);
            if (File.Exists(zipPath)) File.Delete(zipPath);

            Directory.CreateDirectory(rawPath);

            AssetBundleBuild build = new AssetBundleBuild
            {
                assetBundleName = "bbmapbundle",
                assetNames = new[] { currentScene.path }
            };

            BuildPipeline.BuildAssetBundles(rawPath, new[] { build }, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

            string builtBundlePath = Path.Combine(rawPath, "bbmapbundle");
            string finalBundlePath = Path.Combine(cacheMapPath, "map.bundle");

            File.Copy(builtBundlePath, finalBundlePath, true);

            // hash for map accuracy
            string hashInput = Convert.ToBase64String(File.ReadAllBytes(finalBundlePath));
            string mapHash = ComputeSHA256(hashInput);

            EditorMapMetaData metadata = new EditorMapMetaData
            {
                MapName = settings.MapName,
                Description = settings.Description,
                Author = settings.Author,
                EditorVersion = version,

                MapId = settings.Id,
                MapHash = mapHash,

                // local maps don't include steam info, we dont want to install an outdate map
                // but still keep these on the settings, so if we update to steam, keep connection
                SteamItemId = bbMap ? "" : settings.SteamItemId,
                SteamAuthorId = bbMap ? "" : settings.SteamAuthorId,

                LastUpdate = settings.LastUpdate,
                CreationDate = settings.CreationDate,

                Stages = new EditorMapStageMetaData[settings.Stages.Length]
            };

            for (int i = 0; i < metadata.Stages.Length; i++)
            {
                EditorMapStage mapStage = settings.Stages[i];

                metadata.Stages[i] = new EditorMapStageMetaData()
                {
                    StageName = mapStage.StageName,
                    Width = mapStage.ZoneSize.x,
                    Depth = mapStage.ZoneSize.z,
                };
            }

            string metaJson = JsonUtility.ToJson(metadata, true);

            File.WriteAllText(Path.Combine(cacheMapPath, "map.json"), metaJson);

            if (settings.PreviewImage != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(settings.PreviewImage);
                File.Copy(assetPath, Path.Combine(cacheMapPath, "preview.png"), true);
            }

            Directory.Delete(rawPath, true);

            if (bbMap)
            {
                FastZip fastZip = new FastZip();
                fastZip.CreateZip(zipPath, cacheMapPath, true, null);

                EditorUtility.RevealInFinder(zipPath);
                return zipPath;
            }
            else
            {
                return cacheMapPath;
            }
        }

        public static string UploadToSteamWorkshop(string path, EditorMapSettings settings)
        {
            Debug.Log("Running Steam Uploader exe...");

            if (!File.Exists(MapEditorPaths.UploaderPath))
            { Debug.LogError("Failed to find uploader exe."); return ""; }

            System.Diagnostics.Process process = new System.Diagnostics.Process();

            process.StartInfo.FileName = MapEditorPaths.UploaderPath;
            process.StartInfo.Arguments = $"\"{path}\"";

            process.Start();

            while (!process.HasExited) continue;

            if (process.ExitCode != 0)
            { Debug.LogError("Steam upload failed."); return ""; }

            // the upload succeeded, we should update our gamesettings.. we have to find it again
            settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();
            if (settings == null) { Debug.LogError("Failed to export map, no MapSettings found."); return ""; }

            string jsonPath = Path.Combine(path, "map.json");

            if (!File.Exists(jsonPath))
            { Debug.LogError("Failed to reload map json."); return ""; }

            string json = File.ReadAllText(jsonPath);

            EditorMapMetaData metaData = JsonUtility.FromJson<EditorMapMetaData>(json);

            settings.SteamItemId = metaData.SteamItemId;
            settings.SteamAuthorId = metaData.SteamAuthorId;

            EditorUtility.SetDirty(settings);

            Application.OpenURL($"steam://url/CommunityFilePage/{settings.SteamItemId}");
            Debug.Log("Steam upload complete.");

            return metaData.SteamItemId;
        }

        private static string ComputeSHA256(string input)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "").ToLower();
            }
        }
    }
}
