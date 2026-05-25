using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GG.BeanBattles.MapEditor
{
    public class EditorMapDefaultCamera : EditorMapBehaviour
    {
        private Vector3 _gizmoOffset = new Vector3(0, 0, 0);
        private Vector3 _gizmoSize = new Vector3(1, 1, 1);
        private Vector3 _labelOffset = new Vector3(0, 1.5f, 0);

        private Color _color = Color.grey;

        private void OnDrawGizmosSelected()
        {
            EditorMapGizmos.DrawSpawn(transform.position, transform.rotation, _gizmoSize, _gizmoOffset, _color, "Default Camera", _labelOffset);
        }
    }
}