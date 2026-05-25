using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GG.BeanBattles.MapEditor
{
    public class EditorMapTeamSpawn : EditorMapBehaviour
    {
        public int Stage = 0;

        private Vector3 _gizmoOffset = new Vector3(0, 0, 0);
        private Vector3 _gizmoSize = new Vector3(16, 2, 1);
        private Vector3 _labelOffset = new Vector3(0, 2, 0);

        private Color _color = Color.magenta;

        private void OnDrawGizmosSelected()
        {
            EditorMapGizmos.DrawSpawn(transform.position, transform.rotation, _gizmoSize, _gizmoOffset, _color, $"Team Spawn - Stage {Stage}", _labelOffset);
        }
    }
}