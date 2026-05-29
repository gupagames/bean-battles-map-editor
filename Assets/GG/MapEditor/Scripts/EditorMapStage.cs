using System;
using UnityEngine;

namespace GG.BeanBattles.MapEditor
{
    [Serializable]
    public class EditorMapStage
    {
        [Tooltip("What you want to name the specific stage.")]
        public string StageName = "Default";

        [Tooltip("How many weapons spawns you want on that stage.")]
        public int WeaponSpawns = 1;

        [Tooltip("How many vehicle spawns you want on that stage.")]
        public int VehicleSpawns = 0;

        // assigned on load with assignspawns
        [HideInInspector] public EditorMapPlayerSpawn[] PrimaryPlayerSpawns;
        [HideInInspector] public EditorMapPlayerSpawn[] SecondaryPlayerSpawns;
        [HideInInspector] public EditorMapTeamSpawn[] TeamSpawns;

        [Tooltip("How big you want the death zone on the outside to be.")]
        public Vector3Int ZoneSize = new Vector3Int(100, 300, 100);

        [Tooltip("How far from the origin(0,0,0) you want the death zone.")]
        public Vector3 ZoneOffset = new Vector3(0, 0, 0);
    }
}
