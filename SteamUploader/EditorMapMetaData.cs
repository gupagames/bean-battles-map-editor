using System;

namespace GG.BeanBattles.MapEditor
{
    [Serializable]
    public class EditorMapMetaData
    {
        public float EditorVersion = 0.0f;

        public string MapName = "";
        public string Description = "";
        public string Author = "";

        public string MapId = "";
        public string MapHash = "";

        public string SteamItemId = "";
        public string SteamAuthorId = "";

        public string CreationDate = "";
        public string LastUpdate = "";

        public EditorMapStageMetaData[] Stages = null;
    }

    [Serializable]
    public class EditorMapStageMetaData
    {
        public string DisplayName = "";
        public int Width = 0;
        public int Depth = 0;
    }
}
