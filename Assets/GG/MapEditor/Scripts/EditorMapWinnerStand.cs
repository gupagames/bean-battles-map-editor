using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GG.BeanBattles.MapEditor
{
    public class EditorMapWinnerStand : EditorMapBehaviour
    {
        private Vector3 _gizmoOffset = new Vector3(0, 1f, 0);
        private Vector3 _gizmoSize = new Vector3(16, 3, 2);
        private Vector3 _labelOffset = new Vector3(0, 2, 0);

        private Vector3 _cameraGizmoOffset = new Vector3(0, 1.5f, 10);
        private Vector3 _cameraGizmoSize = new Vector3(1, 1, 1);
        private Vector3 _cameraLabelOffset = new Vector3(0, 1, 0);

        private Color _color = Color.white;

        private void OnDrawGizmosSelected()
        {
            EditorMapGizmos.DrawSpawn(transform.position, transform.rotation, _gizmoSize, _gizmoOffset, _color, "Winner Stand", _labelOffset);

            // also draw the camera
            EditorMapGizmos.DrawSpawn(transform.position, transform.rotation, _cameraGizmoSize, _cameraGizmoOffset, _color, "Winner Stand Camera", _cameraLabelOffset);
        }
    }
}