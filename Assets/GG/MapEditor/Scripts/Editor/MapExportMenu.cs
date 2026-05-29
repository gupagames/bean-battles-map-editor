using System.IO;
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

        [MenuItem("GG/Map Editor/Create New Map")]
        public static void CreateMap()
        {
            Debug.Log($"Creating new map...");
            // make unique if already exists
            string newScenePath = "Assets/New Map.unity";
            newScenePath = AssetDatabase.GenerateUniqueAssetPath(newScenePath);

            // open template scene
            var scene = EditorSceneManager.OpenScene(MapEditorPaths.TemplatePath, OpenSceneMode.Single);

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
            Debug.Log($"Validating map...");
            EditorMapSettings settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();
            if (settings == null) { Debug.LogError("Failed to validate map, no MapSettings found."); return; }

            Scene currentScene = EditorSceneManager.GetActiveScene();
            if (!EditorMapValidation.ValidateLoadedMap(currentScene))
            { Debug.LogError("Map validation failed."); return; }

            Debug.Log("Map validation successful.");
        }

        [MenuItem("GG/Map Editor/Export Local/As bbmap File")]
        public static void ExportMap()
        {
            Debug.Log($"Exporting map as bbmap file...");
            EditorMapSettings settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();
            if (settings == null) { Debug.LogError("Failed to export map, no MapSettings found."); return; }

            string path = MapEditorExporter.ExportMap(settings, true);

            if (string.IsNullOrEmpty(path))
            { Debug.LogError("Failed to export map."); return; }

            Debug.Log($"Finished exporting map as bbmap file to path: " + path);
        }

        [MenuItem("GG/Map Editor/Export Local/As New bbmap File")]
        public static void ExportAsNewMap()
        {
            Debug.Log($"Exporting map as new bbmap file...");
            EditorMapSettings settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();
            if (settings == null) { Debug.LogError("Failed to export map, no MapSettings found."); return; }

            // generate new identity
            settings.GenerateMapId();

            string path = MapEditorExporter.ExportMap(settings, true);

            if (string.IsNullOrEmpty(path))
            { Debug.LogError("Failed to export map."); return; }

            Debug.Log($"Finished exporting map as new bbmap file to path: " + path);
        }

        [MenuItem("GG/Map Editor/Publish To Steam/As Workshop Item")]
        public static void PublishMapToSteam()
        {
            Debug.Log($"Publishing map to steam as workshop item...");
            EditorMapSettings settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();
            if (settings == null) { Debug.LogError("Failed to export map, no MapSettings found."); return; }

            var path = MapEditorExporter.ExportMap(settings, false);

            if (string.IsNullOrEmpty(path))
            { Debug.LogError("Failed to export map."); return; }

            string itemId = MapEditorExporter.UploadToSteamWorkshop(path, settings);

            Debug.Log($"Finished publishing/updating map to steam as workshop item: " + itemId);
        }

        [MenuItem("GG/Map Editor/Publish To Steam/As New Workshop Item")]
        public static void PublishAsNewMapToSteam()
        {
            Debug.Log($"Publishing map to steam as new workshop item...");
            EditorMapSettings settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();
            if (settings == null) { Debug.LogError("Failed to export map, no MapSettings found."); return; }

            // generate new identity
            settings.GenerateMapId();
            settings.SteamAuthorId = "";
            settings.SteamItemId = "";

            var path = MapEditorExporter.ExportMap(settings, false);

            if (string.IsNullOrEmpty(path))
            { Debug.LogError("Failed to export map."); return; }

            string itemId = MapEditorExporter.UploadToSteamWorkshop(path, settings);

            Debug.Log($"Finished publishing map to steam as new workshop item with id: " + itemId);
        }
    }
}