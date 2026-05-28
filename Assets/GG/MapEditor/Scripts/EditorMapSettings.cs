using System;
using System.Collections.Generic;
using UnityEngine;

namespace GG.BeanBattles.MapEditor
{
    public class EditorMapSettings : EditorMapBehaviour
    {
        // VERSION 1
        [ShowOnly] public string Id;
        [ShowOnly] public string SteamItemId;
        [ShowOnly] public string SteamAuthorId;

        [ShowOnly] public string CreationDate;
        [ShowOnly] public string LastUpdate;

        public string MapName = "new-map";
        public string Author = "";
        public string Description = "";
        public Texture2D PreviewImage;

        public EditorMapStage[] Stages;

        // assigned on load with assignspawns
        // these are only spawned inside the stage bounds
        // so we dont need to assign per stage
        [NonSerialized] public EditorMapVehicleSpawn[] VehicleSpawns;
        [NonSerialized] public EditorMapWeaponSpawn[] WeaponSpawns;

        // assigned on load with assignspawns
        [NonSerialized] public EditorMapDefaultCamera DefaultCamera;
        [NonSerialized] public EditorMapWinnerStand WinnerStand;

        public void GenerateMapId()
        {
            Id = Guid.NewGuid().ToString();
            CreationDate = DateTime.UtcNow.ToString("O");
        }

        public void AssignSpawns()
        {
            DefaultCamera = FindObjectOfType<EditorMapDefaultCamera>();
            WinnerStand = FindObjectOfType<EditorMapWinnerStand>();
            VehicleSpawns = FindObjectsOfType<EditorMapVehicleSpawn>();
            WeaponSpawns = FindObjectsOfType<EditorMapWeaponSpawn>();

            for (int i = 0; i < Stages.Length; i++)
            {
                EditorMapStage stage = Stages[i];

                List<EditorMapPlayerSpawn> primary = new List<EditorMapPlayerSpawn>();
                List<EditorMapPlayerSpawn> secondary = new List<EditorMapPlayerSpawn>();
                List<EditorMapTeamSpawn> teams = new List<EditorMapTeamSpawn>();

                foreach (var spawn in FindObjectsOfType<EditorMapPlayerSpawn>())
                {
                    if (spawn.Stage != i) continue;
                    if (spawn.SpawnType == PlayerSpawnType.Primary) primary.Add(spawn);
                    if (spawn.SpawnType == PlayerSpawnType.Secondary) secondary.Add(spawn);
                }

                foreach (var spawn in FindObjectsOfType<EditorMapTeamSpawn>())
                {
                    if (spawn.Stage != i) continue;
                    teams.Add(spawn);
                }

                stage.PrimaryPlayerSpawns = primary.ToArray();
                stage.SecondaryPlayerSpawns = secondary.ToArray();
                stage.TeamSpawns = teams.ToArray();
            }
        }

        private void OnDrawGizmosSelected()
        {
            Color color = Color.cyan;
            foreach (var stage in Stages)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(stage.ZoneOffset, stage.ZoneSize);

                EditorMapGizmos.DrawSpawn(stage.ZoneOffset, Quaternion.identity, stage.ZoneSize, Vector3.zero, color, $"Stage {stage.DisplayName}", Vector3.zero);
            }
        }
    }
}
