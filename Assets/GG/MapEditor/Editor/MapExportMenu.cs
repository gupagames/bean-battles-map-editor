using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GG.BeanBattles.MapEditor
{
    public static class MapExporterMenu
    {
        [MenuItem("GG/Show EditorMaps on Disk")]
        public static void OpenPersistentData()
        {
            if (!Directory.Exists(MapEditorPaths.EditorMapsPath)) Directory.CreateDirectory(MapEditorPaths.EditorMapsPath);
            EditorUtility.RevealInFinder(MapEditorPaths.EditorMapsPath);
        }

        [MenuItem("GG/Map Editor/Validate Map")]
        public static void ValidateMap()
        {
            EditorMapSettings settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();
            if (settings == null) { Debug.LogError("Failed to validate map, no MapSettings found."); return; }

            if (!EditorMapValidation.ValidateMap())
            {
                Debug.LogError("Map validation failed.");
                return;
            }

            Debug.Log("Map validation successful.");
        }

        [MenuItem("GG/Map Editor/Export Map")]
        public static void ExportMap()
        {
            EditorMapSettings settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();
            if (settings == null) { Debug.LogError("Failed to export map, no MapSettings found."); return; }

            ExportMap(settings);
        }

        [MenuItem("GG/Map Editor/Upload Map")]
        public static void UploadMap()
        {
            EditorMapSettings settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();
            if (settings == null) { Debug.LogError("Failed to upload map, no MapSettings found."); return; }

            ExportMap(settings);

            Debug.Log("TODO: Upload map to Steam Workshop");
        }

        private static void ExportMap(EditorMapSettings settings)
        {
            Scene currentScene = EditorSceneManager.GetActiveScene();

            if (string.IsNullOrEmpty(currentScene.path))
            {
                Debug.LogError("Scene must be saved before exporting.");
                return;
            }

            if (!EditorMapValidation.ValidateMap())
            {
                Debug.LogError("Failed to export map, validation failed.");
                return;
            }

            string tempMapPath = Path.Combine(MapEditorPaths.EditorMapsCachePath, settings.MapName);
            string rawPath = Path.Combine(tempMapPath, "raw");
            string buildPath = Path.Combine(tempMapPath, "build");

            string zipPath = Path.Combine(MapEditorPaths.EditorMapsPath, settings.MapName + MapEditorPaths.EditorMapExtension);

            if (Directory.Exists(tempMapPath)) Directory.Delete(tempMapPath, true);
            if (File.Exists(zipPath)) File.Delete(zipPath);

            Directory.CreateDirectory(rawPath);
            Directory.CreateDirectory(buildPath);

            AssetBundleBuild build = new AssetBundleBuild
            {
                assetBundleName = "bbmapbundle",
                assetNames = new[] { currentScene.path }
            };

            BuildPipeline.BuildAssetBundles( rawPath, new[] { build }, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);

            string builtBundlePath = Path.Combine(rawPath, "bbmapbundle");
            string finalBundlePath = Path.Combine(buildPath, "map.bundle");

            File.Copy(builtBundlePath, finalBundlePath, true);

            EditorMapMetaData metadata = new EditorMapMetaData
            {
                MapName = settings.MapName,
                Description = settings.Description,
                SceneName = currentScene.name,
                EditorVersion = settings.EditorVersion
            };

            string json = JsonUtility.ToJson(metadata, true);

            File.WriteAllText(Path.Combine(buildPath, "map.json"), json);

            if (settings.PreviewImage != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(settings.PreviewImage);
                File.Copy(assetPath, Path.Combine(buildPath, "preview.png"), true);
            }

            FastZip fastZip = new FastZip();
            fastZip.CreateZip(zipPath, buildPath, true, null);

            Directory.Delete(tempMapPath, true);

            Debug.Log("Exported bbmap: " + zipPath);

            EditorUtility.RevealInFinder(zipPath);
        }
    }
}