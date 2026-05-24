using UnityEngine;

namespace GG.BeanBattles.MapEditor
{
    public class EditorMapSettings : EditorMapBehaviour
    {
        // meta data
        public string MapName = "new-map";
        public string Author = "";
        public string Description = "";
        public Texture2D PreviewImage;

        public int EditorVersion = 1;

        // map data
        public int GridSize;
    }
}
