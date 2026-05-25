using System;
using UnityEngine;

namespace GG.BeanBattles.MapEditor
{
    [Serializable]
    public class EditorMapStage
    {
        public string DisplayName = "Default";

        public int WeaponSpawns;
        public int VehicleSpawns;

        // assigned on export
        [HideInInspector] public EditorMapSpawnPoint[] PrimaryPlayerSpawns;
        [HideInInspector] public EditorMapSpawnPoint[] SecondaryPlayerSpawns;
        [HideInInspector] public EditorMapSpawnPoint[] TeamSpawns;

        public Vector3Int ZoneSize = new Vector3Int(100, 300, 100);
        public Vector3 ZoneOffset = new Vector3(0, 0, 0);
    }
}
