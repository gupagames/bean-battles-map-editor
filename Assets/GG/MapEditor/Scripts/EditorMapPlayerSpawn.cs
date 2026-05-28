using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GG.BeanBattles.MapEditor
{
    public enum PlayerSpawnType { Primary, Secondary }

    public class EditorMapPlayerSpawn : EditorMapBehaviour
    {
        public PlayerSpawnType SpawnType = PlayerSpawnType.Primary;
        public int Stage = 0;

        private Vector3 _gizmoOffset = new Vector3(0, 0, 0);
        private Vector3 _gizmoSize = new Vector3(1, 2, 1);
        private Vector3 _labelOffset = new Vector3(0, 2f, 0);

        private Color _color = Color.green;

        private void OnDrawGizmosSelected()
        {
            EditorMapGizmos.DrawSpawn(transform.position, transform.rotation, _gizmoSize, _gizmoOffset, _color, $"Player Spawn - Stage {Stage} - Type {SpawnType}", _labelOffset);
        }
    }
}
