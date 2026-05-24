using System.IO;
using UnityEngine;

namespace GG.BeanBattles.MapEditor
{
    public static class MapEditorPaths
    {
        public static string EditorMapsPath = Path.Combine(Application.persistentDataPath, "EditorMaps");
        public static string EditorMapsCachePath = Path.Combine(Application.persistentDataPath, "Cache");
        public static string EditorMapExtension = ".bbmap";
    }
}
