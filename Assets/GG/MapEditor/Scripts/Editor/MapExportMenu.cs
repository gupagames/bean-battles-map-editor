using ICSharpCode.SharpZipLib.Zip;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GG.BeanBattles.MapEditor
{
    public static class MapExporterMenu
    {
        private static string _templatePath = "Assets/GG/Scenes/Templete.unity";
        private static string _packagePath = Path.Combine(Application.dataPath, "GG/MapEditor/package.json");
        private static string _uploaderPath = Path.Combine(Application.dataPath, "../Tools/BeanBattlesMapEditorSteamUploader.exe");

        [MenuItem("GG/Show EditorMaps on Disk")]
        public static void OpenPersistentData()
        {
            if (!Directory.Exists(MapEditorPaths.EditorMapsPath)) Directory.CreateDirectory(MapEditorPaths.EditorMapsPath);
            EditorUtility.RevealInFinder(MapEditorPaths.EditorMapsPath);
        }

        [MenuItem("GG/Map Editor/Create New Map")]
        public static void CreateMap()
        {
            // make unique if already exists
            string newScenePath = "Assets/New Map.unity";
            newScenePath = AssetDatabase.GenerateUniqueAssetPath(newScenePath);

            // open template scene
            var scene = EditorSceneManager.OpenScene(_templatePath, OpenSceneMode.Single);

            EditorMapSettings settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();

            if (settings == null)
            { Debug.LogError("No EditorMapSettings found in template scene."); return; }

            // generate new identity
            settings.GenerateMapId();

            EditorUtility.SetDirty(settings);
            EditorSceneManager.SaveScene(scene, newScenePath);
            AssetDatabase.Refresh();

            Debug.Log($"Created new map scene: {newScenePath}");
        }

        [MenuItem("GG/Map Editor/Validate Map")]
        public static void ValidateMap()
        {
            EditorMapSettings settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();
            if (settings == null) { Debug.LogError("Failed to validate map, no MapSettings found."); return; }

            settings.AssignSpawns();

            if (!EditorMapValidation.ValidateMap())
            {
                Debug.LogError("Map validation failed.");
                return;
            }

            Debug.Log("Map validation successful.");
        }

        [MenuItem("GG/Map Editor/Export Map File")]
        public static void ExportMap()
        {
            EditorMapSettings settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();
            if (settings == null) { Debug.LogError("Failed to export map, no MapSettings found."); return; }

            // generate new identity
            settings.GenerateMapId();

            settings.AssignSpawns();

            ExportMap(settings, true);
        }

        [MenuItem("GG/Map Editor/Export As New Map File")]
        public static void ExportAsNewMap()
        {
            EditorMapSettings settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();
            if (settings == null) { Debug.LogError("Failed to export map, no MapSettings found."); return; }

            settings.AssignSpawns();

            ExportMap(settings, true);
        }

        [MenuItem("GG/Map Editor/Publish Map To Steam Workshop")]
        public static void PublishMapToSteam()
        {
            EditorMapSettings settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();
            if (settings == null) { Debug.LogError("Failed to export map, no MapSettings found."); return; }

            settings.AssignSpawns();

            var path = ExportMap(settings, false);

            UploadToSteamWorkshop(path, settings);
        }

        [MenuItem("GG/Map Editor/Publish As New Map To Steam Workshop")]
        public static void PublishAsNewMapToSteam()
        {
            EditorMapSettings settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();
            if (settings == null) { Debug.LogError("Failed to export map, no MapSettings found."); return; }

            settings.AssignSpawns();
            settings.GenerateMapId();
            settings.SteamAuthorId = "";
            settings.SteamItemId = "";

            var path = ExportMap(settings, false);

            UploadToSteamWorkshop(path, settings);
        }

        private static string ExportMap(EditorMapSettings settings, bool bbMap)
        {
            Scene currentScene = EditorSceneManager.GetActiveScene();
            ZipConstants.DefaultCodePage = 65001;

            if (string.IsNullOrEmpty(currentScene.path))
            {
                Debug.LogError("Scene must be saved before exporting.");
                return "";
            }

            if (!EditorMapValidation.ValidateMap())
            {
                Debug.LogError("Failed to export map, validation failed.");
                return "";
            }

            // used to assign version
            string versionJson = File.ReadAllText(_packagePath);
            MapEditorPackage package = JsonUtility.FromJson<MapEditorPackage>(versionJson);
            float version = -1f; float.TryParse(package.version, out version);

            if (version < 1)
            {
                Debug.LogError("Failed to get editor version.");
                return "";
            }

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
                    DisplayName = mapStage.DisplayName,
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
                Debug.Log("Exported bbmap: " + zipPath);

                return zipPath;
            }
            else
            {

                // EditorUtility.RevealInFinder(cacheMapPath);
                Debug.Log("Exported map folder: " + cacheMapPath);

                return cacheMapPath;
            }
        }

        private static void UploadToSteamWorkshop(string path, EditorMapSettings settings)
        {
            Debug.Log("Publishing To Steam...");

            Debug.Log(_uploaderPath);
            if (!File.Exists(_uploaderPath))
            { Debug.LogError("Failed to find uploader exe."); return; }

            System.Diagnostics.Process process = new System.Diagnostics.Process();

            process.StartInfo.FileName = _uploaderPath;
            process.StartInfo.Arguments = $"\"{path}\"";

            process.Start();

            while (!process.HasExited) continue;

            if (process.ExitCode != 0)
            { Debug.LogError("Steam upload failed."); return; }

            // the upload succeeded, we should update our gamesettings.. we have to find it again
            settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();
            if (settings == null) { Debug.LogError("Failed to export map, no MapSettings found."); return; }

            string jsonPath = Path.Combine(path, "map.json");

            if (!File.Exists(jsonPath))
            { Debug.LogError("Failed to reload map json."); return; }

            string json = File.ReadAllText(jsonPath);

            EditorMapMetaData metaData = JsonUtility.FromJson<EditorMapMetaData>(json);

            settings.SteamItemId = metaData.SteamItemId;
            settings.SteamAuthorId = metaData.SteamAuthorId;

            EditorUtility.SetDirty(settings);

            Application.OpenURL($"steam://url/CommunityFilePage/{settings.SteamItemId}");
            Debug.Log("Steam upload complete.");
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