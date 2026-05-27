using System;
using UnityEngine;

namespace GG.BeanBattles.MapEditor
{
    public static class EditorMapValidation
    {
        public static readonly Type[] ValidTypes =
        {
            typeof(Transform),
            typeof(Renderer),
            typeof(MeshFilter),
            typeof(Collider),
            typeof(Light),
            typeof(EditorMapBehaviour),
            typeof(Tree)
        };

        public static bool ValidateMap()
        {
            // TODO: Validate EditorMapSettings exists

            // Vadilate Objects / Types
            if (!ValidateTypes()) return false;

            // TODO: Validate player spawns / vehcicals spanws, weaspons spanws

            // TODO: max name length, max author length > add to map install too?

            // TODO: No profanity in author or name

            // TODO: Validate max triangle count

            // TODO: Validate max texture resolution

            // TODO: Validate max material count

            // TODO: Validate max realtime lights

            // TODO: Validate mesh collider restrictions

            // TODO: Validate allowed shader list

            // TODO: Validate bundle size limit

            // TODO: Validate map bounds/max world size

            // TODO: Validate no invalid  postions etc transforms (NaN/Infinity)

            // TODO: Add runtime validation after bundle load

            // TODO: Add map/game version compatibility validation

            return true;
        }

        public static bool ValidateMapInstall(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;

            if (!System.IO.Directory.Exists(path)) return false;

            var files = System.IO.Directory.GetFiles(path);

            bool hasBundle = false;
            bool hasJson = false;
            bool hasPreview = false;

            foreach (var file in files)
            {
                string name = System.IO.Path.GetFileName(file);

                if (name == "map.bundle") hasBundle = true;
                else if (name == "map.json") hasJson = true;
                else if (name == "preview.png") hasPreview = true;
                else return false; // unknown file found reject immediately
            }

            bool hasAll = hasBundle && hasJson && hasPreview;
            if (!hasAll) return false;

            string jsonPath = System.IO.Path.Combine(path, "map.json");

            return ValidateMetaData(jsonPath);

        }

        public static bool ValidateMetaData(string jsonPath)
        {
            if (string.IsNullOrEmpty(jsonPath)) return false;
            if (!System.IO.File.Exists(jsonPath)) return false;

            string json = System.IO.File.ReadAllText(jsonPath);

            EditorMapMetaData meta;

            try { meta = JsonUtility.FromJson<EditorMapMetaData>(json); }
            catch { return false; }

            return ValidateMetaData(meta);
        }

        public static bool ValidateMetaData(EditorMapMetaData meta)
        {
            if (meta == null) return false;
            if (string.IsNullOrEmpty(meta.MapId)) return false;
            if (string.IsNullOrEmpty(meta.MapName)) return false;
            if (meta.Stages == null || meta.Stages.Length == 0) return false;

            return true;
        }

        public static bool ValidateMapSettings()
        {
            return true;
        }

        public static bool ValidateTypes()
        {
            Component[] components = UnityEngine.Object.FindObjectsOfType<Component>();

            foreach (Component component in components)
            {
                if (component == null)
                {
                    Debug.LogError("Missing script detected.");
                    return false;
                }

                Type type = component.GetType();

                if (!IsAllowedComponent(type))
                {
                    Debug.LogError($"Disallowed component: {type.Name} on GameObject: {component.gameObject.name}");
                    return false;
                }
            }

            return true;
        }

        public static bool IsAllowedComponent(Type type)
        {
            // is assignable so we can do inheritance to make it easier
            foreach (Type validType in ValidTypes) if (validType.IsAssignableFrom(type)) return true;
            return false;
        }
    }
}