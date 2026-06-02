using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GG.BeanBattles.MapEditor
{
    public class EditorMapZoneFinish : EditorMapBehaviour
    {
        [Tooltip("Specifies which stage the zone finish should be used on.")]
        public int Stage = 0;

        private Vector3 _gizmoOffset = new Vector3(0, 0, 0);
        private Vector3 _gizmoSize = new Vector3(25, 5, 25);
        private Vector3 _labelOffset = new Vector3(0, 3.5f, 0);

        private Color _color = Color.white;

        private void OnDrawGizmosSelected()
        {
            EditorMapGizmos.DrawSpawn(transform.position, transform.rotation, _gizmoSize, _gizmoOffset, _color, $"Zone Finish - Stage {Stage}", _labelOffset);
        }
    }
}