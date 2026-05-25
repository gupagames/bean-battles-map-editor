using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GG.BeanBattles.MapEditor
{

    public class EditorMapVehicleSpawn : EditorMapBehaviour
    {
        private Vector3 _gizmoOffset = new Vector3(0, .5f, 2);
        private Vector3 _gizmoSize = new Vector3(3.5f, 3, 5);
        private Vector3 _labelOffset = new Vector3(0, 2.5f, 0);

        private Color _color = Color.red;

        private void OnDrawGizmosSelected()
        {
            EditorMapGizmos.DrawSpawn(transform.position, transform.rotation, _gizmoSize, _gizmoOffset, _color, $"Vehicle Spawn", _labelOffset);
        }
    }
}