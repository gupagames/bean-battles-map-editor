using System.IO;
using UnityEngine;

namespace GG.BeanBattles.MapEditor
{
    public static class MapEditorPaths
    {
        public static string EditorMapsPath = Path.Combine(Application.persistentDataPath, "EditorMaps");
        public static string EditorMapsCachePath = Path.Combine(Application.persistentDataPath, "Cache");
        public static string EditorMapExtension = ".bbmap";

        public static string TemplatePath = "Assets/GG/Scenes/Templete.unity";
        public static string PackagePath = Path.Combine(Application.dataPath, "GG/MapEditor/package.json");
        public static string UploaderPath = Path.Combine(Application.dataPath, "../Tools/BeanBattlesMapEditorSteamUploader.exe");
    }
}
