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

        [MenuItem("GG/Map Editor/Export/As Map")]
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

        [MenuItem("GG/Map Editor/Export/As New Map")]
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

        [MenuItem("GG/Map Editor/Export/As Project")]
        public static void ExportProject()
        {
            Debug.Log($"Exporting as bbmapproject...");
            EditorMapSettings settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();
            if (settings == null) { Debug.LogError("Failed to export project, no MapSettings found."); return; }

            string path = EditorUtility.OpenFolderPanel("Export bbmapproject", "", "");

            if (string.IsNullOrEmpty(path))
            { Debug.LogError("Failed to export bbmapproject. path null"); return; }

            bool result = MapEditorExporter.ExportProject(settings, path);

            if (!result)
            { Debug.LogError("Failed to export bbmapproject."); return; }

            Debug.Log($"Finished exporting bbmapproject to path: " + path);
        }

        [MenuItem("GG/Map Editor/Import/From Project")]
        public static void ImportProject()
        {
            Debug.Log($"Importing bbmapproject...");

            string path = EditorUtility.OpenFolderPanel("Import bbmapproject", "", "");

            if (string.IsNullOrEmpty(path))
            { Debug.LogError("Failed to import bbmapproject. path null"); return; }

            bool result = MapEditorImporter.ImportProject(path);

            if (!result)
            { Debug.LogError("Failed to import bbmapproject."); return; }

            Debug.Log($"Finished importing bbmapproject");
        }

        [MenuItem("GG/Map Editor/Publish/As Steam Workshop Item")]
        public static void PublishMapToSteam()
        {
            Debug.Log($"Publishing map to steam as workshop item...");
            EditorMapSettings settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();
            if (settings == null) { Debug.LogError("Failed to export map, no MapSettings found."); return; }

            var path = MapEditorExporter.ExportMap(settings, false);

            if (string.IsNullOrEmpty(path))
            { Debug.LogError("Failed to export map."); return; }

            string itemId = MapEditorExporter.UploadToSteamWorkshop(path, settings);

            if (string.IsNullOrEmpty(itemId))
            { Debug.LogError("Failed to publish map to steam. Check console app logs. Maybe you should be publishing as new item?"); return; }

            Debug.Log($"Finished publishing/updating map to steam as workshop item: " + itemId);
        }

        [MenuItem("GG/Map Editor/Publish/As New Steam Workshop Item")]
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

            if (string.IsNullOrEmpty(itemId))
            { Debug.LogError("Failed to publish map to steam. Check console app logs."); return; }

            Debug.Log($"Finished publishing map to steam as new workshop item with id: " + itemId);
        }


        [MenuItem("GG/Map Editor/Debug/Change Map Id")]
        public static void ChangeMapId()
        {

            ShowStringInput("Change Map Id", "Enter Map Id (Only change this if something is broken):", value =>
            {
                EditorMapSettings settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();
                if (settings == null) { Debug.LogError("Failed to change map id, no MapSettings found."); return; }

                Undo.RecordObject(settings, "Change Map Id");
                settings.Id = value;
                EditorUtility.SetDirty(settings);

                Debug.Log($"Map Id changed to: {value}. Save the scene to finalize.");
            });
        }

        [MenuItem("GG/Map Editor/Debug/Change Map Steam Item Id")]
        public static void ChangeMapSteamItemId()
        {
            ShowStringInput("Change Map Steam Item Id", "Enter Steam Item Id (Only change this if something is broken):", value =>
            {
                EditorMapSettings settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();
                if (settings == null) { Debug.LogError("Failed to change steam item id, no MapSettings found."); return; }

                Undo.RecordObject(settings, "Change Map Steam Item Id");
                settings.SteamItemId = value;
                EditorUtility.SetDirty(settings);

                Debug.Log($"Steam Item Id changed to: {value}. Save the scene to finalize.");
            });
        }

        [MenuItem("GG/Map Editor/Debug/Change Map Steam Author Id")]
        public static void ChangeMapSteamAuthorId()
        {
            ShowStringInput("Change Map Steam Author Id", "Enter Steam Author Id (Only change this if something is broken):", value =>
            {
                EditorMapSettings settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();
                if (settings == null) { Debug.LogError("Failed to change author id, no MapSettings found."); return; }

                Undo.RecordObject(settings, "Change Map Steam Author Id");
                settings.SteamAuthorId = value;
                EditorUtility.SetDirty(settings);

                Debug.Log($"Steam Author Id changed to: {value}. Save the scene to finalize.");
            });
        }

        private static void ShowStringInput(string title, string label, Action<string> onConfirm)
        {
            StringInputWindow.Show(title, label, onConfirm);
        }

        private class StringInputWindow : EditorWindow
        {
            private string label;
            private string value;
            private Action<string> onConfirm;

            public static void Show(string title, string label, Action<string> onConfirm)
            {
                var window = CreateInstance<StringInputWindow>();

                window.titleContent = new GUIContent(title);
                window.label = label;
                window.onConfirm = onConfirm;

                window.minSize = new Vector2(400, 75);
                window.maxSize = new Vector2(400, 75);

                window.ShowUtility();
            }

            private void OnGUI()
            {
                EditorGUILayout.LabelField(label);

                GUI.SetNextControlName("InputField");
                value = EditorGUILayout.TextField(value);

                GUILayout.Space(10);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Cancel"))
                    {
                        Close();
                    }

                    if (GUILayout.Button("OK"))
                    {
                        onConfirm?.Invoke(value);
                        Close();
                    }
                }

                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
                {
                    onConfirm?.Invoke(value);
                    Close();
                    Event.current.Use();
                }

                EditorGUI.FocusTextInControl("InputField");
            }
        }
    }
}