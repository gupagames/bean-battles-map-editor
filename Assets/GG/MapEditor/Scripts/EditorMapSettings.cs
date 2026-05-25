using System.Collections.Generic;
using UnityEngine;

namespace GG.BeanBattles.MapEditor
{
    public class EditorMapSettings : EditorMapBehaviour
    {
        // VERSION 1
        public string MapName = "new-map";
        public string Author = "";
        public string Description = "";
        public Texture2D PreviewImage;

        public EditorMapStage[] Stages;

        // assigned on export
        // these are only spawned inside the stage bounds
        // so we dont need to assign per stage
        [HideInInspector] public EditorMapSpawnPoint[] VehicleSpawns;
        [HideInInspector] public EditorMapSpawnPoint[] WeaponSpawns;

        public void AssignSpawns()
        {
            // Get map spawns (weapon / vehcle)
            List<EditorMapSpawnPoint> vehicleSpawns = new List<EditorMapSpawnPoint>();
            List<EditorMapSpawnPoint> weaponSpawns = new List<EditorMapSpawnPoint>();

            EditorMapVehicleSpawn[] vehicleObjects = FindObjectsOfType<EditorMapVehicleSpawn>();

            foreach (var spawn in vehicleObjects)
            {
                vehicleSpawns.Add(new EditorMapSpawnPoint
                {
                    Position = spawn.transform.position,
                    Rotation = spawn.transform.rotation
                });
            }

            EditorMapWeaponSpawn[] weaponObjects = FindObjectsOfType<EditorMapWeaponSpawn>();

            foreach (var spawn in weaponObjects)
            {
                weaponSpawns.Add(new EditorMapSpawnPoint
                {
                    Position = spawn.transform.position,
                    Rotation = spawn.transform.rotation
                });
            }

            VehicleSpawns = vehicleSpawns.ToArray();
            WeaponSpawns = weaponSpawns.ToArray();

            // STAGE SPAWNS
            for (int i = 0; i < Stages.Length; i++)
            {
                EditorMapStage stage = Stages[i];

                List<EditorMapSpawnPoint> primary = new List<EditorMapSpawnPoint>();
                List<EditorMapSpawnPoint> secondary = new List<EditorMapSpawnPoint>();
                List<EditorMapSpawnPoint> teams = new List<EditorMapSpawnPoint>();

                EditorMapPlayerSpawn[] playerSpawns = FindObjectsOfType<EditorMapPlayerSpawn>();

                foreach (var spawn in playerSpawns)
                {
                    if (spawn.Stage != i) continue;

                    EditorMapSpawnPoint point = new EditorMapSpawnPoint
                    {
                        Position = spawn.transform.position,
                        Rotation = spawn.transform.rotation
                    };

                    if (spawn.SpawnType == PlayerSpawnType.Primary) primary.Add(point);
                    if (spawn.SpawnType == PlayerSpawnType.Secondary) secondary.Add(point);
                }

                EditorMapTeamSpawn[] teamSpawns = FindObjectsOfType<EditorMapTeamSpawn>();

                foreach (var spawn in teamSpawns)
                {
                    if (spawn.Stage != i) continue;

                    teams.Add(new EditorMapSpawnPoint
                    {
                        Position = spawn.transform.position,
                        Rotation = spawn.transform.rotation
                    });
                }

                stage.PrimaryPlayerSpawns = primary.ToArray();
                stage.SecondaryPlayerSpawns = secondary.ToArray();
                stage.TeamSpawns = teams.ToArray();
            }
        }


        private void OnDrawGizmosSelected()
        {
            foreach (var stage in Stages)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(stage.ZoneOffset, stage.ZoneSize);
            }
        }
    }
}
