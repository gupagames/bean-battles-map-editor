using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GG.BeanBattles.MapEditor
{

    public class EditorMapWeaponSpawn : EditorMapBehaviour
    {
        private Vector3 _gizmoOffset = new Vector3(0, 0, 0);
        private Vector3 _gizmoSize = new Vector3(2, 2, 2);
        private Vector3 _labelOffset = new Vector3(0, 2, 0);

        private Color _color = Color.yellow;

        private void OnDrawGizmosSelected()
        {
            var relativeMatrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            Gizmos.color = _color;
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = relativeMatrix;
            Gizmos.DrawWireCube(_gizmoOffset, _gizmoSize);

#if UNITY_EDITOR
            SceneView sceneView = SceneView.currentDrawingSceneView;

            if (sceneView == null)
                return;

            Camera cam = sceneView.camera;

            float distance = Vector3.Distance(cam.transform.position, transform.position);

            // scale based on distance
            int fontSize = Mathf.Clamp((int)(200 / distance), 1, 40);

            if (fontSize <= 1) return; // too small, just dont draw it

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.normal.textColor = _color;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = fontSize;

            string label = $"WeaponSpawn";
            Vector3 labelPos = _gizmoOffset + _labelOffset;
            Handles.matrix = relativeMatrix;
            Handles.Label(labelPos, label, style);
            Handles.matrix = oldMatrix;
#endif
        }
    }
}