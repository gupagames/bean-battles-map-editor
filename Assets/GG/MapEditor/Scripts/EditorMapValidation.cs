using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GG.BeanBattles.MapEditor
{
    public static class EditorMapValidation
    {
        // we do array over hashset so we can do IsAssignableFrom
        public static readonly Type[] ValidTypes =
        {
            typeof(Transform),
            typeof(Renderer),
            typeof(MeshFilter),
            typeof(Collider),
            typeof(Light),
            typeof(EditorMapBehaviour),
            typeof(Tree),
            typeof(AudioSource),
            typeof(ParticleSystem),
            typeof(ParticleSystemRenderer),
        };

        public static readonly HashSet<string> ProfanityList = LoadProfanityList();

        public static int NameMinLength = 3;
        public static int NameMaxLength = 16;

        public static int DescMinLength = 0;
        public static int DescMaxLength = 500;

        public static int RequiredPlayerSpawns = 16;
        public static int RequiredTeamSpawns = 4;

        public static int RequiredWeaponSpawns = 1;

        public static int MaxTriangles = 500000;
        public static int MaxTextureDimension = 4096;
        public static int MaxRealtimeLights = 64;
        public static int MaxMeshCollidersVerts = 255;
        public static int MaxTotalTransforms = 5000;

        public static int MaxParticleSystems = 10;
        public static int MaxParticlesPerSystem = 1000;

        public static bool ValidateLoadedMap(Scene mapScene)
        {
            // Validate EditorMapSettings exists and is valid
            EditorMapSettings settings = UnityEngine.Object.FindObjectOfType<EditorMapSettings>();

            // settings doesnt exist
            if (settings == null)
            { Debug.LogError("Failed to validate map, no MapSettings found."); return false; }

            // settings not valid
            // this validates spawns, names, and needed items
            if (!ValidateMapSettings(settings))
            { Debug.LogError("Failed to validate map, MapSettings Invalid"); return false; }

            // there will be multiple scenes when loaded in bb, so need to only do map checks for this scene
            // all editor stuff from mapsettings above should be only one the scene, so we dont need to limit where we search
            GameObject[] sceneObjects = GetSceneObjects(mapScene);

            // validate Objects / Types
            if (!ValidateTypes(sceneObjects))
            { Debug.LogError("Failed to validate map, types invalid"); return false; }

            // general map checks
            if (!ValidateTriangleCount(sceneObjects))
            { Debug.LogError("Failed to validate map, triangle count invalid"); return false; }

            if (!ValidateTextureResolution(sceneObjects))
            { Debug.LogError("Failed to validate map, texture resolution invalid"); return false; }

            if (!ValidateRealtimeLights(sceneObjects))
            { Debug.LogError("Failed to validate map, realtime lights invalid"); return false; }

            if (!ValidateMeshColliders(sceneObjects))
            { Debug.LogError("Failed to validate map, mesh colliders invalid"); return false; }

            if (!ValidateTransforms(sceneObjects))
            { Debug.LogError("Failed to validate map, transforms invalid"); return false; }

            if (!ValidateParticleSystems(sceneObjects))
            { Debug.LogError("Failed to validate map, particle systems invalid"); return false; }

            return true;
        }

        public static bool ValidateMapInstall(string path)
        {
            // missing path
            if (string.IsNullOrEmpty(path))
            { Debug.LogError("Failed to install, path missing"); return false; }

            // missing path
            if (!System.IO.Directory.Exists(path))
            { Debug.LogError($"Failed to install, directory not found: {path}"); return false; }

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
                // unknown file
                else { Debug.LogError($"Failed to install, unknown file found: {name}"); return false; }
            }

            // missing files
            if (!hasBundle)
            { Debug.LogError("Failed to install, map.bundle missing"); return false; }

            if (!hasJson)
            { Debug.LogError("Failed to install, map.json missing"); return false; }

            if (!hasPreview)
            { Debug.LogError("Failed to install, preview.png missing"); return false; }


            string jsonPath = System.IO.Path.Combine(path, "map.json");
            return ValidateMetaData(jsonPath);
        }

        public static bool ValidateMetaData(string jsonPath, string hash = "")
        {
            // missing path / file
            if (string.IsNullOrEmpty(jsonPath))
            { Debug.LogError("Failed to validate metadata, path missing"); return false; }

            // missing path / file
            if (!System.IO.File.Exists(jsonPath))
            { Debug.LogError($"Failed to validate metadata, file not found: {jsonPath}"); return false; }

            // invalid json
            string json = System.IO.File.ReadAllText(jsonPath);
            EditorMapMetaData meta;
            try { meta = JsonUtility.FromJson<EditorMapMetaData>(json); }
            catch (Exception e) { Debug.LogError($"Failed to validate metadata, JSON parse error: {e.Message}"); return false; }

            return ValidateMetaData(meta, hash);
        }

        public static bool ValidateMetaData(EditorMapMetaData meta, string hash = "")
        {
            // has all needed values
            if (meta == null)
            { Debug.LogError("Failed to validate metadata, missing"); return false; }

            if (string.IsNullOrEmpty(meta.MapId))
            { Debug.LogError("Failed to validate metadata, MapId missing"); return false; }

            // has stages
            if (meta.Stages == null || meta.Stages.Length == 0)
            { Debug.LogError("Failed to validate metadata, no stages found"); return false; }

            // has valid map name, profanity and length
            if (!IsValidDisplayString(meta.MapName, NameMinLength, NameMaxLength))
            { Debug.LogError($"Failed to validate metadata, MapName invalid: {meta.MapName}"); return false; }

            // has valid map author, profanity and length
            if (!IsValidDisplayString(meta.Author, NameMinLength, NameMaxLength))
            { Debug.LogError($"Failed to validate metadata, Author invalid: {meta.Author}"); return false; }

            // has valid description, profanity and length
            if (!IsValidDisplayString(meta.Description, DescMinLength, DescMaxLength))
            { Debug.LogError($"Failed to validate metadata, Description invalid: {meta.Description}"); return false; }

            // each stage has valid name, profanity and length
            for (int i = 0; i < meta.Stages.Length; i++)
            {
                if (!IsValidDisplayString(meta.Stages[i].StageName, NameMinLength, NameMaxLength))
                { Debug.LogError($"Failed to validate metadata, Stage[{i}] name invalid: {meta.Stages[i].StageName}"); return false; }
            }

            // validate hash matches if given one
            if (!string.IsNullOrEmpty(hash) && hash != meta.MapHash)
            { Debug.LogError($"Failed to validate metadata, hash mismatch"); return false; }

            return true;
        }

        public static bool ValidateMapSettings(EditorMapSettings settings)
        {
            if (settings == null)
            { Debug.LogError("Failed to validate map settings, null"); return false; }

            // make sure that spawns are asssinged
            settings.AssignSpawns();

            // has valid id
            if (string.IsNullOrEmpty(settings.Id))
            { Debug.LogError("Failed to validate map settings, Id missing"); return false; }

            // has valid map name, no profanity and proper length
            if (!IsValidDisplayString(settings.MapName, NameMinLength, NameMaxLength))
            { Debug.LogError($"Failed to validate map settings, MapName invalid: {settings.MapName}"); return false; }

            // has valid map author, no profanity and proper length
            if (!IsValidDisplayString(settings.Author, NameMinLength, NameMaxLength))
            { Debug.LogError($"Failed to validate map settings, Author invalid: {settings.Author}"); return false; }

            // has valid description, no profanity and proper length
            if (!IsValidDisplayString(settings.Description, DescMinLength, DescMaxLength))
            { Debug.LogError($"Failed to validate map settings, Description invalid: {settings.Description}"); return false; }

            // has preview image
            if (settings.PreviewImage == null)
            { Debug.LogError("Failed to validate map settings, PreviewImage missing"); return false; }

            // has winner stand
            if (settings.WinnerStand == null)
            { Debug.LogError("Failed to validate map settings, WinnerStand missing"); return false; }

            // has default camera
            if (settings.DefaultCamera == null)
            { Debug.LogError("Failed to validate map settings, DefaultCamera missing"); return false; }

            // has at least 1 stage
            if (settings.Stages == null || settings.Stages.Length == 0)
            { Debug.LogError("Failed to validate map settings, no stages found"); return false; }

            for (int i = 0; i < settings.Stages.Length; i++)
            {
                EditorMapStage stage = settings.Stages[i];

                if (stage == null)
                { Debug.LogError($"Failed to validate map settings, Stage[{i}] is null"); return false; }

                // has valid description, no profanity and proper length
                if (!IsValidDisplayString(stage.StageName, NameMinLength, NameMaxLength))
                { Debug.LogError($"Failed to validate map settings, Stage[{i}] name invalid: {stage.StageName}"); return false; }

                // at least 1 weapon will spawn
                if (stage.WeaponSpawns < 1)
                { Debug.LogError($"Failed to validate map settings, Stage[{i}] must have at least 1 weapon spawn"); return false; }

                // zone size is valid
                if (stage.ZoneSize.x <= 0 || stage.ZoneSize.y <= 0 || stage.ZoneSize.z <= 0)
                { Debug.LogError($"Failed to validate map settings, Stage[{i}] ZoneSize invalid: {stage.ZoneSize}"); return false; }

                // has RequiredPlayerSpawns player spawns and no null refs
                int primaryCount = stage.PrimaryPlayerSpawns?.Length ?? 0;
                int secondaryCount = stage.SecondaryPlayerSpawns?.Length ?? 0;
                int totalPlayerSpawns = primaryCount + secondaryCount;

                if (totalPlayerSpawns < RequiredPlayerSpawns)
                { Debug.LogError($"Failed to validate map settings, Stage[{i}] has {totalPlayerSpawns}/{RequiredPlayerSpawns} player spawns"); return false; }

                if (stage.PrimaryPlayerSpawns.Any(s => s == null))
                { Debug.LogError($"Failed to validate map settings, Stage[{i}] has null primary player spawn"); return false; }

                if (stage.SecondaryPlayerSpawns.Any(s => s == null))
                { Debug.LogError($"Failed to validate map settings, Stage[{i}] has null secondary player spawn"); return false; }

                // has RequiredTeamSpawns team spawns and no null refs
                if (stage.TeamSpawns == null || stage.TeamSpawns.Length < RequiredTeamSpawns)
                { Debug.LogError($"Failed to validate map settings, Stage[{i}] has {stage.TeamSpawns?.Length ?? 0}/{RequiredTeamSpawns} team spawns"); return false; }

                if (stage.TeamSpawns.Any(s => s == null))
                { Debug.LogError($"Failed to validate map settings, Stage[{i}] has null team spawn"); return false; }
            }

            // has at least RequiredWeaponSpawns weapon spawn
            if (settings.WeaponSpawns == null || settings.WeaponSpawns.Length < RequiredWeaponSpawns)
            { Debug.LogError($"Failed to validate map settings, has {settings.WeaponSpawns?.Length ?? 0}/{RequiredWeaponSpawns} weapon spawns"); return false; }

            if (settings.WeaponSpawns.Any(s => s == null))
            { Debug.LogError("Failed to validate map settings, null ref found in weapon spawns"); return false; }

            // we dont need to have vehicles so skip

            return true;
        }


        // HELPERS
        private static bool IsValidDisplayString(string str, int minLength, int maxLength)
        {
            if (string.IsNullOrEmpty(str)) return minLength == 0;

            if (str.Length < minLength || str.Length > maxLength) return false;

            string lowerStr = str.ToLower();
            var words = Regex.Split(lowerStr, @"\W+");
            if (words.Any(w => ProfanityList.Contains(w))) return false;

            return true;
        }

        // this runs in client too so any modifications to files should be fine
        private static HashSet<string> LoadProfanityList()
        {
            TextAsset file = Resources.Load<TextAsset>("profanity");

            if (file == null)
            { Debug.LogWarning("Profanity list not found in Resources."); return new HashSet<string>(); }

            return new HashSet<string>(file.text.Split('\n').Select(w => w.Trim().ToLower()).Where(w => !string.IsNullOrWhiteSpace(w)));
        }


        private static bool IsAllowedComponent(Type type)
        {
            // is assignable so we can do inheritance to make it easier
            foreach (Type validType in ValidTypes) if (validType.IsAssignableFrom(type)) return true;
            return false;
        }

        private static GameObject[] GetSceneObjects(Scene scene)
        {
            List<GameObject> objects = new List<GameObject>();
            foreach (GameObject root in scene.GetRootGameObjects())
                objects.AddRange(root.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject));
            return objects.ToArray();
        }

        private static bool ValidateTypes(GameObject[] objects)
        {
            foreach (GameObject go in objects)
            {
                foreach (Component component in go.GetComponents<Component>())
                {
                    if (component == null)
                    { Debug.LogError($"Missing script detected on: {go.name}"); return false; }

                    if (!IsAllowedComponent(component.GetType()))
                    { Debug.LogError($"Disallowed component: {component.GetType().Name} on: {go.name}"); return false; }
                }
            }
            return true;
        }

        private static bool ValidateTriangleCount(GameObject[] objects)
        {
            int total = 0;
            foreach (GameObject go in objects)
            {
                MeshFilter mf = go.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null) continue;
                total += mf.sharedMesh.triangles.Length / 3;
            }
            if (total > MaxTriangles)
            { Debug.LogError($"Failed to validate map, mesh triangle count too high: {total}/{MaxTriangles}. You likely have too detailed of objects, please check you models"); return false; }
            return true;
        }

        private static bool ValidateTextureResolution(GameObject[] objects)
        {
            foreach (GameObject go in objects)
            {
                Renderer r = go.GetComponent<Renderer>();
                if (r == null || r.sharedMaterials == null) continue;
                foreach (var mat in r.sharedMaterials)
                {
                    if (mat == null) continue;
                    Texture tex = mat.mainTexture;
                    if (tex == null) continue;
                    if (tex.width > MaxTextureDimension || tex.height > MaxTextureDimension)
                    { Debug.LogError($"Failed to validate map, texture too large: {tex.name} {tex.width}x{tex.height}, please use a smaller texture."); return false; }
                }
            }
            return true;
        }

        private static bool ValidateRealtimeLights(GameObject[] objects)
        {
            int count = 0;
            foreach (GameObject go in objects)
            {
                Light light = go.GetComponent<Light>();
                if (light == null) continue;
#if UNITY_EDITOR
                // lightmapBakeType is editor only, so this check only runs on export
                // client skips this, preferring false negatives over false positives
                if (light.lightmapBakeType == LightmapBakeType.Realtime) count++;
#endif
            }
            if (count > MaxRealtimeLights)
            { Debug.LogError($"Failed to validate map, too many realtime lights: {count}/{MaxRealtimeLights}. Please use less lights"); return false; }
            return true;
        }

        private static bool ValidateMeshColliders(GameObject[] objects)
        {
            foreach (GameObject go in objects)
            {
                MeshCollider mc = go.GetComponent<MeshCollider>();
                if (mc == null || mc.sharedMesh == null) continue;
                int verts = mc.sharedMesh.vertexCount;
                if (verts > MaxMeshCollidersVerts)
                { Debug.LogError($"Failed to validate map, MeshCollider too complex: {go.name} {verts}/{MaxMeshCollidersVerts} verts. Your object colliders are too detailed. Use less detailed models or swap to box etc. colliders."); return false; }
            }
            return true;
        }

        private static bool ValidateTransforms(GameObject[] objects)
        {
            if (objects.Length > MaxTotalTransforms)
            { Debug.LogError($"Failed to validate map, too many objects: {objects.Length}/{MaxTotalTransforms}"); return false; }

            foreach (GameObject go in objects)
            {
                Vector3 pos = go.transform.position;
                Vector3 scale = go.transform.localScale;

                if (float.IsNaN(pos.x) || float.IsNaN(pos.y) || float.IsNaN(pos.z) || float.IsInfinity(pos.x) || float.IsInfinity(pos.y) || float.IsInfinity(pos.z))
                { Debug.LogError($"Failed to validate map, invalid position on: {go.name}"); return false; }

                if (float.IsNaN(scale.x) || float.IsNaN(scale.y) || float.IsNaN(scale.z) || float.IsInfinity(scale.x) || float.IsInfinity(scale.y) || float.IsInfinity(scale.z))
                { Debug.LogError($"Failed to validate map, invalid scale on: {go.name}"); return false; }
            }
            return true;
        }

        private static bool ValidateParticleSystems(GameObject[] objects)
        {
            List<ParticleSystem> particles = new List<ParticleSystem>();
            foreach (GameObject go in objects)
            {
                ParticleSystem ps = go.GetComponent<ParticleSystem>();
                if (ps != null) particles.Add(ps);
            }

            if (particles.Count > MaxParticleSystems)
            { Debug.LogError($"Failed to validate map, too many particle systems: {particles.Count}/{MaxParticleSystems}"); return false; }

            foreach (var ps in particles)
            {
                if (ps.main.maxParticles > MaxParticlesPerSystem)
                { Debug.LogError($"Failed to validate map, too many particles on: {ps.gameObject.name} {ps.main.maxParticles}/{MaxParticlesPerSystem}"); return false; }
            }
            return true;
        }
    }
}