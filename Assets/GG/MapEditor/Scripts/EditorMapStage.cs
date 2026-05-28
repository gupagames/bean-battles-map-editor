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

        // assigned on load with assignspawns
        [HideInInspector] public EditorMapPlayerSpawn[] PrimaryPlayerSpawns;
        [HideInInspector] public EditorMapPlayerSpawn[] SecondaryPlayerSpawns;
        [HideInInspector] public EditorMapTeamSpawn[] TeamSpawns;

        public Vector3Int ZoneSize = new Vector3Int(100, 300, 100);
        public Vector3 ZoneOffset = new Vector3(0, 0, 0);
    }
}
