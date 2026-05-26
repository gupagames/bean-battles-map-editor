using System;

namespace GG.BeanBattles.MapEditor
{
    [Serializable]
    public class EditorMapMetaData
    {
        public float EditorVersion;

        public string MapName;
        public string Description;
        public string Author;

        public string MapId;
        public string MapHash;

        public string SteamItemId;
        public string SteamAuthorId;

        public string CreationDate;
        public string LastUpdate;

        public EditorMapStageMetaData[] Stages;
    }

    [Serializable]
    public class EditorMapStageMetaData
    {
        public string DisplayName;
        public int Width;
        public int Depth;
    }
}
